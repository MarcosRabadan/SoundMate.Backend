using SoundMate.Domain.Genres;

namespace SoundMate.Application.Abstractions.Persistence;

public interface IGenreRepository
{
    Task<Genre?> GetByIdAsync(GenreId id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(GenreId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Genre>> ListActiveAsync(CancellationToken cancellationToken = default);
}
