using AgencyLayer.RoadmapCrew.Engines;
using AgencyLayer.RoadmapCrew.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace AgencyLayer.RoadmapCrew.Infrastructure;

/// <summary>
/// Provides extension methods for registering RoadmapCrew services
/// with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the RoadmapCrew agent team services.
    /// Requires <see cref="AgencyLayer.Agents.Ports.ISpecializedAgentPort"/> to be registered separately.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRoadmapCrewServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IRoadmapCrewPort, RoadmapCrewEngine>();

        return services;
    }
}
