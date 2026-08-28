using Microsoft.AspNetCore.Mvc;
using SoundMate.Application.Disciplines;
using SoundMate.Application.Disciplines.DTO;
using SoundMate.Domain.Disciplines;

namespace SoundMate.API.Controllers;

/// <summary>
/// The catalogue of things that can be studied and taught: instruments and music-theory subjects.
/// <para>
/// Seeded reference data, so this is read-only. It exists because every other endpoint that takes
/// a <c>disciplineId</c> expects a seeded GUID: without a listing there is no selector to pick one
/// from, and the rest of the surface cannot be used at all.
/// </para>
/// <para>
/// Open on purpose even once auth lands — a list of instruments is not anybody's private data.
/// </para>
/// </summary>
[ApiController]
[Route("api/disciplines")]
[Produces("application/json")]
public sealed class DisciplinesController : ControllerBase
{
    private readonly IDisciplineService _disciplines;

    public DisciplinesController(IDisciplineService disciplines) => _disciplines = disciplines;

    /// <summary>Lists the active disciplines, optionally narrowed to one family.</summary>
    /// <param name="category">
    /// <c>Keyboard</c>, <c>PluckedString</c>, <c>BowedString</c>, <c>Woodwind</c>, <c>Brass</c>,
    /// <c>Percussion</c>, <c>Voice</c> or <c>MusicTheory</c>. Omit it for the whole catalogue.
    /// </param>
    /// <response code="200">The list, ordered by family and then by name.</response>
    /// <response code="400">The category is not one of the defined families.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DisciplineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<DisciplineDto>>> List(
        [FromQuery] DisciplineCategory? category,
        CancellationToken cancellationToken)
        => Ok(await _disciplines.ListAsync(category, cancellationToken));
}
