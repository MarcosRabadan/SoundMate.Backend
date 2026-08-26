using Microsoft.AspNetCore.Mvc;
using SoundMate.Application.Users;
using SoundMate.Application.Users.DTO;

namespace SoundMate.API.Controllers;

/// <summary>
/// People. A <c>User</c> is global and unique by email: the same person keeps one account across
/// every academy they belong to, so this is registration, not "create a member of somewhere".
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
    /// <response code="200">Found.</response>
    /// <response code="404">No user with that id.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);

        return user is null ? NotFound() : Ok(user);
    }
}
