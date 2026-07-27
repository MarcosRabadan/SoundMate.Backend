namespace SoundMate.Domain.Users;

public readonly record struct UserProfileId(Guid Value)
{
    public static UserProfileId New() => new(Guid.NewGuid());

    public static UserProfileId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
