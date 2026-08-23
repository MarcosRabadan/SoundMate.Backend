using System.Net;

namespace SoundMate.Infrastructure.Tests.Agendia;

/// <summary>
/// Answers HTTP calls from a queue of canned responses and records what was asked, so the
/// Agendia tests can assert on the number of round trips as well as the outcome.
/// </summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

    /// <summary>Every request that reached this handler, in order.</summary>
    public List<HttpRequestMessage> Requests { get; } = new();

    /// <summary>Queues a response for the next call.</summary>
    public StubHttpMessageHandler Respond(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        _responses.Enqueue(respond);
        return this;
    }

    /// <summary>Queues a JSON response for the next call.</summary>
    public StubHttpMessageHandler RespondJson(HttpStatusCode status, string json)
        => Respond(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        });

    /// <summary>Queues a thrown exception for the next call, standing in for an unreachable host.</summary>
    public StubHttpMessageHandler RespondThrowing(Exception exception)
        => Respond(_ => throw exception);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                           CancellationToken cancellationToken)
    {
        Requests.Add(request);

        if (_responses.Count == 0)
            throw new InvalidOperationException($"No response queued for {request.Method} {request.RequestUri}.");

        return Task.FromResult(_responses.Dequeue()(request));
    }
}
