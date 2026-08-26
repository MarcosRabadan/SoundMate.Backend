using Microsoft.AspNetCore.Mvc;
using SoundMate.Application.Abstractions.Agendia;

namespace SoundMate.API.Controllers;

/// <summary>
/// Diagnostics for the Agendia integration.
/// </summary>
[ApiController]
[Route("api/agendia")]
[Produces("application/json")]
public class AgendiaController : ControllerBase
{
    private readonly IAgendiaClient _agendia;
    private readonly IHostEnvironment _environment;

    public AgendiaController(IAgendiaClient agendia, IHostEnvironment environment)
    {
        _agendia = agendia;
        _environment = environment;
    }

    /// <summary>
    /// Checks the connection to Agendia end to end: authenticates machine-to-machine and reports
    /// the identity Agendia reads from the resulting token.
    /// </summary>
    /// <remarks>
    /// **Development only.** SoundMate has no authentication yet, so this would otherwise be an
    /// anonymous endpoint publishing our clientId and the accepted issuer. Once authentication
    /// lands, replace the environment check with an admin-only policy - do not simply delete it.
    ///
    /// A failure answers **503** and still carries the body: the reason is the valuable part, and
    /// an exception would answer worse. But the status has to say it too - a monitor, a curl or an
    /// EnsureSuccessStatusCode would otherwise read a broken integration as green. Same contract
    /// as an ASP.NET health check: 200 healthy, 503 unhealthy, details in the body.
    /// </remarks>
    /// <param name="cancellationToken">Token to cancel the call.</param>
    [HttpGet("connection")]
    [ProducesResponseType(typeof(AgendiaConnectionCheck), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AgendiaConnectionCheck), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgendiaConnectionCheck>> CheckConnection(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        var check = await _agendia.CheckConnectionAsync(cancellationToken);

        return check.Succeeded
            ? Ok(check)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, check);
    }
}
