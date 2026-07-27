using Microsoft.EntityFrameworkCore;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Teaching;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Repositories;

internal sealed class TeacherDisciplineRepository : ITeacherDisciplineRepository
{
    private readonly SoundMateDbContext _context;

    public TeacherDisciplineRepository(SoundMateDbContext context) => _context = context;

    public Task<TeacherDiscipline?> GetByIdAsync(TeacherDisciplineId id, CancellationToken cancellationToken = default)
        => _context.TeacherDisciplines.FirstOrDefaultAsync(td => td.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TeacherDiscipline>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default)
        => await _context.TeacherDisciplines
            .Where(td => td.UserId == userId)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(UserId userId, DisciplineId disciplineId, CancellationToken cancellationToken = default)
        => _context.TeacherDisciplines.AnyAsync(
            td => td.UserId == userId && td.DisciplineId == disciplineId,
            cancellationToken);

    public async Task AddAsync(TeacherDiscipline teacherDiscipline, CancellationToken cancellationToken = default)
        => await _context.TeacherDisciplines.AddAsync(teacherDiscipline, cancellationToken);

    public void Remove(TeacherDiscipline teacherDiscipline) => _context.TeacherDisciplines.Remove(teacherDiscipline);
}
