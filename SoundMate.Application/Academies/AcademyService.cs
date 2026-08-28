using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Application.Academies.DTO;
using SoundMate.Application.Common.Exceptions;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Memberships;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Academies;

/// <inheritdoc cref="IAcademyService"/>
internal sealed class AcademyService : IAcademyService
{
    /// <summary>
    /// The unique index on <c>Academies.Slug</c>. Mirrors <c>AcademyConfiguration</c>, where
    /// <c>HasIndex(a => a.Slug).IsUnique()</c> makes EF generate this name. Matching on it rather
    /// than on any unique violation means a future second index on the table cannot silently start
    /// reporting itself as a duplicate slug.
    /// </summary>
    private const string SlugUniqueIndex = "IX_Academies_Slug";

    private readonly IAcademyRepository _academies;
    private readonly IUserRepository _users;
    private readonly IMembershipRepository _memberships;
    private readonly IUnitOfWork _unitOfWork;

    public AcademyService(IAcademyRepository academies,
                          IUserRepository users,
                          IMembershipRepository memberships,
                          IUnitOfWork unitOfWork)
    {
        _academies = academies;
        _users = users;
        _memberships = memberships;
        _unitOfWork = unitOfWork;
    }

    public async Task<AcademyDto> CreateAsync(CreateAcademyDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        // Slug.Create both validates and normalises (trim + lowercase), so the uniqueness check
        // below runs on exactly the value that will be stored.
        var slug = Slug.Create(dto.Slug);

        // The domain cannot check this: aggregates reference each other by identity, never by
        // navigation, so Academy.Create takes a UserId it has no way to resolve. Deleted counts as
        // missing — an academy owned by a closed account is the orphan #6 went out of its way to
        // avoid creating.
        var owner = await _users.GetByIdAsync(UserId.From(dto.OwnerUserId), cancellationToken);
        if (owner is null || owner.IsDeleted)
            throw new UserNotFoundException(dto.OwnerUserId);

        if (await _academies.ExistsBySlugAsync(slug, cancellationToken))
            throw new SlugAlreadyTakenException(slug.Value);

        var academy = Academy.Create(dto.Name, dto.Type, slug, owner.Id);
        await _academies.AddAsync(academy, cancellationToken);

        // The anchor, in the same SaveChanges. A Membership is what makes a relationship real in
        // this model — HasActiveMembershipAsync is the gate every booking passes through — so an
        // academy whose owner has no membership would be born claiming its owner does not belong
        // to it. Two AddAsync, one commit: either both rows land or neither does.
        await _memberships.AddAsync(
            Membership.Create(owner.Id, academy.Id, MembershipRole.Owner),
            cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException ex) when (ex.ConstraintName == SlugUniqueIndex)
        {
            // The check above is a separate statement, so two creations with the same slug in
            // flight at once both pass it and the index rejects the loser with a 23505. Unhandled
            // that is a 500 — a server fault for the same "taken" answer the check gives.
            throw new SlugAlreadyTakenException(slug.Value, ex);
        }

        return academy.ToDto();
    }

    public async Task<AcademyDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var academy = await _academies.GetByIdAsync(AcademyId.From(id), cancellationToken);

