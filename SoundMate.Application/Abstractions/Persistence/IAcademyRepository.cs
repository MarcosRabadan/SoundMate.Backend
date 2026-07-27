using SoundMate.Domain.Academies;

namespace SoundMate.Application.Abstractions.Persistence;

public interface IAcademyRepository
{
    Task<Academy?> GetByIdAsync(AcademyId id, CancellationToken cancellationToken = default);

    Task<Academy?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken cancellationToken = default);

    Task AddAsync(Academy academy, CancellationToken cancellationToken = default);

    void Update(Academy academy);

    void Remove(Academy academy);
}
