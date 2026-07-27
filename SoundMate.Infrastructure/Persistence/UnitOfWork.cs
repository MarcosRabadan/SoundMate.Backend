using SoundMate.Application.Abstractions.Persistence;

namespace SoundMate.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly SoundMateDbContext _context;

    public UnitOfWork(SoundMateDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
