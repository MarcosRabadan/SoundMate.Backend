using Microsoft.EntityFrameworkCore;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Repositories;

internal sealed class UserEducationRepository : IUserEducationRepository
{
    private readonly SoundMateDbContext _context;

    public UserEducationRepository(SoundMateDbContext context) => _context = context;

    public Task<UserEducation?> GetByIdAsync(UserEducationId id, CancellationToken cancellationToken = default)
        => _context.UserEducations.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<UserEducation>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default)
        => await _context.UserEducations
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EndYear)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(UserEducation education, CancellationToken cancellationToken = default)
        => await _context.UserEducations.AddAsync(education, cancellationToken);

    public void Update(UserEducation education) => _context.UserEducations.Update(education);

    public void Remove(UserEducation education) => _context.UserEducations.Remove(education);
}
