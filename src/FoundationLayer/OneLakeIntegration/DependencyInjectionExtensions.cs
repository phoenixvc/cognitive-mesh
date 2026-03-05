using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CognitiveMesh.FoundationLayer.EnterpriseConnectors;

namespace CognitiveMesh.FoundationLayer.OneLakeIntegration;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds OneLake integration services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOneLakeIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        // Register the OneLake integration manager as a scoped service
        services.AddScoped<OneLakeIntegrationManager>(sp =>
        {
            var connectionString = configuration.GetConnectionString("OneLake") ?? 
                throw new InvalidOperationException("OneLake connection string is not configured");
            
            var logger = sp.GetRequiredService<ILogger<OneLakeIntegrationManager>>();
            var featureFlagManager = sp.GetRequiredService<IFeatureFlagManager>();
            
            return new OneLakeIntegrationManager(connectionString, logger, featureFlagManager);
        });
        
        return services;
    }
}
