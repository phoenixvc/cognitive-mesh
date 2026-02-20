using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CognitiveMesh.MetacognitiveLayer.Telemetry.Ports;

/// <summary>
/// Port interface for telemetry instrumentation following hexagonal architecture.
/// Provides methods to create activities (spans), record metrics, and handle exceptions
/// within the CognitiveMesh distributed tracing and metrics pipeline.
/// </summary>
public interface ITelemetryPort
{
    /// <summary>
    /// Starts a new <see cref="Activity"/> (distributed trace span) with the specified operation name and kind.
    /// </summary>
    /// <param name="operationName">The name of the operation being traced.</param>
    /// <param name="kind">The kind of activity. Defaults to <see cref="ActivityKind.Internal"/>.</param>
    /// <returns>
    /// An <see cref="Activity"/> instance if a listener is sampling the operation; otherwise <c>null</c>.
    /// </returns>
    Activity? StartActivity(string operationName, ActivityKind kind = ActivityKind.Internal);

    /// <summary>
    /// Records a custom metric value with optional dimensional tags.
    /// </summary>
    /// <param name="name">The metric instrument name (e.g., <c>mesh.request.duration</c>).</param>
    /// <param name="value">The value to record.</param>
    /// <param name="tags">Optional key-value pairs used as metric dimensions.</param>
    void RecordMetric(string name, double value, KeyValuePair<string, object?>[]? tags = null);

    /// <summary>
    /// Records an exception event on the given <see cref="Activity"/>.
    /// </summary>
    /// <param name="activity">The activity to annotate. If <c>null</c>, the call is a no-op.</param>
    /// <param name="exception">The exception to record.</param>
    void RecordException(Activity? activity, Exception exception);

    /// <summary>
    /// Sets the status of the given <see cref="Activity"/>.
    /// </summary>
    /// <param name="activity">The activity whose status should be set. If <c>null</c>, the call is a no-op.</param>
    /// <param name="status">The <see cref="ActivityStatusCode"/> to set.</param>
    /// <param name="description">An optional human-readable description of the status.</param>
    void SetActivityStatus(Activity? activity, ActivityStatusCode status, string? description = null);

    /// <summary>
    /// Creates a <see cref="Counter{T}"/> instrument for monotonically increasing values.
    /// </summary>
    /// <typeparam name="T">The numeric type of the counter (must be a value type).</typeparam>
    /// <param name="name">The instrument name.</param>
    /// <param name="unit">The optional unit of measure (e.g., <c>ms</c>, <c>By</c>).</param>
    /// <param name="description">An optional human-readable description.</param>
    /// <returns>A new <see cref="Counter{T}"/> instance.</returns>
    Counter<T> CreateCounter<T>(string name, string? unit = null, string? description = null) where T : struct;

    /// <summary>
    /// Creates a <see cref="Histogram{T}"/> instrument for recording value distributions.
    /// </summary>
    /// <typeparam name="T">The numeric type of the histogram (must be a value type).</typeparam>
    /// <param name="name">The instrument name.</param>
    /// <param name="unit">The optional unit of measure (e.g., <c>ms</c>, <c>By</c>).</param>
    /// <param name="description">An optional human-readable description.</param>
    /// <returns>A new <see cref="Histogram{T}"/> instance.</returns>
    Histogram<T> CreateHistogram<T>(string name, string? unit = null, string? description = null) where T : struct;
}
