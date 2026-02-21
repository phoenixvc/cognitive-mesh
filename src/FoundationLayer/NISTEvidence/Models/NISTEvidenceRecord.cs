namespace CognitiveMesh.FoundationLayer.NISTEvidence.Models;

/// <summary>
/// Represents a NIST AI RMF evidence record that captures compliance artifacts
/// mapped to specific framework statements, topics, and pillars.
/// </summary>
public sealed class NISTEvidenceRecord
{
    /// <summary>
    /// The unique identifier for this evidence record.
    /// </summary>
    public Guid EvidenceId { get; set; }

    /// <summary>
    /// The NIST AI RMF statement identifier this evidence supports (e.g., "GV-1.1-001").
    /// </summary>
    public string StatementId { get; set; } = string.Empty;

    /// <summary>
    /// The NIST AI RMF topic identifier (e.g., "GV-1").
    /// </summary>
    public string TopicId { get; set; } = string.Empty;

    /// <summary>
    /// The NIST AI RMF pillar identifier (e.g., "GOVERN", "MAP", "MEASURE", "MANAGE").
    /// </summary>
    public string PillarId { get; set; } = string.Empty;

    /// <summary>
    /// The type of artifact (e.g., "Policy", "TestResult", "AuditLog", "Documentation").
    /// </summary>
    public string ArtifactType { get; set; } = string.Empty;

    /// <summary>
    /// The content or body of the evidence artifact.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The identifier of the user or system that submitted this evidence.
    /// </summary>
    public string SubmittedBy { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when this evidence was submitted.
    /// </summary>
    public DateTimeOffset SubmittedAt { get; set; }

    /// <summary>
    /// Tags associated with this evidence record for categorization and search.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// The file size of the evidence artifact in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// The identifier of the reviewer assigned to this evidence, if any.
    /// </summary>
    public string? ReviewerId { get; set; }

    /// <summary>
    /// The current review status of this evidence record.
    /// </summary>
    public EvidenceReviewStatus ReviewStatus { get; set; } = EvidenceReviewStatus.Pending;

    /// <summary>
    /// Notes provided by the reviewer, if any.
    /// </summary>
    public string? ReviewerNotes { get; set; }

    /// <summary>
    /// The version number of this evidence record, incremented on updates.
    /// </summary>
    public int VersionNumber { get; set; } = 1;

    /// <summary>
    /// Indicates whether this evidence record has been archived.
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// The audit trail capturing all actions performed on this evidence record.
    /// </summary>
    public List<EvidenceAuditEntry> AuditTrail { get; set; } = new();
}
