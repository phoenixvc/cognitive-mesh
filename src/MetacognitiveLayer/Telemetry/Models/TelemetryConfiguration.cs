namespace CognitiveMesh.MetacognitiveLayer.Telemetry.Models;

/// <summary>
/// Configuration settings for the CognitiveMesh OpenTelemetry instrumentation.
/// Bind this to the "Telemetry" configuration section.
/// </summary>
public sealed record TelemetryConfiguration
{
    /// <summary>
    /// Gets the OTLP exporter endpoint.
    /// Defaults to <c>http://localhost:4317</c>.
    /// </summary>
    public string OtlpEndpoint { get; init; } = "http://localhost:4317";

    /// <summary>
    /// Gets the logical service name reported to the telemetry backend.
    /// Defaults to <c>CognitiveMesh</c>.
    /// </summary>
    public string ServiceName { get; init; } = "CognitiveMesh";

    /// <summary>
    /// Gets the service version reported to the telemetry backend.
    /// </summary>
    public string ServiceVersion { get; init; } = "1.0.0";

    /// <summary>
    /// Gets a value indicating whether the console exporter is enabled.
    /// Should be <c>false</c> in production environments.
    /// </summary>
    public bool EnableConsoleExporter { get; init; }

    /// <summary>
    /// Gets the sampling ratio for distributed traces.
    /// A value of <c>1.0</c> samples every trace; <c>0.0</c> samples none.
    /// Defaults to <c>1.0</c>.
    /// </summary>
    public double SamplingRatio { get; init; } = 1.0;
}
