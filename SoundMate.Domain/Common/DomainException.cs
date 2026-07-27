namespace SoundMate.Domain.Common;

/// <summary>
/// Thrown when a domain invariant would be violated (creating an invalid email,
/// reactivating a membership that has already left...). Outer layers translate it
/// into an appropriate HTTP response.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
