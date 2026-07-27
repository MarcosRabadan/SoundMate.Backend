using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Memberships;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Configurations;

internal sealed class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("Memberships");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasConversion(id => id.Value, value => MembershipId.From(value))
            .ValueGeneratedNever();

        builder.Property(m => m.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(m => m.AcademyId)
            .HasConversion(id => id.Value, value => AcademyId.From(value))
            .IsRequired();

        builder.Property(m => m.Role)
            .HasConversion<int>();

        builder.Property(m => m.Status)
            .HasConversion<int>();

        // A person cannot hold the same role twice in the same academy.
        builder.HasIndex(m => new { m.UserId, m.AcademyId, m.Role }).IsUnique();

        // Frequent lookups: "academies of this user" / "members of this academy".
        builder.HasIndex(m => m.AcademyId);
    }
}
