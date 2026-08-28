using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Tests.Fakes;

/// <summary>
/// In-memory <see cref="IUserProfileRepository"/>.
/// <para>
/// <see cref="DiscardAdds"/> exists for one test: the branch where a unique violation fires and the
/// re-read that follows finds <b>nothing</b>. That combination means the index claimed a row exists
/// while it does not, and the service is supposed to give up and let the original error travel
/// rather than paper over a problem it does not understand. Without this flag every add is visible
/// straight away and the branch is unreachable.
/// </para>
/// </summary>
internal sealed class FakeUserProfileRepository : IUserProfileRepository
{
    private readonly List<UserProfile> _profiles = [];

    /// <summary>Everything handed to <see cref="AddAsync"/>, stored or not.</summary>
    public List<UserProfile> Added { get; } = [];

    /// <summary>When true, adds are recorded but never become readable.</summary>
    public bool DiscardAdds { get; set; }

    public void Seed(UserProfile profile) => _profiles.Add(profile);

    public Task<UserProfile?> GetByIdAsync(UserProfileId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_profiles.FirstOrDefault(p => p.Id == id));

    public Task<UserProfile?> GetByUserAsync(UserId userId, CancellationToken cancellationToken = default)
        => Task.FromResult(_profiles.FirstOrDefault(p => p.UserId == userId));

    public Task<bool> ExistsForUserAsync(UserId userId, CancellationToken cancellationToken = default)
        => Task.FromResult(_profiles.Any(p => p.UserId == userId));

    public Task AddAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        Added.Add(profile);

        if (!DiscardAdds)
            _profiles.Add(profile);

        return Task.CompletedTask;
    }

    public void Update(UserProfile profile) { }

    public void Remove(UserProfile profile) => _profiles.Remove(profile);
}
