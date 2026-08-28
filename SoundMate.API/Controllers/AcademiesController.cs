using Microsoft.AspNetCore.Mvc;
using SoundMate.Application.Academies;
using SoundMate.Application.Academies.DTO;

namespace SoundMate.API.Controllers;

/// <summary>
/// Organizations: an academy with several teachers, or a private teacher — an academy of one
/// person (<c>SoloTeacher</c>).
/// <para>
/// <b>None of this is authenticated yet.</b> Anyone can open an academy in somebody else's name,
/// cancel one that is not theirs, or move it to another plan. Acceptable while nothing is
/// deployed; when auth lands, <c>POST</c> must require that <c>OwnerUserId</c> be the caller's own
/// id, and the rest becomes "owner or admin".
/// </para>
/// </summary>
[ApiController]
[Route("api/academies")]
[Produces("application/json")]
public sealed class AcademiesController : ControllerBase
{
    private readonly IAcademyService _academies;

    public AcademiesController(IAcademyService academies) => _academies = academies;

    /// <summary>Opens an academy. Its owner gets the <c>Owner</c> membership in the same write.</summary>
    /// <response code="201">Created. The <c>Location</c> header points at it.</response>
    /// <response code="400">Malformed; the body lists the offending fields.</response>
    /// <response code="404">The owner does not exist, or is deleted.</response>
    /// <response code="409">That slug is already taken.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AcademyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AcademyDto>> Create([FromBody] CreateAcademyDto dto,
                                                       CancellationToken cancellationToken)
    {
        var academy = await _academies.CreateAsync(dto, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = academy.Id }, academy);
    }

    /// <summary>Returns a single academy by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AcademyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AcademyDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var academy = await _academies.GetByIdAsync(id, cancellationToken);

        return academy is null ? NotFound() : Ok(academy);
    }

    /// <summary>Returns a single academy by its public handle.</summary>
    /// <remarks>
    /// A path segment rather than a query string, unlike the user lookup by email: here the query
    /// string is reserved for filters that return a collection (see <c>?ownerId=</c>), and one
    /// route cannot sometimes answer with an object and sometimes with an array.
    /// </remarks>
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(AcademyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AcademyDto>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var academy = await _academies.GetBySlugAsync(slug, cancellationToken);

        return academy is null ? NotFound() : Ok(academy);
    }

    /// <summary>Every academy owned by a given user. An owner with none gets an empty list.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AcademyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AcademyDto>>> ListByOwner([FromQuery] Guid ownerId,
                                                                          CancellationToken cancellationToken)
        => Ok(await _academies.ListByOwnerAsync(ownerId, cancellationToken));

    /// <summary>Renames the academy. The slug, the plan and the owner are not reachable here.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AcademyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AcademyDto>> Update(Guid id,
                                                       [FromBody] UpdateAcademyDto dto,
                                                       CancellationToken cancellationToken)
        => Ok(await _academies.UpdateAsync(id, dto, cancellationToken));

    /// <summary>Changes the public handle.</summary>
    /// <remarks>
    /// This breaks every existing link to the academy, and releases the old handle for another one
    /// to claim — after which a stale link resolves to a different academy rather than 404ing.
    /// Its own endpoint so that it cannot happen as a side effect of an ordinary edit.
    /// </remarks>
    /// <response code="409">That slug is already taken.</response>
    [HttpPut("{id:guid}/slug")]
    [ProducesResponseType(typeof(AcademyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AcademyDto>> ChangeSlug(Guid id,
                                                           [FromBody] ChangeSlugDto dto,
                                                           CancellationToken cancellationToken)
        => Ok(await _academies.ChangeSlugAsync(id, dto, cancellationToken));

    /// <summary>Moves the academy to another subscription plan.</summary>
    [HttpPut("{id:guid}/plan")]
    [ProducesResponseType(typeof(AcademyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AcademyDto>> ChangePlan(Guid id,
                                                           [FromBody] ChangePlanDto dto,
                                                           CancellationToken cancellationToken)
        => Ok(await _academies.ChangePlanAsync(id, dto, cancellationToken));

    /// <summary>Suspends the academy. Reversible with <c>activate</c>.</summary>
    /// <response code="400">It is cancelled, and a cancelled academy cannot change.</response>
    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType(typeof(AcademyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AcademyDto>> Suspend(Guid id, CancellationToken cancellationToken)
        => Ok(await _academies.SuspendAsync(id, cancellationToken));

    /// <summary>Lifts a suspension.</summary>
    /// <response code="400">It is cancelled, and a cancelled academy cannot change.</response>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(AcademyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AcademyDto>> Activate(Guid id, CancellationToken cancellationToken)
        => Ok(await _academies.ActivateAsync(id, cancellationToken));

    /// <summary>
    /// Closes the academy for business. While cancelled it cannot be renamed, re-slugged,
    /// re-planned, suspended or activated — but it stays readable, because its history and its
    /// reviews do not stop existing. <c>reopen</c> undoes it. To make it disappear from the API,
    /// delete it as well.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(AcademyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AcademyDto>> Cancel(Guid id, CancellationToken cancellationToken)
        => Ok(await _academies.CancelAsync(id, cancellationToken));

    /// <summary>Brings a cancelled academy back into business.</summary>
    /// <remarks>
    /// The way out of <c>cancel</c>: a lapsed subscription is not a death sentence, and a customer
    /// who comes back and pays should get their academy — and its slug, and its history — back.
    /// It only undoes a cancellation; a suspended academy stays suspended, because lifting a
    /// suspension is what <c>activate</c> is for.
    /// </remarks>
    /// <response code="200">Reopened, or was never cancelled.</response>
    [HttpPost("{id:guid}/reopen")]
    [ProducesResponseType(typeof(AcademyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AcademyDto>> Reopen(Guid id, CancellationToken cancellationToken)
        => Ok(await _academies.ReopenAsync(id, cancellationToken));

    /// <summary>
    /// Deletes the academy. This is a <b>soft</b> delete: the row stays, so the memberships and
    /// reviews pointing at it are not orphaned, and its slug stays reserved. Reversible.
    /// </summary>
    /// <response code="204">Deleted, or already was.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _academies.DeleteAsync(id, cancellationToken);

        return NoContent();
    }

    /// <summary>Brings a soft-deleted academy back, exactly as it was.</summary>
    [HttpPost("{id:guid}/restore")]
    [ProducesResponseType(typeof(AcademyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AcademyDto>> Restore(Guid id, CancellationToken cancellationToken)
        => Ok(await _academies.RestoreAsync(id, cancellationToken));

    /// <summary>
    /// Removes the academy for good. <b>Irreversible.</b>
    /// </summary>
    /// <remarks>
    /// A separate route rather than a flag on <c>DELETE</c>: it should not be reachable by
    /// forgetting a default. It refuses while the academy still has members, but reviews are not
    /// covered — a teacher's rating is held per academy, so purging leaves reviews scoring a place
    /// that no longer exists.
    /// </remarks>
    /// <response code="409">Somebody still belongs to it.</response>
    [HttpDelete("{id:guid}/permanent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Purge(Guid id, CancellationToken cancellationToken)
    {
        await _academies.PurgeAsync(id, cancellationToken);

        return NoContent();
    }
}
