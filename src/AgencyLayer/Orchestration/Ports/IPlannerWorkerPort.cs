namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Status of a worker.
/// </summary>
public enum PlannerWorkerStatus
{
    /// <summary>Worker is idle and available.</summary>
    Idle,
    /// <summary>Worker is processing a task.</summary>
    Busy,
    /// <summary>Worker is paused.</summary>
    Paused,
    /// <summary>Worker has failed.</summary>
    Failed,
    /// <summary>Worker is offline.</summary>
    Offline
}

/// <summary>
/// A worker in the planner-worker pool.
/// </summary>
public class Worker
{
    /// <summary>Worker identifier.</summary>
    public required string WorkerId { get; init; }

    /// <summary>Worker name.</summary>
    public required string Name { get; init; }

    /// <summary>Worker capabilities.</summary>
    public IReadOnlyList<string> Capabilities { get; init; } = Array.Empty<string>();

    /// <summary>Current status.</summary>
    public PlannerWorkerStatus Status { get; init; }

    /// <summary>Current task ID if busy.</summary>
    public string? CurrentTaskId { get; init; }

    /// <summary>Model used by this worker.</summary>
    public string? Model { get; init; }

    /// <summary>Tasks completed.</summary>
    public int TasksCompleted { get; init; }

    /// <summary>Average task duration.</summary>
    public TimeSpan AverageTaskDuration { get; init; }
}

/// <summary>
/// A task for workers.
/// </summary>
public class WorkerTask
{
    /// <summary>Task identifier.</summary>
    public string TaskId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Task description.</summary>
    public required string Description { get; init; }

    /// <summary>Required capabilities.</summary>
    public IReadOnlyList<string> RequiredCapabilities { get; init; } = Array.Empty<string>();

    /// <summary>Task input.</summary>
    public required string Input { get; init; }

    /// <summary>Expected output format.</summary>
    public string? OutputFormat { get; init; }

    /// <summary>Priority (lower = higher priority).</summary>
    public int Priority { get; init; } = 100;

    /// <summary>Dependencies on other tasks.</summary>
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();

    /// <summary>Timeout for this task.</summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>Context from planner.</summary>
    public string? PlannerContext { get; init; }

    /// <summary>When the task was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Result of a worker task.
/// </summary>
public class WorkerTaskResult
{
    /// <summary>Task identifier.</summary>
    public required string TaskId { get; init; }

    /// <summary>Worker that executed.</summary>
    public required string WorkerId { get; init; }

    /// <summary>Whether task succeeded.</summary>
    public required bool Success { get; init; }

    /// <summary>Task output.</summary>
    public string? Output { get; init; }

    /// <summary>Error if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Execution duration.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Whether this needs planner review.</summary>
    public bool NeedsPlannerReview { get; init; }

    /// <summary>Reason for needing review.</summary>
    public string? ReviewReason { get; init; }

    /// <summary>When completed.</summary>
    public DateTimeOffset CompletedAt { get; init; }
}

/// <summary>
/// A plan created by the planner.
/// </summary>
public class WorkerPlan
{
    /// <summary>Plan identifier.</summary>
    public string PlanId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Original goal.</summary>
    public required string Goal { get; init; }

    /// <summary>Tasks in the plan.</summary>
    public IReadOnlyList<WorkerTask> Tasks { get; init; } = Array.Empty<WorkerTask>();

    /// <summary>Estimated total duration.</summary>
    public TimeSpan? EstimatedDuration { get; init; }

    /// <summary>When the plan was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Planner reasoning.</summary>
    public string? PlannerReasoning { get; init; }
}

/// <summary>
/// Configuration for planner-worker pattern.
/// </summary>
public class PlannerWorkerConfiguration
{
    /// <summary>Model for planner.</summary>
    public required string PlannerModel { get; init; }

    /// <summary>Model for workers (default).</summary>
    public required string WorkerModel { get; init; }

    /// <summary>Maximum concurrent workers.</summary>
    public int MaxConcurrentWorkers { get; init; } = 5;

