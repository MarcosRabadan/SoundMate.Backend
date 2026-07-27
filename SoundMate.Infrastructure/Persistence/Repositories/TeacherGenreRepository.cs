using Microsoft.EntityFrameworkCore;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Genres;
using SoundMate.Domain.Teaching;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Repositories;

internal sealed class TeacherGenreRepository : ITeacherGenreRepository
{
    private readonly SoundMateDbContext _context;

    public TeacherGenreRepository(SoundMateDbContext context) => _context = context;

    public Task<TeacherGenre?> GetByIdAsync(TeacherGenreId id, CancellationToken cancellationToken = default)
        => _context.TeacherGenres.FirstOrDefaultAsync(tg => tg.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TeacherGenre>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default)
        => await _context.TeacherGenres
            .Where(tg => tg.UserId == userId)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(UserId userId, GenreId genreId, CancellationToken cancellationToken = default)
        => _context.TeacherGenres.AnyAsync(
            tg => tg.UserId == userId && tg.GenreId == genreId,
            cancellationToken);

    public async Task AddAsync(TeacherGenre teacherGenre, CancellationToken cancellationToken = default)
        => await _context.TeacherGenres.AddAsync(teacherGenre, cancellationToken);

    public void Remove(TeacherGenre teacherGenre) => _context.TeacherGenres.Remove(teacherGenre);
}
