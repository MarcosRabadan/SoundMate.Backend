using Shouldly;
using SoundMate.Domain.Common;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Genres;
using SoundMate.Domain.Teaching;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Tests.Teaching;

public class TeachingSpecialtyTests
{
    [Fact]
    public void TaughtDiscipline_Create_Valid_SetsFields()
    {
        var userId = UserId.New();
        var disciplineId = DisciplineId.New();

        var td = TaughtDiscipline.Create(userId, disciplineId);

        td.UserId.ShouldBe(userId);
        td.DisciplineId.ShouldBe(disciplineId);
    }

    [Fact]
    public void TaughtDiscipline_EmptyUser_Throws()
        => Should.Throw<DomainException>(() => TaughtDiscipline.Create(default, DisciplineId.New()));

    [Fact]
    public void TaughtDiscipline_EmptyDiscipline_Throws()
        => Should.Throw<DomainException>(() => TaughtDiscipline.Create(UserId.New(), default));

    [Fact]
    public void TaughtGenre_Create_Valid_SetsFields()
    {
        var userId = UserId.New();
        var genreId = GenreId.New();

        var tg = TaughtGenre.Create(userId, genreId);

        tg.UserId.ShouldBe(userId);
        tg.GenreId.ShouldBe(genreId);
    }

    [Fact]
    public void TaughtGenre_EmptyUser_Throws()
        => Should.Throw<DomainException>(() => TaughtGenre.Create(default, GenreId.New()));

    [Fact]
    public void TaughtGenre_EmptyGenre_Throws()
        => Should.Throw<DomainException>(() => TaughtGenre.Create(UserId.New(), default));
}
