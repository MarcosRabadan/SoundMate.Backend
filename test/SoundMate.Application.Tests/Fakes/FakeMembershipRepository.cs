using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Memberships;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Tests.Fakes;

/// <summary>
/// In-memory <see cref="IMembershipRepository"/>. The services read from it to refuse purging a
/// user or an academy that still has relationships hanging off it, and <c>AcademyService</c> also
/// writes the owner's membership when an academy is created.
/// </summary>
internal sealed class FakeMembershipRepository : IMembershipRepository
{
    private readonly List<Membership> _memberships = [];

    /// <summary>Everything handed to <see cref="AddAsync"/>, saved or not.</summary>
    public List<Membership> Added { get; } = [];

    public void Seed(Membership membership) => _memberships.Add(membership);

    public Task<IReadOnlyList<Membership>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Membership>>(
            _memberships.Where(m => m.UserId == userId).ToList());

    public Task<IReadOnlyList<Membership>> ListByAcademyAsync(AcademyId academyId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Membership>>(
            _memberships.Where(m => m.AcademyId == academyId).ToList());

    public Task<Membership?> GetByIdAsync(MembershipId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_memberships.FirstOrDefault(m => m.Id == id));

    public Task<Membership?> GetActiveAsync(UserId userId, AcademyId academyId, CancellationToken cancellationToken = default)
        => Task.FromResult(_memberships.FirstOrDefault(m => m.UserId == userId && m.AcademyId == academyId));

    public Task<bool> HasActiveMembershipAsync(UserId userId, AcademyId academyId, CancellationToken cancellationToken = default)
        => Task.FromResult(_memberships.Any(m => m.UserId == userId && m.AcademyId == academyId));

    public Task AddAsync(Membership membership, CancellationToken cancellationToken = default)
    {
        Added.Add(membership);
        _memberships.Add(membership);
        return Task.CompletedTask;
    }

    public void Update(Membership membership) { }

    public void Remove(Membership membership) => _memberships.Remove(membership);
}
