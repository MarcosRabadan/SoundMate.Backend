namespace SoundMate.Application.Abstractions.Security;

/// <summary>
/// Turns a plaintext password into the value the domain stores, and checks a candidate against
/// one. The plaintext never reaches the domain: <c>User.Register</c> already takes a hash.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Produces a salted hash of <paramref name="password"/>.</summary>
    string Hash(string password);

    /// <summary>
    /// True when <paramref name="password"/> matches <paramref name="storedHash"/>. A malformed
    /// stored value answers false rather than throwing: a corrupted row must not be able to take
    /// the whole login path down.
    /// </summary>
    bool Verify(string password, string storedHash);
}
