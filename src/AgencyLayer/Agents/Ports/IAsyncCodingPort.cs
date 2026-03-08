namespace AgencyLayer.Agents.Ports;

/// <summary>
/// A coding task.
/// </summary>
public class CodingTask
{
    /// <summary>Task identifier.</summary>
    public string TaskId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Task description.</summary>
    public required string Description { get; init; }

    /// <summary>Task type.</summary>
    public CodingTaskType Type { get; init; }

    /// <summary>Target files.</summary>
    public IReadOnlyList<string> TargetFiles { get; init; } = Array.Empty<string>();

    /// <summary>Context files.</summary>
    public IReadOnlyList<string> ContextFiles { get; init; } = Array.Empty<string>();

    /// <summary>Requirements.</summary>
    public IReadOnlyList<string> Requirements { get; init; } = Array.Empty<string>();

    /// <summary>Test requirements.</summary>
    public IReadOnlyList<string> TestRequirements { get; init; } = Array.Empty<string>();

    /// <summary>Priority.</summary>
    public int Priority { get; init; } = 100;

    /// <summary>Timeout.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromHours(1);

    /// <summary>Metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Coding task type.
/// </summary>
public enum CodingTaskType
{
    /// <summary>Implement new feature.</summary>
    Feature,
    /// <summary>Fix a bug.</summary>
    BugFix,
    /// <summary>Refactor code.</summary>
    Refactor,
    /// <summary>Add tests.</summary>
    Test,
    /// <summary>Update documentation.</summary>
    Documentation,
    /// <summary>Performance optimization.</summary>
    Optimization
}

/// <summary>
/// Task status.
/// </summary>
public enum CodingTaskStatus
{
    /// <summary>Queued.</summary>
    Queued,
    /// <summary>InProgress.</summary>
    InProgress,
    /// <summary>Review.</summary>
    Review,
    /// <summary>Testing.</summary>
    Testing,
    /// <summary>Completed.</summary>
    Completed,
    /// <summary>Failed.</summary>
    Failed,
    /// <summary>Cancelled.</summary>
    Cancelled
}

/// <summary>
/// Coding task result.
/// </summary>
public class CodingTaskResult
{
    /// <summary>Task identifier.</summary>
    public required string TaskId { get; init; }

    /// <summary>Status.</summary>
    public CodingTaskStatus Status { get; init; }

    /// <summary>Changes made.</summary>
    public IReadOnlyList<FileChange> Changes { get; init; } = Array.Empty<FileChange>();

    /// <summary>Summary of work.</summary>
    public string? Summary { get; init; }

    /// <summary>Tests added/modified.</summary>
    public IReadOnlyList<string> TestsModified { get; init; } = Array.Empty<string>();

    /// <summary>Test results.</summary>
    public TestResults? TestResults { get; init; }

    /// <summary>Commit SHA (if committed).</summary>
    public string? CommitSha { get; init; }

    /// <summary>Branch name.</summary>
    public string? BranchName { get; init; }

    /// <summary>Error if failed.</summary>
    public string? Error { get; init; }

    /// <summary>Duration.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Started at.</summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>Completed at.</summary>
    public DateTimeOffset? CompletedAt { get; init; }
}

/// <summary>
/// A file change.
/// </summary>
public class FileChange
{
    /// <summary>File path.</summary>
    public required string FilePath { get; init; }

    /// <summary>Change type.</summary>
    public FileChangeType ChangeType { get; init; }

    /// <summary>Diff.</summary>
    public string? Diff { get; init; }

    /// <summary>Lines added.</summary>
    public int LinesAdded { get; init; }

    /// <summary>Lines removed.</summary>
    public int LinesRemoved { get; init; }
}

/// <summary>
/// File change type.
/// </summary>
public enum FileChangeType
{
    /// <summary>Added.</summary>
    Added,
    /// <summary>Modified.</summary>
    Modified,
    /// <summary>Deleted.</summary>
    Deleted,
    /// <summary>Renamed.</summary>
    Renamed
}

/// <summary>
/// Test results.
/// </summary>
public class TestResults
{
    /// <summary>Total tests.</summary>
    public int Total { get; init; }

    /// <summary>Passed.</summary>
    public int Passed { get; init; }

    /// <summary>Failed.</summary>
    public int Failed { get; init; }

    /// <summary>All tests passed.</summary>
    public bool AllPassed => Failed == 0;

    /// <summary>Failed test names.</summary>
    public IReadOnlyList<string> FailedTests { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Port for asynchronous coding agent pipeline.
/// Implements the "Asynchronous Coding Agent Pipeline" pattern.
/// </summary>
public interface IAsyncCodingPort
{
    /// <summary>
    /// Submits a coding task.
    /// </summary>
    Task<string> SubmitTaskAsync(
        CodingTask task,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets task status.
    /// </summary>
    Task<CodingTaskResult?> GetTaskStatusAsync(
        string taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets task result.
    /// </summary>
    Task<CodingTaskResult> WaitForTaskAsync(
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
    /// Lists pending tasks.
    /// </summary>
    Task<IReadOnlyList<CodingTask>> GetPendingTasksAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists completed tasks.
    /// </summary>
    Task<IReadOnlyList<CodingTaskResult>> GetCompletedTasksAsync(
        int limit = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reviews task output before applying.
    /// </summary>
    Task<bool> ReviewTaskOutputAsync(
        string taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies task changes.
    /// </summary>
    Task<bool> ApplyTaskChangesAsync(
        string taskId,
        bool createCommit = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams task progress.
    /// </summary>
    IAsyncEnumerable<(string Stage, string Message)> StreamProgressAsync(
        string taskId,
        CancellationToken cancellationToken = default);
}
