namespace AgencyLayer.CognitiveSovereignty.Models;

/// <summary>
/// Represents an audit log entry recording a sovereignty-related event,
/// such as mode changes, overrides, or approval actions.
/// </summary>
public class SovereigntyAuditEntry
{
    /// <summary>
    /// Unique identifier for this audit entry.
    /// </summary>
    public string EntryId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identifier of the user affected by or initiating the action.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Description of the action that occurred (e.g., "ModeChanged", "OverrideCreated").
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// The sovereignty state before this action, if applicable.
    /// </summary>
    public string PreviousState { get; set; } = string.Empty;

    /// <summary>
    /// The sovereignty state after this action, if applicable.
    /// </summary>
    public string NewState { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable reason or justification for the action.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the action occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
