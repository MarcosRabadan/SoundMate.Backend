using SoundMate.Application.Academies.DTO;
using SoundMate.Domain.Academies;

namespace SoundMate.Application.Academies;

/// <summary>
/// <c>Academy</c> to <see cref="AcademyDto"/>, by hand — same reasoning as <c>UserMapper</c>: the
/// only patched AutoMapper versions are past the point where it stopped being MIT, and with typed
/// ids and value objects every member had to be declared explicitly anyway.
/// </summary>
internal static class AcademyMapper
{
    public static AcademyDto ToDto(this Academy academy) => new()
    {
        Id = academy.Id.Value,
        Name = academy.Name,
        Type = academy.Type,
        Slug = academy.Slug.Value,
        OwnerUserId = academy.OwnerId.Value,
        Plan = academy.Plan,
        Status = academy.Status,
        CreatedAtUtc = academy.CreatedAtUtc
    };

    public static IReadOnlyList<AcademyDto> ToDtos(this IEnumerable<Academy> academies)
        => academies.Select(ToDto).ToList();
}
