using Microsoft.EntityFrameworkCore;
using Npgsql;
using SoundMate.Application.Abstractions.Persistence;

namespace SoundMate.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly SoundMateDbContext _context;

    public UnitOfWork(SoundMateDbContext context) => _context = context;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres)
        {
            // Postgres 23505. This is the ONLY place that knows what that number means: the
            // Application layer references neither EF Core nor Npgsql, so without the translation
            // a lost race against a unique index would surface to callers as a 500 — a server
            // fault for something the caller could be told plainly.
            throw new UniqueConstraintViolationException(postgres.ConstraintName, ex);
        }
    }
}
