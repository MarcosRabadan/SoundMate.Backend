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
    /// <summary>When set, the next save throws as if this index had rejected the write.</summary>
    public string? FailWithUniqueViolationOn { get; set; }

    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;

        if (FailWithUniqueViolationOn is not null)
        {
            throw new UniqueConstraintViolationException(
                FailWithUniqueViolationOn,
                new InvalidOperationException("simulated 23505"));
        }

        return Task.FromResult(1);
    }
}
