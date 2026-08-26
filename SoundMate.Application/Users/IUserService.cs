using SoundMate.Application.Common.Exceptions;
using SoundMate.Application.Users.DTO;

namespace SoundMate.Application.Users;

/// <summary>
/// Use cases that operate on a <c>User</c>.
/// <para>
/// Reads return <c>null</c> when there is nothing to return; the operations that change something
/// throw <see cref="UserNotFoundException"/> instead, so a caller cannot mistake "nothing to
/// update" for "updated nothing".
/// </para>
/// <para>
/// <b>A soft-deleted user is invisible to everything here except <see cref="RestoreAsync"/> and
/// <see cref="PurgeAsync"/>.</b> Reads answer as if they did not exist and mutations answer
/// "not found" — the record survives for the sake of the rows that reference it, not to keep
/// being usable.
/// </para>
/// </summary>
public interface IUserService
{
    /// <summary>Registers a new person.</summary>
    /// <exception cref="EmailAlreadyRegisteredException">
    /// The email already belongs to someone — <b>including someone soft-deleted</b>. A deleted
    /// user keeps their email reserved; see <see cref="RestoreAsync"/>.
    /// </exception>
    /// <exception cref="Domain.Common.DomainException">The email is malformed, or the name is empty.</exception>
    Task<UserDto> RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default);

    /// <summary>Returns the user, or <c>null</c> when no such id exists or they are deleted.</summary>
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the user with that email, or <c>null</c>. Matching is case-insensitive, because one
    /// email is one person. A malformed email is not an error here — it simply matches nobody.
    /// </summary>
    Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Changes the name and phone. Neither the email nor the password is reachable here.</summary>
    /// <exception cref="UserNotFoundException">No such user, or they are deleted.</exception>
    Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default);

    /// <summary>Replaces the password, after checking the current one.</summary>
    /// <exception cref="UserNotFoundException">No such user, or they are deleted.</exception>
    /// <exception cref="IncorrectPasswordException">The current password did not match.</exception>
    Task ChangePasswordAsync(Guid id, ChangePasswordDto dto, CancellationToken cancellationToken = default);

    /// <summary>Marks the email as verified.</summary>
    /// <exception cref="UserNotFoundException">No such user, or they are deleted.</exception>
    Task<UserDto> VerifyEmailAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Suspends the user: a moderation decision about somebody who is still here. Unrelated to
    /// deletion — a suspended user can be deleted, and restoring them brings the suspension back.
    /// </summary>
    /// <exception cref="UserNotFoundException">No such user, or they are deleted.</exception>
    Task<UserDto> SuspendAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Lifts a suspension.</summary>
    /// <exception cref="UserNotFoundException">No such user, or they are deleted.</exception>
    Task<UserDto> ReactivateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes the user. The row stays, so nothing that references their id is orphaned, and
    /// their email stays reserved. Reversible with <see cref="RestoreAsync"/>. Idempotent.
    /// </summary>
    /// <exception cref="UserNotFoundException">No such user.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Brings a soft-deleted user back, exactly as they were. Idempotent — restoring a user who
    /// was never deleted simply returns them.
    /// </summary>
    /// <exception cref="UserNotFoundException">No such user.</exception>
    Task<UserDto> RestoreAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the row for good. <b>Irreversible, and it orphans data</b> — read the remarks on
    /// the implementation before calling it. <see cref="DeleteAsync"/> is what you almost always
    /// want.
    /// </summary>
    /// <exception cref="UserNotFoundException">No such user.</exception>
    /// <exception cref="UserStillHasMembershipsException">They still belong to an academy.</exception>
    Task PurgeAsync(Guid id, CancellationToken cancellationToken = default);
}
