using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoundMate.Application.Abstractions.Agendia;

namespace SoundMate.Infrastructure.Agendia;

/// <summary>
/// Wires the Agendia integration. Separate from <see cref="DependencyInjection.AddInfrastructure"/>
/// because it needs configuration rather than just a connection string, and because a deployment
/// that does not talk to Agendia can simply not call it.
/// </summary>
public static class AgendiaDependencyInjection
{
    public static IServiceCollection AddAgendiaIntegration(this IServiceCollection services,
                                                           IConfiguration configuration)
    {
        var section = configuration.GetSection(AgendiaOptions.SectionName);
        services.Configure<AgendiaOptions>(section);

        // Fail at startup rather than on the first call: a missing base address would otherwise
        // surface as a confusing "invalid request URI" the first time someone books something.
        var baseUrl = section["BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                $"{AgendiaOptions.SectionName}:BaseUrl is not configured. It is the address of the " +
                "Agendia API, e.g. https://localhost:7097.");
        }

        services.AddHttpClient(AgendiaHttpClients.Name, http => http.BaseAddress = new Uri(baseUrl));

        // Singleton so the service token is cached across requests: Agendia issues short-lived
        // tokens with no refresh, so re-authenticating per call would double every round trip.
        services.AddSingleton<AgendiaServiceTokenProvider>();

        services.AddHttpClient<IAgendiaClient, AgendiaClient>(http => http.BaseAddress = new Uri(baseUrl));

        return services;
    }
}
