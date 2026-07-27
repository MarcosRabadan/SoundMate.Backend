namespace SoundMate.Domain.Users;

public readonly record struct UserEducationId(Guid Value)
{
    public static UserEducationId New() => new(Guid.NewGuid());

    public static UserEducationId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
