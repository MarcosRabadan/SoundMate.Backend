using SoundMate.Application.Abstractions.Persistence;

namespace SoundMate.Application.Tests.Fakes;

/// <summary>
/// <see cref="IUnitOfWork"/> that can be told to fail the way the database does.
/// <para>
/// <see cref="FailWithUniqueViolationOn"/> is the point of this class: the duplicate-email race is
/// only reachable when the save loses against the unique index, which no amount of in-memory
/// set-up reproduces on its own.
/// </para>
/// </summary>
internal sealed class FakeUnitOfWork : IUnitOfWork
{
    /// <summary>When set, <b>every</b> save throws as if this index had rejected the write.</summary>
    public string? FailWithUniqueViolationOn { get; set; }

    /// <summary>
    /// When set, only the <b>next</b> save throws; the ones after it succeed.
    /// <para>
    /// That asymmetry is the point: a recovery that re-reads the winning row and saves again can
    /// only be tested if the second save is allowed to work. With the always-fail flag the retry
    /// would look like an infinite loop rather than a fix.
    /// </para>
    /// </summary>
    public string? FailNextSaveWithUniqueViolationOn { get; set; }

    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;

        if (FailNextSaveWithUniqueViolationOn is not null)
        {
            var constraint = FailNextSaveWithUniqueViolationOn;
            FailNextSaveWithUniqueViolationOn = null;

            throw new UniqueConstraintViolationException(
                constraint,
                new InvalidOperationException("simulated 23505"));
        }

        if (FailWithUniqueViolationOn is not null)
        {
            throw new UniqueConstraintViolationException(
                FailWithUniqueViolationOn,
                new InvalidOperationException("simulated 23505"));
        }

        return Task.FromResult(1);
    }
}
