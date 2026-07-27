using SoundMate.Domain.Users;

namespace SoundMate.Application.Abstractions.Persistence;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByIdAsync(UserProfileId id, CancellationToken cancellationToken = default);

    Task<UserProfile?> GetByUserAsync(UserId userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsForUserAsync(UserId userId, CancellationToken cancellationToken = default);

    Task AddAsync(UserProfile profile, CancellationToken cancellationToken = default);

    void Update(UserProfile profile);

    void Remove(UserProfile profile);
}
