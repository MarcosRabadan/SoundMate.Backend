using Shouldly;
using SoundMate.Domain.Common;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Tests.Users;

public class UserDisciplineTests
{
    [Fact]
    public void Create_Valid_SetsFields()
    {
        var userId = UserId.New();
        var disciplineId = DisciplineId.New();

        var ud = UserDiscipline.Create(userId, disciplineId, MusicLevel.Advanced);

        ud.UserId.ShouldBe(userId);
        ud.DisciplineId.ShouldBe(disciplineId);
        ud.Level.ShouldBe(MusicLevel.Advanced);
    }

    [Fact]
    public void Create_EmptyUser_Throws()
        => Should.Throw<DomainException>(
            () => UserDiscipline.Create(default, DisciplineId.New(), MusicLevel.Beginner));

    [Fact]
    public void Create_EmptyDiscipline_Throws()
        => Should.Throw<DomainException>(
            () => UserDiscipline.Create(UserId.New(), default, MusicLevel.Beginner));

    [Fact]
    public void Create_UndefinedLevel_Throws()
        => Should.Throw<DomainException>(
            () => UserDiscipline.Create(UserId.New(), DisciplineId.New(), (MusicLevel)42));

    [Fact]
    public void ChangeLevel_Valid_Updates()
    {
        var ud = UserDiscipline.Create(UserId.New(), DisciplineId.New(), MusicLevel.Beginner);
        ud.ChangeLevel(MusicLevel.Intermediate);
        ud.Level.ShouldBe(MusicLevel.Intermediate);
    }

    [Fact]
    public void ChangeLevel_Undefined_Throws()
    {
        var ud = UserDiscipline.Create(UserId.New(), DisciplineId.New(), MusicLevel.Beginner);
        Should.Throw<DomainException>(() => ud.ChangeLevel((MusicLevel)42));
    }
}
