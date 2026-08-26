using SoundMate.Application.Users.DTO;

namespace SoundMate.Application.Users;

/// <summary>Use cases that operate on a <c>User</c>.</summary>
public interface IUserService
{
    /// <summary>
    /// Registers a new person and returns them.
    /// </summary>
    /// <exception cref="Common.Exceptions.EmailAlreadyRegisteredException">
    /// The email already belongs to someone.
    /// </exception>
    /// <exception cref="Domain.Common.DomainException">
    /// The email is malformed, or the name is empty.
    /// </exception>
    Task<UserDto> RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default);

    /// <summary>Returns the user, or <c>null</c> when no such id exists.</summary>
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