        return academy is null || academy.IsDeleted ? null : academy.ToDto();
    }

    public async Task<AcademyDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        // A malformed slug matches nobody, which is the honest answer. Letting Slug.Create throw
        // would turn a lookup that found nothing into a 400.
        if (!Slug.IsValid(slug))
            return null;

        var academy = await _academies.GetBySlugAsync(Slug.Create(slug), cancellationToken);

        return academy is null || academy.IsDeleted ? null : academy.ToDto();
    }

    public async Task<IReadOnlyList<AcademyDto>> ListByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        var academies = await _academies.ListByOwnerAsync(UserId.From(ownerUserId), cancellationToken);

        // The repository hands back everything it has; deciding what a caller may see is this
        // layer's job, not the query's.
        return academies.Where(a => !a.IsDeleted).ToDtos();
    }

    public Task<AcademyDto> UpdateAsync(Guid id, UpdateAcademyDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return MutateAsync(id, academy => academy.Rename(dto.Name), cancellationToken);
    }

    public async Task<AcademyDto> ChangeSlugAsync(Guid id, ChangeSlugDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var academy = await GetOrThrowAsync(id, cancellationToken);
        var slug = Slug.Create(dto.Slug);

        // Re-sending the slug it already has is a no-op, not a conflict with itself.
        if (academy.Slug == slug)
            return academy.ToDto();

        if (await _academies.ExistsBySlugAsync(slug, cancellationToken))
            throw new SlugAlreadyTakenException(slug.Value);

        academy.ChangeSlug(slug);

        _academies.Update(academy);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException ex) when (ex.ConstraintName == SlugUniqueIndex)
        {
            throw new SlugAlreadyTakenException(slug.Value, ex);
        }

        return academy.ToDto();
    }

    public Task<AcademyDto> ChangePlanAsync(Guid id, ChangePlanDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return MutateAsync(id, academy => academy.ChangePlan(dto.Plan), cancellationToken);
    }

    public Task<AcademyDto> SuspendAsync(Guid id, CancellationToken cancellationToken = default)
        => MutateAsync(id, academy => academy.Suspend(), cancellationToken);

    public Task<AcademyDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        => MutateAsync(id, academy => academy.Activate(), cancellationToken);

    public Task<AcademyDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
        => MutateAsync(id, academy => academy.Cancel(), cancellationToken);

    public async Task<AcademyDto> ReopenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Not MutateAsync: that would answer "not found" for a deleted academy, which is true and
        // useless. Reopen and restore undo different things and are easy to mix up, so say which
        // one this needed.
        var academy = await FindAnyAsync(id, cancellationToken);

        if (academy.IsDeleted)
            throw new AcademyIsDeletedException(id);

        academy.Reopen();

        _academies.Update(academy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return academy.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Deliberately finds deleted academies too: deleting twice is a no-op, not a 404.
        var academy = await FindAnyAsync(id, cancellationToken);

        academy.Delete();

        _academies.Update(academy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<AcademyDto> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // The one read that has to see past the soft delete — otherwise nothing could reverse it.
        var academy = await FindAnyAsync(id, cancellationToken);

        academy.Restore();

        _academies.Update(academy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return academy.ToDto();
    }

    /// <summary>
    /// Removes the row for good.
    /// <para>
    /// <b>Nothing at the database level stops this from orphaning data.</b> Two aggregates carry
    /// an <c>AcademyId</c> with no enforced foreign key — deliberate, so a future
    /// database-per-service split stays cheap: <c>Membership</c> and <c>TeacherReview</c>.
    /// Refusing while members remain covers the first. It does <b>not</b> cover the second: a
    /// teacher's rating is held per academy, so purging one leaves reviews scoring a place that no
    /// longer exists, and any average computed over them is quietly wrong.
    /// </para>
    /// <para>
    /// <see cref="DeleteAsync"/> is the answer almost every time. This is for actual erasure and
    /// wants a real cascade first.
    /// </para>
    /// </summary>
    public async Task PurgeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Finds deleted academies too: purging is normally the second step after a soft delete.
        var academy = await FindAnyAsync(id, cancellationToken);

        var members = await _memberships.ListByAcademyAsync(academy.Id, cancellationToken);
        if (members.Count > 0)
            throw new AcademyStillHasMembersException(id, members.Count);

        _academies.Remove(academy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<AcademyDto> MutateAsync(Guid id, Action<Academy> change, CancellationToken cancellationToken)
    {
        var academy = await GetOrThrowAsync(id, cancellationToken);

        change(academy);

        _academies.Update(academy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return academy.ToDto();
    }

    /// <summary>
    /// The live academy, or <see cref="AcademyNotFoundException"/>. A soft-deleted one is "not
    /// found" as far as every ordinary operation is concerned.
    /// </summary>
    private async Task<Academy> GetOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var academy = await FindAnyAsync(id, cancellationToken);

        return academy.IsDeleted ? throw new AcademyNotFoundException(id) : academy;
    }

    /// <summary>The academy whether or not it is soft-deleted. Only the lifecycle operations use it.</summary>
    private async Task<Academy> FindAnyAsync(Guid id, CancellationToken cancellationToken)
        => await _academies.GetByIdAsync(AcademyId.From(id), cancellationToken)
           ?? throw new AcademyNotFoundException(id);
}
