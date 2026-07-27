using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Teaching;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Configurations;

internal sealed class TeacherReviewConfiguration : IEntityTypeConfiguration<TeacherReview>
{
    public void Configure(EntityTypeBuilder<TeacherReview> builder)
    {
        // Stars are constrained to 1-5 at the database level.
        builder.ToTable("TeacherReviews", t =>
            t.HasCheckConstraint("CK_TeacherReviews_Stars", "[Stars] >= 1 AND [Stars] <= 5"));

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => TeacherReviewId.From(value))
            .ValueGeneratedNever();

        builder.Property(r => r.TeacherUserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(r => r.ReviewerUserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(r => r.AcademyId)
            .HasConversion(id => id.Value, value => AcademyId.From(value))
            .IsRequired();

        builder.Property(r => r.Stars);

        builder.Property(r => r.Comment)
            .HasMaxLength(2000);

        // Average rating is computed per (teacher, academy) — index the pair.
        builder.HasIndex(r => new { r.TeacherUserId, r.AcademyId });

        // One review per reviewer, teacher and academy (update it, don't duplicate).
        builder.HasIndex(r => new { r.ReviewerUserId, r.TeacherUserId, r.AcademyId }).IsUnique();
    }
}
