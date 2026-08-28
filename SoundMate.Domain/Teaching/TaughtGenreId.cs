namespace SoundMate.Domain.Teaching;

public readonly record struct TaughtGenreId(Guid Value)
{
    public static TaughtGenreId New() => new(Guid.NewGuid());

    public static TaughtGenreId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
