using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SoundMate.Application.Academies;
using SoundMate.Application.Disciplines;
using SoundMate.Application.Users;

namespace SoundMate.Application;

/// <summary>
/// Registers the Application layer: validators, mapping profiles and the use-case services.
/// Lives here because the service implementations are <c>internal</c> and can only be wired from
/// their own assembly.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Scans this assembly, so a new validator is picked up by existing: adding one is a single
        // file, never a file plus a registration somebody forgets.
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        // Mapping needs nothing registered: it is static extension methods (see UserMapper).
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IAcademyService, AcademyService>();
        services.AddScoped<IStudiedDisciplineService, StudiedDisciplineService>();

        // The catalogue: read-only, and the reason the rest is usable at all — a discipline id
        // is a seeded GUID nobody can guess.
        services.AddScoped<IDisciplineService, DisciplineService>();

        return services;
    }
}
