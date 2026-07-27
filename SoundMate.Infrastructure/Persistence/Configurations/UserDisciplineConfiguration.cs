using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Configurations;

internal sealed class UserDisciplineConfiguration : IEntityTypeConfiguration<UserDiscipline>
{
    public void Configure(EntityTypeBuilder<UserDiscipline> builder)
    {
        builder.ToTable("UserDisciplines");

        builder.HasKey(ud => ud.Id);

        builder.Property(ud => ud.Id)
            .HasConversion(id => id.Value, value => UserDisciplineId.From(value))
            .ValueGeneratedNever();

        builder.Property(ud => ud.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(ud => ud.DisciplineId)
            .HasConversion(id => id.Value, value => DisciplineId.From(value))
            .IsRequired();

        builder.Property(ud => ud.Level)
            .HasConversion<int>();

        // A user cannot list the same discipline twice.
        builder.HasIndex(ud => new { ud.UserId, ud.DisciplineId }).IsUnique();

        // "Who studies this discipline" lookups.
        builder.HasIndex(ud => ud.DisciplineId);
    }
}
