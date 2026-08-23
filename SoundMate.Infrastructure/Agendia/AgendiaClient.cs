using System.Net.Http.Headers;
using System.Net.Http.Json;
using SoundMate.Application.Abstractions.Agendia;

namespace SoundMate.Infrastructure.Agendia;

/// <summary>
/// Talks to Agendia over HTTP, attaching the machine-to-machine token.
///
/// The token is attached here rather than by a DelegatingHandler on purpose: with a single call
/// so far, a handler would be a layer earning nothing. Once the provisioning calls arrive and
/// every method needs the header, moving it is a few lines - and worth doing then, because
/// "remember to attach the token" is exactly the kind of step that gets forgotten.
/// </summary>
internal sealed class AgendiaClient : IAgendiaClient
{
    private readonly HttpClient _http;
    private readonly AgendiaServiceTokenProvider _tokens;

    public AgendiaClient(HttpClient http, AgendiaServiceTokenProvider tokens)
    {
        _http = http;
        _tokens = tokens;
    }

    /// <inheritdoc />
    public async Task<AgendiaConnectionCheck> CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await _tokens.GetTokenAsync(cancellationToken);

            using var request = new HttpRequestMessage(HttpMethod.Get, "api/ping");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _http.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // A 401 here and not at the token call means Agendia issued the token and then
                // refused it: the issuer or the audience do not line up.
                return AgendiaConnectionCheck.Failed(
                    $"Agendia answered {(int)response.StatusCode} {response.StatusCode} to the ping.");
            }

            var ping = await response.Content.ReadFromJsonAsync<AgendiaPingResponse>(cancellationToken);
            if (ping is null)
                return AgendiaConnectionCheck.Failed("Agendia answered the ping with an empty body.");

            return new AgendiaConnectionCheck(
                Succeeded: true,
                ServiceName: ping.Service,
                Subject: ping.UserId,
                Roles: ping.Roles ?? Array.Empty<string>(),
                Issuer: ping.Issuer,
                TokenUse: ping.TokenUse,
                Error: null);
        }
        catch (AgendiaAuthenticationException ex)
        {
            return AgendiaConnectionCheck.Failed(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            // Unreachable, DNS, refused connection, TLS. Reported rather than thrown so a
            // connection CHECK answers instead of blowing up - that is what it is for.
            return AgendiaConnectionCheck.Failed($"Could not reach Agendia: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return AgendiaConnectionCheck.Failed($"Agendia did not answer in time: {ex.Message}");
        }
    }

    private sealed record AgendiaPingResponse(
        string? Service,
        DateTime UtcNow,
        string? UserId,
        IReadOnlyList<string>? Roles,
        string? Issuer,
        string? TokenUse);
}
