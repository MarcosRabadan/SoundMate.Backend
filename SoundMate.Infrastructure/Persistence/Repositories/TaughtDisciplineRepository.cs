using Microsoft.EntityFrameworkCore;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Teaching;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Repositories;

internal sealed class TaughtDisciplineRepository : ITaughtDisciplineRepository
{
    private readonly SoundMateDbContext _context;

    public TaughtDisciplineRepository(SoundMateDbContext context) => _context = context;

    public Task<TaughtDiscipline?> GetByIdAsync(TaughtDisciplineId id, CancellationToken cancellationToken = default)
        => _context.TaughtDisciplines.FirstOrDefaultAsync(td => td.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TaughtDiscipline>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default)
        => await _context.TaughtDisciplines
            .Where(td => td.UserId == userId)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(UserId userId, DisciplineId disciplineId, CancellationToken cancellationToken = default)
        => _context.TaughtDisciplines.AnyAsync(
            td => td.UserId == userId && td.DisciplineId == disciplineId,
            cancellationToken);

    public async Task AddAsync(TaughtDiscipline taughtDiscipline, CancellationToken cancellationToken = default)
        => await _context.TaughtDisciplines.AddAsync(taughtDiscipline, cancellationToken);

    public void Remove(TaughtDiscipline taughtDiscipline) => _context.TaughtDisciplines.Remove(taughtDiscipline);
}
