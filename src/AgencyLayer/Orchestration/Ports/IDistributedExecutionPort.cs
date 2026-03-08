namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// A distributed worker.
/// </summary>
public class DistributedWorker
{
    /// <summary>Worker identifier.</summary>
    public required string WorkerId { get; init; }

    /// <summary>Worker name.</summary>
    public required string Name { get; init; }

    /// <summary>Worker endpoint.</summary>
    public required string Endpoint { get; init; }

    /// <summary>Worker region.</summary>
    public string? Region { get; init; }

    /// <summary>Worker status.</summary>
    public WorkerStatus Status { get; init; }

    /// <summary>Available capacity.</summary>
    public int AvailableCapacity { get; init; }

    /// <summary>Maximum capacity.</summary>
    public int MaxCapacity { get; init; }

    /// <summary>Supported capabilities.</summary>
    public IReadOnlyList<string> Capabilities { get; init; } = Array.Empty<string>();

    /// <summary>Last heartbeat.</summary>
    public DateTimeOffset LastHeartbeat { get; init; }

    /// <summary>Metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Worker status.
/// </summary>
public enum WorkerStatus
{
    /// <summary>Worker is available.</summary>
    Available,
    /// <summary>Worker is busy.</summary>
    Busy,
    /// <summary>Worker is draining.</summary>
    Draining,
    /// <summary>Worker is offline.</summary>
    Offline,
    /// <summary>Worker is unhealthy.</summary>
    Unhealthy
}

/// <summary>
/// A distributed task.
/// </summary>
public class DistributedTask
{
    /// <summary>Task identifier.</summary>
    public string TaskId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Task type.</summary>
    public required string TaskType { get; init; }

    /// <summary>Task payload (JSON).</summary>
    public required string Payload { get; init; }

    /// <summary>Required capabilities.</summary>
    public IReadOnlyList<string> RequiredCapabilities { get; init; } = Array.Empty<string>();

    /// <summary>Priority (higher = more important).</summary>
    public int Priority { get; init; } = 100;

    /// <summary>Timeout.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>Retry count.</summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>Preferred region.</summary>
    public string? PreferredRegion { get; init; }
}

/// <summary>
/// Result of distributed execution.
/// </summary>
public class DistributedTaskResult
{
    /// <summary>Task identifier.</summary>
    public required string TaskId { get; init; }

    /// <summary>Worker that executed.</summary>
    public required string WorkerId { get; init; }

    /// <summary>Result payload (JSON).</summary>
    public string? Result { get; init; }

    /// <summary>Whether successful.</summary>
    public bool Success { get; init; }

    /// <summary>Error if failed.</summary>
    public string? Error { get; init; }

    /// <summary>Execution duration.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Retry count used.</summary>
    public int RetryCount { get; init; }

    /// <summary>Completed at.</summary>
    public DateTimeOffset CompletedAt { get; init; }
}

/// <summary>
/// Port for distributed execution with cloud workers.
/// Implements the "Distributed Execution with Cloud Workers" pattern.
/// </summary>
public interface IDistributedExecutionPort
{
    /// <summary>
    /// Submits a task for distributed execution.
    /// </summary>
    Task<string> SubmitTaskAsync(
        DistributedTask task,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets task result.
    /// </summary>
    Task<DistributedTaskResult?> GetResultAsync(
        string taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for task completion.
    /// </summary>
    Task<DistributedTaskResult> WaitForCompletionAsync(
        string taskId,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a task.
    /// </summary>
    Task CancelTaskAsync(
        string taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists available workers.
    /// </summary>
    Task<IReadOnlyList<DistributedWorker>> ListWorkersAsync(
        WorkerStatus? status = null,
        IEnumerable<string>? capabilities = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a worker.
    /// </summary>
    Task RegisterWorkerAsync(
        DistributedWorker worker,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregisters a worker.
    /// </summary>
    Task UnregisterWorkerAsync(
        string workerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends worker heartbeat.
    /// </summary>
    Task HeartbeatAsync(
        string workerId,
        int availableCapacity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets execution statistics.
    /// </summary>
    Task<DistributedExecutionStats> GetStatisticsAsync(
        DateTimeOffset? since = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Distributed execution statistics.
/// </summary>
public class DistributedExecutionStats
{
    public int TotalTasks { get; init; }
    public int CompletedTasks { get; init; }
    public int FailedTasks { get; init; }
    public int PendingTasks { get; init; }
    public int ActiveWorkers { get; init; }
    public TimeSpan AverageExecutionTime { get; init; }
    public Dictionary<string, int> TasksByRegion { get; init; } = new();
}
