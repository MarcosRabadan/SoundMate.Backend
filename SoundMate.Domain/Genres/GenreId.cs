namespace SoundMate.Domain.Genres;

public readonly record struct GenreId(Guid Value)
{
    public static GenreId New() => new(Guid.NewGuid());

    public static GenreId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
