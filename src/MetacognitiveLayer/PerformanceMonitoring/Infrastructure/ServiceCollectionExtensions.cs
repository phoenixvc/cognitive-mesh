using MetacognitiveLayer.PerformanceMonitoring;
using MetacognitiveLayer.PerformanceMonitoring.Adapters;
using MetacognitiveLayer.PerformanceMonitoring.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace MetacognitiveLayer.PerformanceMonitoring.Infrastructure;

/// <summary>
/// Provides extension methods for registering performance monitoring services
/// with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all performance monitoring services including the in-memory metrics store,
    /// the <see cref="PerformanceMonitor"/> engine, and the <see cref="PerformanceMonitoringAdapter"/>
    /// that implements <see cref="IPerformanceMonitoringPort"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPerformanceMonitoring(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register InMemoryMetricsStoreAdapter as singleton for both IMetricsStore and the concrete type
        services.AddSingleton<InMemoryMetricsStoreAdapter>();
        services.AddSingleton<IMetricsStore>(sp => sp.GetRequiredService<InMemoryMetricsStoreAdapter>());

        // Register the PerformanceMonitor engine as singleton
        services.AddSingleton<PerformanceMonitor>();

        // Register the adapter as the port implementation
        services.AddSingleton<IPerformanceMonitoringPort, PerformanceMonitoringAdapter>();

        return services;
    }
}
