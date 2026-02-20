using CognitiveMesh.BusinessApplications.ImpactMetrics.Engines;
using CognitiveMesh.BusinessApplications.ImpactMetrics.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace CognitiveMesh.BusinessApplications.ImpactMetrics.Infrastructure;

/// <summary>
/// Extension methods for registering Impact Metrics services in the
/// dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all Impact Metrics services to the specified
    /// <see cref="IServiceCollection"/>, registering the
    /// <see cref="ImpactMetricsEngine"/> as all four port interfaces
    /// required by the Impact-Driven AI Metrics subsystem.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddImpactMetricsServices(this IServiceCollection services)
    {
        // Register the engine as a scoped service so that all four port interfaces
        // share the same in-memory state within a single request scope.
        services.AddScoped<ImpactMetricsEngine>();

        services.AddScoped<IPsychologicalSafetyPort>(sp => sp.GetRequiredService<ImpactMetricsEngine>());
        services.AddScoped<IMissionAlignmentPort>(sp => sp.GetRequiredService<ImpactMetricsEngine>());
        services.AddScoped<IAdoptionTelemetryPort>(sp => sp.GetRequiredService<ImpactMetricsEngine>());
        services.AddScoped<IImpactAssessmentPort>(sp => sp.GetRequiredService<ImpactMetricsEngine>());

        return services;
    }
}
