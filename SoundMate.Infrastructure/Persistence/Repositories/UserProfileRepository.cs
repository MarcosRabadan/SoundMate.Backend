using Microsoft.EntityFrameworkCore;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Repositories;

internal sealed class UserProfileRepository : IUserProfileRepository
{
    private readonly SoundMateDbContext _context;

    public UserProfileRepository(SoundMateDbContext context) => _context = context;

    public Task<UserProfile?> GetByIdAsync(UserProfileId id, CancellationToken cancellationToken = default)
        => _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<UserProfile?> GetByUserAsync(UserId userId, CancellationToken cancellationToken = default)
        => _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    public Task<bool> ExistsForUserAsync(UserId userId, CancellationToken cancellationToken = default)
        => _context.UserProfiles.AnyAsync(p => p.UserId == userId, cancellationToken);

    public async Task AddAsync(UserProfile profile, CancellationToken cancellationToken = default)
        => await _context.UserProfiles.AddAsync(profile, cancellationToken);

    public void Update(UserProfile profile) => _context.UserProfiles.Update(profile);

    public void Remove(UserProfile profile) => _context.UserProfiles.Remove(profile);
}
