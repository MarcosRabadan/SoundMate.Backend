namespace SoundMate.Application.Common.Exceptions;

/// <summary>
/// Another academy already answers to that slug. A conflict over existing state (409), not a
/// malformed request (400) — the slug is well-formed, it is just spoken for.
/// <para>
/// Soft-deleted academies still hold theirs. Their rows keep the value in the unique index, and
/// releasing it would let a stale public link resolve to a different academy — worse than the
/// link simply breaking.
/// </para>
/// </summary>
public sealed class SlugAlreadyTakenException : Exception
{
    public SlugAlreadyTakenException(string slug, Exception? innerException = null)
        : base($"The slug '{slug}' is already taken.", innerException) => Slug = slug;

    /// <summary>The slug that was already in use.</summary>
    public string Slug { get; }
}
