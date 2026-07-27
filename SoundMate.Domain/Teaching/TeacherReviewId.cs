namespace SoundMate.Domain.Teaching;

public readonly record struct TeacherReviewId(Guid Value)
{
    public static TeacherReviewId New() => new(Guid.NewGuid());

    public static TeacherReviewId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
