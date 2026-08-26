using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoundMate.Application.Abstractions.Persistence;
using SoundMate.Application.Abstractions.Security;
using SoundMate.Infrastructure.Persistence;
using SoundMate.Infrastructure.Persistence.Repositories;
using SoundMate.Infrastructure.Security;

namespace SoundMate.Infrastructure;

/// <summary>
/// Registers the Infrastructure layer in the DI container. Lives here because the repository
/// implementations are <c>internal</c> and can only be wired from their own assembly.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SoundMateDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Stateless and thread-safe: the salt is generated per call and the iteration count
        // travels inside each stored hash.
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAcademyRepository, AcademyRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IDisciplineRepository, DisciplineRepository>();
        services.AddScoped<IUserDisciplineRepository, UserDisciplineRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IUserEducationRepository, UserEducationRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<ITeacherDisciplineRepository, TeacherDisciplineRepository>();
        services.AddScoped<ITeacherGenreRepository, TeacherGenreRepository>();
        services.AddScoped<ITeacherReviewRepository, TeacherReviewRepository>();

        return services;
    }
}
