namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Type of human intervention required.
/// </summary>
public enum InterventionType
{
    /// <summary>Approval needed to proceed.</summary>
    Approval,
    /// <summary>Decision choice needed.</summary>
    Decision,
    /// <summary>Information input needed.</summary>
    Input,
    /// <summary>Review and feedback needed.</summary>
    Review,
    /// <summary>Escalation to human operator.</summary>
    Escalation,
    /// <summary>Confirmation of action.</summary>
    Confirmation
}

/// <summary>
/// Priority of the intervention request.
/// </summary>
public enum InterventionPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Status of an intervention request.
/// </summary>
public enum InterventionStatus
{
    /// <summary>Waiting for human response.</summary>
    Pending,
    /// <summary>Human is reviewing.</summary>
    InReview,
    /// <summary>Human has responded.</summary>
    Responded,
    /// <summary>Request timed out.</summary>
    TimedOut,
    /// <summary>Request was cancelled.</summary>
    Cancelled
}

/// <summary>
/// A request for human intervention.
/// </summary>
public class InterventionRequest
{
    /// <summary>Request identifier.</summary>
    public string RequestId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Type of intervention.</summary>
    public required InterventionType Type { get; init; }

    /// <summary>Priority.</summary>
    public InterventionPriority Priority { get; init; } = InterventionPriority.Medium;

    /// <summary>Title/summary.</summary>
    public required string Title { get; init; }

    /// <summary>Detailed description.</summary>
    public required string Description { get; init; }

    /// <summary>Context information.</summary>
    public string? Context { get; init; }

    /// <summary>Options for decision type.</summary>
    public IReadOnlyList<string> Options { get; init; } = Array.Empty<string>();

    /// <summary>Default option if timeout.</summary>
    public string? DefaultOption { get; init; }

    /// <summary>Timeout for response.</summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>Requesting agent.</summary>
    public required string AgentId { get; init; }

    /// <summary>Workflow/task ID.</summary>
    public string? WorkflowId { get; init; }

    /// <summary>When requested.</summary>
    public DateTimeOffset RequestedAt { get; init; }

    /// <summary>Metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// A response to an intervention request.
/// </summary>
public class InterventionResponse
{
    /// <summary>Request ID being responded to.</summary>
    public required string RequestId { get; init; }

    /// <summary>Response decision/choice.</summary>
    public required string Decision { get; init; }

    /// <summary>Additional feedback.</summary>
    public string? Feedback { get; init; }

    /// <summary>Input data if requested.</summary>
    public Dictionary<string, string> InputData { get; init; } = new();

    /// <summary>Who responded.</summary>
    public required string RespondedBy { get; init; }

    /// <summary>When responded.</summary>
    public DateTimeOffset RespondedAt { get; init; }

    /// <summary>Response time.</summary>
    public TimeSpan ResponseTime { get; init; }
}

/// <summary>
/// An intervention request with status.
/// </summary>
public class InterventionRequestStatus
{
    /// <summary>The request.</summary>
    public required InterventionRequest Request { get; init; }

    /// <summary>Current status.</summary>
    public InterventionStatus Status { get; init; }

    /// <summary>The response if available.</summary>
    public InterventionResponse? Response { get; init; }

    /// <summary>Time until timeout.</summary>
    public TimeSpan? TimeUntilTimeout { get; init; }
}

/// <summary>
/// Configuration for human-in-the-loop.
/// </summary>
public class HITLConfiguration
{
    /// <summary>Default timeout for requests.</summary>
    public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromHours(1);

    /// <summary>Whether to allow auto-approval on timeout.</summary>
    public bool AllowAutoApproval { get; init; } = false;

    /// <summary>Actions that always require approval.</summary>
    public IReadOnlyList<string> AlwaysRequireApproval { get; init; } = Array.Empty<string>();

    /// <summary>Actions that never require approval.</summary>
    public IReadOnlyList<string> NeverRequireApproval { get; init; } = Array.Empty<string>();

    /// <summary>Notification channels.</summary>
    public IReadOnlyList<string> NotificationChannels { get; init; } = Array.Empty<string>();

    /// <summary>Escalation timeout (auto-escalate if no response).</summary>
    public TimeSpan? EscalationTimeout { get; init; }
}

/// <summary>
/// Port for human-in-the-loop operations.
/// Implements the "Human-in-the-Loop (HITL)" pattern.
/// </summary>
public interface IHumanInTheLoopPort
{
    /// <summary>
    /// Requests human approval.
    /// </summary>
    Task<InterventionResponse> RequestApprovalAsync(
        string title,
        string description,
        string agentId,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests human decision.
    /// </summary>
    Task<InterventionResponse> RequestDecisionAsync(
        string title,
        string description,
        IReadOnlyList<string> options,
        string agentId,
        string? defaultOption = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests human input.
    /// </summary>
    Task<InterventionResponse> RequestInputAsync(
        string title,
        string description,
        IReadOnlyList<string> requiredFields,
        string agentId,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an intervention request.
    /// </summary>
    Task<string> CreateRequestAsync(
        InterventionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for a response to a request.
    /// </summary>
    Task<InterventionResponse> WaitForResponseAsync(
        string requestId,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Responds to a request.
    /// </summary>
    Task RespondAsync(
        InterventionResponse response,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending requests.
    /// </summary>
    Task<IReadOnlyList<InterventionRequestStatus>> GetPendingRequestsAsync(
        InterventionPriority? minPriority = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific request status.
    /// </summary>
    Task<InterventionRequestStatus?> GetRequestStatusAsync(
        string requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a pending request.
    /// </summary>
    Task CancelRequestAsync(
        string requestId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or sets configuration.
    /// </summary>
    Task<HITLConfiguration> ConfigureAsync(
        HITLConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if action requires approval.
    /// </summary>
    Task<bool> RequiresApprovalAsync(
        string action,
        string agentId,
        CancellationToken cancellationToken = default);
}
