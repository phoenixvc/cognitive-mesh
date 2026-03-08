namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Status of an autonomous loop.
/// </summary>
public enum LoopStatus
{
    /// <summary>Loop is initializing.</summary>
    Initializing,
    /// <summary>Loop is running.</summary>
    Running,
    /// <summary>Loop is paused.</summary>
    Paused,
    /// <summary>Loop is waiting for input.</summary>
    WaitingForInput,
    /// <summary>Loop is completing.</summary>
    Completing,
    /// <summary>Loop has completed.</summary>
    Completed,
    /// <summary>Loop was terminated.</summary>
    Terminated,
    /// <summary>Loop encountered an error.</summary>
    Error
}

/// <summary>
/// Reason for loop termination.
/// </summary>
public enum TerminationReason
{
    /// <summary>Goal was achieved.</summary>
    GoalAchieved,
    /// <summary>Maximum iterations reached.</summary>
    MaxIterationsReached,
    /// <summary>Maximum time reached.</summary>
    MaxTimeReached,
    /// <summary>Budget exhausted.</summary>
    BudgetExhausted,
    /// <summary>User requested termination.</summary>
    UserRequested,
    /// <summary>Error threshold exceeded.</summary>
    ErrorThresholdExceeded,
    /// <summary>No progress detected.</summary>
    NoProgress,
    /// <summary>Safety boundary triggered.</summary>
    SafetyBoundary
}

/// <summary>
/// Bounds for the autonomous loop.
/// </summary>
public class LoopBounds
{
    /// <summary>Maximum iterations allowed.</summary>
    public int MaxIterations { get; init; } = 100;

    /// <summary>Maximum duration.</summary>
    public TimeSpan MaxDuration { get; init; } = TimeSpan.FromHours(1);

    /// <summary>Maximum budget.</summary>
    public decimal? MaxBudget { get; init; }

    /// <summary>Maximum consecutive errors before termination.</summary>
    public int MaxConsecutiveErrors { get; init; } = 3;

    /// <summary>Maximum total errors before termination.</summary>
    public int MaxTotalErrors { get; init; } = 10;

    /// <summary>Iterations without progress before termination.</summary>
    public int MaxIterationsWithoutProgress { get; init; } = 5;

    /// <summary>Progress check interval.</summary>
    public TimeSpan ProgressCheckInterval { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>Human checkpoint interval (requires approval to continue).</summary>
    public TimeSpan? HumanCheckpointInterval { get; init; }

    /// <summary>Iteration count for human checkpoint.</summary>
    public int? HumanCheckpointEveryNIterations { get; init; }
}

/// <summary>
/// Configuration for autonomous loop.
/// </summary>
public class AutonomousLoopConfiguration
{
    /// <summary>Configuration identifier.</summary>
    public string ConfigurationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Loop bounds.</summary>
    public LoopBounds Bounds { get; init; } = new();

    /// <summary>Model to use.</summary>
    public string? Model { get; init; }

    /// <summary>Whether to enable self-correction.</summary>
    public bool EnableSelfCorrection { get; init; } = true;

    /// <summary>Whether to enable progress tracking.</summary>
    public bool EnableProgressTracking { get; init; } = true;

    /// <summary>Whether to enable state persistence.</summary>
    public bool EnableStatePersistence { get; init; } = true;

    /// <summary>Tools available to the loop.</summary>
    public IReadOnlyList<string> AvailableTools { get; init; } = Array.Empty<string>();

    /// <summary>Safety boundaries (actions that trigger termination).</summary>
    public IReadOnlyList<string> SafetyBoundaries { get; init; } = Array.Empty<string>();
}

/// <summary>
/// An iteration in the loop.
/// </summary>
public class LoopIteration
{
    /// <summary>Iteration number.</summary>
    public int IterationNumber { get; init; }

    /// <summary>Action taken.</summary>
    public required string Action { get; init; }

    /// <summary>Observation from action.</summary>
    public string? Observation { get; init; }

    /// <summary>Reasoning for the action.</summary>
    public string? Reasoning { get; init; }

    /// <summary>Whether progress was made.</summary>
    public bool MadeProgress { get; init; }

    /// <summary>Error if any.</summary>
    public string? Error { get; init; }

