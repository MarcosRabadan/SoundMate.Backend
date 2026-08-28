using SoundMate.Domain.Disciplines;

namespace SoundMate.Application.Disciplines.DTO;

/// <summary>
/// One entry of the discipline catalogue, as the API hands it back.
/// <para>
/// <see cref="Id"/> is the value every other endpoint expects: it is a seeded GUID, so without
/// this listing a caller has no way to learn it and nothing that takes a <c>disciplineId</c> can
/// be used at all.
/// </para>
/// </summary>
public sealed record DisciplineDto
{
    /// <summary>The catalogue id to send when adding or changing a studied discipline.</summary>
    public required Guid Id { get; init; }

    /// <summary>"Piano", "Classical guitar", "Harmony"...</summary>
    public required string Name { get; init; }

    /// <summary>
    /// The family it belongs to, for grouping a selector. A real enum, not a string: it travels
    /// by name anyway thanks to the converter in <c>Program.cs</c>.
    /// </summary>
    public required DisciplineCategory Category { get; init; }
}
