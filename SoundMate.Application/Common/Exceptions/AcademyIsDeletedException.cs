namespace SoundMate.Application.Common.Exceptions;

/// <summary>
/// The academy exists but is soft-deleted, and the operation asked for is not the one that undoes
/// that.
/// <para>
/// It exists because <c>restore</c> and <c>reopen</c> undo different things — a delete and a
/// cancellation — and they are easy to confuse. Answering a <c>reopen</c> on a deleted academy
/// with a bare "not found" is technically true and useless: the caller is holding a valid id and
/// has no way to learn which of the two they wanted. Telling them leaks nothing, since
/// <c>restore</c> would reveal exactly the same thing.
/// </para>
/// <para>
/// Only the lifecycle operations answer this way. Ordinary ones — rename, change plan, suspend —
/// keep saying "not found", because to them a deleted academy really is gone.
/// </para>
/// </summary>
public sealed class AcademyIsDeletedException : Exception
{
    public AcademyIsDeletedException(Guid id)
        : base($"Academy '{id}' is deleted. Restore it first — 'reopen' undoes a cancellation, " +
               "'restore' undoes a deletion.") => Id = id;

    public Guid Id { get; }
}
