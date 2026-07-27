using Shouldly;
using SoundMate.Domain.Common;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Tests.Users;

public class UserEducationTests
{
    [Fact]
    public void Create_Valid_SetsFields()
    {
        var userId = UserId.New();
        var education = UserEducation.Create(userId, "  Bachelor's in Music  ", "  Conservatory  ", 2015, 2019, "  notes  ");

        education.UserId.ShouldBe(userId);
        education.Title.ShouldBe("Bachelor's in Music");   // trimmed
        education.Institution.ShouldBe("Conservatory");    // trimmed
        education.StartYear.ShouldBe(2015);
        education.EndYear.ShouldBe(2019);
        education.Description.ShouldBe("notes");
    }

    [Fact]
    public void Create_EmptyUser_Throws()
        => Should.Throw<DomainException>(() => UserEducation.Create(default, "Title"));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyTitle_Throws(string? title)
        => Should.Throw<DomainException>(() => UserEducation.Create(UserId.New(), title!));

    [Fact]
    public void Create_EndBeforeStart_Throws()
        => Should.Throw<DomainException>(
            () => UserEducation.Create(UserId.New(), "Title", startYear: 2020, endYear: 2018));

    [Fact]
    public void Create_Ongoing_NullEnd_IsAllowed()
    {
        var education = UserEducation.Create(UserId.New(), "Title", startYear: 2023);
        education.EndYear.ShouldBeNull();
    }

    [Fact]
    public void Create_SameStartAndEndYear_IsAllowed()
    {
        var education = UserEducation.Create(UserId.New(), "Workshop", startYear: 2021, endYear: 2021);
        education.EndYear.ShouldBe(2021);
    }

    [Fact]
    public void Update_EndBeforeStart_Throws()
    {
        var education = UserEducation.Create(UserId.New(), "Title");
        Should.Throw<DomainException>(() => education.Update("Title", null, 2020, 2018, null));
    }
}
