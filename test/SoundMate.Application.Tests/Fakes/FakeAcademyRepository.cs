using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Tests.Fakes;

/// <summary>
/// In-memory <see cref="IAcademyRepository"/>. Hand-written rather than a mocking library, the
/// same style the other fakes here use.
/// <para>
/// Like <see cref="FakeUserRepository"/>, <see cref="Added"/> is kept apart from the stored set so
/// a test can inspect what was handed over even when the save is made to fail. And, like the real
/// repository, nothing here filters soft-deleted rows: that policy belongs to the service.
/// </para>
/// </summary>
internal sealed class FakeAcademyRepository : IAcademyRepository
{
    private readonly List<Academy> _academies = [];

    /// <summary>Everything handed to <see cref="AddAsync"/>, saved or not.</summary>
    public List<Academy> Added { get; } = [];

    public void Seed(Academy academy) => _academies.Add(academy);

    public Task<Academy?> GetByIdAsync(AcademyId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_academies.FirstOrDefault(a => a.Id == id));

    public Task<Academy?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
        => Task.FromResult(_academies.FirstOrDefault(a => a.Slug == slug));

    public Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
        => Task.FromResult(_academies.Any(a => a.Slug == slug));

    public Task<IReadOnlyList<Academy>> ListByOwnerAsync(UserId ownerId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Academy>>(
            _academies.Where(a => a.OwnerId == ownerId).ToList());

    public Task AddAsync(Academy academy, CancellationToken cancellationToken = default)
    {
        Added.Add(academy);
        _academies.Add(academy);
        return Task.CompletedTask;
    }

    public void Update(Academy academy) { }

    public void Remove(Academy academy) => _academies.Remove(academy);
}
