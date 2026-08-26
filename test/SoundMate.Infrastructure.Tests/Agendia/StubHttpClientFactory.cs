namespace SoundMate.Infrastructure.Tests.Agendia;

/// <summary>Hands out clients over one stub handler, standing in for IHttpClientFactory.</summary>
internal sealed class StubHttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler _handler;

    public StubHttpClientFactory(HttpMessageHandler handler) => _handler = handler;

    public HttpClient CreateClient(string name) =>
        // disposeHandler:false - the provider disposes the client it creates on every call, and
        // that must not take the shared stub down with it.
        new(_handler, disposeHandler: false) { BaseAddress = new Uri("https://agendia.test") };
}
