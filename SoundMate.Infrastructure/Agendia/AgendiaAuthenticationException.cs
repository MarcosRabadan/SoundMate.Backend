namespace SoundMate.Infrastructure.Agendia;

/// <summary>
/// Agendia would not issue a service token. Almost always a configuration problem on one of the
/// two sides rather than a transient fault, so it is worth its own type.
/// </summary>
public sealed class AgendiaAuthenticationException : Exception
{
    public AgendiaAuthenticationException(string message) : base(message)
    {
    }
}
