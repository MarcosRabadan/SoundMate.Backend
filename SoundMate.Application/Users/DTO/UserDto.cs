using SoundMate.Domain.Users;

namespace SoundMate.Application.Users.DTO;

/// <summary>
/// A user as the API hands it back.
/// <para>
/// <b>Never add the password hash to this type</b>, not even temporarily "to debug something".
/// This record is the shape that goes on the wire, so anything added here is published to every
/// caller. <c>UserDtoTests</c> pins the exact member list precisely so that such an addition
/// fails a test rather than leaking quietly.
/// </para>
/// <para>
/// Everything except <see cref="Phone"/> is <c>required</c>: <c>UserMapper</c> is the only thing
/// that builds one of these, and <c>required</c> turns "forgot to map a field" into a compile
/// error instead of a silently default value.
/// </para>
/// </summary>
public sealed record UserDto
{
    /// <summary>The user's identifier, unwrapped from <c>UserId</c>.</summary>
    public required Guid Id { get; init; }

    /// <summary>The email, unwrapped from the <c>Email</c> value object.</summary>
    public required string Email { get; init; }

    /// <summary>Display name.</summary>
    public required string FullName { get; init; }

    /// <summary>Phone number, when the user gave one. The only optional member.</summary>
    public string? Phone { get; init; }

    /// <summary>
    /// Whether the account is active or suspended.
    /// <para>
    /// A real enum, and it still goes on the wire as a name — <c>"Suspended"</c>, not <c>2</c> —
    /// because <c>JsonStringEnumConverter</c> is registered globally in <c>Program.cs</c>. The
    /// numeric values stay a storage detail, OpenAPI documents the allowed set, and C# consumers
    /// get the type.
    /// </para>
    /// </summary>
    public required UserStatus Status { get; init; }

    /// <summary>When the user registered. UTC, like every instant in SoundMate.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}
