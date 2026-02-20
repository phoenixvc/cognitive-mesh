namespace CognitiveMesh.AgencyLayer.RealTime.Models;

/// <summary>
/// DTO representing a change in an agent's operational status, broadcast to subscribed clients.
/// </summary>
/// <param name="AgentId">The unique identifier of the agent whose status changed.</param>
/// <param name="Status">The new status of the agent (e.g., "Active", "Idle", "Degraded").</param>
/// <param name="Timestamp">The timestamp when the status change occurred.</param>
/// <param name="Details">Optional human-readable details about the status change.</param>
public record AgentStatusUpdate(
    string AgentId,
    string Status,
    DateTimeOffset Timestamp,
    string? Details = null);
