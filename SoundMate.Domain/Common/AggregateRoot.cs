namespace SoundMate.Domain.Common;

/// <summary>
/// Aggregate root: the entry point to the aggregate and the type repositories operate on.
/// Construction and mutation go through its own methods so invariants are always upheld.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    protected AggregateRoot(TId id) : base(id) { }

    protected AggregateRoot() { }
}
