using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoundMate.Domain.Disciplines;

namespace SoundMate.Infrastructure.Persistence.Configurations;

internal sealed class DisciplineConfiguration : IEntityTypeConfiguration<Discipline>
{
    public void Configure(EntityTypeBuilder<Discipline> builder)
    {
        builder.ToTable("Disciplines");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasConversion(id => id.Value, value => DisciplineId.From(value))
            .ValueGeneratedNever();

        builder.Property(d => d.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(d => d.Name).IsUnique();

        builder.Property(d => d.Category)
            .HasConversion<int>();

        builder.Property(d => d.IsActive);

        Seed(builder);
    }

    // Stable GUIDs (grouped by category) so HasData is reproducible across migrations.
    private static void Seed(EntityTypeBuilder<Discipline> builder)
    {
        builder.HasData(
            // Keyboard
            Make("10000000-0000-0000-0000-000000000001", "Piano", DisciplineCategory.Keyboard),
            Make("10000000-0000-0000-0000-000000000002", "Keyboard", DisciplineCategory.Keyboard),
            Make("10000000-0000-0000-0000-000000000003", "Organ", DisciplineCategory.Keyboard),
            Make("10000000-0000-0000-0000-000000000004", "Accordion", DisciplineCategory.Keyboard),

            // Plucked strings
            Make("20000000-0000-0000-0000-000000000001", "Classical guitar", DisciplineCategory.PluckedString),
            Make("20000000-0000-0000-0000-000000000002", "Acoustic guitar", DisciplineCategory.PluckedString),
            Make("20000000-0000-0000-0000-000000000003", "Electric guitar", DisciplineCategory.PluckedString),
            Make("20000000-0000-0000-0000-000000000004", "Flamenco guitar", DisciplineCategory.PluckedString),
            Make("20000000-0000-0000-0000-000000000005", "Electric bass", DisciplineCategory.PluckedString),
            Make("20000000-0000-0000-0000-000000000006", "Ukulele", DisciplineCategory.PluckedString),
            Make("20000000-0000-0000-0000-000000000007", "Banjo", DisciplineCategory.PluckedString),
            Make("20000000-0000-0000-0000-000000000008", "Mandolin", DisciplineCategory.PluckedString),
            Make("20000000-0000-0000-0000-000000000009", "Harp", DisciplineCategory.PluckedString),
            Make("20000000-0000-0000-0000-00000000000a", "Bandurria", DisciplineCategory.PluckedString),
            Make("20000000-0000-0000-0000-00000000000b", "Lute", DisciplineCategory.PluckedString),

            // Bowed strings
            Make("30000000-0000-0000-0000-000000000001", "Violin", DisciplineCategory.BowedString),
            Make("30000000-0000-0000-0000-000000000002", "Viola", DisciplineCategory.BowedString),
            Make("30000000-0000-0000-0000-000000000003", "Cello", DisciplineCategory.BowedString),
            Make("30000000-0000-0000-0000-000000000004", "Double bass", DisciplineCategory.BowedString),

            // Woodwind
            Make("40000000-0000-0000-0000-000000000001", "Flute", DisciplineCategory.Woodwind),
            Make("40000000-0000-0000-0000-000000000002", "Recorder", DisciplineCategory.Woodwind),
            Make("40000000-0000-0000-0000-000000000003", "Clarinet", DisciplineCategory.Woodwind),
            Make("40000000-0000-0000-0000-000000000004", "Saxophone", DisciplineCategory.Woodwind),
            Make("40000000-0000-0000-0000-000000000005", "Oboe", DisciplineCategory.Woodwind),
            Make("40000000-0000-0000-0000-000000000006", "Bassoon", DisciplineCategory.Woodwind),

            // Brass
            Make("50000000-0000-0000-0000-000000000001", "Trumpet", DisciplineCategory.Brass),
            Make("50000000-0000-0000-0000-000000000002", "Trombone", DisciplineCategory.Brass),
            Make("50000000-0000-0000-0000-000000000003", "French horn", DisciplineCategory.Brass),
            Make("50000000-0000-0000-0000-000000000004", "Tuba", DisciplineCategory.Brass),
            Make("50000000-0000-0000-0000-000000000005", "Euphonium", DisciplineCategory.Brass),
            Make("50000000-0000-0000-0000-000000000006", "Cornet", DisciplineCategory.Brass),
            Make("50000000-0000-0000-0000-000000000007", "Flugelhorn", DisciplineCategory.Brass),

            // Percussion
            Make("60000000-0000-0000-0000-000000000001", "Drum kit", DisciplineCategory.Percussion),
            Make("60000000-0000-0000-0000-000000000002", "Snare drum", DisciplineCategory.Percussion),
            Make("60000000-0000-0000-0000-000000000003", "Cajón", DisciplineCategory.Percussion),
            Make("60000000-0000-0000-0000-000000000004", "Congas", DisciplineCategory.Percussion),
            Make("60000000-0000-0000-0000-000000000005", "Bongos", DisciplineCategory.Percussion),
            Make("60000000-0000-0000-0000-000000000006", "Djembe", DisciplineCategory.Percussion),
            Make("60000000-0000-0000-0000-000000000007", "Timpani", DisciplineCategory.Percussion),
            Make("60000000-0000-0000-0000-000000000008", "Mallet percussion", DisciplineCategory.Percussion),

            // Voice
            Make("70000000-0000-0000-0000-000000000001", "Singing", DisciplineCategory.Voice),
            Make("70000000-0000-0000-0000-000000000002", "Classical voice", DisciplineCategory.Voice),

            // Music theory (non-instrument subjects)
            Make("80000000-0000-0000-0000-000000000001", "Music theory", DisciplineCategory.MusicTheory),
            Make("80000000-0000-0000-0000-000000000002", "Harmony", DisciplineCategory.MusicTheory),
            Make("80000000-0000-0000-0000-000000000003", "Composition", DisciplineCategory.MusicTheory),
            Make("80000000-0000-0000-0000-000000000004", "Music analysis", DisciplineCategory.MusicTheory),
            Make("80000000-0000-0000-0000-000000000005", "Music history", DisciplineCategory.MusicTheory),
            Make("80000000-0000-0000-0000-000000000006", "Improvisation", DisciplineCategory.MusicTheory));
    }

    private static Discipline Make(string id, string name, DisciplineCategory category)
        => new(DisciplineId.From(new Guid(id)), name, category);
}
