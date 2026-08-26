namespace SoundMate.Application.Abstractions.Agendia;

/// <summary>
/// Outcome of a connection check against Agendia. A failed check is NOT an exception: the whole
/// point is to report what went wrong, so the caller can read <see cref="Error"/> instead of
/// catching something.
/// </summary>
/// <param name="Succeeded">True when Agendia accepted the credentials and answered.</param>
/// <param name="ServiceName">The service that answered ("MRC.Agendia"), when it did.</param>
/// <param name="Subject">The identity Agendia read from the token: our clientId.</param>
/// <param name="Roles">The roles Agendia resolved from the token.</param>
/// <param name="Issuer">The issuer Agendia accepted.</param>
/// <param name="TokenUse">"service" when Agendia recognised it as a machine token.</param>
/// <param name="Error">Why the check failed, when it did. Null on success.</param>
public record AgendiaConnectionCheck(
    bool Succeeded,
    string? ServiceName,
    string? Subject,
    IReadOnlyList<string> Roles,
    string? Issuer,
    string? TokenUse,
    string? Error)
{
    /// <summary>A check that never reached Agendia, or that Agendia rejected.</summary>
    public static AgendiaConnectionCheck Failed(string error)
        => new(false, null, null, Array.Empty<string>(), null, null, error);
}
