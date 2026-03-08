namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Status of a canary deployment.
/// </summary>
public enum CanaryStatus
{
    /// <summary>Canary deployment is being prepared.</summary>
    Preparing,
    /// <summary>Canary is active and receiving traffic.</summary>
    Active,
    /// <summary>Canary is paused for investigation.</summary>
    Paused,
    /// <summary>Canary was rolled back.</summary>
    RolledBack,
    /// <summary>Canary was promoted to full deployment.</summary>
    Promoted,
    /// <summary>Canary failed and was automatically rolled back.</summary>
    Failed
}

/// <summary>
/// Type of change being canaried.
/// </summary>
public enum CanaryChangeType
{
    /// <summary>Agent policy or configuration change.</summary>
    AgentPolicy,
    /// <summary>Model or prompt change.</summary>
    ModelUpdate,
    /// <summary>Tool capability change.</summary>
    ToolUpdate,
    /// <summary>Authority scope modification.</summary>
    AuthorityScope,
    /// <summary>Workflow definition change.</summary>
    WorkflowDefinition
}

/// <summary>
/// Configuration for a canary deployment.
/// </summary>
public class CanaryConfiguration
{
    /// <summary>Unique identifier for this canary.</summary>
    public string CanaryId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Human-readable name.</summary>
    public required string Name { get; init; }

    /// <summary>Type of change being deployed.</summary>
    public required CanaryChangeType ChangeType { get; init; }

    /// <summary>Percentage of traffic to route to canary (1-100).</summary>
    public int TrafficPercentage { get; init; } = 5;

    /// <summary>Minimum duration before allowing promotion.</summary>
    public TimeSpan MinimumBakeTime { get; init; } = TimeSpan.FromMinutes(30);

    /// <summary>Maximum duration before automatic decision.</summary>
    public TimeSpan MaximumDuration { get; init; } = TimeSpan.FromHours(24);

    /// <summary>Whether to automatically rollback on failure.</summary>
    public bool AutoRollbackEnabled { get; init; } = true;

    /// <summary>Whether to automatically promote on success.</summary>
    public bool AutoPromoteEnabled { get; init; } = false;

    /// <summary>Success criteria for the canary.</summary>
    public CanarySuccessCriteria SuccessCriteria { get; init; } = new();

