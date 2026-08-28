using SoundMate.Domain.Genres;
using SoundMate.Domain.Teaching;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Abstractions.Persistence;

public interface ITaughtGenreRepository
{
    Task<TaughtGenre?> GetByIdAsync(TaughtGenreId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaughtGenre>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(UserId userId, GenreId genreId, CancellationToken cancellationToken = default);

    Task AddAsync(TaughtGenre taughtGenre, CancellationToken cancellationToken = default);

    void Remove(TaughtGenre taughtGenre);
}
