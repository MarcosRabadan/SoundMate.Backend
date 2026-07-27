namespace SoundMate.Domain.Academies;

public readonly record struct AcademyId(Guid Value)
{
    public static AcademyId New() => new(Guid.NewGuid());

    public static AcademyId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
