using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Abstractions.Persistence;

public interface IUserDisciplineRepository
{
    Task<UserDiscipline?> GetByIdAsync(UserDisciplineId id, CancellationToken cancellationToken = default);

    Task<UserDiscipline?> GetAsync(UserId userId, DisciplineId disciplineId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDiscipline>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(UserId userId, DisciplineId disciplineId, CancellationToken cancellationToken = default);

    Task AddAsync(UserDiscipline userDiscipline, CancellationToken cancellationToken = default);

    void Update(UserDiscipline userDiscipline);

    void Remove(UserDiscipline userDiscipline);
}
