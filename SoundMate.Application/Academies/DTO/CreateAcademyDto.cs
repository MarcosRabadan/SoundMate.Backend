using SoundMate.Domain.Academies;

namespace SoundMate.Application.Academies.DTO;

/// <summary>
/// What a caller sends to open an academy.
/// <para>
/// The plan is not here: every academy starts on <c>Free</c> and moves with its own endpoint.
/// Letting a caller pick their own plan at creation time would be a billing decision taken by
/// whoever is holding the keyboard.
/// </para>
/// </summary>
public sealed record CreateAcademyDto
{
    /// <summary>Display name, e.g. "Academia Do Re Mi".</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// <c>Academy</c> for an organisation, <c>SoloTeacher</c> for a private teacher — an academy
    /// of one person. Accepted by name or by number.
    /// </summary>
    public AcademyType Type { get; init; }

    /// <summary>The public, URL-friendly handle. Lowercased and trimmed on the way in.</summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// The person who owns it. They must exist and not be deleted, and they get the <c>Owner</c>
    /// membership as part of the same write.
    /// </summary>
    public Guid OwnerUserId { get; init; }
}
