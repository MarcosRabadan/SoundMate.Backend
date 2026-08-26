namespace SoundMate.Application.Users.DTO;

/// <summary>
/// The editable part of a user's own details.
/// <para>
/// It carries neither <c>Id</c> nor <c>Email</c>, and that is the point. The id comes from the
/// route, so a body that also carried one would let a caller edit somebody else by disagreeing
/// with the URL. And the email is a person's global identity in SoundMate — the domain has no
/// <c>ChangeEmail</c> at all, so there is nothing here to change it with.
/// </para>
/// </summary>
public sealed record UpdateUserDto
{
    /// <summary>Display name. Required: the domain refuses a blank one.</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>Phone number. Send <c>null</c> to clear it.</summary>
    public string? Phone { get; init; }
}
