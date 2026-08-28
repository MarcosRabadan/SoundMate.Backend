using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Teaching;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Abstractions.Persistence;

public interface ITaughtDisciplineRepository
{
    Task<TaughtDiscipline?> GetByIdAsync(TaughtDisciplineId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaughtDiscipline>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(UserId userId, DisciplineId disciplineId, CancellationToken cancellationToken = default);

    Task AddAsync(TaughtDiscipline taughtDiscipline, CancellationToken cancellationToken = default);

    void Remove(TaughtDiscipline taughtDiscipline);
}
