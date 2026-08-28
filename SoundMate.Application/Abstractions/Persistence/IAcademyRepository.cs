using SoundMate.Domain.Academies;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Abstractions.Persistence;

public interface IAcademyRepository
{
    Task<Academy?> GetByIdAsync(AcademyId id, CancellationToken cancellationToken = default);

    Task<Academy?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every academy owned by that user, soft-deleted ones included — filtering is the caller's
    /// call, not the repository's. Backed by <c>IX_Academies_OwnerId</c>, which existed for this
    /// query before anything could make it.
    /// </summary>
    Task<IReadOnlyList<Academy>> ListByOwnerAsync(UserId ownerId, CancellationToken cancellationToken = default);

    Task AddAsync(Academy academy, CancellationToken cancellationToken = default);

    void Update(Academy academy);

    void Remove(Academy academy);
}
