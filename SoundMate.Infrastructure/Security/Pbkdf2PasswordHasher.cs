using System.Security.Cryptography;
using SoundMate.Application.Abstractions.Security;

namespace SoundMate.Infrastructure.Security;

/// <summary>
/// PBKDF2 (HMAC-SHA256) password hashing.
/// <para>
/// Stored format: <c>pbkdf2-sha256$&lt;iterations&gt;$&lt;salt-b64&gt;$&lt;hash-b64&gt;</c> — the
/// same self-describing shape Agendia uses for service-client secrets. Self-describing matters:
/// the iteration count travels with each hash, so raising it later keeps every existing hash
/// verifiable instead of locking everyone out.
/// </para>
/// </summary>
internal sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Prefix = "pbkdf2-sha256";

    /// <summary>
    /// Six times Agendia's count, deliberately. Its hasher protects machine secrets, which are
    /// long random strings; this one protects passwords people choose and reuse, so the work
    /// factor has to carry the weight the entropy does not. This is the OWASP figure for
    /// PBKDF2-HMAC-SHA256 and costs a few hundred milliseconds per attempt, which is the point.
    /// </summary>
    private const int DefaultIterations = 600_000;

    private const int SaltSize = 16;  // bytes
    private const int KeySize = 32;   // bytes, a 256-bit derived key

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, DefaultIterations, HashAlgorithmName.SHA256, KeySize);

        return string.Join('$',
            Prefix,
            DefaultIterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(key));
    }

    public bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            return false;

        var parts = storedHash.Split('$');
        if (parts.Length != 4 || parts[0] != Prefix)
            return false;

        if (!int.TryParse(parts[1], out var iterations) || iterations <= 0)
            return false;

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            // A corrupted row answers "wrong password" instead of taking the login path down.
            return false;
        }

        if (salt.Length == 0 || expected.Length == 0)
            return false;

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);

        // Constant-time: a byte-by-byte comparison leaks how much of the hash matched through how
        // long it took to say no.
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
