using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Tests.Fakes;

/// <summary>
/// In-memory <see cref="IStudiedDisciplineRepository"/>.
/// <para>
/// <see cref="Added"/> is separate from the stored rows because the service calls
/// <c>AddAsync</c> and then <c>SaveChangesAsync</c>: a test that makes the save fail still needs
/// to see what was handed over.
/// </para>
/// </summary>
internal sealed class FakeStudiedDisciplineRepository : IStudiedDisciplineRepository
{
    private readonly List<StudiedDiscipline> _rows = [];

    /// <summary>Everything handed to <see cref="AddAsync"/>, saved or not.</summary>
    public List<StudiedDiscipline> Added { get; } = [];

    public void Seed(StudiedDiscipline row) => _rows.Add(row);

    public Task<StudiedDiscipline?> GetByIdAsync(StudiedDisciplineId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_rows.FirstOrDefault(r => r.Id == id));

    public Task<StudiedDiscipline?> GetAsync(UserId userId, DisciplineId disciplineId, CancellationToken cancellationToken = default)
        => Task.FromResult(_rows.FirstOrDefault(r => r.UserId == userId && r.DisciplineId == disciplineId));

    public Task<IReadOnlyList<StudiedDiscipline>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<StudiedDiscipline>>(_rows.Where(r => r.UserId == userId).ToList());

    public Task<bool> ExistsAsync(UserId userId, DisciplineId disciplineId, CancellationToken cancellationToken = default)
        => Task.FromResult(_rows.Any(r => r.UserId == userId && r.DisciplineId == disciplineId));

    public Task AddAsync(StudiedDiscipline row, CancellationToken cancellationToken = default)
    {
        Added.Add(row);
        _rows.Add(row);

        return Task.CompletedTask;
    }

    public void Update(StudiedDiscipline row) { }

    public void Remove(StudiedDiscipline row) => _rows.Remove(row);
}
