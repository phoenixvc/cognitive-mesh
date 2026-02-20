namespace CognitiveMesh.BusinessApplications.ImpactMetrics.Models;

/// <summary>
/// Records a single user interaction with an AI-enabled tool for adoption tracking.
/// </summary>
/// <param name="TelemetryId">Unique identifier for this telemetry record.</param>
/// <param name="UserId">The identifier of the user who performed the action.</param>
/// <param name="TenantId">The tenant to which the user belongs.</param>
/// <param name="ToolId">The identifier of the AI tool being used.</param>
/// <param name="Action">The type of action performed.</param>
/// <param name="Timestamp">When the action occurred.</param>
/// <param name="DurationMs">The duration of the action in milliseconds, if applicable.</param>
/// <param name="Context">Additional contextual information about the action.</param>
public record AdoptionTelemetry(
    string TelemetryId,
    string UserId,
    string TenantId,
    string ToolId,
    AdoptionAction Action,
    DateTimeOffset Timestamp,
    long? DurationMs,
    string? Context);
