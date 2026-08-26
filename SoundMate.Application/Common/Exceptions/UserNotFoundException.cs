namespace SoundMate.Application.Common.Exceptions;

/// <summary>
/// No user with that id. Thrown by the operations that change something, so controllers do not
/// have to branch on a null before every mutation; plain reads return <c>null</c> instead.
/// </summary>
public sealed class UserNotFoundException : Exception
{
    public UserNotFoundException(Guid id)
        : base($"No user with id '{id}'.") => Id = id;

    public Guid Id { get; }
}
