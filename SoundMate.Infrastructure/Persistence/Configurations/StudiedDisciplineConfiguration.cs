using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Configurations;

internal sealed class StudiedDisciplineConfiguration : IEntityTypeConfiguration<StudiedDiscipline>
{
    public void Configure(EntityTypeBuilder<StudiedDiscipline> builder)
    {
        builder.ToTable("StudiedDisciplines");

        builder.HasKey(sd => sd.Id);

        builder.Property(sd => sd.Id)
            .HasConversion(id => id.Value, value => StudiedDisciplineId.From(value))
            .ValueGeneratedNever();

        builder.Property(sd => sd.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(sd => sd.DisciplineId)
            .HasConversion(id => id.Value, value => DisciplineId.From(value))
            .IsRequired();

        builder.Property(sd => sd.Level)
            .HasConversion<int>();

        // A user cannot list the same discipline twice.
        builder.HasIndex(sd => new { sd.UserId, sd.DisciplineId }).IsUnique();

        // "Who studies this discipline" lookups.
        builder.HasIndex(sd => sd.DisciplineId);
    }
}
