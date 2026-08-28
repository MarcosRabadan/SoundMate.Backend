using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Teaching;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Configurations;

internal sealed class TaughtDisciplineConfiguration : IEntityTypeConfiguration<TaughtDiscipline>
{
    public void Configure(EntityTypeBuilder<TaughtDiscipline> builder)
    {
        builder.ToTable("TaughtDisciplines");

        builder.HasKey(td => td.Id);

        builder.Property(td => td.Id)
            .HasConversion(id => id.Value, value => TaughtDisciplineId.From(value))
            .ValueGeneratedNever();

        builder.Property(td => td.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(td => td.DisciplineId)
            .HasConversion(id => id.Value, value => DisciplineId.From(value))
            .IsRequired();

        // A teacher cannot list the same discipline twice.
        builder.HasIndex(td => new { td.UserId, td.DisciplineId }).IsUnique();

        builder.HasIndex(td => td.DisciplineId);
    }
}
