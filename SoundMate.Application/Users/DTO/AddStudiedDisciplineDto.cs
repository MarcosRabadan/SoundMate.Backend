using SoundMate.Domain.Users;

namespace SoundMate.Application.Users.DTO;

/// <summary>
/// Takes up a discipline at a level.
/// <para>
/// The id comes from <c>GET /api/disciplines</c>: it is a seeded GUID, not something a caller can
/// make up.
/// </para>
/// </summary>
public sealed record AddStudiedDisciplineDto
{
    /// <summary>A catalogue id, from <c>GET /api/disciplines</c>.</summary>
    public Guid DisciplineId { get; init; }

    /// <summary>
    /// The level on this discipline. Required — a level is the whole point of studying something,
    /// which is what keeps this apart from the "teaches" relationship.
    /// </summary>
    public MusicLevel Level { get; init; }
}
