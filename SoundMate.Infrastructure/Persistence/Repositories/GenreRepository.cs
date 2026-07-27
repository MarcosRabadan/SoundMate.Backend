using Microsoft.EntityFrameworkCore;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Genres;

namespace SoundMate.Infrastructure.Persistence.Repositories;

internal sealed class GenreRepository : IGenreRepository
{
    private readonly SoundMateDbContext _context;

    public GenreRepository(SoundMateDbContext context) => _context = context;

    public Task<Genre?> GetByIdAsync(GenreId id, CancellationToken cancellationToken = default)
        => _context.Genres.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(GenreId id, CancellationToken cancellationToken = default)
        => _context.Genres.AnyAsync(g => g.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Genre>> ListActiveAsync(CancellationToken cancellationToken = default)
        => await _context.Genres
            .Where(g => g.IsActive)
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken);
}
