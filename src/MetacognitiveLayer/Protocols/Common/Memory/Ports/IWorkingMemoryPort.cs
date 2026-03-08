namespace MetacognitiveLayer.Protocols.Common.Memory.Ports;

/// <summary>
/// Status of a task in working memory.
/// </summary>
public enum TaskStatus
{
    /// <summary>Task is pending.</summary>
    Pending,
    /// <summary>Task is in progress.</summary>
    InProgress,
    /// <summary>Task is completed.</summary>
    Completed,
    /// <summary>Task is blocked.</summary>
    Blocked,
    /// <summary>Task was cancelled.</summary>
    Cancelled
}

/// <summary>
/// Priority of a task.
/// </summary>
public enum TaskPriority
{
    /// <summary>Low priority.</summary>
    Low,

    /// <summary>Medium priority.</summary>
    Medium,

    /// <summary>High priority.</summary>
    High,

    /// <summary>Critical priority.</summary>
    Critical
}

/// <summary>
/// A task in working memory.
/// </summary>
public class WorkingMemoryTask
{
    /// <summary>Unique identifier.</summary>
    public string TaskId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Task description.</summary>
    public required string Description { get; init; }

    /// <summary>Active form of the task (present continuous).</summary>
    public required string ActiveForm { get; init; }

    /// <summary>Current status.</summary>
    public TaskStatus Status { get; init; } = TaskStatus.Pending;

    /// <summary>Priority.</summary>
    public TaskPriority Priority { get; init; } = TaskPriority.Medium;

    /// <summary>Parent task ID (for subtasks).</summary>
    public string? ParentTaskId { get; init; }

    /// <summary>Dependencies (other task IDs).</summary>
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();

    /// <summary>Progress percentage (0-100).</summary>
    public int Progress { get; init; }

    /// <summary>Notes or context.</summary>
    public string? Notes { get; init; }

    /// <summary>When the task was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>When the task was last updated.</summary>
    public DateTimeOffset UpdatedAt { get; init; }

    /// <summary>When the task was completed.</summary>
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>Tags for categorization.</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>Metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// A note or fact in working memory.
/// </summary>
public class WorkingMemoryNote
{
    /// <summary>Unique identifier.</summary>
    public string NoteId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>The note content.</summary>
    public required string Content { get; init; }

    /// <summary>Category or type.</summary>
    public string? Category { get; init; }

    /// <summary>Related task ID.</summary>
    public string? RelatedTaskId { get; init; }

    /// <summary>Importance (0.0 - 1.0).</summary>
    public double Importance { get; init; } = 0.5;

    /// <summary>When the note was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>When the note expires (null = never).</summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>Tags.</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Complete working memory state.
/// </summary>
public class WorkingMemoryState
{
    /// <summary>Session ID.</summary>
    public required string SessionId { get; init; }

    /// <summary>All tasks.</summary>
    public IReadOnlyList<WorkingMemoryTask> Tasks { get; init; } = Array.Empty<WorkingMemoryTask>();

    /// <summary>All notes.</summary>
    public IReadOnlyList<WorkingMemoryNote> Notes { get; init; } = Array.Empty<WorkingMemoryNote>();

    /// <summary>Current goal or objective.</summary>
    public string? CurrentGoal { get; init; }

    /// <summary>Context variables.</summary>
    public Dictionary<string, string> Context { get; init; } = new();

    /// <summary>When the state was last modified.</summary>
    public DateTimeOffset LastModifiedAt { get; init; }
}

/// <summary>
/// Port for working memory (TodoWrite-style task tracking).
/// Implements the "Working Memory via TodoWrite" pattern.
/// </summary>
/// <remarks>
/// This port provides structured working memory for agents to track
/// tasks, notes, and context during complex multi-step operations.
/// It persists across session boundaries.
/// </remarks>
public interface IWorkingMemoryPort
{
    /// <summary>
    /// Gets the current working memory state.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The working memory state.</returns>
    Task<WorkingMemoryState> GetStateAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a task to working memory.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="task">The task to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The task ID.</returns>
    Task<string> AddTaskAsync(
        string sessionId,
        WorkingMemoryTask task,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a task.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="taskId">The task ID.</param>
    /// <param name="status">New status (null = no change).</param>
    /// <param name="progress">New progress (null = no change).</param>
    /// <param name="notes">Notes to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateTaskAsync(
        string sessionId,
        string taskId,
        TaskStatus? status = null,
        int? progress = null,
        string? notes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple tasks at once.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="tasks">Tasks to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task IDs.</returns>
    Task<IReadOnlyList<string>> AddTasksAsync(
        string sessionId,
        IEnumerable<WorkingMemoryTask> tasks,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a task.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="taskId">The task ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveTaskAsync(
        string sessionId,
        string taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a note to working memory.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="note">The note to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The note ID.</returns>
    Task<string> AddNoteAsync(
        string sessionId,
        WorkingMemoryNote note,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a note.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="noteId">The note ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveNoteAsync(
        string sessionId,
        string noteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the current goal.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="goal">The goal.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetGoalAsync(
        string sessionId,
        string goal,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a context variable.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetContextAsync(
        string sessionId,
        string key,
        string value,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears completed tasks.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of tasks cleared.</returns>
    Task<int> ClearCompletedTasksAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears expired notes.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of notes cleared.</returns>
    Task<int> ClearExpiredNotesAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all working memory for a session.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ClearAllAsync(
        string sessionId,
        CancellationToken cancellationToken = default);
}
