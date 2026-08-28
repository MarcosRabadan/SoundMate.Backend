using SoundMate.Application.Academies.DTO;
using SoundMate.Application.Common.Exceptions;

namespace SoundMate.Application.Academies;

/// <summary>
/// Use cases that operate on an <c>Academy</c>.
/// <para>
/// Reads return <c>null</c> (or an empty list); the operations that change something throw
/// <see cref="AcademyNotFoundException"/>, so a caller cannot mistake "nothing to update" for
/// "updated nothing".
/// </para>
/// <para>
/// <b>A soft-deleted academy is invisible to everything except <see cref="RestoreAsync"/> and
/// <see cref="PurgeAsync"/>.</b> The row survives for the sake of the memberships and reviews that
/// reference it, not so it can keep being used.
/// </para>
/// </summary>
public interface IAcademyService
{
    /// <summary>
    /// Opens an academy and gives its owner the <c>Owner</c> membership, in the same write.
    /// </summary>
    /// <exception cref="UserNotFoundException">The owner does not exist, or is deleted.</exception>
    /// <exception cref="SlugAlreadyTakenException">Another academy already answers to that slug.</exception>
    /// <exception cref="Domain.Common.DomainException">The name is blank, or the slug is malformed.</exception>
    Task<AcademyDto> CreateAsync(CreateAcademyDto dto, CancellationToken cancellationToken = default);

    /// <summary>Returns the academy, or <c>null</c> when no such id exists or it is deleted.</summary>
    Task<AcademyDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the academy with that public handle, or <c>null</c>. A malformed slug is not an
    /// error here — it simply matches nobody.
    /// </summary>
    Task<AcademyDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every academy that user owns, deleted ones excluded. An unknown user gets an empty list,
    /// not a 404: "this person owns nothing" and "this person does not exist" are the same answer
    /// to a caller who is allowed to ask.
    /// </summary>
    Task<IReadOnlyList<AcademyDto>> ListByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken = default);

    /// <summary>Renames the academy.</summary>
    /// <exception cref="AcademyNotFoundException">No such academy, or it is deleted.</exception>
    /// <exception cref="Domain.Common.DomainException">It is cancelled, or the name is blank.</exception>
    Task<AcademyDto> UpdateAsync(Guid id, UpdateAcademyDto dto, CancellationToken cancellationToken = default);

    /// <summary>Changes the public handle. See <see cref="ChangeSlugDto"/> for what that costs.</summary>
    /// <exception cref="AcademyNotFoundException">No such academy, or it is deleted.</exception>
    /// <exception cref="SlugAlreadyTakenException">Another academy already answers to it.</exception>
    Task<AcademyDto> ChangeSlugAsync(Guid id, ChangeSlugDto dto, CancellationToken cancellationToken = default);

    /// <summary>Moves the academy to another subscription plan.</summary>
    /// <exception cref="AcademyNotFoundException">No such academy, or it is deleted.</exception>
    Task<AcademyDto> ChangePlanAsync(Guid id, ChangePlanDto dto, CancellationToken cancellationToken = default);

    /// <summary>Suspends the academy: a moderation decision about one that is still running.</summary>
    /// <exception cref="AcademyNotFoundException">No such academy, or it is deleted.</exception>
    /// <exception cref="Domain.Common.DomainException">It is cancelled.</exception>
    Task<AcademyDto> SuspendAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Lifts a suspension.</summary>
    /// <exception cref="AcademyNotFoundException">No such academy, or it is deleted.</exception>
    /// <exception cref="Domain.Common.DomainException">It is cancelled.</exception>
    Task<AcademyDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes the academy for business. Everything else is refused while it is cancelled, but it
    /// stays readable and <see cref="ReopenAsync"/> undoes it. Deleting is a separate decision.
    /// </summary>
    /// <exception cref="AcademyNotFoundException">No such academy, or it is deleted.</exception>
    Task<AcademyDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Brings a cancelled academy back into business. Idempotent, and narrow: it only undoes a
    /// cancellation, so a suspended academy stays suspended.
    /// </summary>
    /// <exception cref="AcademyNotFoundException">No such academy.</exception>
    /// <exception cref="AcademyIsDeletedException">
    /// It is soft-deleted. <see cref="RestoreAsync"/> is the operation that undoes that.
    /// </exception>
    Task<AcademyDto> ReopenAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes the academy. The row stays, so nothing referencing its id is orphaned and its
    /// slug stays reserved. Reversible with <see cref="RestoreAsync"/>. Idempotent.
    /// </summary>
    /// <exception cref="AcademyNotFoundException">No such academy.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Brings a soft-deleted academy back, exactly as it was. Idempotent.</summary>
    /// <exception cref="AcademyNotFoundException">No such academy.</exception>
    Task<AcademyDto> RestoreAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the row for good. <b>Irreversible, and it orphans reviews</b> — read the remarks on
    /// the implementation. <see cref="DeleteAsync"/> is what you almost always want.
    /// </summary>
    /// <exception cref="AcademyNotFoundException">No such academy.</exception>
    /// <exception cref="AcademyStillHasMembersException">Somebody still belongs to it.</exception>
    Task PurgeAsync(Guid id, CancellationToken cancellationToken = default);
}
