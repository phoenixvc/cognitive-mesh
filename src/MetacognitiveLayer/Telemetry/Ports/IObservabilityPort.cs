namespace MetacognitiveLayer.Telemetry.Ports;

/// <summary>
/// Type of telemetry data.
/// </summary>
public enum TelemetryType
{
    /// <summary>Distributed trace.</summary>
    Trace,
    /// <summary>Metric measurement.</summary>
    Metric,
    /// <summary>Log entry.</summary>
    Log,
    /// <summary>Event.</summary>
    Event
}

/// <summary>
/// Severity of a log entry.
/// </summary>
public enum LogSeverity
{
    /// <summary>Debug level.</summary>
    Debug,

    /// <summary>Info level.</summary>
    Info,

    /// <summary>Warning level.</summary>
    Warning,

    /// <summary>Error level.</summary>
    Error,

    /// <summary>Critical level.</summary>
    Critical
}

/// <summary>
/// A span in a distributed trace.
/// </summary>
public class TraceSpan
{
    /// <summary>Span identifier.</summary>
    public required string SpanId { get; init; }

    /// <summary>Trace identifier.</summary>
    public required string TraceId { get; init; }

    /// <summary>Parent span ID (null for root).</summary>
    public string? ParentSpanId { get; init; }

    /// <summary>Operation name.</summary>
    public required string OperationName { get; init; }

    /// <summary>Service name.</summary>
    public string? ServiceName { get; init; }

    /// <summary>Agent ID if applicable.</summary>
    public string? AgentId { get; init; }

    /// <summary>When the span started.</summary>
    public DateTimeOffset StartTime { get; init; }

    /// <summary>Span duration.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Status (ok, error).</summary>
    public string Status { get; init; } = "ok";

    /// <summary>Error message if status is error.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Attributes/tags.</summary>
    public Dictionary<string, string> Attributes { get; init; } = new();

    /// <summary>Events within the span.</summary>
    public IReadOnlyList<SpanEvent> Events { get; init; } = Array.Empty<SpanEvent>();
}

/// <summary>
/// An event within a span.
/// </summary>
public class SpanEvent
{
    /// <summary>Event name.</summary>
    public required string Name { get; init; }

    /// <summary>When the event occurred.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Event attributes.</summary>
    public Dictionary<string, string> Attributes { get; init; } = new();
}

/// <summary>
/// A metric measurement.
/// </summary>
public class MetricMeasurement
{
    /// <summary>Metric name.</summary>
    public required string Name { get; init; }

    /// <summary>Metric value.</summary>
    public required double Value { get; init; }

    /// <summary>Unit of measurement.</summary>
    public string? Unit { get; init; }

    /// <summary>Metric type (counter, gauge, histogram).</summary>
    public required string MetricType { get; init; }

    /// <summary>When measured.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Dimensions/labels.</summary>
    public Dictionary<string, string> Dimensions { get; init; } = new();
}

/// <summary>
/// A log entry.
/// </summary>
public class LogEntry
{
    /// <summary>Log message.</summary>
    public required string Message { get; init; }

    /// <summary>Severity.</summary>
    public LogSeverity Severity { get; init; }

    /// <summary>When logged.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Associated trace ID.</summary>
    public string? TraceId { get; init; }

    /// <summary>Associated span ID.</summary>
    public string? SpanId { get; init; }

    /// <summary>Agent ID.</summary>
    public string? AgentId { get; init; }

    /// <summary>Structured data.</summary>
    public Dictionary<string, object> StructuredData { get; init; } = new();

    /// <summary>Exception details.</summary>
    public string? Exception { get; init; }
}

/// <summary>
/// Configuration for observability.
/// </summary>
public class ObservabilityConfiguration
{
    /// <summary>Whether tracing is enabled.</summary>
    public bool TracingEnabled { get; init; } = true;

    /// <summary>Whether metrics are enabled.</summary>
    public bool MetricsEnabled { get; init; } = true;

    /// <summary>Whether logging is enabled.</summary>
    public bool LoggingEnabled { get; init; } = true;

    /// <summary>Sampling rate for traces (0.0 - 1.0).</summary>
    public double TraceSamplingRate { get; init; } = 1.0;

