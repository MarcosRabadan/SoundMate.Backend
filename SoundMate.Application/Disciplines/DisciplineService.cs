using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Application.Disciplines.DTO;
using SoundMate.Domain.Common;
using SoundMate.Domain.Disciplines;

namespace SoundMate.Application.Disciplines;

/// <inheritdoc cref="IDisciplineService"/>
internal sealed class DisciplineService : IDisciplineService
{
    private readonly IDisciplineRepository _disciplines;

    public DisciplineService(IDisciplineRepository disciplines) => _disciplines = disciplines;

    public async Task<IReadOnlyList<DisciplineDto>> ListAsync(DisciplineCategory? category = null,
                                                              CancellationToken cancellationToken = default)
    {
        if (category is null)
            return (await _disciplines.ListActiveAsync(cancellationToken)).ToDtos();

        // Model binding lets any integer through as an enum, so ?category=99 arrives here as a
        // perfectly typed value that means nothing. Filtering on it would answer 200 with an empty
        // list and let the caller believe the family is empty; Guard.Defined turns it into the 400
        // it is. Same guard the aggregates use, so "valid category" has one definition.
        Guard.Defined(category.Value, "Category");

        return (await _disciplines.ListByCategoryAsync(category.Value, cancellationToken)).ToDtos();
    }
}
