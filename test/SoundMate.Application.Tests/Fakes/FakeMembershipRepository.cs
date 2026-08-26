using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Memberships;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Tests.Fakes;

/// <summary>
/// In-memory <see cref="IMembershipRepository"/>. <see cref="UserService"/> only reads from it —
/// to refuse deleting a user who still belongs somewhere — so only <c>ListByUserAsync</c> needs
/// to do anything real.
/// </summary>
internal sealed class FakeMembershipRepository : IMembershipRepository
{
    private readonly List<Membership> _memberships = [];

    public void Seed(Membership membership) => _memberships.Add(membership);

    public Task<IReadOnlyList<Membership>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Membership>>(
            _memberships.Where(m => m.UserId == userId).ToList());

    public Task<Membership?> GetByIdAsync(MembershipId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_memberships.FirstOrDefault(m => m.Id == id));

    public Task<Membership?> GetActiveAsync(UserId userId, AcademyId academyId, CancellationToken cancellationToken = default)
        => Task.FromResult(_memberships.FirstOrDefault(m => m.UserId == userId && m.AcademyId == academyId));

    public Task<bool> HasActiveMembershipAsync(UserId userId, AcademyId academyId, CancellationToken cancellationToken = default)
        => Task.FromResult(_memberships.Any(m => m.UserId == userId && m.AcademyId == academyId));

    public Task AddAsync(Membership membership, CancellationToken cancellationToken = default)
    {
        _memberships.Add(membership);
        return Task.CompletedTask;
    }

    public void Update(Membership membership) { }

    public void Remove(Membership membership) => _memberships.Remove(membership);
}