    /// <summary>Minimum log severity to record.</summary>
    public LogSeverity MinLogSeverity { get; init; } = LogSeverity.Info;

    /// <summary>Metric export interval.</summary>
    public TimeSpan MetricExportInterval { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>OTLP endpoint for export.</summary>
    public string? OtlpEndpoint { get; init; }

    /// <summary>Service name.</summary>
    public string ServiceName { get; init; } = "cognitive-mesh";
}

/// <summary>
/// Port for full-stack observability.
/// Implements the "Full-Stack Observability (Traces, Logs, Metrics)" pattern.
/// </summary>
/// <remarks>
/// This port provides unified observability for agentic systems
/// including distributed tracing, metrics collection, and structured
/// logging with correlation across agent boundaries.
/// </remarks>
public interface IObservabilityPort
{
    /// <summary>
    /// Starts a new trace span.
    /// </summary>
    /// <param name="operationName">Operation name.</param>
    /// <param name="parentSpanId">Parent span (null for root).</param>
    /// <param name="attributes">Initial attributes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The span.</returns>
    Task<TraceSpan> StartSpanAsync(
        string operationName,
        string? parentSpanId = null,
        Dictionary<string, string>? attributes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends a span.
    /// </summary>
    /// <param name="spanId">The span ID.</param>
    /// <param name="status">Status (ok, error).</param>
    /// <param name="errorMessage">Error message if failed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task EndSpanAsync(
        string spanId,
        string status = "ok",
        string? errorMessage = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an event to a span.
    /// </summary>
    /// <param name="spanId">The span ID.</param>
    /// <param name="eventName">Event name.</param>
    /// <param name="attributes">Event attributes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddSpanEventAsync(
        string spanId,
        string eventName,
        Dictionary<string, string>? attributes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a metric measurement.
    /// </summary>
    /// <param name="measurement">The measurement.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordMetricAsync(
        MetricMeasurement measurement,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments a counter metric.
    /// </summary>
    /// <param name="name">Counter name.</param>
    /// <param name="value">Increment value.</param>
    /// <param name="dimensions">Dimensions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task IncrementCounterAsync(
        string name,
        double value = 1,
        Dictionary<string, string>? dimensions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a histogram value.
    /// </summary>
    /// <param name="name">Histogram name.</param>
    /// <param name="value">Value to record.</param>
    /// <param name="unit">Unit of measurement.</param>
    /// <param name="dimensions">Dimensions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordHistogramAsync(
        string name,
        double value,
        string? unit = null,
        Dictionary<string, string>? dimensions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a message.
    /// </summary>
    /// <param name="entry">The log entry.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task LogAsync(
        LogEntry entry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets trace by ID.
    /// </summary>
    /// <param name="traceId">The trace ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All spans in the trace.</returns>
    Task<IReadOnlyList<TraceSpan>> GetTraceAsync(
        string traceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries metrics.
    /// </summary>
    /// <param name="name">Metric name.</param>
    /// <param name="since">Start time.</param>
    /// <param name="until">End time.</param>
    /// <param name="dimensions">Filter by dimensions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Metric measurements.</returns>
    Task<IReadOnlyList<MetricMeasurement>> QueryMetricsAsync(
        string name,
        DateTimeOffset since,
        DateTimeOffset? until = null,
        Dictionary<string, string>? dimensions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries logs.
    /// </summary>
    /// <param name="since">Start time.</param>
    /// <param name="until">End time.</param>
    /// <param name="minSeverity">Minimum severity.</param>
    /// <param name="agentId">Filter by agent.</param>
    /// <param name="limit">Maximum entries.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Log entries.</returns>
    Task<IReadOnlyList<LogEntry>> QueryLogsAsync(
        DateTimeOffset since,
        DateTimeOffset? until = null,
        LogSeverity? minSeverity = null,
        string? agentId = null,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or sets configuration.
    /// </summary>
    /// <param name="configuration">Configuration to set (null = get).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current configuration.</returns>
    Task<ObservabilityConfiguration> ConfigureAsync(
        ObservabilityConfiguration? configuration = null,
        CancellationToken cancellationToken = default);
}
