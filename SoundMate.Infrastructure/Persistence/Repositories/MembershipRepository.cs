using Microsoft.EntityFrameworkCore;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Memberships;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Repositories;

internal sealed class MembershipRepository : IMembershipRepository
{
    private readonly SoundMateDbContext _context;

    public MembershipRepository(SoundMateDbContext context) => _context = context;

    public Task<Membership?> GetByIdAsync(MembershipId id, CancellationToken cancellationToken = default)
        => _context.Memberships.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<Membership?> GetActiveAsync(UserId userId, AcademyId academyId, CancellationToken cancellationToken = default)
        => _context.Memberships.FirstOrDefaultAsync(
            m => m.UserId == userId && m.AcademyId == academyId && m.Status == MembershipStatus.Active,
            cancellationToken);

    public Task<bool> HasActiveMembershipAsync(UserId userId, AcademyId academyId, CancellationToken cancellationToken = default)
        => _context.Memberships.AnyAsync(
            m => m.UserId == userId && m.AcademyId == academyId && m.Status == MembershipStatus.Active,
            cancellationToken);

    public async Task<IReadOnlyList<Membership>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default)
        => await _context.Memberships
            .Where(m => m.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Membership membership, CancellationToken cancellationToken = default)
        => await _context.Memberships.AddAsync(membership, cancellationToken);

    public void Update(Membership membership) => _context.Memberships.Update(membership);

    public void Remove(Membership membership) => _context.Memberships.Remove(membership);
}
