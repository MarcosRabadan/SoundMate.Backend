using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Configurations;

internal sealed class UserEducationConfiguration : IEntityTypeConfiguration<UserEducation>
{
    public void Configure(EntityTypeBuilder<UserEducation> builder)
    {
        // If both years are set, the end cannot be before the start.
        builder.ToTable("UserEducations", t =>
            t.HasCheckConstraint(
                "CK_UserEducations_Years",
                "\"StartYear\" IS NULL OR \"EndYear\" IS NULL OR \"EndYear\" >= \"StartYear\""));

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => UserEducationId.From(value))
            .ValueGeneratedNever();

        builder.Property(e => e.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.HasIndex(e => e.UserId);

        builder.Property(e => e.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Institution)
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);
    }
}
