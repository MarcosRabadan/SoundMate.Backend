using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Application.Common.Exceptions;
using SoundMate.Application.Users.DTO;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Users;

/// <inheritdoc cref="IStudiedDisciplineService"/>
internal sealed class StudiedDisciplineService : IStudiedDisciplineService
{
    /// <summary>
    /// The unique index behind "a user cannot list the same discipline twice". Mirrors
    /// <c>StudiedDisciplineConfiguration</c>, where
    /// <c>HasIndex(sd => new { sd.UserId, sd.DisciplineId }).IsUnique()</c> makes EF generate this
    /// name. Matching on it rather than on any unique violation means a future second index on the
    /// table cannot silently start reporting itself as a duplicate discipline.
    /// </summary>
    private const string UserDisciplineUniqueIndex = "IX_StudiedDisciplines_UserId_DisciplineId";

    private readonly IStudiedDisciplineRepository _studied;
    private readonly IDisciplineRepository _disciplines;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public StudiedDisciplineService(IStudiedDisciplineRepository studied,
                                    IDisciplineRepository disciplines,
                                    IUserRepository users,
                                    IUnitOfWork unitOfWork)
    {
        _studied = studied;
        _disciplines = disciplines;
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<StudiedDisciplineDto>> ListByUserAsync(Guid userId,
                                                                           CancellationToken cancellationToken = default)
    {
        var user = await GetLiveUserAsync(userId, cancellationToken);

        var rows = await _studied.ListByUserAsync(user.Id, cancellationToken);
        if (rows.Count == 0)
            return [];

        // One catalogue read for the whole list, not one per row. It deliberately does not filter
        // on IsActive: a retired discipline still has to render with its name for whoever already
        // studies it.
        var catalogue = await _disciplines.ListByIdsAsync(
            rows.Select(r => r.DisciplineId).Distinct().ToList(),
            cancellationToken);

        // Same order as the catalogue listing, so a screen showing both does not have to re-sort.
        return rows.ToDtos(catalogue)
            .OrderBy(d => d.Category)
            .ThenBy(d => d.Name, StringComparer.Ordinal)
            .ToList();
    }

    public async Task<StudiedDisciplineDto> AddAsync(Guid userId,
                                                     AddStudiedDisciplineDto dto,
                                                     CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var user = await GetLiveUserAsync(userId, cancellationToken);
        var disciplineId = DisciplineId.From(dto.DisciplineId);

        // The domain cannot check this: aggregates reference each other by identity, with no
        // navigation and no enforced FK, so nothing else stops a row pointing at a discipline that
        // was never seeded.
        var discipline = await _disciplines.GetByIdAsync(disciplineId, cancellationToken)
                         ?? throw new DisciplineNotFoundException(dto.DisciplineId);

        // Retired from the catalogue: no new references. Existing ones keep working — that is what
        // IsActive is for.
        if (!discipline.IsActive)
            throw new DisciplineNotAvailableException(dto.DisciplineId, discipline.Name);

        if (await _studied.ExistsAsync(user.Id, disciplineId, cancellationToken))
            throw new DisciplineAlreadyAddedException(userId, dto.DisciplineId);

        var studied = StudiedDiscipline.Create(user.Id, disciplineId, dto.Level);
        await _studied.AddAsync(studied, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException ex) when (ex.ConstraintName == UserDisciplineUniqueIndex)
        {
            // The check above is a separate statement, so two adds of the same discipline in
            // flight at once both pass it and the index rejects the loser with a 23505. Unhandled
            // that is a 500 — a server fault for the same "already there" answer the check gives.
            //
            // 409 is right here, unlike the PUT of the profile in #11: a POST promises nothing
            // about repeating it, so there is no idempotence to keep by recovering.
            throw new DisciplineAlreadyAddedException(userId, dto.DisciplineId, ex);
        }

        // No extra read: the catalogue entry is already in hand from the check above.
        return studied.ToDto(discipline);
    }

    public async Task<StudiedDisciplineDto> ChangeLevelAsync(Guid userId,
                                                             Guid disciplineId,
                                                             ChangeLevelDto dto,
                                                             CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var user = await GetLiveUserAsync(userId, cancellationToken);
        var id = DisciplineId.From(disciplineId);

        var studied = await _studied.GetAsync(user.Id, id, cancellationToken)
                      ?? throw new StudiedDisciplineNotFoundException(userId, disciplineId);

        // No IsActive check on the way through: the row exists, so the discipline was on offer
        // when it was taken up, and a later catalogue decision must not freeze somebody's level.
        var discipline = await _disciplines.GetByIdAsync(id, cancellationToken)
                         ?? throw new DisciplineNotFoundException(disciplineId);

        studied.ChangeLevel(dto.Level);

        _studied.Update(studied);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return studied.ToDto(discipline);
    }

    public async Task RemoveAsync(Guid userId, Guid disciplineId, CancellationToken cancellationToken = default)
    {
        var user = await GetLiveUserAsync(userId, cancellationToken);

        var studied = await _studied.GetAsync(user.Id, DisciplineId.From(disciplineId), cancellationToken)
                      ?? throw new StudiedDisciplineNotFoundException(userId, disciplineId);

        _studied.Remove(studied);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// The user, or <see cref="UserNotFoundException"/> — a deleted one counts as missing, so a
    /// closed account cannot keep serving or growing a public skill list.
    /// </summary>
    private async Task<User> GetLiveUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(UserId.From(userId), cancellationToken);

        return user is null || user.IsDeleted ? throw new UserNotFoundException(userId) : user;
    }
}
