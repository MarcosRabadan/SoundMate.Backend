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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository users,
                       IUnitOfWork unitOfWork,
                       IPasswordHasher passwordHasher)
    {
        _users = users;
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

        return user?.ToDto();
    }
}
