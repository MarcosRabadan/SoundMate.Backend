namespace SoundMate.Application.Users.DTO;

/// <summary>
/// A user's profile as the API hands it back.
/// <para>
/// It carries the <b>user's</b> id, not the profile's. Nobody outside this layer ever holds a
/// <c>UserProfileId</c> — the profile is reached through its owner (<c>/api/users/{userId}/profile</c>)
/// because that is the only id a caller has. Publishing a second identifier would just invite
/// somebody to build a route around it.
/// </para>
/// </summary>
public sealed record UserProfileDto
{
    /// <summary>The user this profile belongs to.</summary>
    public required Guid UserId { get; init; }

    /// <summary>The bio, or <c>null</c> when the profile exists but has not been filled in.</summary>
    public string? Description { get; init; }

    /// <summary>An absolute http or https URL, or <c>null</c>.</summary>
    public string? AvatarUrl { get; init; }

    /// <summary>Last change. UTC, like every instant in SoundMate.</summary>
    public required DateTime UpdatedAtUtc { get; init; }
}
