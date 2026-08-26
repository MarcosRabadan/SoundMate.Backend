namespace SoundMate.Application.Abstractions.Agendia;

/// <summary>
/// Calls into Agendia, the scheduling microservice that owns calendars and bookings.
/// Implemented in Infrastructure, which handles the machine-to-machine credentials and the
/// HTTP details.
/// </summary>
public interface IAgendiaClient
{
    /// <summary>
    /// Checks the connection end to end: authenticates machine-to-machine and asks Agendia what
    /// identity it reads from the resulting token.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the call.</param>
    /// <returns>What Agendia answered, or the reason the call did not get through.</returns>
    Task<AgendiaConnectionCheck> CheckConnectionAsync(CancellationToken cancellationToken = default);
}
