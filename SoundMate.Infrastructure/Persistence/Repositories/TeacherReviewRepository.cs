using Microsoft.EntityFrameworkCore;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Teaching;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Repositories;

internal sealed class TeacherReviewRepository : ITeacherReviewRepository
{
    private readonly SoundMateDbContext _context;

    public TeacherReviewRepository(SoundMateDbContext context) => _context = context;

    public Task<TeacherReview?> GetByIdAsync(TeacherReviewId id, CancellationToken cancellationToken = default)
        => _context.TeacherReviews.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<TeacherReview?> GetAsync(UserId reviewerUserId, UserId teacherUserId, AcademyId academyId, CancellationToken cancellationToken = default)
        => _context.TeacherReviews.FirstOrDefaultAsync(
            r => r.ReviewerUserId == reviewerUserId && r.TeacherUserId == teacherUserId && r.AcademyId == academyId,
            cancellationToken);

    public async Task<IReadOnlyList<TeacherReview>> ListByTeacherAsync(UserId teacherUserId, AcademyId academyId, CancellationToken cancellationToken = default)
        => await _context.TeacherReviews
            .Where(r => r.TeacherUserId == teacherUserId && r.AcademyId == academyId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<RatingSummary> GetRatingAsync(UserId teacherUserId, AcademyId academyId, CancellationToken cancellationToken = default)
    {
        var reviews = _context.TeacherReviews
            .Where(r => r.TeacherUserId == teacherUserId && r.AcademyId == academyId);

        var count = await reviews.CountAsync(cancellationToken);
        if (count == 0)
            return new RatingSummary(0, 0);

        var average = await reviews.AverageAsync(r => r.Stars, cancellationToken);
        return new RatingSummary(average, count);
    }

    public async Task AddAsync(TeacherReview review, CancellationToken cancellationToken = default)
        => await _context.TeacherReviews.AddAsync(review, cancellationToken);

    public void Update(TeacherReview review) => _context.TeacherReviews.Update(review);

    public void Remove(TeacherReview review) => _context.TeacherReviews.Remove(review);
}
