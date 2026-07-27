using SoundMate.Domain.Academies;
using SoundMate.Domain.Common;
using SoundMate.Domain.Users;

namespace SoundMate.Domain.Teaching;

/// <summary>
/// A single rating a user gives a teacher within an academy (1–5 stars + optional comment).
/// The star count shown for a teacher is the AVERAGE of these per (teacher, academy), never a
/// hand-set field. A user cannot review themselves, and stars are always within 1–5.
/// </summary>
public sealed class TeacherReview : AggregateRoot<TeacherReviewId>
{
    public const int MinStars = 1;
    public const int MaxStars = 5;

    public UserId TeacherUserId { get; private set; }
    public UserId ReviewerUserId { get; private set; }
    public AcademyId AcademyId { get; private set; }
    public int Stars { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private TeacherReview() { }

    private TeacherReview(
        TeacherReviewId id,
        UserId teacherUserId,
        UserId reviewerUserId,
        AcademyId academyId,
        int stars,
        string? comment) : base(id)
    {
        TeacherUserId = teacherUserId;
        ReviewerUserId = reviewerUserId;
        AcademyId = academyId;
        Stars = stars;
        Comment = Normalize(comment);
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static TeacherReview Create(
        UserId teacherUserId,
        UserId reviewerUserId,
        AcademyId academyId,
        int stars,
        string? comment = null)
    {
        Guard.NotEmpty(teacherUserId, "Teacher");
        Guard.NotEmpty(reviewerUserId, "Reviewer");
        Guard.NotEmpty(academyId, "Academy");

        if (teacherUserId == reviewerUserId)
            throw new DomainException("A user cannot review themselves.");

        return new TeacherReview(
            TeacherReviewId.New(),
            teacherUserId,
            reviewerUserId,
            academyId,
            Guard.InRange(stars, MinStars, MaxStars, "Stars"),
            comment);
    }

    /// <summary>Updates an existing review (a reviewer keeps one review per teacher/academy).</summary>
    public void Update(int stars, string? comment)
    {
        Stars = Guard.InRange(stars, MinStars, MaxStars, "Stars");
        Comment = Normalize(comment);
    }

    private static string? Normalize(string? text)
        => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
