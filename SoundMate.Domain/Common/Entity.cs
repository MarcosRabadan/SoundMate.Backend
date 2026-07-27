namespace SoundMate.Domain.Common;

/// <summary>
/// Base for every domain entity. Identity is defined by <see cref="Id"/>, not by the
/// properties: two entities of the same type with the same Id are the same entity, even if
/// the rest of their data differs. The Id is set through the constructor and never mutated
/// from outside.
/// </summary>
public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    protected Entity(TId id) => Id = id;

    // Parameterless constructor for EF Core materialization.
    protected Entity() { }

    public override bool Equals(object? obj)
        => obj is Entity<TId> entity && entity.GetType() == GetType() && entity.Id.Equals(Id);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}
