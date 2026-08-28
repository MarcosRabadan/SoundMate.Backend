namespace SoundMate.Application.Common.Exceptions;

/// <summary>
/// The user exists but has no profile.
/// <para>
/// Distinct from <see cref="UserNotFoundException"/> on purpose: "there is no such person" and
/// "that person has not written a bio" are different answers, and a caller building a profile page
/// needs to tell them apart — one is a dead link, the other is an empty state with a "complete your
/// profile" button.
/// </para>
/// <para>
/// Only <c>GET</c> and <c>DELETE</c> can produce it. <c>PUT</c> creates the profile when it is
/// missing, so for that verb there is nothing to not find.
/// </para>
/// </summary>
public sealed class UserProfileNotFoundException : Exception
{
    public UserProfileNotFoundException(Guid userId)
        : base($"User '{userId}' has no profile.") => UserId = userId;

    /// <summary>The user who has no profile — not the profile's own id.</summary>
    public Guid UserId { get; }
}
