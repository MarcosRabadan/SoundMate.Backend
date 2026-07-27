using SoundMate.Domain.Academies;
using SoundMate.Domain.Teaching;
using SoundMate.Domain.Users;

namespace SoundMate.Application.Abstractions.Persistence;

public interface ITeacherReviewRepository
{
    Task<TeacherReview?> GetByIdAsync(TeacherReviewId id, CancellationToken cancellationToken = default);

    /// <summary>The single review a reviewer left for a teacher in an academy, if any.</summary>
    Task<TeacherReview?> GetAsync(UserId reviewerUserId, UserId teacherUserId, AcademyId academyId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeacherReview>> ListByTeacherAsync(UserId teacherUserId, AcademyId academyId, CancellationToken cancellationToken = default);

    /// <summary>Average stars and review count for a teacher in an academy, computed on the fly.</summary>
    Task<RatingSummary> GetRatingAsync(UserId teacherUserId, AcademyId academyId, CancellationToken cancellationToken = default);

    Task AddAsync(TeacherReview review, CancellationToken cancellationToken = default);

    void Update(TeacherReview review);

    void Remove(TeacherReview review);
}
