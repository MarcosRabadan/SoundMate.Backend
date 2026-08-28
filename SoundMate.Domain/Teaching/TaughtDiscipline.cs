using SoundMate.Domain.Common;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Teaching;

/// <summary>
/// A discipline the teacher specializes in / teaches (global to the person, not per academy).
/// This is the "teaches" relationship, distinct from <c>StudiedDiscipline</c> ("studies at a
/// level"): teaching a discipline carries no student level.
/// </summary>
public sealed class TaughtDiscipline : AggregateRoot<TaughtDisciplineId>
{
    public UserId UserId { get; private set; }
    public DisciplineId DisciplineId { get; private set; }

    private TaughtDiscipline() { }

    private TaughtDiscipline(TaughtDisciplineId id, UserId userId, DisciplineId disciplineId) : base(id)
    {
        UserId = userId;
        DisciplineId = disciplineId;
    }

    public static TaughtDiscipline Create(UserId userId, DisciplineId disciplineId)
        => new(
            TaughtDisciplineId.New(),
            Guard.NotEmpty(userId, "User"),
            Guard.NotEmpty(disciplineId, "Discipline"));
}
