namespace SoundMate.Domain.Users;

public readonly record struct UserDisciplineId(Guid Value)
{
    public static UserDisciplineId New() => new(Guid.NewGuid());

    public static UserDisciplineId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
