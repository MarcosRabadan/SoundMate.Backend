using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoundMate.Domain.Genres;

namespace SoundMate.Infrastructure.Persistence.Configurations;

internal sealed class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.ToTable("Genres");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .HasConversion(id => id.Value, value => GenreId.From(value))
            .ValueGeneratedNever();

        builder.Property(g => g.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(g => g.Name).IsUnique();

        builder.Property(g => g.IsActive);

        Seed(builder);
    }

    // Stable GUIDs (prefix 9-) so HasData is reproducible across migrations.
    private static void Seed(EntityTypeBuilder<Genre> builder)
    {
        builder.HasData(
            Make("90000000-0000-0000-0000-000000000001", "Classical"),
            Make("90000000-0000-0000-0000-000000000002", "Opera"),
            Make("90000000-0000-0000-0000-000000000003", "Baroque"),
            Make("90000000-0000-0000-0000-000000000004", "Contemporary classical"),
            Make("90000000-0000-0000-0000-000000000005", "Jazz"),
            Make("90000000-0000-0000-0000-000000000006", "Blues"),
            Make("90000000-0000-0000-0000-000000000007", "Swing"),
            Make("90000000-0000-0000-0000-000000000008", "Bossa nova"),
            Make("90000000-0000-0000-0000-000000000009", "Rock"),
            Make("90000000-0000-0000-0000-00000000000a", "Hard rock"),
            Make("90000000-0000-0000-0000-00000000000b", "Metal"),
            Make("90000000-0000-0000-0000-00000000000c", "Punk"),
            Make("90000000-0000-0000-0000-00000000000d", "Indie"),
            Make("90000000-0000-0000-0000-00000000000e", "Pop"),
            Make("90000000-0000-0000-0000-00000000000f", "Funk"),
            Make("90000000-0000-0000-0000-000000000010", "Soul"),
            Make("90000000-0000-0000-0000-000000000011", "R&B"),
            Make("90000000-0000-0000-0000-000000000012", "Hip hop"),
            Make("90000000-0000-0000-0000-000000000013", "Rap"),
            Make("90000000-0000-0000-0000-000000000014", "Reggae"),
            Make("90000000-0000-0000-0000-000000000015", "Ska"),
            Make("90000000-0000-0000-0000-000000000016", "Reggaeton"),
            Make("90000000-0000-0000-0000-000000000017", "Electronic"),
            Make("90000000-0000-0000-0000-000000000018", "House"),
            Make("90000000-0000-0000-0000-000000000019", "Techno"),
            Make("90000000-0000-0000-0000-00000000001a", "Folk"),
            Make("90000000-0000-0000-0000-00000000001b", "Country"),
            Make("90000000-0000-0000-0000-00000000001c", "Flamenco"),
            Make("90000000-0000-0000-0000-00000000001d", "Latin"),
            Make("90000000-0000-0000-0000-00000000001e", "Salsa"),
            Make("90000000-0000-0000-0000-00000000001f", "Tango"),
            Make("90000000-0000-0000-0000-000000000020", "Gospel"),
            Make("90000000-0000-0000-0000-000000000021", "Film music"));
    }

    private static Genre Make(string id, string name)
        => new(GenreId.From(new Guid(id)), name);
}
