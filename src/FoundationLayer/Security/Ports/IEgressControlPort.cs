namespace FoundationLayer.Security.Ports;

/// <summary>
/// Types of egress traffic.
/// </summary>
public enum EgressType
{
    /// <summary>HTTP/HTTPS traffic.</summary>
    Http,
    /// <summary>WebSocket connections.</summary>
    WebSocket,
    /// <summary>Raw TCP connections.</summary>
    Tcp,
    /// <summary>UDP traffic.</summary>
    Udp,
    /// <summary>Email (SMTP).</summary>
    Email,
    /// <summary>DNS queries.</summary>
    Dns
}

/// <summary>
/// Decision for an egress request.
/// </summary>
public enum EgressDecision
{
    /// <summary>Traffic is allowed.</summary>
    Allow,
    /// <summary>Traffic is blocked.</summary>
    Block,
    /// <summary>Traffic requires approval.</summary>
    RequiresApproval,
    /// <summary>Traffic is rate-limited.</summary>
    RateLimited
}

/// <summary>
/// Represents an egress allowlist rule.
/// </summary>
public class EgressRule
{
    /// <summary>Unique identifier for the rule.</summary>
    public string RuleId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Human-readable name.</summary>
    public required string Name { get; init; }

    /// <summary>Type of egress this rule applies to.</summary>
    public required EgressType Type { get; init; }

    /// <summary>Destination pattern (hostname, IP, CIDR, or regex).</summary>
    public required string DestinationPattern { get; init; }

    /// <summary>Port or port range (e.g., "443", "8080-8090").</summary>
    public string? Port { get; init; }

    /// <summary>Decision when this rule matches.</summary>
    public EgressDecision Decision { get; init; } = EgressDecision.Allow;

    /// <summary>Agents this rule applies to (empty = all agents).</summary>
    public IReadOnlyList<string> ApplicableAgents { get; init; } = Array.Empty<string>();

    /// <summary>Tools this rule applies to (empty = all tools).</summary>
    public IReadOnlyList<string> ApplicableTools { get; init; } = Array.Empty<string>();

    /// <summary>Rate limit if applicable (requests per minute).</summary>
    public int? RateLimitPerMinute { get; init; }

    /// <summary>Maximum data transfer in bytes per request.</summary>
    public long? MaxBytesPerRequest { get; init; }

    /// <summary>Whether to log matching traffic.</summary>
    public bool LogMatches { get; init; } = true;

    /// <summary>Rule priority (lower = higher priority).</summary>
    public int Priority { get; init; } = 100;
}

/// <summary>
/// Request to check egress permission.
/// </summary>
public class EgressCheckRequest
{
    /// <summary>The agent making the request.</summary>
    public required string AgentId { get; init; }

    /// <summary>The tool making the request (if applicable).</summary>
    public string? ToolId { get; init; }

    /// <summary>Type of egress.</summary>
    public required EgressType Type { get; init; }

    /// <summary>Destination hostname or IP.</summary>
    public required string Destination { get; init; }

    /// <summary>Destination port.</summary>
    public int? Port { get; init; }

    /// <summary>Estimated data size in bytes.</summary>
    public long? EstimatedBytes { get; init; }

    /// <summary>Purpose of the egress (for audit).</summary>
    public string? Purpose { get; init; }
}

/// <summary>
/// Result of an egress permission check.
/// </summary>
public class EgressCheckResult
{
    /// <summary>The decision.</summary>
    public required EgressDecision Decision { get; init; }

    /// <summary>The rule that matched (null if default deny).</summary>
    public string? MatchedRuleId { get; init; }

    /// <summary>Reason for the decision.</summary>
    public string? Reason { get; init; }

    /// <summary>Remaining rate limit quota.</summary>
    public int? RemainingQuota { get; init; }

    /// <summary>Approval request ID if RequiresApproval.</summary>
    public string? ApprovalRequestId { get; init; }

    /// <summary>Wait time in seconds if rate limited.</summary>
    public int? RetryAfterSeconds { get; init; }
}

/// <summary>
/// Record of egress traffic.
/// </summary>
public class EgressRecord
{
    public required string RecordId { get; init; }
    public required string AgentId { get; init; }
    public string? ToolId { get; init; }
    public required EgressType Type { get; init; }
    public required string Destination { get; init; }
    public int? Port { get; init; }
    public long BytesSent { get; init; }
    public long BytesReceived { get; init; }
    public required EgressDecision Decision { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public TimeSpan? Duration { get; init; }
}

/// <summary>
/// Port for egress lockdown and exfiltration prevention.
/// Implements the "Egress Lockdown (No-Exfiltration Channel)" pattern.
/// </summary>
/// <remarks>
/// This port provides network egress control to prevent data exfiltration
/// by agents and tools. All outbound traffic must be explicitly allowed
/// via allowlist rules, implementing a default-deny policy.
/// </remarks>
public interface IEgressControlPort
{
    /// <summary>
    /// Checks if egress traffic is permitted.
    /// </summary>
    /// <param name="request">The egress check request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The check result.</returns>
    Task<EgressCheckResult> CheckEgressAsync(
        EgressCheckRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records egress traffic (for audit and rate limiting).
    /// </summary>
    /// <param name="record">The egress record.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordEgressAsync(
        EgressRecord record,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds or updates an egress rule.
    /// </summary>
    /// <param name="rule">The rule to add or update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpsertRuleAsync(
        EgressRule rule,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an egress rule.
    /// </summary>
    /// <param name="ruleId">The rule ID to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveRuleAsync(
        string ruleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all egress rules.
    /// </summary>
    /// <param name="type">Filter by egress type (null = all).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of rules.</returns>
    Task<IReadOnlyList<EgressRule>> ListRulesAsync(
        EgressType? type = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets egress statistics for an agent.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <param name="since">Start time for statistics.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Egress statistics.</returns>
    Task<IReadOnlyList<EgressRecord>> GetEgressHistoryAsync(
        string agentId,
        DateTimeOffset since,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a pending egress request.
    /// </summary>
    /// <param name="approvalRequestId">The approval request ID.</param>
    /// <param name="approvedBy">Who approved the request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ApproveEgressRequestAsync(
        string approvalRequestId,
        string approvedBy,
        CancellationToken cancellationToken = default);
}
