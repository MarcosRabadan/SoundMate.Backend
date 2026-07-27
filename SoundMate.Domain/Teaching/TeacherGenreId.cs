namespace SoundMate.Domain.Teaching;

public readonly record struct TeacherGenreId(Guid Value)
{
    public static TeacherGenreId New() => new(Guid.NewGuid());

    public static TeacherGenreId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
