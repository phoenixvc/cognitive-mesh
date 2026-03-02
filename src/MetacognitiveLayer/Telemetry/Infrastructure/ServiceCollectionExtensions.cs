using CognitiveMesh.MetacognitiveLayer.Telemetry.Adapters;
using CognitiveMesh.MetacognitiveLayer.Telemetry.Engines;
using CognitiveMesh.MetacognitiveLayer.Telemetry.Models;
using CognitiveMesh.MetacognitiveLayer.Telemetry.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CognitiveMesh.MetacognitiveLayer.Telemetry.Infrastructure;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register
/// CognitiveMesh telemetry services and OpenTelemetry instrumentation.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds CognitiveMesh telemetry services to the specified <see cref="IServiceCollection"/>.
    /// Reads configuration from the <c>Telemetry</c> section of the provided <see cref="IConfiguration"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration containing a <c>Telemetry</c> section.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="configuration"/> is <c>null</c>.
    /// </exception>
    public static IServiceCollection AddCognitiveMeshTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var telemetryConfig = new TelemetryConfiguration();
        configuration.GetSection("Telemetry").Bind(telemetryConfig);

        // Register the configuration as a singleton for consumers that need it
        services.AddSingleton(telemetryConfig);

        // Register the engine as a singleton for both the concrete type and the port interface
        services.AddSingleton<TelemetryEngine>();
        services.AddSingleton<ITelemetryPort>(sp => sp.GetRequiredService<TelemetryEngine>());
        services.AddSingleton<OpenTelemetryAdapter>();

        // Configure OpenTelemetry exporters and instrumentation
        OpenTelemetryAdapter.ConfigureOpenTelemetry(services, telemetryConfig);

        return services;
    }
}
