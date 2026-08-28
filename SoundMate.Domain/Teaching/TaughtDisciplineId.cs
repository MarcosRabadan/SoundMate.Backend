namespace SoundMate.Domain.Teaching;

public readonly record struct TaughtDisciplineId(Guid Value)
{
    public static TaughtDisciplineId New() => new(Guid.NewGuid());

    public static TaughtDisciplineId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