    /// <summary>Agents or workflows to include in the canary.</summary>
    public IReadOnlyList<string> TargetIds { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Success criteria for canary evaluation.
/// </summary>
public class CanarySuccessCriteria
{
    /// <summary>Maximum error rate allowed (0.0 - 1.0).</summary>
    public double MaxErrorRate { get; init; } = 0.01;

    /// <summary>Maximum latency increase percentage.</summary>
    public double MaxLatencyIncreasePercent { get; init; } = 10;

    /// <summary>Minimum success rate required (0.0 - 1.0).</summary>
    public double MinSuccessRate { get; init; } = 0.99;

    /// <summary>Minimum number of samples required for evaluation.</summary>
    public int MinSampleCount { get; init; } = 100;

    /// <summary>Custom metrics to evaluate (name → threshold).</summary>
    public Dictionary<string, double> CustomMetrics { get; init; } = new();
}

/// <summary>
/// Metrics collected during canary deployment.
/// </summary>
public class CanaryMetrics
{
    /// <summary>Total requests to canary.</summary>
    public int TotalRequests { get; init; }

    /// <summary>Successful requests.</summary>
    public int SuccessfulRequests { get; init; }

    /// <summary>Failed requests.</summary>
    public int FailedRequests { get; init; }

    /// <summary>Error rate (0.0 - 1.0).</summary>
    public double ErrorRate { get; init; }

    /// <summary>Average latency in milliseconds.</summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>P95 latency in milliseconds.</summary>
    public double P95LatencyMs { get; init; }

    /// <summary>P99 latency in milliseconds.</summary>
    public double P99LatencyMs { get; init; }

    /// <summary>Comparison with baseline (percentage difference).</summary>
    public Dictionary<string, double> BaselineComparison { get; init; } = new();

    /// <summary>Custom metric values.</summary>
    public Dictionary<string, double> CustomMetrics { get; init; } = new();

    /// <summary>When metrics were last updated.</summary>
    public DateTimeOffset LastUpdatedAt { get; init; }
}

/// <summary>
/// Represents an active canary deployment.
/// </summary>
public class CanaryDeployment
{
    /// <summary>The canary configuration.</summary>
    public required CanaryConfiguration Configuration { get; init; }

    /// <summary>Current status.</summary>
    public required CanaryStatus Status { get; init; }

    /// <summary>When the canary started.</summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>When the canary ended (if terminal state).</summary>
    public DateTimeOffset? EndedAt { get; init; }

    /// <summary>Current metrics.</summary>
    public CanaryMetrics? Metrics { get; init; }

    /// <summary>Whether success criteria are currently met.</summary>
    public bool? MeetsCriteria { get; init; }

    /// <summary>Reason for rollback or failure.</summary>
    public string? FailureReason { get; init; }

    /// <summary>Who triggered the current state.</summary>
    public string? TriggeredBy { get; init; }
}

/// <summary>
/// Port for canary rollout and automatic rollback.
/// Implements the "Canary Rollout and Automatic Rollback" pattern.
/// </summary>
/// <remarks>
/// This port enables safe deployment of agent policy and configuration changes
/// by routing a small percentage of traffic to the new version and monitoring
/// for regressions before full rollout.
/// </remarks>
public interface ICanaryRolloutPort
{
    /// <summary>
    /// Starts a new canary deployment.
    /// </summary>
    /// <param name="configuration">The canary configuration.</param>
    /// <param name="changePayload">The change to deploy (JSON or serialized config).</param>
    /// <param name="startedBy">Who started the canary.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The canary deployment.</returns>
    Task<CanaryDeployment> StartCanaryAsync(
        CanaryConfiguration configuration,
        string changePayload,
        string startedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current state of a canary.
    /// </summary>
    /// <param name="canaryId">The canary ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The canary deployment.</returns>
    Task<CanaryDeployment?> GetCanaryAsync(
        string canaryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists active canary deployments.
    /// </summary>
    /// <param name="changeType">Filter by change type (null = all).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active canaries.</returns>
    Task<IReadOnlyList<CanaryDeployment>> ListActiveCanariesAsync(
        CanaryChangeType? changeType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Promotes a canary to full deployment.
    /// </summary>
    /// <param name="canaryId">The canary to promote.</param>
    /// <param name="promotedBy">Who promoted the canary.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PromoteCanaryAsync(
        string canaryId,
        string promotedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back a canary deployment.
    /// </summary>
    /// <param name="canaryId">The canary to rollback.</param>
    /// <param name="reason">Reason for rollback.</param>
    /// <param name="rolledBackBy">Who triggered the rollback.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RollbackCanaryAsync(
        string canaryId,
        string reason,
        string rolledBackBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses a canary deployment for investigation.
    /// </summary>
    /// <param name="canaryId">The canary to pause.</param>
    /// <param name="reason">Reason for pausing.</param>
    /// <param name="pausedBy">Who paused the canary.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PauseCanaryAsync(
        string canaryId,
        string reason,
        string pausedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused canary.
    /// </summary>
    /// <param name="canaryId">The canary to resume.</param>
    /// <param name="resumedBy">Who resumed the canary.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ResumeCanaryAsync(
        string canaryId,
        string resumedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the traffic percentage for a canary.
    /// </summary>
    /// <param name="canaryId">The canary ID.</param>
    /// <param name="newPercentage">New traffic percentage (1-100).</param>
    /// <param name="updatedBy">Who made the change.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateTrafficPercentageAsync(
        string canaryId,
        int newPercentage,
        string updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if a request should be routed to canary or baseline.
    /// </summary>
    /// <param name="targetId">The agent or workflow ID.</param>
    /// <param name="requestId">Unique request identifier for consistent routing.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Canary ID if should route to canary, null for baseline.</returns>
    Task<string?> ShouldRouteToCanaryAsync(
        string targetId,
        string requestId,
        CancellationToken cancellationToken = default);
}
