using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Abstractions.Persistence;

public interface IStudiedDisciplineRepository
{
    Task<StudiedDiscipline?> GetByIdAsync(StudiedDisciplineId id, CancellationToken cancellationToken = default);

    Task<StudiedDiscipline?> GetAsync(UserId userId, DisciplineId disciplineId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudiedDiscipline>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(UserId userId, DisciplineId disciplineId, CancellationToken cancellationToken = default);

    Task AddAsync(StudiedDiscipline studiedDiscipline, CancellationToken cancellationToken = default);

    void Update(StudiedDiscipline studiedDiscipline);

    void Remove(StudiedDiscipline studiedDiscipline);
}
