namespace SoundMate.Domain.Disciplines;

public readonly record struct DisciplineId(Guid Value)
{
    public static DisciplineId New() => new(Guid.NewGuid());

    public static DisciplineId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
