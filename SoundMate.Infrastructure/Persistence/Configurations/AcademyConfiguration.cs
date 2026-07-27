using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Configurations;

internal sealed class AcademyConfiguration : IEntityTypeConfiguration<Academy>
{
    public void Configure(EntityTypeBuilder<Academy> builder)
    {
        builder.ToTable("Academies");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => AcademyId.From(value))
            .ValueGeneratedNever();

        builder.Property(a => a.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Type)
            .HasConversion<int>();

        builder.Property(a => a.Slug)
            .HasConversion(slug => slug.Value, value => Slug.Create(value))
            .HasMaxLength(Slug.MaxLength)
            .IsRequired();

        builder.HasIndex(a => a.Slug).IsUnique();

        // Owner referenced by identity. Indexed to look up "academies of X", but with no
        // enforced FK between aggregates (keeps a future DB-per-service split easy).
        builder.Property(a => a.OwnerId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.HasIndex(a => a.OwnerId);

        builder.Property(a => a.Plan)
            .HasConversion<int>();

        builder.Property(a => a.Status)
            .HasConversion<int>();
    }
}
