using SoundMate.Application.Disciplines.DTO;
using SoundMate.Domain.Disciplines;

namespace SoundMate.Application.Disciplines;

/// <summary>
/// Read-only access to the discipline catalogue.
/// <para>
/// Read-only because the catalogue is seeded reference data: adding or retiring an entry is
/// catalogue administration, a different job with a different audience. Nothing here writes.
/// </para>
/// </summary>
public interface IDisciplineService
{
    /// <summary>
    /// Every active discipline, or only those of one family.
    /// <para>
    /// Inactive ones are never listed — <c>IsActive</c> exists precisely to stop offering
    /// something without deleting it. Rows that already reference a retired discipline keep
    /// working and keep showing its name; they just cannot be created any more.
    /// </para>
    /// </summary>
    /// <exception cref="Domain.Common.DomainException">The category is not one of the defined values.</exception>
    Task<IReadOnlyList<DisciplineDto>> ListAsync(DisciplineCategory? category = null,
                                                 CancellationToken cancellationToken = default);
}
