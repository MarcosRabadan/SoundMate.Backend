using SoundMate.Domain.Common;

namespace SoundMate.Domain.Genres;

/// <summary>
/// A musical genre in the catalog (Classical, Jazz, Flamenco, Metal...). Seeded reference
/// data; a teacher's genres reference it. <see cref="IsActive"/> soft-hides one.
/// </summary>
public sealed class Genre : AggregateRoot<GenreId>
{
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; }

    private Genre() { }

    public Genre(GenreId id, string name, bool isActive = true) : base(id)
    {
        Name = Guard.NotNullOrWhiteSpace(name, "Name");
        IsActive = isActive;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
