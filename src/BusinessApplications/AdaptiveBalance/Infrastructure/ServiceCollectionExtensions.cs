using CognitiveMesh.BusinessApplications.AdaptiveBalance.Ports;
using CognitiveMesh.BusinessApplications.AdaptiveBalance.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CognitiveMesh.BusinessApplications.AdaptiveBalance.Infrastructure;

/// <summary>
/// Extension methods for registering Adaptive Balance services
/// in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Adaptive Balance services to the specified
    /// <see cref="IServiceCollection"/>, registering the in-memory
    /// <see cref="AdaptiveBalanceService"/> as the implementation for
    /// <see cref="IAdaptiveBalanceServicePort"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddAdaptiveBalanceServices(this IServiceCollection services)
    {
        services.AddSingleton<IAdaptiveBalanceServicePort, AdaptiveBalanceService>();
        return services;
    }
}
