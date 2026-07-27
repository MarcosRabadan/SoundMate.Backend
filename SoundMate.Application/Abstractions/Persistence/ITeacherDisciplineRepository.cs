using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Teaching;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Abstractions.Persistence;

public interface ITeacherDisciplineRepository
{
    Task<TeacherDiscipline?> GetByIdAsync(TeacherDisciplineId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeacherDiscipline>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(UserId userId, DisciplineId disciplineId, CancellationToken cancellationToken = default);

    Task AddAsync(TeacherDiscipline teacherDiscipline, CancellationToken cancellationToken = default);

    void Remove(TeacherDiscipline teacherDiscipline);
}
