using Microsoft.EntityFrameworkCore;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Disciplines;

namespace SoundMate.Infrastructure.Persistence.Repositories;

internal sealed class DisciplineRepository : IDisciplineRepository
{
    private readonly SoundMateDbContext _context;

    public DisciplineRepository(SoundMateDbContext context) => _context = context;

    public Task<Discipline?> GetByIdAsync(DisciplineId id, CancellationToken cancellationToken = default)
        => _context.Disciplines.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(DisciplineId id, CancellationToken cancellationToken = default)
        => _context.Disciplines.AnyAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Discipline>> ListActiveAsync(CancellationToken cancellationToken = default)
        => await _context.Disciplines
            .Where(d => d.IsActive)
            .OrderBy(d => d.Category)
            .ThenBy(d => d.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Discipline>> ListByCategoryAsync(DisciplineCategory category, CancellationToken cancellationToken = default)
        => await _context.Disciplines
            .Where(d => d.Category == category && d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Discipline>> ListByIdsAsync(IReadOnlyCollection<DisciplineId> ids,
                                                                CancellationToken cancellationToken = default)
    {
        // An empty IN () is a query with no possible answer; skipping it saves the round trip.
        if (ids.Count == 0)
            return [];

        // No IsActive filter here, on purpose — see the interface.
        return await _context.Disciplines
            .Where(d => ids.Contains(d.Id))
            .ToListAsync(cancellationToken);
    }
}
