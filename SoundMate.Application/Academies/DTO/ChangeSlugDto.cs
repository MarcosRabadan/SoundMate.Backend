namespace SoundMate.Application.Academies.DTO;

/// <summary>
/// A new public handle for an academy.
/// <para>
/// Its own endpoint rather than a field on <see cref="UpdateAcademyDto"/>, because it is not an
/// edit like the others: the slug is what public URLs are built from. Changing it <b>breaks every
/// existing link</b>, and it releases the old value for another academy to claim — after which a
/// stale link resolves to somebody else's academy instead of 404ing. That is a product decision,
/// and it should be deliberate enough to need its own call.
/// </para>
/// </summary>
public sealed record ChangeSlugDto
{
    /// <summary>The new handle. Lowercased and trimmed on the way in.</summary>
    public string Slug { get; init; } = string.Empty;
}
