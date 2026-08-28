using Microsoft.EntityFrameworkCore;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Genres;
using SoundMate.Domain.Teaching;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Repositories;

internal sealed class TaughtGenreRepository : ITaughtGenreRepository
{
    private readonly SoundMateDbContext _context;

    public TaughtGenreRepository(SoundMateDbContext context) => _context = context;

    public Task<TaughtGenre?> GetByIdAsync(TaughtGenreId id, CancellationToken cancellationToken = default)
        => _context.TaughtGenres.FirstOrDefaultAsync(tg => tg.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TaughtGenre>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default)
        => await _context.TaughtGenres
            .Where(tg => tg.UserId == userId)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(UserId userId, GenreId genreId, CancellationToken cancellationToken = default)
        => _context.TaughtGenres.AnyAsync(
            tg => tg.UserId == userId && tg.GenreId == genreId,
            cancellationToken);

    public async Task AddAsync(TaughtGenre taughtGenre, CancellationToken cancellationToken = default)
        => await _context.TaughtGenres.AddAsync(taughtGenre, cancellationToken);

    public void Remove(TaughtGenre taughtGenre) => _context.TaughtGenres.Remove(taughtGenre);
}
