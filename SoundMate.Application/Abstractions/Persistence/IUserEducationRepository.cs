using SoundMate.Domain.Users;

namespace SoundMate.Application.Abstractions.Persistence;

public interface IUserEducationRepository
{
    Task<UserEducation?> GetByIdAsync(UserEducationId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserEducation>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default);

    Task AddAsync(UserEducation education, CancellationToken cancellationToken = default);

    void Update(UserEducation education);

    void Remove(UserEducation education);
}
