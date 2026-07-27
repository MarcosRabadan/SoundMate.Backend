namespace SoundMate.Domain.Teaching;

public readonly record struct TeacherDisciplineId(Guid Value)
{
    public static TeacherDisciplineId New() => new(Guid.NewGuid());

    public static TeacherDisciplineId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
