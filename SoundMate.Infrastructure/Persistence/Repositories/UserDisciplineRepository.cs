using Microsoft.EntityFrameworkCore;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Repositories;

internal sealed class UserDisciplineRepository : IUserDisciplineRepository
{
    private readonly SoundMateDbContext _context;

    public UserDisciplineRepository(SoundMateDbContext context) => _context = context;

    public Task<UserDiscipline?> GetByIdAsync(UserDisciplineId id, CancellationToken cancellationToken = default)
        => _context.UserDisciplines.FirstOrDefaultAsync(ud => ud.Id == id, cancellationToken);

    public Task<UserDiscipline?> GetAsync(UserId userId, DisciplineId disciplineId, CancellationToken cancellationToken = default)
        => _context.UserDisciplines.FirstOrDefaultAsync(
            ud => ud.UserId == userId && ud.DisciplineId == disciplineId,
            cancellationToken);

    public async Task<IReadOnlyList<UserDiscipline>> ListByUserAsync(UserId userId, CancellationToken cancellationToken = default)
        => await _context.UserDisciplines
            .Where(ud => ud.UserId == userId)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(UserId userId, DisciplineId disciplineId, CancellationToken cancellationToken = default)
        => _context.UserDisciplines.AnyAsync(
            ud => ud.UserId == userId && ud.DisciplineId == disciplineId,
            cancellationToken);

    public async Task AddAsync(UserDiscipline userDiscipline, CancellationToken cancellationToken = default)
        => await _context.UserDisciplines.AddAsync(userDiscipline, cancellationToken);

    public void Update(UserDiscipline userDiscipline) => _context.UserDisciplines.Update(userDiscipline);

    public void Remove(UserDiscipline userDiscipline) => _context.UserDisciplines.Remove(userDiscipline);
}
