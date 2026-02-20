using CognitiveMesh.AgencyLayer.RealTime.Adapters;
using CognitiveMesh.AgencyLayer.RealTime.Hubs;
using CognitiveMesh.AgencyLayer.RealTime.Ports;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace CognitiveMesh.AgencyLayer.RealTime.Infrastructure;

/// <summary>
/// Extension methods for registering Cognitive Mesh real-time services and mapping hub endpoints.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Cognitive Mesh real-time notification services into the dependency injection container.
    /// This registers <see cref="SignalRNotificationAdapter"/> as a singleton implementation of
    /// <see cref="IRealTimeNotificationPort"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddCognitiveMeshRealTime(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<SignalRNotificationAdapter>();
        services.AddSingleton<IRealTimeNotificationPort>(sp =>
            sp.GetRequiredService<SignalRNotificationAdapter>());

        return services;
    }

    /// <summary>
    /// Maps the Cognitive Mesh SignalR hub endpoint to the specified route builder.
    /// The hub is accessible at <c>/hubs/cognitive-mesh</c>.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder to map the hub to.</param>
    /// <returns>The hub endpoint convention builder for further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="endpoints"/> is null.</exception>
    public static HubEndpointConventionBuilder MapCognitiveMeshHubs(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapHub<CognitiveMeshHub>("/hubs/cognitive-mesh");
    }
}
