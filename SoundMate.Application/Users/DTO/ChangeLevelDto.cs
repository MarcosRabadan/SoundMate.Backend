using SoundMate.Domain.Users;

namespace SoundMate.Application.Users.DTO;

/// <summary>
/// The new level for a discipline already being studied. The discipline itself is in the route:
/// changing which discipline a row points at is not an edit, it is a delete and an add.
/// </summary>
public sealed record ChangeLevelDto
{
    public MusicLevel Level { get; init; }
}
