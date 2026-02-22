using System.Diagnostics;
using System.Diagnostics.Metrics;
using CognitiveMesh.MetacognitiveLayer.Telemetry.Ports;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.MetacognitiveLayer.Telemetry.Engines;

/// <summary>
/// Core telemetry engine that wraps <see cref="System.Diagnostics.ActivitySource"/> and
/// <see cref="System.Diagnostics.Metrics.Meter"/> to provide distributed tracing and metrics
/// for the CognitiveMesh platform. Implements <see cref="ITelemetryPort"/>.
/// </summary>
public sealed class TelemetryEngine : ITelemetryPort, IDisposable
{
    /// <summary>
    /// The well-known <see cref="ActivitySource"/> name used by CognitiveMesh tracing.
    /// </summary>
    public const string ActivitySourceName = "CognitiveMesh";

    /// <summary>
    /// The well-known <see cref="Meter"/> name used by CognitiveMesh metrics.
    /// </summary>
    public const string MeterName = "CognitiveMesh";

    private static readonly ActivitySource CognitiveMeshActivitySource = new(ActivitySourceName);
    private static readonly Meter CognitiveMeshMeter = new(MeterName);

    private readonly ILogger<TelemetryEngine> _logger;

    // Well-known metrics
    private readonly Histogram<double> _requestDuration;
    private readonly Counter<long> _requestCount;
    private readonly UpDownCounter<int> _activeAgents;
    private readonly Histogram<double> _reasoningLatency;
    private readonly Histogram<double> _workflowStepDuration;
    private readonly Counter<long> _errorCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryEngine"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for structured logging.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is <c>null</c>.</exception>
    public TelemetryEngine(ILogger<TelemetryEngine> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _requestDuration = CognitiveMeshMeter.CreateHistogram<double>(
            "mesh.request.duration",
            unit: "ms",
            description: "Duration of mesh requests in milliseconds");

        _requestCount = CognitiveMeshMeter.CreateCounter<long>(
            "mesh.request.count",
            description: "Total number of mesh requests");

        _activeAgents = CognitiveMeshMeter.CreateUpDownCounter<int>(
            "mesh.agent.active",
            description: "Number of currently active agents in the mesh");

        _reasoningLatency = CognitiveMeshMeter.CreateHistogram<double>(
            "mesh.reasoning.latency",
            unit: "ms",
            description: "Latency of reasoning operations in milliseconds");

        _workflowStepDuration = CognitiveMeshMeter.CreateHistogram<double>(
            "mesh.workflow.step.duration",
            unit: "ms",
            description: "Duration of individual workflow steps in milliseconds");

        _errorCount = CognitiveMeshMeter.CreateCounter<long>(
            "mesh.error.count",
            description: "Total number of errors across the mesh");

        _logger.LogInformation("TelemetryEngine initialized with ActivitySource '{ActivitySource}' and Meter '{Meter}'",
            ActivitySourceName, MeterName);
    }

    /// <inheritdoc />
    public Activity? StartActivity(string operationName, ActivityKind kind = ActivityKind.Internal)
    {
        var activity = CognitiveMeshActivitySource.StartActivity(operationName, kind);

        if (activity is not null)
        {
            _logger.LogDebug("Started activity '{OperationName}' with TraceId '{TraceId}'",
                operationName, activity.TraceId);
        }

        return activity;
    }

    /// <inheritdoc />
    public void RecordMetric(string name, double value, KeyValuePair<string, object?>[]? tags = null)
    {
        _logger.LogDebug("Recording metric '{MetricName}' with value {Value}", name, value);

        // For ad-hoc metrics not covered by well-known instruments, create or retrieve a histogram
        // Well-known metrics should use their dedicated Record* methods instead
        var histogram = CognitiveMeshMeter.CreateHistogram<double>(name);

        if (tags is not null)
        {
            histogram.Record(value, tags);
        }
        else
        {
            histogram.Record(value);
        }
    }

    /// <inheritdoc />
    public void RecordException(Activity? activity, Exception exception)
    {
        if (activity is null)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(exception);

        var tagsCollection = new ActivityTagsCollection
        {
            { "exception.type", exception.GetType().FullName },
            { "exception.message", exception.Message },
            { "exception.stacktrace", exception.StackTrace }
        };

        activity.AddEvent(new ActivityEvent("exception", tags: tagsCollection));
        activity.SetStatus(ActivityStatusCode.Error, exception.Message);

        _logger.LogDebug("Recorded exception '{ExceptionType}' on activity '{OperationName}'",
            exception.GetType().Name, activity.OperationName);
    }

