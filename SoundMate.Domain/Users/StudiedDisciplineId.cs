namespace SoundMate.Domain.Users;

public readonly record struct StudiedDisciplineId(Guid Value)
{
    public static StudiedDisciplineId New() => new(Guid.NewGuid());

    public static StudiedDisciplineId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
