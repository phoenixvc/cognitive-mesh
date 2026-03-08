namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Handoff type.
/// </summary>
public enum HandoffType
{
    /// <summary>Background to foreground.</summary>
    BackgroundToForeground,
    /// <summary>Foreground to background.</summary>
    ForegroundToBackground,
    /// <summary>Agent to agent.</summary>
    AgentToAgent,
    /// <summary>Agent to human.</summary>
    AgentToHuman,
    /// <summary>Human to agent.</summary>
    HumanToAgent
}

/// <summary>
/// Handoff state.
/// </summary>
public enum HandoffState
{
    /// <summary>Handoff initiated.</summary>
    Initiated,
    /// <summary>Waiting for recipient.</summary>
    Pending,
    /// <summary>In progress.</summary>
    InProgress,
    /// <summary>Completed.</summary>
    Completed,
    /// <summary>Failed.</summary>
    Failed,
    /// <summary>Cancelled.</summary>
    Cancelled
}

/// <summary>
/// A handoff request.
/// </summary>
public class HandoffRequest
{
    /// <summary>Request identifier.</summary>
    public string RequestId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Handoff type.</summary>
    public HandoffType Type { get; init; }

    /// <summary>Source identifier.</summary>
    public required string SourceId { get; init; }

    /// <summary>Target identifier.</summary>
    public required string TargetId { get; init; }

    /// <summary>Task or context being handed off.</summary>
    public required string TaskId { get; init; }

    /// <summary>State to transfer (JSON).</summary>
    public string? State { get; init; }

    /// <summary>Message to recipient.</summary>
    public string? Message { get; init; }

    /// <summary>Priority.</summary>
    public int Priority { get; init; } = 100;

    /// <summary>Timeout for handoff.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>Whether seamless (no user notification).</summary>
    public bool Seamless { get; init; } = true;

    /// <summary>Requested at.</summary>
    public DateTimeOffset RequestedAt { get; init; }
}

/// <summary>
/// Handoff status.
/// </summary>
public class HandoffStatus
{
    /// <summary>Request identifier.</summary>
    public required string RequestId { get; init; }

    /// <summary>Current state.</summary>
    public HandoffState State { get; init; }

    /// <summary>Source acknowledged.</summary>
    public bool SourceAcknowledged { get; init; }

    /// <summary>Target acknowledged.</summary>
    public bool TargetAcknowledged { get; init; }

    /// <summary>State transferred.</summary>
    public bool StateTransferred { get; init; }

    /// <summary>Error if failed.</summary>
    public string? Error { get; init; }

    /// <summary>Started at.</summary>
    public DateTimeOffset? StartedAt { get; init; }

    /// <summary>Completed at.</summary>
    public DateTimeOffset? CompletedAt { get; init; }
}

/// <summary>
/// Handoff result.
/// </summary>
public class HandoffResult
{
    /// <summary>Request identifier.</summary>
    public required string RequestId { get; init; }

    /// <summary>Whether successful.</summary>
    public bool Success { get; init; }

    /// <summary>New owner after handoff.</summary>
    public string? NewOwnerId { get; init; }

    /// <summary>Error if failed.</summary>
    public string? Error { get; init; }

    /// <summary>Duration.</summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Port for seamless background-to-foreground handoff.
/// Implements the "Seamless Background-to-Foreground Handoff" pattern.
/// </summary>
public interface IHandoffPort
{
    /// <summary>
    /// Initiates a handoff.
    /// </summary>
    Task<HandoffStatus> InitiateHandoffAsync(
        HandoffRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledges a handoff as source.
    /// </summary>
    Task<HandoffStatus> AcknowledgeAsSourceAsync(
        string requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Accepts a handoff as target.
    /// </summary>
    Task<HandoffResult> AcceptHandoffAsync(
        string requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejects a handoff.
    /// </summary>
    Task<HandoffStatus> RejectHandoffAsync(
        string requestId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets handoff status.
    /// </summary>
    Task<HandoffStatus?> GetStatusAsync(
        string requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists pending handoffs for a target.
    /// </summary>
    Task<IReadOnlyList<HandoffRequest>> GetPendingHandoffsAsync(
        string targetId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a handoff.
    /// </summary>
    Task<HandoffStatus> CancelHandoffAsync(
        string requestId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams handoff events.
    /// </summary>
    IAsyncEnumerable<HandoffStatus> StreamHandoffEventsAsync(
        string? sourceId = null,
        string? targetId = null,
        CancellationToken cancellationToken = default);
}
