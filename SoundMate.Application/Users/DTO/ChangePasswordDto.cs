namespace SoundMate.Application.Users.DTO;

/// <summary>
/// A password change. Both values are plaintext and must not travel past
/// <c>UserService.ChangePasswordAsync</c>, which is also the reason this type must never be
/// logged whole.
/// </summary>
public sealed record ChangePasswordDto
{
    /// <summary>
    /// The password in force right now. Required, and actually checked — without it the endpoint
    /// would let anyone who can reach it take over any account.
    /// </summary>
    public string CurrentPassword { get; init; } = string.Empty;

    /// <summary>The replacement.</summary>
    public string NewPassword { get; init; } = string.Empty;
}
