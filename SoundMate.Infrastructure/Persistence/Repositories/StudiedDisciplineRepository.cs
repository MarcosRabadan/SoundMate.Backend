using Microsoft.EntityFrameworkCore;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Repositories;

internal sealed class StudiedDisciplineRepository : IStudiedDisciplineRepository
{
    private readonly SoundMateDbContext _context;

    public StudiedDisciplineRepository(SoundMateDbContext context) => _context = context;

    public Task<StudiedDiscipline?> GetByIdAsync(StudiedDisciplineId id, CancellationToken cancellationToken = default)
        => _context.StudiedDisciplines.FirstOrDefaultAsync(sd => sd.Id == id, cancellationToken);

    public Task<StudiedDiscipline?> GetAsync(UserId userId, DisciplineId disciplineId, CancellationToken cancellationToken = default)
        => _context.StudiedDisciplines.FirstOrDefaultAsync(
            sd => sd.UserId == userId && sd.DisciplineId == disciplineId,
            cancellationToken);

    public async Task<IReadOnlyList<StudiedDiscipline>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default)
        => await _context.StudiedDisciplines
            .Where(sd => sd.UserId == userId)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(UserId userId, DisciplineId disciplineId, CancellationToken cancellationToken = default)
        => _context.StudiedDisciplines.AnyAsync(
            sd => sd.UserId == userId && sd.DisciplineId == disciplineId,
            cancellationToken);

    public async Task AddAsync(StudiedDiscipline studiedDiscipline, CancellationToken cancellationToken = default)
        => await _context.StudiedDisciplines.AddAsync(studiedDiscipline, cancellationToken);

    public void Update(StudiedDiscipline studiedDiscipline) => _context.StudiedDisciplines.Update(studiedDiscipline);

    public void Remove(StudiedDiscipline studiedDiscipline) => _context.StudiedDisciplines.Remove(studiedDiscipline);
}
