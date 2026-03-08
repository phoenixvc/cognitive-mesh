namespace BusinessApplications.Compliance.Ports;

/// <summary>
/// Type of audit event.
/// </summary>
public enum AuditEventType
{
    /// <summary>Decision made by agent.</summary>
    Decision,
    /// <summary>Action taken.</summary>
    Action,
    /// <summary>Data access.</summary>
    DataAccess,
    /// <summary>Configuration change.</summary>
    ConfigChange,
    /// <summary>Authentication event.</summary>
    Authentication,
    /// <summary>Authorization event.</summary>
    Authorization,
    /// <summary>Error or exception.</summary>
    Error,
    /// <summary>Human intervention.</summary>
    HumanIntervention
}

/// <summary>
/// An audit event.
/// </summary>
public class AuditEvent
{
    /// <summary>Event identifier.</summary>
    public string EventId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Event type.</summary>
    public required AuditEventType Type { get; init; }

    /// <summary>Event description.</summary>
    public required string Description { get; init; }

    /// <summary>Actor (who/what performed the action).</summary>
    public required string ActorId { get; init; }

    /// <summary>Actor type (user, agent, system).</summary>
    public required string ActorType { get; init; }

    /// <summary>Target resource.</summary>
    public string? TargetId { get; init; }

    /// <summary>Target type.</summary>
    public string? TargetType { get; init; }

    /// <summary>Action performed.</summary>
    public required string Action { get; init; }

    /// <summary>Outcome (success, failure, pending).</summary>
    public required string Outcome { get; init; }

    /// <summary>Reasoning or justification.</summary>
    public string? Reasoning { get; init; }

    /// <summary>Before state (JSON).</summary>
    public string? BeforeState { get; init; }

    /// <summary>After state (JSON).</summary>
    public string? AfterState { get; init; }

    /// <summary>IP address or source.</summary>
    public string? Source { get; init; }

    /// <summary>Session identifier.</summary>
    public string? SessionId { get; init; }

    /// <summary>Correlation ID for tracing.</summary>
    public string? CorrelationId { get; init; }

    /// <summary>When the event occurred.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Tags for categorization.</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>Additional metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();

    /// <summary>Hash of the event for integrity.</summary>
    public string? IntegrityHash { get; init; }
}

/// <summary>
/// Query for audit events.
/// </summary>
public class AuditQuery
{
    /// <summary>Filter by event type.</summary>
    public IReadOnlyList<AuditEventType>? EventTypes { get; init; }

    /// <summary>Filter by actor.</summary>
    public string? ActorId { get; init; }

    /// <summary>Filter by target.</summary>
    public string? TargetId { get; init; }

    /// <summary>Filter by action.</summary>
    public string? Action { get; init; }

    /// <summary>Filter by outcome.</summary>
    public string? Outcome { get; init; }

    /// <summary>Start time.</summary>
    public DateTimeOffset? Since { get; init; }

    /// <summary>End time.</summary>
    public DateTimeOffset? Until { get; init; }

    /// <summary>Filter by tags.</summary>
    public IReadOnlyList<string>? Tags { get; init; }

    /// <summary>Text search in description.</summary>
    public string? SearchText { get; init; }

    /// <summary>Maximum results.</summary>
    public int Limit { get; init; } = 100;

    /// <summary>Offset for pagination.</summary>
    public int Offset { get; init; } = 0;

    /// <summary>Sort order (asc, desc).</summary>
    public string SortOrder { get; init; } = "desc";
}

/// <summary>
/// Audit report.
/// </summary>
public class AuditReport
{
    /// <summary>Report identifier.</summary>
    public string ReportId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Report title.</summary>
    public required string Title { get; init; }

    /// <summary>Report period start.</summary>
    public DateTimeOffset PeriodStart { get; init; }

    /// <summary>Report period end.</summary>
    public DateTimeOffset PeriodEnd { get; init; }

    /// <summary>Total events in period.</summary>
    public int TotalEvents { get; init; }

    /// <summary>Events by type.</summary>
    public Dictionary<AuditEventType, int> EventsByType { get; init; } = new();

    /// <summary>Events by actor.</summary>
    public Dictionary<string, int> EventsByActor { get; init; } = new();

    /// <summary>Events by outcome.</summary>
    public Dictionary<string, int> EventsByOutcome { get; init; } = new();

    /// <summary>Anomalies detected.</summary>
    public IReadOnlyList<string> Anomalies { get; init; } = Array.Empty<string>();

    /// <summary>When generated.</summary>
    public DateTimeOffset GeneratedAt { get; init; }
}

/// <summary>
/// Port for comprehensive audit trail.
/// Implements the "Comprehensive Audit Trail" pattern.
/// </summary>
public interface IAuditTrailPort
{
    /// <summary>
    /// Records an audit event.
    /// </summary>
    Task RecordAsync(
        AuditEvent auditEvent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a decision with reasoning.
    /// </summary>
    Task RecordDecisionAsync(
        string actorId,
        string description,
        string reasoning,
        string outcome,
        string? targetId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records an action.
    /// </summary>
    Task RecordActionAsync(
        string actorId,
        string action,
        string description,
        string outcome,
        string? beforeState = null,
        string? afterState = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries audit events.
    /// </summary>
    Task<IReadOnlyList<AuditEvent>> QueryAsync(
        AuditQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an event by ID.
    /// </summary>
    Task<AuditEvent?> GetEventAsync(
        string eventId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets events for a correlation ID.
    /// </summary>
    Task<IReadOnlyList<AuditEvent>> GetCorrelatedEventsAsync(
        string correlationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an audit report.
    /// </summary>
    Task<AuditReport> GenerateReportAsync(
        string title,
        DateTimeOffset since,
        DateTimeOffset until,
        AuditQuery? additionalFilters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies integrity of audit events.
    /// </summary>
    Task<bool> VerifyIntegrityAsync(
        DateTimeOffset since,
        DateTimeOffset until,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports audit events.
    /// </summary>
    Task<string> ExportAsync(
        AuditQuery query,
        string format = "json",
        CancellationToken cancellationToken = default);
}
