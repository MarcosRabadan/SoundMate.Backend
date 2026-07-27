using SoundMate.Domain.Common;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Teaching;

/// <summary>
/// A discipline the teacher specializes in / teaches (global to the person, not per academy).
/// This is the "teaches" relationship, distinct from <c>UserDiscipline</c> ("studies at a
/// level"): teaching a discipline carries no student level.
/// </summary>
public sealed class TeacherDiscipline : AggregateRoot<TeacherDisciplineId>
{
    public UserId UserId { get; private set; }
    public DisciplineId DisciplineId { get; private set; }

    private TeacherDiscipline() { }

    private TeacherDiscipline(TeacherDisciplineId id, UserId userId, DisciplineId disciplineId) : base(id)
    {
        UserId = userId;
        DisciplineId = disciplineId;
    }

    public static TeacherDiscipline Create(UserId userId, DisciplineId disciplineId)
        => new(
            TeacherDisciplineId.New(),
            Guard.NotEmpty(userId, "User"),
            Guard.NotEmpty(disciplineId, "Discipline"));
}
