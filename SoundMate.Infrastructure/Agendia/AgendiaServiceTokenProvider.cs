using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace SoundMate.Infrastructure.Agendia;

/// <summary>
/// Holds the machine-to-machine token Agendia issues, and renews it when it is about to expire.
///
/// Agendia's client-credentials flow has NO refresh token by design: when the access token runs
/// out the service asks for another with its secret. Caching it here keeps that from becoming a
/// round trip on every single call.
/// </summary>
internal sealed class AgendiaServiceTokenProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AgendiaOptions _options;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private string? _token;
    private DateTime _renewAtUtc;

    public AgendiaServiceTokenProvider(IHttpClientFactory httpClientFactory, IOptions<AgendiaOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    /// <summary>Returns a usable service token, requesting a new one only when needed.</summary>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The bearer token to send to Agendia.</returns>
    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (IsUsable())
            return _token!;

        await _gate.WaitAsync(cancellationToken);
        try
        {
            // Re-check inside the gate: several callers can queue up on a cold start, and only
            // the first should spend a round trip.
            if (IsUsable())
                return _token!;

            // Resolved per call, not held: this provider is a singleton, and hanging on to one
            // HttpClient would defeat the handler rotation IHttpClientFactory exists for.
            using var http = _httpClientFactory.CreateClient(AgendiaHttpClients.Name);

            var response = await http.PostAsJsonAsync(
                "api/auth/service-token",
                new ServiceTokenRequest(_options.ClientId, _options.ClientSecret),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new AgendiaAuthenticationException(
                    $"Agendia refused the service credentials with {(int)response.StatusCode} " +
                    $"{response.StatusCode}. Check Agendia's ServiceClients registry and the " +
                    "Agendia:ClientId / Agendia:ClientSecret configured here.");
            }

            var issued = await response.Content.ReadFromJsonAsync<ServiceTokenResponse>(cancellationToken)
                ?? throw new AgendiaAuthenticationException("Agendia returned an empty service-token body.");

            _token = issued.AccessToken;

            // ExpiresAt travels without a zone; Agendia builds it from DateTime.UtcNow, so it is
            // UTC and is read as such rather than trusting the parsed Kind.
            var expiresAtUtc = DateTime.SpecifyKind(issued.ExpiresAt, DateTimeKind.Utc);
            _renewAtUtc = expiresAtUtc.AddSeconds(-_options.ExpirySafetyMarginSeconds);

            return _token;
        }
        finally
        {
            _gate.Release();
        }
    }

    private bool IsUsable() => _token is not null && DateTime.UtcNow < _renewAtUtc;

    private sealed record ServiceTokenRequest(string ClientId, string ClientSecret);

    private sealed record ServiceTokenResponse(string AccessToken, DateTime ExpiresAt, string TokenType);
}
