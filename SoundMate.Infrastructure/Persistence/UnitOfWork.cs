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
            // A failed save leaves its entries tracked as Added, so a second SaveChanges would
            // replay the very insert the index just rejected. Detaching them leaves the context
            // usable, which is what lets a caller recover by re-reading and applying to the row
            // that won — the only correct answer for an idempotent PUT. Callers that simply turn
            // this into a 409 are unaffected: their scoped context is discarded either way.
            foreach (var entry in ex.Entries)
                entry.State = EntityState.Detached;

            // Postgres 23505. This is the ONLY place that knows what that number means: the
            // Application layer references neither EF Core nor Npgsql, so without the translation
            // a lost race against a unique index would surface to callers as a 500 — a server
            // fault for something the caller could be told plainly.
            throw new UniqueConstraintViolationException(postgres.ConstraintName, ex);
        }
    }
}