    /// <summary>Cost of this iteration.</summary>
    public decimal? Cost { get; init; }

    /// <summary>When the iteration started.</summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>Duration.</summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// State of an autonomous loop.
/// </summary>
public class AutonomousLoopState
{
    /// <summary>Loop identifier.</summary>
    public required string LoopId { get; init; }

    /// <summary>Goal of the loop.</summary>
    public required string Goal { get; init; }

    /// <summary>Current status.</summary>
    public LoopStatus Status { get; init; }

    /// <summary>Current iteration.</summary>
    public int CurrentIteration { get; init; }

    /// <summary>Total cost so far.</summary>
    public decimal TotalCost { get; init; }

    /// <summary>Consecutive errors.</summary>
    public int ConsecutiveErrors { get; init; }

    /// <summary>Total errors.</summary>
    public int TotalErrors { get; init; }

    /// <summary>Iterations without progress.</summary>
    public int IterationsWithoutProgress { get; init; }

    /// <summary>Progress towards goal (0.0 - 1.0).</summary>
    public double Progress { get; init; }

    /// <summary>Last checkpoint.</summary>
    public string? LastCheckpoint { get; init; }

    /// <summary>When the loop started.</summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>Elapsed time.</summary>
    public TimeSpan Elapsed { get; init; }

    /// <summary>Termination reason if terminated.</summary>
    public TerminationReason? TerminationReason { get; init; }

    /// <summary>Final result if completed.</summary>
    public string? FinalResult { get; init; }
}

/// <summary>
/// Port for continuous autonomous task loops with bounds.
/// Implements the "Continuous Autonomous Task Loop (bounded)" pattern.
/// </summary>
/// <remarks>
/// This port enables autonomous agent loops with explicit bounds
/// on iterations, time, budget, and errors, with progress tracking
/// and safety boundaries for controlled autonomy.
/// </remarks>
public interface IAutonomousLoopPort
{
    /// <summary>
    /// Starts an autonomous loop.
    /// </summary>
    /// <param name="goal">The goal to achieve.</param>
    /// <param name="configuration">Loop configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loop state.</returns>
    Task<AutonomousLoopState> StartLoopAsync(
        string goal,
        AutonomousLoopConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs one iteration of the loop.
    /// </summary>
    /// <param name="loopId">The loop ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The iteration result.</returns>
    Task<LoopIteration> RunIterationAsync(
        string loopId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the loop until completion or termination.
    /// </summary>
    /// <param name="loopId">The loop ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Final loop state.</returns>
    Task<AutonomousLoopState> RunToCompletionAsync(
        string loopId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current loop state.
    /// </summary>
    /// <param name="loopId">The loop ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loop state.</returns>
    Task<AutonomousLoopState> GetStateAsync(
        string loopId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets iterations from a loop.
    /// </summary>
    /// <param name="loopId">The loop ID.</param>
    /// <param name="skip">Iterations to skip.</param>
    /// <param name="take">Iterations to take.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Iterations.</returns>
    Task<IReadOnlyList<LoopIteration>> GetIterationsAsync(
        string loopId,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses a running loop.
    /// </summary>
    /// <param name="loopId">The loop ID.</param>
    /// <param name="reason">Reason for pausing.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PauseLoopAsync(
        string loopId,
        string? reason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused loop.
    /// </summary>
    /// <param name="loopId">The loop ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ResumeLoopAsync(
        string loopId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Terminates a loop.
    /// </summary>
    /// <param name="loopId">The loop ID.</param>
    /// <param name="reason">Termination reason.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task TerminateLoopAsync(
        string loopId,
        TerminationReason reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves human checkpoint to continue.
    /// </summary>
    /// <param name="loopId">The loop ID.</param>
    /// <param name="approvedBy">Who approved.</param>
    /// <param name="feedback">Optional feedback for the agent.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ApproveCheckpointAsync(
        string loopId,
        string approvedBy,
        string? feedback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates loop bounds mid-execution.
    /// </summary>
    /// <param name="loopId">The loop ID.</param>
    /// <param name="bounds">New bounds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateBoundsAsync(
        string loopId,
        LoopBounds bounds,
        CancellationToken cancellationToken = default);
}
