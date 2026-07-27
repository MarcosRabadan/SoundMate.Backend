using SoundMate.Domain.Common;
using SoundMate.Domain.Disciplines;

namespace SoundMate.Domain.Users;

/// <summary>
/// A user's level on a single discipline ("Pepito plays piano at Advanced"). One row per
/// discipline studied; a teacher who never studies simply has none. References the user and
/// the discipline by identity.
/// </summary>
public sealed class UserDiscipline : AggregateRoot<UserDisciplineId>
{
    public UserId UserId { get; private set; }
    public DisciplineId DisciplineId { get; private set; }
    public MusicLevel Level { get; private set; }

    private UserDiscipline() { }

    private UserDiscipline(UserDisciplineId id, UserId userId, DisciplineId disciplineId, MusicLevel level) : base(id)
    {
        UserId = userId;
        DisciplineId = disciplineId;
        Level = level;
    }

    public static UserDiscipline Create(UserId userId, DisciplineId disciplineId, MusicLevel level)
        => new(
            UserDisciplineId.New(),
            Guard.NotEmpty(userId, "User"),
            Guard.NotEmpty(disciplineId, "Discipline"),
            Guard.Defined(level, "Level"));

    public void ChangeLevel(MusicLevel level)
        => Level = Guard.Defined(level, "Level");
}
