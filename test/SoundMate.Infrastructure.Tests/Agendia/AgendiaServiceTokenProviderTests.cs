using System.Net;
using Microsoft.Extensions.Options;
using Shouldly;
using SoundMate.Infrastructure.Agendia;

namespace SoundMate.Infrastructure.Tests.Agendia;

/// <summary>
/// The machine-to-machine token Agendia issues is short-lived and has NO refresh token, so the
/// provider caches it and asks for another only when it is about to expire.
/// </summary>
public class AgendiaServiceTokenProviderTests
{
    private static string TokenJson(string token, DateTime expiresAtUtc) =>
        $$"""
        { "accessToken": "{{token}}", "expiresAt": "{{expiresAtUtc:yyyy-MM-ddTHH:mm:ss}}", "tokenType": "Bearer" }
        """;

    private static AgendiaServiceTokenProvider Build(StubHttpMessageHandler handler, int marginSeconds = 60)
        => new(new StubHttpClientFactory(handler),
               Options.Create(new AgendiaOptions
               {
                   BaseUrl = "https://agendia.test",
                   ClientId = "soundmate",
                   ClientSecret = "secret",
                   ExpirySafetyMarginSeconds = marginSeconds
               }));

    [Fact]
    public async Task Asks_Agendia_for_a_token_and_returns_it()
    {
        var handler = new StubHttpMessageHandler()
            .RespondJson(HttpStatusCode.OK, TokenJson("token-1", DateTime.UtcNow.AddMinutes(15)));

        var token = await Build(handler).GetTokenAsync();

        token.ShouldBe("token-1");
        handler.Requests.Single().RequestUri!.AbsolutePath.ShouldBe("/api/auth/service-token");
    }

    [Fact]
    public async Task Reuses_the_cached_token_instead_of_authenticating_again()
    {
        var handler = new StubHttpMessageHandler()
            .RespondJson(HttpStatusCode.OK, TokenJson("token-1", DateTime.UtcNow.AddMinutes(15)));
        var provider = Build(handler);

        await provider.GetTokenAsync();
        var second = await provider.GetTokenAsync();

        second.ShouldBe("token-1");
        handler.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Renews_a_token_that_is_inside_the_safety_margin()
    {
        // Expires in 30s with a 60s margin: already spent, even though Agendia would still
        // accept it. Renewing early is the point - a token must never expire mid-flight.
        var handler = new StubHttpMessageHandler()
            .RespondJson(HttpStatusCode.OK, TokenJson("token-1", DateTime.UtcNow.AddSeconds(30)))
            .RespondJson(HttpStatusCode.OK, TokenJson("token-2", DateTime.UtcNow.AddMinutes(15)));
        var provider = Build(handler);

        await provider.GetTokenAsync();
        var second = await provider.GetTokenAsync();

        second.ShouldBe("token-2");
        handler.Requests.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Rejected_credentials_fail_loudly()
    {
        // A configuration problem on one of the two sides, not a transient fault: it deserves a
        // message that says where to look rather than a bare 401 bubbling up.
        var handler = new StubHttpMessageHandler()
            .RespondJson(HttpStatusCode.Unauthorized, """{ "code": "INVALID_SERVICE_CREDENTIALS" }""");

        var act = async () => await Build(handler).GetTokenAsync();

        var exception = await act.ShouldThrowAsync<AgendiaAuthenticationException>();
        exception.Message.ShouldContain("ServiceClients");
    }

    [Fact]
    public async Task Concurrent_callers_on_a_cold_start_authenticate_once()
    {
        var handler = new StubHttpMessageHandler()
            .RespondJson(HttpStatusCode.OK, TokenJson("token-1", DateTime.UtcNow.AddMinutes(15)));
        var provider = Build(handler);

        var tokens = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => provider.GetTokenAsync()));

        tokens.ShouldAllBe(token => token == "token-1");
        handler.Requests.Count.ShouldBe(1);
    }
}
