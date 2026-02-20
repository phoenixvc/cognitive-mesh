using CognitiveMesh.MetacognitiveLayer.Telemetry.Engines;
using CognitiveMesh.MetacognitiveLayer.Telemetry.Models;
using CognitiveMesh.MetacognitiveLayer.Telemetry.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CognitiveMesh.MetacognitiveLayer.Telemetry.Adapters;

/// <summary>
/// Adapter that bridges the <see cref="TelemetryEngine"/> to the OpenTelemetry SDK exporters.
/// Most instrumentation work is handled by <see cref="TelemetryEngine"/> and the OTEL SDK
/// auto-instrumentation; this adapter provides the DI wiring and exporter configuration.
/// </summary>
public sealed class OpenTelemetryAdapter
{
    private readonly ILogger<OpenTelemetryAdapter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenTelemetryAdapter"/> class.
    /// </summary>
    /// <param name="engine">The telemetry engine that provides instrumentation primitives.</param>
    /// <param name="logger">The logger instance for structured logging.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="engine"/> or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    public OpenTelemetryAdapter(TelemetryEngine engine, ILogger<OpenTelemetryAdapter> logger)
    {
        Engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("OpenTelemetryAdapter initialized");
    }

    /// <summary>
    /// Gets the underlying <see cref="TelemetryEngine"/> used for instrumentation.
    /// </summary>
    public TelemetryEngine Engine { get; }

    /// <summary>
    /// Configures OpenTelemetry tracing, metrics, and exporters on the given <see cref="IServiceCollection"/>.
    /// Registers the OTEL SDK pipeline (sources, meters, exporters) but does not register
    /// <see cref="TelemetryEngine"/> or <see cref="ITelemetryPort"/> -- that responsibility
    /// belongs to <see cref="Infrastructure.ServiceCollectionExtensions.AddCognitiveMeshTelemetry"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The telemetry configuration settings.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="configuration"/> is <c>null</c>.
    /// </exception>
    public static void ConfigureOpenTelemetry(IServiceCollection services, TelemetryConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: configuration.ServiceName,
                serviceVersion: configuration.ServiceVersion);

        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource(TelemetryEngine.ActivitySourceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .SetSampler(new TraceIdRatioBasedSampler(configuration.SamplingRatio))
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(configuration.OtlpEndpoint);
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddMeter(TelemetryEngine.MeterName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(configuration.OtlpEndpoint);
                    });
            });
    }
}