    /// <summary>Whether to enable parallel execution.</summary>
    public bool EnableParallelExecution { get; init; } = true;

    /// <summary>Whether planner should review all results.</summary>
    public bool PlannerReviewsAllResults { get; init; } = false;

    /// <summary>Re-planning threshold (failed tasks).</summary>
    public int ReplanningThreshold { get; init; } = 2;

    /// <summary>Maximum re-planning attempts.</summary>
    public int MaxReplanningAttempts { get; init; } = 3;
}

/// <summary>
/// Status of a plan execution.
/// </summary>
public class PlanExecutionStatus
{
    /// <summary>Plan identifier.</summary>
    public required string PlanId { get; init; }

    /// <summary>Tasks pending.</summary>
    public int TasksPending { get; init; }

    /// <summary>Tasks in progress.</summary>
    public int TasksInProgress { get; init; }

    /// <summary>Tasks completed.</summary>
    public int TasksCompleted { get; init; }

    /// <summary>Tasks failed.</summary>
    public int TasksFailed { get; init; }

    /// <summary>Overall progress (0.0 - 1.0).</summary>
    public double Progress { get; init; }

    /// <summary>Active workers.</summary>
    public int ActiveWorkers { get; init; }

    /// <summary>Elapsed time.</summary>
    public TimeSpan Elapsed { get; init; }

    /// <summary>Whether re-planning is needed.</summary>
    public bool NeedsReplanning { get; init; }
}

/// <summary>
/// Port for planner-worker separation in long-running tasks.
/// Implements the "Planner-Worker Separation for Long-Running Agents" pattern.
/// </summary>
/// <remarks>
/// This port separates planning from execution using a high-capability
/// planner agent and multiple worker agents for parallel execution,
/// with re-planning capabilities when workers encounter issues.
/// </remarks>
public interface IPlannerWorkerPort
{
    /// <summary>
    /// Creates a plan for a goal.
    /// </summary>
    /// <param name="goal">The goal to plan for.</param>
    /// <param name="context">Additional context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The plan.</returns>
    Task<WorkerPlan> CreatePlanAsync(
        string goal,
        string? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a plan using workers.
    /// </summary>
    /// <param name="plan">The plan to execute.</param>
    /// <param name="configuration">Configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Results for all tasks.</returns>
    Task<IReadOnlyList<WorkerTaskResult>> ExecutePlanAsync(
        WorkerPlan plan,
        PlannerWorkerConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a plan execution.
    /// </summary>
    /// <param name="planId">The plan ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Execution status.</returns>
    Task<PlanExecutionStatus> GetStatusAsync(
        string planId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-plans based on failed tasks.
    /// </summary>
    /// <param name="originalPlan">The original plan.</param>
    /// <param name="failedTasks">Tasks that failed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Revised plan.</returns>
    Task<WorkerPlan> ReplanAsync(
        WorkerPlan originalPlan,
        IReadOnlyList<WorkerTaskResult> failedTasks,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available workers.
    /// </summary>
    /// <param name="requiredCapabilities">Required capabilities (null = all).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Available workers.</returns>
    Task<IReadOnlyList<Worker>> GetWorkersAsync(
        IEnumerable<string>? requiredCapabilities = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a task to a worker.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <param name="workerId">Specific worker (null = auto-assign).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The assigned worker.</returns>
    Task<Worker> AssignTaskAsync(
        WorkerTask task,
        string? workerId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reviews a task result (planner review).
    /// </summary>
    /// <param name="result">The task result.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Whether result is acceptable and feedback.</returns>
    Task<(bool IsAcceptable, string? Feedback)> ReviewResultAsync(
        WorkerTaskResult result,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a plan execution.
    /// </summary>
    /// <param name="planId">The plan ID.</param>
    /// <param name="reason">Cancellation reason.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CancelPlanAsync(
        string planId,
        string reason,
        CancellationToken cancellationToken = default);
}