    /// <inheritdoc />
    public void SetActivityStatus(Activity? activity, ActivityStatusCode status, string? description = null)
    {
        if (activity is null)
        {
            return;
        }

        activity.SetStatus(status, description);

        _logger.LogDebug("Set status '{Status}' on activity '{OperationName}'",
            status, activity.OperationName);
    }

    /// <inheritdoc />
    public Counter<T> CreateCounter<T>(string name, string? unit = null, string? description = null) where T : struct
    {
        _logger.LogDebug("Creating counter '{Name}' (unit: {Unit})", name, unit ?? "none");
        return CognitiveMeshMeter.CreateCounter<T>(name, unit, description);
    }

    /// <inheritdoc />
    public Histogram<T> CreateHistogram<T>(string name, string? unit = null, string? description = null) where T : struct
    {
        _logger.LogDebug("Creating histogram '{Name}' (unit: {Unit})", name, unit ?? "none");
        return CognitiveMeshMeter.CreateHistogram<T>(name, unit, description);
    }

    // -----------------------------------------------------------------
    // Well-known metric recording methods
    // -----------------------------------------------------------------

    /// <summary>
    /// Records the duration of a mesh request.
    /// </summary>
    /// <param name="milliseconds">The request duration in milliseconds.</param>
    /// <param name="endpoint">The endpoint that was called.</param>
    public void RecordRequestDuration(double milliseconds, string endpoint)
    {
        _requestDuration.Record(milliseconds,
            new KeyValuePair<string, object?>("endpoint", endpoint));
    }

    /// <summary>
    /// Increments the mesh request counter.
    /// </summary>
    /// <param name="endpoint">The endpoint that was called.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    public void RecordRequestCount(string endpoint, int statusCode)
    {
        _requestCount.Add(1,
            new KeyValuePair<string, object?>("endpoint", endpoint),
            new KeyValuePair<string, object?>("status_code", statusCode));
    }

    /// <summary>
    /// Adjusts the active agent gauge by the specified delta.
    /// Use <c>+1</c> when an agent starts and <c>-1</c> when it stops.
    /// </summary>
    /// <param name="delta">The value to add (positive) or subtract (negative).</param>
    /// <param name="agentType">The type or name of the agent.</param>
    public void RecordActiveAgents(int delta, string agentType)
    {
        _activeAgents.Add(delta,
            new KeyValuePair<string, object?>("agent_type", agentType));
    }

    /// <summary>
    /// Records the latency of a reasoning operation.
    /// </summary>
    /// <param name="milliseconds">The reasoning latency in milliseconds.</param>
    /// <param name="reasoningType">The type of reasoning (e.g., Debate, Sequential).</param>
    public void RecordReasoningLatency(double milliseconds, string reasoningType)
    {
        _reasoningLatency.Record(milliseconds,
            new KeyValuePair<string, object?>("reasoning_type", reasoningType));
    }

    /// <summary>
    /// Records the duration of a single workflow step.
    /// </summary>
    /// <param name="milliseconds">The step duration in milliseconds.</param>
    /// <param name="workflowName">The name of the workflow.</param>
    /// <param name="stepName">The name of the step within the workflow.</param>
    public void RecordWorkflowStepDuration(double milliseconds, string workflowName, string stepName)
    {
        _workflowStepDuration.Record(milliseconds,
            new KeyValuePair<string, object?>("workflow", workflowName),
            new KeyValuePair<string, object?>("step", stepName));
    }

    /// <summary>
    /// Increments the error counter.
    /// </summary>
    /// <param name="errorType">The category or type of the error.</param>
    /// <param name="source">The component that produced the error.</param>
    public void RecordError(string errorType, string source)
    {
        _errorCount.Add(1,
            new KeyValuePair<string, object?>("error_type", errorType),
            new KeyValuePair<string, object?>("source", source));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // ActivitySource and Meter are static and shared; do not dispose them here.
        // Individual activities are disposed by their callers.
        _logger.LogInformation("TelemetryEngine disposed");
    }
}
