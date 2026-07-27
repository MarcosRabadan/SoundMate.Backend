namespace SoundMate.Domain.Users;

/// <summary>
/// Strongly-typed user identifier. It prevents mixing up a user id with an academy id:
/// the compiler will not let one be passed where the other is expected. Backed by a
/// plain <see cref="Guid"/> in the database.
/// </summary>
public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());

    public static UserId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
