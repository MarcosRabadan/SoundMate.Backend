using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Disciplines;

namespace SoundMate.Application.Tests.Fakes;

/// <summary>
/// In-memory <see cref="IDisciplineRepository"/> over a handful of catalogue entries.
/// <para>
/// It mirrors one asymmetry that matters: the two listings that feed a selector hide inactive
/// entries, while <see cref="ListByIdsAsync"/> does not. A fake that filtered everywhere would
/// make the "a retired discipline still shows for whoever studies it" tests pass for the wrong
/// reason — or fail for one.
/// </para>
/// </summary>
internal sealed class FakeDisciplineRepository : IDisciplineRepository
{
    private readonly List<Discipline> _disciplines = [];

    public void Seed(Discipline discipline) => _disciplines.Add(discipline);

    public Task<Discipline?> GetByIdAsync(DisciplineId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_disciplines.FirstOrDefault(d => d.Id == id));

    public Task<bool> ExistsAsync(DisciplineId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_disciplines.Any(d => d.Id == id));

    public Task<IReadOnlyList<Discipline>> ListActiveAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Discipline>>(
            _disciplines.Where(d => d.IsActive)
                .OrderBy(d => d.Category)
                .ThenBy(d => d.Name, StringComparer.Ordinal)
                .ToList());

    public Task<IReadOnlyList<Discipline>> ListByCategoryAsync(DisciplineCategory category, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Discipline>>(
            _disciplines.Where(d => d.Category == category && d.IsActive)
                .OrderBy(d => d.Name, StringComparer.Ordinal)
                .ToList());

    /// <summary>Inactive entries included, exactly like the real one.</summary>
    public Task<IReadOnlyList<Discipline>> ListByIdsAsync(IReadOnlyCollection<DisciplineId> ids,
                                                          CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Discipline>>(
            _disciplines.Where(d => ids.Contains(d.Id)).ToList());
}
