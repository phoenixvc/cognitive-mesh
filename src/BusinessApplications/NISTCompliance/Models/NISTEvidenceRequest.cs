namespace CognitiveMesh.BusinessApplications.NISTCompliance.Models;

/// <summary>
/// Represents a request to submit evidence for a NIST AI RMF compliance statement.
/// </summary>
public class NISTEvidenceRequest
{
    /// <summary>
    /// The identifier of the NIST statement this evidence is being submitted for.
    /// </summary>
    public string StatementId { get; set; } = string.Empty;

    /// <summary>
    /// The type of artifact being submitted (e.g., "Document", "Screenshot", "AuditReport").
    /// </summary>
    public string ArtifactType { get; set; } = string.Empty;

    /// <summary>
    /// The content or description of the evidence being submitted.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The identifier of the person submitting the evidence.
    /// </summary>
    public string SubmittedBy { get; set; } = string.Empty;

    /// <summary>
    /// Optional tags for categorizing the evidence.
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// The size of the submitted file in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }
}
