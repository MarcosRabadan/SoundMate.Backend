using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Tests.Fakes;

/// <summary>
/// In-memory <see cref="IUserRepository"/>. Hand-written rather than a mocking library, matching
/// how the Infrastructure tests already stub things out in this repo.
/// <para>
/// <see cref="Added"/> is deliberately separate from the stored users: the service calls
/// <c>AddAsync</c> and then <c>SaveChangesAsync</c>, and a test needs to be able to see the entity
/// that was handed over even when the save is made to fail.
/// </para>
/// </summary>
internal sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = [];

    /// <summary>Everything handed to <see cref="AddAsync"/>, saved or not.</summary>
    public List<User> Added { get; } = [];

    public void Seed(User user) => _users.Add(user);

    public Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
        => Task.FromResult(_users.FirstOrDefault(u => u.Email == email));

    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
        => Task.FromResult(_users.Any(u => u.Email == email));

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        Added.Add(user);
        _users.Add(user);
        return Task.CompletedTask;
    }

    public void Update(User user) { }

    public void Remove(User user) => _users.Remove(user);
}
