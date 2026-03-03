namespace CognitiveMesh.BusinessApplications.NISTCompliance.Models;

/// <summary>
/// Represents a single entry in the NIST AI RMF compliance audit trail.
/// </summary>
public class NISTAuditEntry
{
    /// <summary>
    /// The unique identifier for this audit entry.
    /// </summary>
    public Guid EntryId { get; set; }

    /// <summary>
    /// The type of action that was performed (e.g., "EvidenceSubmitted", "ReviewCompleted").
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// The identifier of the user who performed the action.
    /// </summary>
    public string PerformedBy { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when the action was performed.
    /// </summary>
    public DateTimeOffset PerformedAt { get; set; }

    /// <summary>
    /// Additional details about the action performed.
    /// </summary>
    public string Details { get; set; } = string.Empty;
}
