using Microsoft.AspNetCore.Mvc;
using SoundMate.Application.Users;
using SoundMate.Application.Users.DTO;

namespace SoundMate.API.Controllers;

/// <summary>
/// What a person <b>studies</b>, and at what level — "Pepito plays piano at Advanced".
/// <para>
/// The route says which of the two person-to-discipline relationships this is on purpose.
/// <c>/api/users/{id}/disciplines</c> would read as the union of studying and teaching;
/// <c>taught-disciplines</c> is reserved for what somebody teaches, which carries no level and is
/// global to them rather than per academy.
/// </para>
/// <para>
/// Addressed by the <b>catalogue</b> id from <c>GET /api/disciplines</c>, not by the row's own id:
/// a <c>StudiedDisciplineId</c> exists in the database and never leaves it, because the id the
/// caller is holding is the one they just picked in a selector.
/// </para>
/// <para>
/// <b>Not authenticated yet.</b> Anyone can give anyone Superior on the violin. When auth lands
/// this becomes "self or admin".
/// </para>
/// </summary>
[ApiController]
[Route("api/users/{userId:guid}/studied-disciplines")]
[Produces("application/json")]
public sealed class StudiedDisciplinesController : ControllerBase
{
    private readonly IStudiedDisciplineService _studied;

    public StudiedDisciplinesController(IStudiedDisciplineService studied) => _studied = studied;

    /// <summary>Lists what that person studies, with each discipline's name and family resolved.</summary>
    /// <remarks>
    /// Disciplines retired from the catalogue are included: they are still true of whoever studies
    /// them, and hiding them would make a level silently disappear from somebody's profile.
    /// </remarks>
    /// <response code="200">The list. Empty when they study nothing, which is a normal state for a teacher.</response>
    /// <response code="404">No such user, or the user is deleted.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StudiedDisciplineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<StudiedDisciplineDto>>> List(Guid userId,
                                                                              CancellationToken cancellationToken)
        => Ok(await _studied.ListByUserAsync(userId, cancellationToken));

    /// <summary>Takes up a discipline at a level.</summary>
    /// <remarks>
    /// There is a <c>POST</c> here, unlike the profile of #11, because this is a collection:
    /// "you already study piano" is something the caller knows about themselves and wants to be
    /// told, so the 409 is a useful answer rather than a leaked persistence detail.
    /// <para>
    /// The <c>Location</c> header points at the collection. There is no per-item <c>GET</c> — the
    /// list is what a profile screen reads, and one endpoint per row would be a route nobody calls.
    /// </para>
    /// </remarks>
    /// <response code="201">Added.</response>
    /// <response code="400">The discipline id is missing or the level is not a defined value.</response>
    /// <response code="404">No such user, or that id is not in the catalogue. The body says which.</response>
    /// <response code="409">They already study it, or the discipline is no longer offered.</response>
    [HttpPost]
    [ProducesResponseType(typeof(StudiedDisciplineDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StudiedDisciplineDto>> Add(Guid userId,
                                                              [FromBody] AddStudiedDisciplineDto dto,
                                                              CancellationToken cancellationToken)
    {
        var added = await _studied.AddAsync(userId, dto, cancellationToken);

        return CreatedAtAction(nameof(List), new { userId }, added);
    }

    /// <summary>Changes the level on a discipline already being studied.</summary>
    /// <remarks>
    /// One discipline per call, on purpose. A <c>PUT</c> carrying the whole array would be
    /// destructive — anything missing from it gets deleted — so two tabs open and the last one to
    /// save wipes out what the other added. A list of disciplines is three to five entries.
    /// </remarks>
    /// <response code="200">Saved.</response>
    /// <response code="400">The level is not a defined value.</response>
    /// <response code="404">No such user, or they do not study that discipline.</response>
    [HttpPut("{disciplineId:guid}")]
    [ProducesResponseType(typeof(StudiedDisciplineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudiedDisciplineDto>> ChangeLevel(Guid userId,
                                                                      Guid disciplineId,
                                                                      [FromBody] ChangeLevelDto dto,
                                                                      CancellationToken cancellationToken)
        => Ok(await _studied.ChangeLevelAsync(userId, disciplineId, dto, cancellationToken));

    /// <summary>Stops studying a discipline. The person keeps existing; the row goes.</summary>
    /// <response code="204">Gone.</response>
    /// <response code="404">No such user, or they do not study that discipline.</response>
    [HttpDelete("{disciplineId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(Guid userId, Guid disciplineId, CancellationToken cancellationToken)
    {
        await _studied.RemoveAsync(userId, disciplineId, cancellationToken);

        return NoContent();
    }
}
