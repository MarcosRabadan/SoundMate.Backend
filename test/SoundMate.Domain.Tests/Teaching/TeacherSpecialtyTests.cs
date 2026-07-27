using Shouldly;
using SoundMate.Domain.Common;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Genres;
using SoundMate.Domain.Teaching;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Tests.Teaching;

public class TeacherSpecialtyTests
{
    [Fact]
    public void TeacherDiscipline_Create_Valid_SetsFields()
    {
        var userId = UserId.New();
        var disciplineId = DisciplineId.New();

        var td = TeacherDiscipline.Create(userId, disciplineId);

        td.UserId.ShouldBe(userId);
        td.DisciplineId.ShouldBe(disciplineId);
    }

    [Fact]
    public void TeacherDiscipline_EmptyUser_Throws()
        => Should.Throw<DomainException>(() => TeacherDiscipline.Create(default, DisciplineId.New()));

    [Fact]
    public void TeacherDiscipline_EmptyDiscipline_Throws()
        => Should.Throw<DomainException>(() => TeacherDiscipline.Create(UserId.New(), default));

    [Fact]
    public void TeacherGenre_Create_Valid_SetsFields()
    {
        var userId = UserId.New();
        var genreId = GenreId.New();

        var tg = TeacherGenre.Create(userId, genreId);

        tg.UserId.ShouldBe(userId);
        tg.GenreId.ShouldBe(genreId);
    }

    [Fact]
    public void TeacherGenre_EmptyUser_Throws()
        => Should.Throw<DomainException>(() => TeacherGenre.Create(default, GenreId.New()));

    [Fact]
    public void TeacherGenre_EmptyGenre_Throws()
        => Should.Throw<DomainException>(() => TeacherGenre.Create(UserId.New(), default));
}
