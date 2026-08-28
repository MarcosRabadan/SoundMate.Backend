using SoundMate.Application.Common.Exceptions;
using SoundMate.Application.Users.DTO;

namespace SoundMate.Application.Users;

/// <summary>
/// A user's profile, reached through its owner because that is the only id a caller has.
/// <para>
/// A soft-deleted user has no reachable profile: every operation here answers
/// <see cref="UserNotFoundException"/> for one, exactly as the rest of the user surface does.
/// </para>
/// </summary>
public interface IUserProfileService
{
    /// <summary>Returns the user's profile, or <c>null</c> when they have not got one.</summary>
    /// <exception cref="UserNotFoundException">No such user, or they are deleted.</exception>
    Task<UserProfileDto?> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the whole profile, creating it if the user has not got one yet.
    /// <para>
    /// An upsert on purpose: this is a <c>PUT</c> on a singleton sub-resource, so it describes what
    /// the profile should be, and whether a row already existed is our problem rather than the
    /// caller's. Idempotent — including under a race, see the implementation.
    /// </para>
    /// </summary>
    /// <exception cref="UserNotFoundException">No such user, or they are deleted.</exception>
    /// <exception cref="Domain.Common.DomainException">The avatar is not an absolute http(s) URL.</exception>
    Task<UserProfileDto> SaveAsync(Guid userId, UpdateUserProfileDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the profile. A plain delete, not a soft one: nothing references a
    /// <c>UserProfileId</c>, so there is nothing to orphan, and the profile is content rather than
    /// identity — losing it costs a bio, not a person.
    /// </summary>
    /// <exception cref="UserNotFoundException">No such user, or they are deleted.</exception>
    /// <exception cref="UserProfileNotFoundException">That user has no profile.</exception>
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
}
