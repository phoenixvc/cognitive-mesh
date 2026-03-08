namespace MetacognitiveLayer.PerformanceMonitoring.Ports;

/// <summary>
/// Type of progress update.
/// </summary>
public enum ProgressUpdateType
{
    /// <summary>Status message.</summary>
    Status,
    /// <summary>Percentage progress.</summary>
    Percentage,
    /// <summary>Milestone reached.</summary>
    Milestone,
    /// <summary>Subtask update.</summary>
    Subtask,
    /// <summary>Metric update.</summary>
    Metric,
    /// <summary>Warning.</summary>
    Warning,
    /// <summary>Error (non-fatal).</summary>
    Error,
    /// <summary>Completion.</summary>
    Completion
}

/// <summary>
/// A progress update.
/// </summary>
public class ProgressUpdate
{
    /// <summary>Update identifier.</summary>
    public string UpdateId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Operation being tracked.</summary>
    public required string OperationId { get; init; }

    /// <summary>Update type.</summary>
    public ProgressUpdateType Type { get; init; }

    /// <summary>Message or description.</summary>
    public required string Message { get; init; }

    /// <summary>Progress percentage (0-100).</summary>
    public double? Percentage { get; init; }

    /// <summary>Current step number.</summary>
    public int? CurrentStep { get; init; }

    /// <summary>Total steps.</summary>
    public int? TotalSteps { get; init; }

    /// <summary>Subtask name (if subtask update).</summary>
    public string? SubtaskName { get; init; }

    /// <summary>Metric name (if metric update).</summary>
    public string? MetricName { get; init; }

    /// <summary>Metric value.</summary>
    public double? MetricValue { get; init; }

    /// <summary>Elapsed time.</summary>
    public TimeSpan Elapsed { get; init; }

    /// <summary>Estimated remaining time.</summary>
    public TimeSpan? EstimatedRemaining { get; init; }

    /// <summary>When the update was created.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Additional metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// A tracked operation.
/// </summary>
public class TrackedOperation
{
    /// <summary>Operation identifier.</summary>
    public required string OperationId { get; init; }

    /// <summary>Operation name.</summary>
    public required string Name { get; init; }

    /// <summary>Description.</summary>
    public string? Description { get; init; }

    /// <summary>Total steps (if known).</summary>
    public int? TotalSteps { get; init; }

    /// <summary>Current progress (0-100).</summary>
    public double Progress { get; init; }

    /// <summary>Whether operation is active.</summary>
    public bool IsActive { get; init; }

    /// <summary>Whether operation completed successfully.</summary>
    public bool? IsSuccess { get; init; }

    /// <summary>When started.</summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>When completed (if done).</summary>
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>Latest status message.</summary>
    public string? LatestStatus { get; init; }

    /// <summary>Subtasks.</summary>
    public IReadOnlyList<TrackedOperation> Subtasks { get; init; } = Array.Empty<TrackedOperation>();
}

/// <summary>
/// Configuration for progress tracking.
/// </summary>
public class ProgressTrackingConfiguration
{
    /// <summary>Minimum interval between updates.</summary>
    public TimeSpan MinUpdateInterval { get; init; } = TimeSpan.FromMilliseconds(100);

    /// <summary>Whether to track subtasks.</summary>
    public bool TrackSubtasks { get; init; } = true;

    /// <summary>Whether to estimate remaining time.</summary>
    public bool EstimateRemaining { get; init; } = true;

    /// <summary>Maximum history to retain.</summary>
    public int MaxHistoryUpdates { get; init; } = 100;

    /// <summary>Whether to persist updates.</summary>
    public bool PersistUpdates { get; init; } = false;
}

/// <summary>
/// Port for streaming progress updates.
/// Implements the "Streaming Structured Output" / "Progress Feedback" pattern.
/// </summary>
/// <remarks>
/// This port provides real-time streaming of operation progress,
/// enabling UIs and other consumers to display live updates
/// for long-running agent operations.
/// </remarks>
public interface IStreamingProgressPort
{
    /// <summary>
    /// Starts tracking an operation.
    /// </summary>
    /// <param name="name">Operation name.</param>
    /// <param name="totalSteps">Total steps (if known).</param>
    /// <param name="configuration">Configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The operation ID.</returns>
    Task<string> StartOperationAsync(
        string name,
        int? totalSteps = null,
        ProgressTrackingConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports a progress update.
    /// </summary>
    /// <param name="operationId">The operation ID.</param>
    /// <param name="update">The update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ReportProgressAsync(
        string operationId,
        ProgressUpdate update,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports simple percentage progress.
    /// </summary>
    /// <param name="operationId">The operation ID.</param>
    /// <param name="percentage">Progress percentage (0-100).</param>
    /// <param name="message">Status message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ReportPercentageAsync(
        string operationId,
        double percentage,
        string? message = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports step completion.
    /// </summary>
    /// <param name="operationId">The operation ID.</param>
    /// <param name="stepNumber">Completed step number.</param>
    /// <param name="stepName">Step name or description.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ReportStepAsync(
        string operationId,
        int stepNumber,
        string stepName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a subtask.
    /// </summary>
    /// <param name="operationId">Parent operation ID.</param>
    /// <param name="subtaskName">Subtask name.</param>
    /// <param name="totalSteps">Total steps for subtask.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Subtask operation ID.</returns>
    Task<string> StartSubtaskAsync(
        string operationId,
        string subtaskName,
        int? totalSteps = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes an operation.
    /// </summary>
    /// <param name="operationId">The operation ID.</param>
    /// <param name="success">Whether successful.</param>
    /// <param name="message">Completion message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CompleteOperationAsync(
        string operationId,
        bool success,
        string? message = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to progress updates.
    /// </summary>
    /// <param name="operationId">The operation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Stream of updates.</returns>
    IAsyncEnumerable<ProgressUpdate> SubscribeAsync(
        string operationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current operation state.
    /// </summary>
    /// <param name="operationId">The operation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tracked operation.</returns>
    Task<TrackedOperation?> GetOperationAsync(
        string operationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets update history.
    /// </summary>
    /// <param name="operationId">The operation ID.</param>
    /// <param name="limit">Maximum updates.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Update history.</returns>
    Task<IReadOnlyList<ProgressUpdate>> GetHistoryAsync(
        string operationId,
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active operations.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Active operations.</returns>
    Task<IReadOnlyList<TrackedOperation>> GetActiveOperationsAsync(
        CancellationToken cancellationToken = default);
}
