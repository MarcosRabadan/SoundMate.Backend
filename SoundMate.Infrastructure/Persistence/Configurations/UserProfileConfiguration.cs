using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Configurations;

internal sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => UserProfileId.From(value))
            .ValueGeneratedNever();

        builder.Property(p => p.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        // One profile per user.
        builder.HasIndex(p => p.UserId).IsUnique();

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.AvatarUrl)
            .HasMaxLength(500);
    }
}
