using Microsoft.Extensions.DependencyInjection;

using CognitiveMesh.AgencyLayer.SelfHealing.Engines;
using CognitiveMesh.AgencyLayer.SelfHealing.Ports;

namespace CognitiveMesh.AgencyLayer.SelfHealing.Extensions;

/// <summary>
/// Extension methods for registering self-healing services with the dependency injection container.
/// </summary>
public static class SelfHealingServiceExtensions
{
    /// <summary>
    /// Adds self-healing services, including the remediation policy decision engine,
    /// to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddSelfHealingServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IRemediationDecisionPort, RemediationPolicyDecisionEngine>();

        return services;
    }
}
