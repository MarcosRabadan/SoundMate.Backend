namespace SoundMate.Infrastructure.Agendia;

/// <summary>
/// How to reach Agendia. Bound from the "Agendia" configuration section.
/// <see cref="ClientSecret"/> is a credential: it belongs in user-secrets (dev) or environment
/// variables (prod), never in appsettings.json.
/// </summary>
public class AgendiaOptions
{
    /// <summary>Configuration section these options are bound from.</summary>
    public const string SectionName = "Agendia";

    /// <summary>Base address of the Agendia API, e.g. "https://localhost:7097".</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>The clientId Agendia knows us by. "soundmate" in its ServiceClients registry.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>The matching secret. Agendia stores only its PBKDF2 hash.</summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// How long before real expiry a cached service token is considered spent. Covers the
    /// round trip and any clock skew between the two services, so a token never expires
    /// mid-flight.
    /// </summary>
    public int ExpirySafetyMarginSeconds { get; set; } = 60;

    /// <summary>
    /// Skips TLS certificate validation on every call to Agendia. <b>Development only.</b>
    /// <para>
    /// It exists for one situation: SoundMate in a container while Agendia runs on the host.
    /// Agendia redirects HTTP to <c>https://host.docker.internal:7097</c>, and the ASP.NET
    /// development certificate was issued for <c>localhost</c> — wrong name, and its CA is not in
    /// the container's trust store either. Both failures are about the local certificate, not
    /// about Agendia.
    /// </para>
    /// <para>
    /// On in production it would accept any certificate from anyone and hand our service token to
    /// whoever answered. It stays off unless something sets it, and the only thing that does is
    /// <c>deploy/docker-compose.yml</c>.
    /// </para>
    /// </summary>
    public bool DangerousAcceptAnyServerCertificate { get; set; }
}
