namespace SoundMate.Application.Academies.DTO;

/// <summary>
/// The editable details of an academy.
/// <para>
/// Only the name. The id comes from the route, the owner is not transferable through an edit —
/// that is a takeover vector, and there is no <c>ChangeOwner</c> in the domain — the plan is a
/// billing decision with its own endpoint, and the slug is a public identifier with consequences
/// of its own (see <see cref="ChangeSlugDto"/>).
/// </para>
/// </summary>
public sealed record UpdateAcademyDto
{
    /// <summary>Display name. Required: the domain refuses a blank one.</summary>
    public string Name { get; init; } = string.Empty;
}
