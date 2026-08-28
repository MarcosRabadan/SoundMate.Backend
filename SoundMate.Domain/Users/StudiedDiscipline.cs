using SoundMate.Domain.Common;
using SoundMate.Domain.Disciplines;

namespace SoundMate.Domain.Users;

/// <summary>
/// A user's level on a single discipline ("Pepito plays piano at Advanced"). One row per
/// discipline studied; a teacher who never studies simply has none. References the user and
/// the discipline by identity.
/// <para>
/// This is the "studies" relationship, distinct from <c>TaughtDiscipline</c> ("teaches", global
/// to the person and with no level). The two are structurally alike and stay apart on purpose:
/// a level is required to study and means nothing to teach.
/// </para>
/// </summary>
public sealed class StudiedDiscipline : AggregateRoot<StudiedDisciplineId>
{
    public UserId UserId { get; private set; }
    public DisciplineId DisciplineId { get; private set; }
    public MusicLevel Level { get; private set; }

    /// <summary>
    /// When this row was created — <b>not</b> when the person took up the instrument. If "playing
    /// since" ever becomes interesting it is a separate field the user supplies, because the clock
    /// cannot know it.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>Last level change.</summary>
    public DateTime UpdatedAtUtc { get; private set; }

    private StudiedDiscipline() { }

    private StudiedDiscipline(StudiedDisciplineId id, UserId userId, DisciplineId disciplineId, MusicLevel level) : base(id)
    {
        UserId = userId;
        DisciplineId = disciplineId;
        Level = level;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public static StudiedDiscipline Create(UserId userId, DisciplineId disciplineId, MusicLevel level)
        => new(
            StudiedDisciplineId.New(),
            Guard.NotEmpty(userId, "User"),
            Guard.NotEmpty(disciplineId, "Discipline"),
            Guard.Defined(level, "Level"));

    public void ChangeLevel(MusicLevel level)
    {
        Level = Guard.Defined(level, "Level");
        Touch();
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
