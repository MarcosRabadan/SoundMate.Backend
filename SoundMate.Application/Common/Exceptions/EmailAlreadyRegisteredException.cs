namespace SoundMate.Application.Common.Exceptions;

/// <summary>
/// The email already belongs to somebody. A <c>User</c> is globally unique by email, so this is a
/// conflict over existing state (409), not a malformed request (400).
/// </summary>
public sealed class EmailAlreadyRegisteredException : Exception
{
    public EmailAlreadyRegisteredException(string email, Exception? innerException = null)
        : base($"The email '{email}' is already registered.", innerException)
        => Email = email;

    /// <summary>The email that was already taken.</summary>
    public string Email { get; }
}
