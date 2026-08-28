using SoundMate.Application.Users.DTO;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Users;

/// <summary>
/// <c>StudiedDiscipline</c> plus its catalogue entry to <see cref="StudiedDisciplineDto"/>, by
/// hand — same reasoning as <c>UserMapper</c>.
/// <para>
/// It takes two arguments because the row only holds a <c>DisciplineId</c>: aggregates reference
/// each other by identity, so the name has to be fetched and handed in rather than navigated to.
/// </para>
/// </summary>
internal static class StudiedDisciplineMapper
{
    public static StudiedDisciplineDto ToDto(this StudiedDiscipline studied, Discipline discipline) => new()
    {
        DisciplineId = studied.DisciplineId.Value,
        Name = discipline.Name,
        Category = discipline.Category,
        Level = studied.Level,
        CreatedAtUtc = studied.CreatedAtUtc,
        UpdatedAtUtc = studied.UpdatedAtUtc
    };

    public static IReadOnlyList<StudiedDisciplineDto> ToDtos(this IEnumerable<StudiedDiscipline> studied,
                                                             IReadOnlyList<Discipline> catalogue)
    {
        var byId = catalogue.ToDictionary(d => d.Id);
        var dtos = new List<StudiedDisciplineDto>();

        foreach (var row in studied)
        {
            // A row pointing at something absent from the catalogue should not be reachable: the
            // service checks on the way in, and catalogue entries are deactivated rather than
            // deleted. If one ever appears anyway, skipping it keeps the rest of the list
            // readable instead of failing the whole request over a single orphan.
            if (byId.TryGetValue(row.DisciplineId, out var discipline))
                dtos.Add(row.ToDto(discipline));
        }

        return dtos;
    }
}
