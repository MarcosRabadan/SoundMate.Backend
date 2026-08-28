namespace SoundMate.Application.Common.Exceptions;

/// <summary>
/// The discipline exists but has been retired from the catalogue, so it cannot be taken up now.
/// <para>
/// A conflict over existing state (409), not a "not found" (404): the id is real and the caller is
/// not confused about it — the catalogue simply stopped offering it. People who already study it
/// are untouched, on purpose. <c>IsActive</c> exists to stop offering something without deleting
/// it, so accepting new references would contradict it while removing the old ones would punish
/// somebody who did nothing.
/// </para>
/// </summary>
public sealed class DisciplineNotAvailableException : Exception
{
    public DisciplineNotAvailableException(Guid disciplineId, string name)
        : base($"Discipline '{name}' is no longer offered.")
    {
        DisciplineId = disciplineId;
        Name = name;
    }

    public Guid DisciplineId { get; }

    public string Name { get; }
}
