using Microsoft.AspNetCore.Mvc;

namespace SoundMate.API.Controllers;

/// <summary>
/// What is actually running here.
/// <para>
/// It exists because a version that only lives in <c>Directory.Build.props</c> answers nothing at
/// three in the morning: the question is never "what does the repo say" but "which build is this
/// environment on". Deliberately anonymous and free of anything sensitive — no environment name,
/// no host, no configuration — so it stays safe to leave open once authentication lands.
/// </para>
/// </summary>
[ApiController]
[Route("api/version")]
[Produces("application/json")]
public sealed class VersionController : ControllerBase
{
    /// <summary>Returns the product and version of the running build.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(VersionResponse), StatusCodes.Status200OK)]
    public ActionResult<VersionResponse> Get()
        => Ok(new VersionResponse { Product = BuildInfo.Product, Version = BuildInfo.Version });

    /// <summary>The running build's identity.</summary>
    public sealed record VersionResponse
    {
        /// <summary>Always "SoundMate".</summary>
        public required string Product { get; init; }

        /// <summary>SemVer, e.g. "0.2.0".</summary>
        public required string Version { get; init; }
    }
}
