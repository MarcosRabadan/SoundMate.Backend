using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Application.Abstractions.Security;
using SoundMate.Application.Common.Exceptions;
using SoundMate.Application.Users.DTO;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Users;

/// <inheritdoc cref="IUserService"/>
internal sealed class UserService : IUserService
{
    /// <summary>
    /// The unique index on <c>Users.Email</c>. It mirrors <c>UserConfiguration</c>, where
    /// <c>HasIndex(u => u.Email).IsUnique()</c> makes EF generate this name. Matching on it rather
    /// than on any unique violation means a future second index on the table cannot silently start
    /// reporting itself as a duplicate email.
    /// </summary>
    private const string EmailUniqueIndex = "IX_Users_Email";

    private readonly IUserRepository _users;
    private readonly IMembershipRepository _memberships;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository users,
                       IMembershipRepository memberships,
                       IUnitOfWork unitOfWork,
                       IPasswordHasher passwordHasher)
    {
        _users = users;
        _memberships = memberships;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        // Email.Create both validates and normalises. Doing it first means the uniqueness check
        // below runs on the same value that will be stored.
        var email = Email.Create(dto.Email);

        // The friendly path: one cheap read that answers "taken" before doing any work.
        //
        // This counts soft-deleted users, on purpose. Their row still holds the email in the
        // unique index, so pretending it is free would only turn this 409 into a 23505 further
        // down. More importantly, eight tables still reference that user's id: handing their
        // email to somebody new would create a second person wearing the first one's identity.
        // Restoring is how a deleted account comes back.
        if (await _users.ExistsByEmailAsync(email, cancellationToken))
            throw new EmailAlreadyRegisteredException(email.Value);

        var user = User.Register(
            email,
            _passwordHasher.Hash(dto.Password),
            dto.FullName,
            dto.Phone);

        await _users.AddAsync(user, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException ex) when (ex.ConstraintName == EmailUniqueIndex)
        {
            // The check above is not a guarantee: it is a separate statement, so two registrations
            // for the same email in flight at once both pass it and the index rejects the loser.
            // Postgres reports 23505, which unhandled reaches the caller as a 500 — a server fault
            // for what is really the same "already taken" answer the check gives. So: same answer.
            throw new EmailAlreadyRegisteredException(email.Value, ex);
        }

        return user.ToDto();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(UserId.From(id), cancellationToken);

        return user is null || user.IsDeleted ? null : user.ToDto();
    }

    public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // A malformed email matches nobody, which is the honest answer. Letting Email.Create throw
        // would turn a lookup that found nothing into a 400, and would confirm to a prober that
        // their input at least parsed.
        if (!Email.IsValid(email))
            return null;

        var user = await _users.GetByEmailAsync(Email.Create(email), cancellationToken);

        return user is null || user.IsDeleted ? null : user.ToDto();
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var user = await GetOrThrowAsync(id, cancellationToken);

        // Two behaviour methods rather than setters: each one keeps its own invariant and stamps
        // UpdatedAtUtc. Rename refuses a blank name, so a bad request never half-applies.
        user.Rename(dto.FullName);
        user.ChangePhone(dto.Phone);

        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var user = await GetOrThrowAsync(id, cancellationToken);

        // The whole point of the endpoint being safe. Without it, reaching this route is the same
        // as owning the account.
        if (!_passwordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new IncorrectPasswordException();

        user.ChangePasswordHash(_passwordHasher.Hash(dto.NewPassword));

        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<UserDto> VerifyEmailAsync(Guid id, CancellationToken cancellationToken = default)
        => MutateAsync(id, user => user.VerifyEmail(), cancellationToken);

    public Task<UserDto> SuspendAsync(Guid id, CancellationToken cancellationToken = default)
        => MutateAsync(id, user => user.Suspend(), cancellationToken);

    public Task<UserDto> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
        => MutateAsync(id, user => user.Reactivate(), cancellationToken);

    /// <summary>
    /// Soft-deletes: stamps <c>DeletedAtUtc</c> and leaves the row alone.
    /// <para>
    /// No membership check here, unlike <see cref="PurgeAsync"/>. Nothing is orphaned — the row
    /// and its id survive — and the operation is reversible, so refusing would be ceremony.
    /// </para>
    /// </summary>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Deliberately finds deleted users too: deleting twice is a no-op, not a 404.
        var user = await FindAnyAsync(id, cancellationToken);

        user.Delete();

        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // The one read that has to see past the soft delete — otherwise nothing could ever
        // reverse it.
        var user = await FindAnyAsync(id, cancellationToken);

        user.Restore();

        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }

    /// <summary>
    /// Removes the row for good.
    /// <para>
    /// <b>This orphans data and nothing at the database level stops it.</b> Aggregates reference
    /// each other by identity with no enforced foreign keys — deliberate, so a future
    /// database-per-service split stays cheap — and eight tables carry a <c>UserId</c>. Refusing
    /// while a <c>Membership</c> exists covers the anchor relationship, the one that always exists
    /// when the user belongs anywhere. It does not cover the rest: a user with no memberships can
    /// still leave behind <c>UserProfile</c>, <c>UserEducation</c>, <c>StudiedDiscipline</c>,
    /// <c>TaughtDiscipline</c>, <c>TaughtGenre</c> and <c>TeacherReview</c> rows, and Agendia
    /// keeps an <c>Employee</c> pointing at them.
    /// </para>
    /// <para>
    /// <see cref="DeleteAsync"/> is the answer almost every time. This one is for actual erasure
    /// — a retention policy expiring, a GDPR request — and wants a real cascade first.
    /// </para>
    /// </summary>
    public async Task PurgeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Finds deleted users too: purging is normally the second step after a soft delete.
        var user = await FindAnyAsync(id, cancellationToken);

        var memberships = await _memberships.ListByUserAsync(user.Id, cancellationToken);
        if (memberships.Count > 0)
            throw new UserStillHasMembershipsException(id, memberships.Count);

        _users.Remove(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<UserDto> MutateAsync(Guid id, Action<User> change, CancellationToken cancellationToken)
    {
        var user = await GetOrThrowAsync(id, cancellationToken);

        change(user);

        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }

    /// <summary>
    /// The live user, or <see cref="UserNotFoundException"/>. A soft-deleted user is "not found"
    /// as far as every ordinary operation is concerned: the row survives for the sake of the ids
    /// pointing at it, not so it can keep being edited.
    /// </summary>
    private async Task<User> GetOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await FindAnyAsync(id, cancellationToken);

        return user.IsDeleted ? throw new UserNotFoundException(id) : user;
    }

    /// <summary>The user whether or not they are soft-deleted. Only the lifecycle operations use it.</summary>
    private async Task<User> FindAnyAsync(Guid id, CancellationToken cancellationToken)
        => await _users.GetByIdAsync(UserId.From(id), cancellationToken)
           ?? throw new UserNotFoundException(id);
}
