using SoundMate.Domain.Genres;
using SoundMate.Domain.Teaching;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Abstractions.Persistence;

public interface ITeacherGenreRepository
{
    Task<TeacherGenre?> GetByIdAsync(TeacherGenreId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeacherGenre>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(UserId userId, GenreId genreId, CancellationToken cancellationToken = default);

    Task AddAsync(TeacherGenre teacherGenre, CancellationToken cancellationToken = default);

    void Remove(TeacherGenre teacherGenre);
}
