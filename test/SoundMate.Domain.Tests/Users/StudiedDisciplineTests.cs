using Shouldly;
using SoundMate.Domain.Common;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Tests.Users;

public class StudiedDisciplineTests
{
    [Fact]
    public void Create_Valid_SetsFields()
    {
        var userId = UserId.New();
        var disciplineId = DisciplineId.New();

        var sd = StudiedDiscipline.Create(userId, disciplineId, MusicLevel.Advanced);

        sd.UserId.ShouldBe(userId);
        sd.DisciplineId.ShouldBe(disciplineId);
        sd.Level.ShouldBe(MusicLevel.Advanced);
    }

    [Fact]
    public void Create_EmptyUser_Throws()
        => Should.Throw<DomainException>(
            () => StudiedDiscipline.Create(default, DisciplineId.New(), MusicLevel.Beginner));

    [Fact]
    public void Create_EmptyDiscipline_Throws()
        => Should.Throw<DomainException>(
            () => StudiedDiscipline.Create(UserId.New(), default, MusicLevel.Beginner));

    [Fact]
    public void Create_UndefinedLevel_Throws()
        => Should.Throw<DomainException>(
            () => StudiedDiscipline.Create(UserId.New(), DisciplineId.New(), (MusicLevel)42));

    [Fact]
    public void ChangeLevel_Valid_Updates()
    {
        var sd = StudiedDiscipline.Create(UserId.New(), DisciplineId.New(), MusicLevel.Beginner);
        sd.ChangeLevel(MusicLevel.Intermediate);
        sd.Level.ShouldBe(MusicLevel.Intermediate);
    }

    [Fact]
    public void ChangeLevel_Undefined_Throws()
    {
        var sd = StudiedDiscipline.Create(UserId.New(), DisciplineId.New(), MusicLevel.Beginner);
        Should.Throw<DomainException>(() => sd.ChangeLevel((MusicLevel)42));
    }

    // ------------------------------------------------------------- timestamps

    [Fact]
    public void Create_StampsBothTimesInUtc()
    {
        var sd = StudiedDiscipline.Create(UserId.New(), DisciplineId.New(), MusicLevel.Beginner);

        sd.CreatedAtUtc.Kind.ShouldBe(DateTimeKind.Utc);
        sd.CreatedAtUtc.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Equal, not merely close: nothing has happened to the row yet, so "last changed" is
        // "created". A later UpdatedAtUtc would claim an edit that never took place.
        sd.UpdatedAtUtc.ShouldBe(sd.CreatedAtUtc);
    }

    [Fact]
    public void ChangeLevel_MovesUpdatedOnly()
    {
        var sd = StudiedDiscipline.Create(UserId.New(), DisciplineId.New(), MusicLevel.Beginner);
        var created = sd.CreatedAtUtc;

        sd.ChangeLevel(MusicLevel.Advanced);

        sd.CreatedAtUtc.ShouldBe(created);
        sd.UpdatedAtUtc.ShouldBeGreaterThanOrEqualTo(created);
    }

    [Fact]
    public void ChangeLevel_Undefined_LeavesTheStampAlone()
    {
        // A refused change is not a change. Touching before the guard would record an edit for a
        // request that was rejected, and the stamp is the only record there is.
        var sd = StudiedDiscipline.Create(UserId.New(), DisciplineId.New(), MusicLevel.Beginner);
        var updated = sd.UpdatedAtUtc;

        Should.Throw<DomainException>(() => sd.ChangeLevel((MusicLevel)42));

        sd.UpdatedAtUtc.ShouldBe(updated);
        sd.Level.ShouldBe(MusicLevel.Beginner);
    }
}
