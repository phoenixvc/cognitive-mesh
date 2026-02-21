namespace AgencyLayer.CognitiveSandwich.Models;

/// <summary>
/// Represents an immutable audit trail entry recording a significant event
/// in a Cognitive Sandwich process, such as phase transitions, step-backs, or validations.
/// </summary>
public class PhaseAuditEntry
{
    /// <summary>
    /// Unique identifier for this audit entry.
    /// </summary>
    public string EntryId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identifier of the process this entry belongs to.
    /// </summary>
    public string ProcessId { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the phase this event relates to.
    /// </summary>
    public string PhaseId { get; set; } = string.Empty;

    /// <summary>
    /// Category of the audit event.
    /// </summary>
    public PhaseAuditEventType EventType { get; set; }

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Identifier of the user or system component that caused the event.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable details about the event.
    /// </summary>
    public string Details { get; set; } = string.Empty;
}
