using System.Text.Json;

namespace AgencyLayer.Orchestration.Checkpointing;

/// <summary>
/// Manages execution checkpoints for durable workflow execution.
/// Enables crash recovery and long-horizon sequential task completion.
/// </summary>
public interface ICheckpointManager
{
    /// <summary>
    /// Saves an execution checkpoint that can be used to resume from this point.
    /// </summary>
    Task SaveCheckpointAsync(ExecutionCheckpoint checkpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific checkpoint by its ID.
    /// </summary>
    Task<ExecutionCheckpoint?> GetCheckpointAsync(string workflowId, string checkpointId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest checkpoint for a workflow, enabling resume-from-last-good-state.
    /// </summary>
    Task<ExecutionCheckpoint?> GetLatestCheckpointAsync(string workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all checkpoints for a workflow in step order.
    /// </summary>
    Task<IEnumerable<ExecutionCheckpoint>> GetWorkflowCheckpointsAsync(string workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all checkpoints for a completed workflow to free resources.
    /// </summary>
    Task PurgeWorkflowCheckpointsAsync(string workflowId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a single checkpoint in a workflow execution chain.
/// </summary>
public class ExecutionCheckpoint
{
    public string CheckpointId { get; set; } = Guid.NewGuid().ToString();
    public string WorkflowId { get; set; } = string.Empty;
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public ExecutionStepStatus Status { get; set; } = ExecutionStepStatus.Completed;
    public string StateJson { get; set; } = "{}";
    public string InputJson { get; set; } = "{}";
    public string OutputJson { get; set; } = "{}";
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ExecutionDuration { get; set; }

    /// <summary>Deserializes the checkpoint state from JSON.</summary>
    public T? DeserializeState<T>() => JsonSerializer.Deserialize<T>(StateJson);

    /// <summary>Deserializes the checkpoint state from JSON with custom options.</summary>
    public T? DeserializeState<T>(JsonSerializerOptions options) => JsonSerializer.Deserialize<T>(StateJson, options);

    /// <summary>Deserializes the checkpoint output from JSON.</summary>
    public T? DeserializeOutput<T>() => JsonSerializer.Deserialize<T>(OutputJson);

    /// <summary>Deserializes the checkpoint output from JSON with custom options.</summary>
    public T? DeserializeOutput<T>(JsonSerializerOptions options) => JsonSerializer.Deserialize<T>(OutputJson, options);
}

/// <summary>
/// Status of a single execution step within a workflow.
/// </summary>
public enum ExecutionStepStatus
{
    /// <summary>Step has not started.</summary>
    Pending,
    /// <summary>Step is currently executing.</summary>
    Running,
    /// <summary>Step completed successfully.</summary>
    Completed,
    /// <summary>Step failed after exhausting retries.</summary>
    Failed,
    /// <summary>Step was skipped (e.g., due to conditional logic).</summary>
    Skipped
}
