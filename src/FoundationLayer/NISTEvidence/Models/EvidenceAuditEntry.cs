namespace CognitiveMesh.FoundationLayer.NISTEvidence.Models;

/// <summary>
/// Represents an audit trail entry for a NIST evidence record,
/// capturing actions performed on the evidence over its lifecycle.
/// </summary>
/// <param name="EntryId">The unique identifier for this audit entry.</param>
/// <param name="Action">The action that was performed (e.g., "Created", "StatusChanged", "Archived").</param>
/// <param name="PerformedBy">The identifier of the user or system that performed the action.</param>
/// <param name="PerformedAt">The timestamp when the action was performed.</param>
/// <param name="Details">Additional details about the action performed.</param>
public sealed record EvidenceAuditEntry(
    Guid EntryId,
    string Action,
    string PerformedBy,
    DateTimeOffset PerformedAt,
    string Details);
