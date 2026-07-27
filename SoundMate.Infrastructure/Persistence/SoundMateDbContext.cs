using Microsoft.EntityFrameworkCore;
using SoundMate.Domain.Academies;
using SoundMate.Domain.Disciplines;
using SoundMate.Domain.Genres;
using SoundMate.Domain.Memberships;
using SoundMate.Domain.Teaching;
using SoundMate.Domain.Users;

namespace SoundMate.Infrastructure.Persistence;

public sealed class SoundMateDbContext : DbContext
{
    public SoundMateDbContext(DbContextOptions<SoundMateDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Academy> Academies => Set<Academy>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Discipline> Disciplines => Set<Discipline>();
    public DbSet<UserDiscipline> UserDisciplines => Set<UserDiscipline>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserEducation> UserEducations => Set<UserEducation>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<TeacherDiscipline> TeacherDisciplines => Set<TeacherDiscipline>();
    public DbSet<TeacherGenre> TeacherGenres => Set<TeacherGenre>();
    public DbSet<TeacherReview> TeacherReviews => Set<TeacherReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Enables the citext type used for case-insensitive email uniqueness.
        modelBuilder.HasPostgresExtension("citext");

        // Each aggregate's mapping lives in its own IEntityTypeConfiguration under Configurations/.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SoundMateDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
