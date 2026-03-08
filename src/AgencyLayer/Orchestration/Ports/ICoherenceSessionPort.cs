namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Status of a long-running session.
/// </summary>
public enum SessionStatus
{
    /// <summary>Session is initializing.</summary>
    Initializing,
    /// <summary>Session is active and running.</summary>
    Active,
    /// <summary>Session is paused.</summary>
    Paused,
    /// <summary>Session is in maintenance mode.</summary>
    Maintenance,
    /// <summary>Session is completing final tasks.</summary>
    Completing,
    /// <summary>Session has completed.</summary>
    Completed,
    /// <summary>Session encountered an error.</summary>
    Error
}

/// <summary>
/// Configuration for extended coherence sessions.
/// </summary>
public class CoherenceSessionConfiguration
{
    /// <summary>Maximum session duration.</summary>
    public TimeSpan MaxDuration { get; init; } = TimeSpan.FromHours(8);

    /// <summary>Context consolidation interval.</summary>
    public TimeSpan ConsolidationInterval { get; init; } = TimeSpan.FromMinutes(30);

    /// <summary>Memory checkpoint interval.</summary>
    public TimeSpan CheckpointInterval { get; init; } = TimeSpan.FromMinutes(15);

    /// <summary>Maximum context tokens before compaction.</summary>
    public int MaxContextTokens { get; init; } = 100000;

    /// <summary>Target context tokens after compaction.</summary>
    public int TargetContextTokens { get; init; } = 50000;

    /// <summary>Whether to enable automatic task decomposition.</summary>
    public bool EnableAutoDecomposition { get; init; } = true;

    /// <summary>Whether to enable progress reporting.</summary>
    public bool EnableProgressReporting { get; init; } = true;

    /// <summary>Progress report interval.</summary>
    public TimeSpan ProgressReportInterval { get; init; } = TimeSpan.FromMinutes(10);
}

/// <summary>
/// A checkpoint in a coherence session.
/// </summary>
public class SessionCheckpoint
{
    /// <summary>Checkpoint identifier.</summary>
    public string CheckpointId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Session ID.</summary>
    public required string SessionId { get; init; }

    /// <summary>Checkpoint number.</summary>
    public int CheckpointNumber { get; init; }

    /// <summary>Consolidated context at this point.</summary>
    public required string ConsolidatedContext { get; init; }

    /// <summary>Active tasks at this point.</summary>
    public IReadOnlyList<string> ActiveTasks { get; init; } = Array.Empty<string>();

    /// <summary>Completed tasks at this point.</summary>
    public IReadOnlyList<string> CompletedTasks { get; init; } = Array.Empty<string>();

    /// <summary>Progress percentage.</summary>
    public double Progress { get; init; }

    /// <summary>Token count at this point.</summary>
    public int TokenCount { get; init; }

    /// <summary>When the checkpoint was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// An extended coherence session.
/// </summary>
public class CoherenceSession
{
    /// <summary>Session identifier.</summary>
    public required string SessionId { get; init; }

    /// <summary>Session goal or objective.</summary>
    public required string Goal { get; init; }

    /// <summary>Current status.</summary>
    public SessionStatus Status { get; init; }

    /// <summary>Configuration.</summary>
    public required CoherenceSessionConfiguration Configuration { get; init; }

    /// <summary>When the session started.</summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>Expected completion time.</summary>
    public DateTimeOffset? ExpectedCompletionAt { get; init; }

    /// <summary>When the session ended.</summary>
    public DateTimeOffset? EndedAt { get; init; }

    /// <summary>Current progress percentage.</summary>
    public double Progress { get; init; }

    /// <summary>Latest checkpoint ID.</summary>
    public string? LatestCheckpointId { get; init; }

    /// <summary>Total checkpoints created.</summary>
    public int TotalCheckpoints { get; init; }

    /// <summary>Current context token count.</summary>
    public int CurrentTokenCount { get; init; }

    /// <summary>Context consolidations performed.</summary>
    public int ConsolidationCount { get; init; }
}

/// <summary>
/// Progress report for a session.
/// </summary>
public class SessionProgressReport
{
    /// <summary>Session ID.</summary>
    public required string SessionId { get; init; }

    /// <summary>Current status.</summary>
    public required SessionStatus Status { get; init; }

    /// <summary>Progress percentage.</summary>
    public double Progress { get; init; }

    /// <summary>Tasks completed.</summary>
    public int TasksCompleted { get; init; }

    /// <summary>Tasks remaining.</summary>
    public int TasksRemaining { get; init; }

    /// <summary>Current task description.</summary>
    public string? CurrentTask { get; init; }

    /// <summary>Elapsed time.</summary>
    public TimeSpan Elapsed { get; init; }

    /// <summary>Estimated remaining time.</summary>
    public TimeSpan? EstimatedRemaining { get; init; }

    /// <summary>Recent accomplishments.</summary>
    public IReadOnlyList<string> RecentAccomplishments { get; init; } = Array.Empty<string>();

    /// <summary>Blockers or issues.</summary>
    public IReadOnlyList<string> Blockers { get; init; } = Array.Empty<string>();

    /// <summary>When the report was generated.</summary>
    public DateTimeOffset GeneratedAt { get; init; }
}

/// <summary>
/// Port for extended coherence work sessions.
/// Implements the "Extended Coherence Work Sessions" pattern.
/// </summary>
/// <remarks>
/// This port enables long-running agent sessions with automatic
/// context consolidation, checkpointing, and progress tracking
/// to maintain coherence over extended work periods.
/// </remarks>
public interface ICoherenceSessionPort
{
    /// <summary>
    /// Starts a new coherence session.
    /// </summary>
    /// <param name="goal">The session goal.</param>
    /// <param name="configuration">Session configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The new session.</returns>
    Task<CoherenceSession> StartSessionAsync(
        string goal,
        CoherenceSessionConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a session by ID.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The session.</returns>
    Task<CoherenceSession?> GetSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a checkpoint.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="context">Current context to consolidate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The checkpoint.</returns>
    Task<SessionCheckpoint> CreateCheckpointAsync(
        string sessionId,
        string context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores from a checkpoint.
    /// </summary>
    /// <param name="checkpointId">The checkpoint ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The restored context.</returns>
    Task<string> RestoreFromCheckpointAsync(
        string checkpointId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consolidates context to reduce token count.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="currentContext">Current context.</param>
    /// <param name="targetTokens">Target token count.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Consolidated context.</returns>
    Task<string> ConsolidateContextAsync(
        string sessionId,
        string currentContext,
        int targetTokens,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates session progress.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="progress">Progress percentage.</param>
    /// <param name="currentTask">Current task description.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateProgressAsync(
        string sessionId,
        double progress,
        string? currentTask = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a progress report.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The progress report.</returns>
    Task<SessionProgressReport> GetProgressReportAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses a session.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="reason">Reason for pausing.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PauseSessionAsync(
        string sessionId,
        string? reason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused session.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ResumeSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes a session.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="summary">Final summary.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CompleteSessionAsync(
        string sessionId,
        string summary,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets session checkpoints.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All checkpoints.</returns>
    Task<IReadOnlyList<SessionCheckpoint>> GetCheckpointsAsync(
        string sessionId,
        CancellationToken cancellationToken = default);
}
