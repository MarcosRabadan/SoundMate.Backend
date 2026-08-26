using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .ValueGeneratedNever();

        // Stored as citext (case-insensitive text) so the unique index treats ana@ and ANA@
        // as the same email. Postgres is case-sensitive by default, unlike SQL Server.
        builder.Property(u => u.Email)
            .HasConversion(email => email.Value, value => Email.Create(value))
            .HasColumnType("citext")
            .IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.Phone)
            .HasMaxLength(30);

        builder.Property(u => u.Status)
            .HasConversion<int>();

        // Computed from DeletedAtUtc, so there is nothing to store. Spelled out rather than left
        // to convention: a read-only property that EF decides to map is a confusing migration.
        builder.Ignore(u => u.IsDeleted);

        // Partial index: only the rows that are still alive. Every normal query wants those, and
        // Postgres keeps the index the size of the live set rather than the whole table.
        builder.HasIndex(u => u.DeletedAtUtc)
            .HasFilter("\"DeletedAtUtc\" IS NULL");
    }
}
