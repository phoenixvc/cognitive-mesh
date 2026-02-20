using CognitiveMesh.BusinessApplications.NISTCompliance.Ports;
using CognitiveMesh.BusinessApplications.NISTCompliance.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CognitiveMesh.BusinessApplications.NISTCompliance.Infrastructure;

/// <summary>
/// Extension methods for registering NIST AI RMF compliance services
/// in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds NIST AI RMF compliance services to the specified
    /// <see cref="IServiceCollection"/>, registering the in-memory
    /// <see cref="NISTComplianceService"/> as the implementation for
    /// <see cref="INISTComplianceServicePort"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddNISTComplianceServices(this IServiceCollection services)
    {
        services.AddSingleton<INISTComplianceServicePort, NISTComplianceService>();
        return services;
    }
}
