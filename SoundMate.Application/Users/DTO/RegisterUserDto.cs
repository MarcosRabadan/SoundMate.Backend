namespace SoundMate.Application.Users.DTO;

/// <summary>
/// What a caller sends to register a person.
/// <para>
/// <see cref="Password"/> is plaintext and must not travel any further than
/// <c>UserService.RegisterAsync</c>, which hashes it before the domain sees it. It is also the
/// reason this type must never be logged whole.
/// </para>
/// </summary>
public sealed record RegisterUserDto
{
    /// <summary>The person's email. It is their global identity across all of SoundMate.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Plaintext password. Hashed on the way in; never stored or returned.</summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>Display name.</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>Optional phone number.</summary>
    public string? Phone { get; init; }
}
