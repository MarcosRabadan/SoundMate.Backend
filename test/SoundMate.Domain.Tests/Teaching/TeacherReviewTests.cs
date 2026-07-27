using Shouldly;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Common;
using SoundMate.Domain.Teaching;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Tests.Teaching;

public class TeacherReviewTests
{
    private static TeacherReview AReview(int stars = 5)
        => TeacherReview.Create(UserId.New(), UserId.New(), AcademyId.New(), stars, "Great");

    [Fact]
    public void Create_Valid_SetsFields()
    {
        var teacher = UserId.New();
        var reviewer = UserId.New();
        var academy = AcademyId.New();

        var review = TeacherReview.Create(teacher, reviewer, academy, 4, "  Good teacher  ");

        review.TeacherUserId.ShouldBe(teacher);
        review.ReviewerUserId.ShouldBe(reviewer);
        review.AcademyId.ShouldBe(academy);
        review.Stars.ShouldBe(4);
        review.Comment.ShouldBe("Good teacher");   // trimmed
        review.CreatedAtUtc.ShouldNotBe(default);
    }

    [Fact]
    public void Create_NullComment_IsAllowed()
        => TeacherReview.Create(UserId.New(), UserId.New(), AcademyId.New(), 3).Comment.ShouldBeNull();

    [Fact]
    public void Create_SelfReview_Throws()
    {
        var user = UserId.New();
        Should.Throw<DomainException>(() => TeacherReview.Create(user, user, AcademyId.New(), 5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Create_StarsOutOfRange_Throws(int stars)
        => Should.Throw<DomainException>(
            () => TeacherReview.Create(UserId.New(), UserId.New(), AcademyId.New(), stars));

    [Fact]
    public void Create_EmptyTeacher_Throws()
        => Should.Throw<DomainException>(
            () => TeacherReview.Create(default, UserId.New(), AcademyId.New(), 5));

    [Fact]
    public void Create_EmptyAcademy_Throws()
        => Should.Throw<DomainException>(
            () => TeacherReview.Create(UserId.New(), UserId.New(), default, 5));

    [Fact]
    public void Update_Valid_ChangesStarsAndComment()
    {
        var review = AReview();
        review.Update(2, "Changed my mind");
        review.Stars.ShouldBe(2);
        review.Comment.ShouldBe("Changed my mind");
    }

    [Fact]
    public void Update_StarsOutOfRange_Throws()
        => Should.Throw<DomainException>(() => AReview().Update(99, null));
}
