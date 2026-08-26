using Microsoft.AspNetCore.Mvc;
using SoundMate.Application.Users;
using SoundMate.Application.Users.DTO;

namespace SoundMate.API.Controllers;

/// <summary>
/// People. A <c>User</c> is global and unique by email: the same person keeps one account across
/// every academy they belong to, so this is registration, not "create a member of somewhere".
/// <para>
/// <b>None of this is authenticated yet.</b> Every route here is open, which is fine while nothing
/// is deployed and untenable the moment something is. When auth lands, most of these become
/// "self or admin", and the email lookup should become admin-only.
/// </para>
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users) => _users = users;

    /// <summary>Registers a new person.</summary>
    /// <response code="201">Registered. The <c>Location</c> header points at the new user.</response>
    /// <response code="400">The request is malformed; the body lists the offending fields.</response>
    /// <response code="409">That email already belongs to someone.</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterUserDto dto,
                                                      CancellationToken cancellationToken)
    {
        var user = await _users.RegisterAsync(dto, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>Returns a single user by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);

        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Finds a user by email. Case-insensitive, because one email is one person.
    /// </summary>
    /// <remarks>
    /// This is a user-enumeration oracle: anyone can ask whether an address is registered. The
    /// register endpoint already leaks the same fact through its 409, so the surface exists either
    /// way — but this makes it cheap and scriptable. Put it behind an admin policy when auth lands.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetByEmail([FromQuery] string email,
                                                        CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(email, cancellationToken);

        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>Changes the name and phone. The email cannot be changed: it is the person's identity.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Update(Guid id,
                                                    [FromBody] UpdateUserDto dto,
                                                    CancellationToken cancellationToken)
        => Ok(await _users.UpdateAsync(id, dto, cancellationToken));

    /// <summary>Replaces the password. The current one has to be supplied and is checked.</summary>
    /// <response code="204">Changed.</response>
    /// <response code="400">Malformed, or the current password did not match.</response>
    [HttpPut("{id:guid}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(Guid id,
                                                    [FromBody] ChangePasswordDto dto,
                                                    CancellationToken cancellationToken)
    {
        await _users.ChangePasswordAsync(id, dto, cancellationToken);

        return NoContent();
    }

    /// <summary>Marks the email as verified.</summary>
    [HttpPost("{id:guid}/verify-email")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> VerifyEmail(Guid id, CancellationToken cancellationToken)
        => Ok(await _users.VerifyEmailAsync(id, cancellationToken));

    /// <summary>Suspends the user. This is the reversible way to take someone out of circulation.</summary>
    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Suspend(Guid id, CancellationToken cancellationToken)
        => Ok(await _users.SuspendAsync(id, cancellationToken));

    /// <summary>Lifts a suspension.</summary>
    [HttpPost("{id:guid}/reactivate")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Reactivate(Guid id, CancellationToken cancellationToken)
        => Ok(await _users.ReactivateAsync(id, cancellationToken));

    /// <summary>
    /// Deletes the user. This is a <b>soft</b> delete: the row stays, so nothing that references
    /// their id is orphaned and their email stays reserved. Reversible with <c>restore</c>.
    /// </summary>
    /// <remarks>
    /// Unrelated to <c>suspend</c>. Suspension is a moderation decision about somebody who is
    /// still here; this says the account is gone. A suspended user can be deleted, and restoring
    /// them brings the suspension back untouched.
    /// </remarks>
    /// <response code="204">Deleted, or already was.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _users.DeleteAsync(id, cancellationToken);

        return NoContent();
    }

    /// <summary>Brings a soft-deleted user back, exactly as they were.</summary>
    /// <response code="200">Restored, or was never deleted.</response>
    [HttpPost("{id:guid}/restore")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Restore(Guid id, CancellationToken cancellationToken)
        => Ok(await _users.RestoreAsync(id, cancellationToken));

    /// <summary>
    /// Removes the user for good. <b>Irreversible.</b>
    /// </summary>
    /// <remarks>
    /// A separate route rather than a flag on <c>DELETE</c>, deliberately: this is the only
    /// irreversible operation in the API, and it should not be reachable by forgetting a default.
    /// It leaves rows in six other tables pointing at an id that no longer exists, and Agendia
    /// keeps its <c>Employee</c>. Use it for actual erasure — a retention policy expiring, a GDPR
    /// request — not for "the user left".
    /// </remarks>
    /// <response code="204">Gone.</response>
    /// <response code="409">They still belong to an academy. Leave it first.</response>
    [HttpDelete("{id:guid}/permanent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Purge(Guid id, CancellationToken cancellationToken)
    {
        await _users.PurgeAsync(id, cancellationToken);

        return NoContent();
    }
}
