namespace SoundMate.Application.Common.Exceptions;

/// <summary>
/// No academy with that id, or it is soft-deleted. Thrown by the operations that change
/// something, so controllers do not have to branch on a null before every mutation; plain reads
/// return <c>null</c> instead.
/// </summary>
public sealed class AcademyNotFoundException : Exception
{
    public AcademyNotFoundException(Guid id)
        : base($"No academy with id '{id}'.") => Id = id;

    public Guid Id { get; }
}
