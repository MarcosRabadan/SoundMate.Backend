using SoundMate.Application.Disciplines.DTO;
using SoundMate.Domain.Disciplines;

namespace SoundMate.Application.Disciplines;

/// <summary>
/// <c>Discipline</c> to <see cref="DisciplineDto"/>, by hand — same reasoning as
/// <c>UserMapper</c> and <c>AcademyMapper</c>.
/// </summary>
internal static class DisciplineMapper
{
    public static DisciplineDto ToDto(this Discipline discipline) => new()
    {
        Id = discipline.Id.Value,
        Name = discipline.Name,
        Category = discipline.Category
    };

    public static IReadOnlyList<DisciplineDto> ToDtos(this IEnumerable<Discipline> disciplines)
        => disciplines.Select(ToDto).ToList();
}
