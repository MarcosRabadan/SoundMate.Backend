using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoundMate.Domain.Genres;
using SoundMate.Domain.Teaching;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Configurations;

internal sealed class TeacherGenreConfiguration : IEntityTypeConfiguration<TeacherGenre>
{
    public void Configure(EntityTypeBuilder<TeacherGenre> builder)
    {
        builder.ToTable("TeacherGenres");

        builder.HasKey(tg => tg.Id);

        builder.Property(tg => tg.Id)
            .HasConversion(id => id.Value, value => TeacherGenreId.From(value))
            .ValueGeneratedNever();

        builder.Property(tg => tg.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(tg => tg.GenreId)
            .HasConversion(id => id.Value, value => GenreId.From(value))
            .IsRequired();

        // A teacher cannot list the same genre twice.
        builder.HasIndex(tg => new { tg.UserId, tg.GenreId }).IsUnique();

        builder.HasIndex(tg => tg.GenreId);
    }
}
