namespace SoundMate.Application.Users.DTO;

/// <summary>
/// The whole editable content of a profile.
/// <para>
/// Both members are optional and <c>null</c> is meaningful: it <b>clears</b> the field. This is the
/// body of a <c>PUT</c>, so it describes the profile in full — sending only a description wipes the
/// avatar, which is what PUT means. If partial edits are ever wanted, that is a PATCH and a
/// different DTO, not a nullable field doing double duty.
/// </para>
/// </summary>
public sealed record UpdateUserProfileDto
{
    /// <summary>The bio. <c>null</c> or blank clears it.</summary>
    public string? Description { get; init; }

    /// <summary>An absolute http or https URL. <c>null</c> or blank clears it.</summary>
    public string? AvatarUrl { get; init; }
}
