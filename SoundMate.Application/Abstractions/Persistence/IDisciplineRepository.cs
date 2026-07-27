using SoundMate.Domain.Disciplines;

namespace SoundMate.Application.Abstractions.Persistence;

public interface IDisciplineRepository
{
    Task<Discipline?> GetByIdAsync(DisciplineId id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(DisciplineId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Discipline>> ListActiveAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Discipline>> ListByCategoryAsync(DisciplineCategory category, CancellationToken cancellationToken = default);
}
