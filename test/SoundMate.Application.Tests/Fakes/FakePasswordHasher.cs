using SoundMate.Application.Abstractions.Security;

namespace SoundMate.Application.Tests.Fakes;

/// <summary>
/// Deterministic stand-in for the real hasher. The service's contract is "whatever the hasher
/// returned is what gets stored" — not which algorithm ran — so these tests do not need PBKDF2,
/// and skipping its 600.000 iterations keeps them instant.
/// </summary>
internal sealed class FakePasswordHasher : IPasswordHasher
{
    public const string Prefix = "fake-hash:";

    public string Hash(string password) => Prefix + password;

    public bool Verify(string password, string storedHash) => storedHash == Prefix + password;
}
