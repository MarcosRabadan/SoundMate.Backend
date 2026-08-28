using SoundMate.Domain.Disciplines;

namespace SoundMate.Application.Abstractions.Persistence;

public interface IDisciplineRepository
{
    Task<Discipline?> GetByIdAsync(DisciplineId id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(DisciplineId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Discipline>> ListActiveAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Discipline>> ListByCategoryAsync(DisciplineCategory category, CancellationToken cancellationToken = default);

    /// <summary>
    /// The disciplines with these ids, in one round trip, <b>inactive ones included</b>.
    /// <para>
    /// Resolving the names of what somebody already studies must not filter on <c>IsActive</c>:
    /// retiring a discipline from the catalogue does not make it untrue that they studied it, and
    /// filtering here would blank the name on rows that are working fine. The other two listings
    /// filter because they feed a selector, which is the opposite job.
    /// </para>
    /// <para>
    /// It exists so a list of studied disciplines costs one catalogue read rather than one per
    /// row. Ids not in the catalogue are simply absent from the result.
    /// </para>
    /// </summary>
    Task<IReadOnlyList<Discipline>> ListByIdsAsync(IReadOnlyCollection<DisciplineId> ids,
                                                   CancellationToken cancellationToken = default);
}
