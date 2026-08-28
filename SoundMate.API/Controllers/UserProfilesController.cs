using Microsoft.AspNetCore.Mvc;
using SoundMate.Application.Users;
using SoundMate.Application.Users.DTO;

namespace SoundMate.API.Controllers;

/// <summary>
/// A user's public profile: bio and avatar. One per person, for anyone — a student can have a
/// description too, not only teachers.
/// <para>
/// Routed under the user because that is the only id a caller has. A <c>UserProfileId</c> exists in
/// the database and never leaves it: exposing a second identifier for a resource that is already
/// uniquely addressable by its owner would only invite a second way to reach it.
/// </para>
/// <para>
/// <b>Not authenticated yet.</b> Anyone can rewrite anyone's bio. When auth lands this becomes
/// "self or admin".
/// </para>
/// </summary>
[ApiController]
[Route("api/users/{userId:guid}/profile")]
[Produces("application/json")]
public sealed class UserProfilesController : ControllerBase
{
    private readonly IUserProfileService _profiles;

    public UserProfilesController(IUserProfileService profiles) => _profiles = profiles;

    /// <summary>Returns the user's profile.</summary>
    /// <response code="200">Found. Its fields may still be empty — that is a filled-in profile with nothing in it.</response>
    /// <response code="404">Either no such user, or that user has no profile. The body says which.</response>
    [HttpGet]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> Get(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetByUserAsync(userId, cancellationToken);

        return profile is null
            ? NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "User profile not found",
                Detail = $"User '{userId}' has no profile."
            })
            : Ok(profile);
    }

    /// <summary>
    /// Sets the profile, creating it if the user has not got one yet.
    /// </summary>
    /// <remarks>
    /// A <c>PUT</c>, so it replaces the whole thing: a body with only a description <b>clears the
    /// avatar</b>. There is no <c>POST</c> because whether a row already existed is not something
    /// the caller should have to find out first.
    /// </remarks>
    /// <response code="200">Saved.</response>
    /// <response code="400">The avatar is not an absolute http(s) URL, or the bio is too long.</response>
    /// <response code="404">No such user, or the user is deleted.</response>
    [HttpPut]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> Save(Guid userId,
                                                         [FromBody] UpdateUserProfileDto dto,
                                                         CancellationToken cancellationToken)
        => Ok(await _profiles.SaveAsync(userId, dto, cancellationToken));

    /// <summary>Removes the profile. The user keeps existing; they just stop having a bio.</summary>
    /// <response code="204">Gone.</response>
    /// <response code="404">No such user, or that user has no profile.</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid userId, CancellationToken cancellationToken)
    {
        await _profiles.DeleteAsync(userId, cancellationToken);

        return NoContent();
    }
}
