using SoundMate.Domain.Common;

namespace SoundMate.Domain.Disciplines;

/// <summary>
/// Something that can be studied and rated with a level: an instrument (piano, violin...) or a
/// music subject (music theory, harmony...). Seeded reference data shared across the app.
/// <see cref="IsActive"/> soft-hides one without deleting it (which would orphan references).
/// </summary>
public sealed class Discipline : AggregateRoot<DisciplineId>
{
    public string Name { get; private set; } = default!;
    public DisciplineCategory Category { get; private set; }
    public bool IsActive { get; private set; }

    private Discipline() { }

    public Discipline(DisciplineId id, string name, DisciplineCategory category, bool isActive = true) : base(id)
    {
        Name = Guard.NotNullOrWhiteSpace(name, "Name");
        Category = Guard.Defined(category, "Category");
        IsActive = isActive;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
