using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using global::FoundationLayer.EnterpriseConnectors;

namespace CognitiveMesh.FoundationLayer.EnterpriseConnectors;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds enterprise connector services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEnterpriseConnectors(this IServiceCollection services, IConfiguration configuration)
    {
        // Register feature flag manager as a singleton since it only depends on IConfiguration
        services.AddSingleton<IFeatureFlagManager, FeatureFlagManager>();
        
        // Add other enterprise connector services here
        
        return services;
    }
}
