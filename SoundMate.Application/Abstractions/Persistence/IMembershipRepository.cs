using SoundMate.Domain.Academies;
using SoundMate.Domain.Memberships;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Abstractions.Persistence;

public interface IMembershipRepository
{
    Task<Membership?> GetByIdAsync(MembershipId id, CancellationToken cancellationToken = default);

    Task<Membership?> GetActiveAsync(UserId userId, AcademyId academyId, CancellationToken cancellationToken = default);

    Task<bool> HasActiveMembershipAsync(UserId userId, AcademyId academyId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Membership>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Everyone who belongs to that academy. Backed by <c>IX_Memberships_AcademyId</c>, which
    /// existed for this query before anything could make it.
    /// </summary>
    Task<IReadOnlyList<Membership>> ListByAcademyAsync(AcademyId academyId, CancellationToken cancellationToken = default);

    Task AddAsync(Membership membership, CancellationToken cancellationToken = default);

    void Update(Membership membership);

    void Remove(Membership membership);
}
