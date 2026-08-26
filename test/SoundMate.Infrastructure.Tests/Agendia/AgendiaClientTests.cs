using System.Net;
using Microsoft.Extensions.Options;
using Shouldly;
using SoundMate.Infrastructure.Agendia;

namespace SoundMate.Infrastructure.Tests.Agendia;

/// <summary>
/// The connection check answers instead of throwing: its whole job is to report what is wrong,
/// so every failure mode comes back as a result the caller can read.
/// </summary>
public class AgendiaClientTests
{
    private const string TokenJson =
        """{ "accessToken": "token-1", "expiresAt": "2099-01-01T00:00:00", "tokenType": "Bearer" }""";

    private static AgendiaClient Build(StubHttpMessageHandler handler)
    {
        var options = Options.Create(new AgendiaOptions
        {
            BaseUrl = "https://agendia.test",
            ClientId = "soundmate",
            ClientSecret = "secret"
        });
        var factory = new StubHttpClientFactory(handler);
        var http = factory.CreateClient(AgendiaHttpClients.Name);

        return new AgendiaClient(http, new AgendiaServiceTokenProvider(factory, options));
    }

    [Fact]
    public async Task Reports_the_identity_Agendia_read_from_our_token()
    {
        var handler = new StubHttpMessageHandler()
            .RespondJson(HttpStatusCode.OK, TokenJson)
            .RespondJson(HttpStatusCode.OK, """
                {
                  "service": "MRC.Agendia",
                  "utcNow": "2026-08-23T10:15:00Z",
                  "userId": "soundmate",
                  "roles": [ "Admin" ],
                  "issuer": "MRC.Agendia",
                  "tokenUse": "service"
                }
                """);

        var check = await Build(handler).CheckConnectionAsync();

        check.Succeeded.ShouldBeTrue();
        check.ServiceName.ShouldBe("MRC.Agendia");
        check.Subject.ShouldBe("soundmate");
        check.Roles.ShouldBe(new[] { "Admin" });
        check.TokenUse.ShouldBe("service");
        check.Error.ShouldBeNull();
    }

    [Fact]
    public async Task Sends_the_service_token_as_a_bearer_on_the_ping()
    {
        var handler = new StubHttpMessageHandler()
            .RespondJson(HttpStatusCode.OK, TokenJson)
            .RespondJson(HttpStatusCode.OK, """{ "service": "MRC.Agendia", "roles": [] }""");

        await Build(handler).CheckConnectionAsync();

        var ping = handler.Requests.Last();
        ping.RequestUri!.AbsolutePath.ShouldBe("/api/ping");
        ping.Headers.Authorization!.Scheme.ShouldBe("Bearer");
        ping.Headers.Authorization.Parameter.ShouldBe("token-1");
    }

    [Fact]
    public async Task Bad_service_credentials_are_reported_not_thrown()
    {
        var handler = new StubHttpMessageHandler()
            .RespondJson(HttpStatusCode.Unauthorized, """{ "code": "INVALID_SERVICE_CREDENTIALS" }""");

        var check = await Build(handler).CheckConnectionAsync();

        check.Succeeded.ShouldBeFalse();
        check.Error.ShouldContain("ServiceClients");
    }

    [Fact]
    public async Task A_token_Agendia_issues_but_then_refuses_is_reported()
    {
        // The confusing one: authentication worked, so the credentials are right, but the ping
        // came back 401. That means the issuer or the audience do not line up between the two
        // services - not a credentials problem, and worth telling apart.
        var handler = new StubHttpMessageHandler()
            .RespondJson(HttpStatusCode.OK, TokenJson)
            .RespondJson(HttpStatusCode.Unauthorized, "");

        var check = await Build(handler).CheckConnectionAsync();

        check.Succeeded.ShouldBeFalse();
        check.Error.ShouldContain("401");
    }

    [Fact]
    public async Task An_unreachable_Agendia_is_reported_not_thrown()
    {
        var handler = new StubHttpMessageHandler()
            .RespondThrowing(new HttpRequestException("Connection refused"));

        var check = await Build(handler).CheckConnectionAsync();

        check.Succeeded.ShouldBeFalse();
        check.Error.ShouldContain("Could not reach Agendia");
    }
}
