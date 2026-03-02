namespace CognitiveMesh.FoundationLayer.EvidenceArtifacts.Models;

/// <summary>
/// Represents an evidence artifact used in the Adaptive Balance (PRD-002) framework.
/// Artifacts capture supporting evidence from various sources for compliance tracking.
/// </summary>
public sealed class EvidenceArtifact
{
    /// <summary>
    /// The unique identifier for this artifact.
    /// </summary>
    public Guid ArtifactId { get; set; }

    /// <summary>
    /// The type of source that produced this artifact (e.g., "AuditLog", "TestResult", "PolicyDocument").
    /// </summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    /// The content or body of the artifact.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The identifier of the user or system that submitted this artifact.
    /// </summary>
    public string SubmittedBy { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when this artifact was submitted.
    /// </summary>
    public DateTimeOffset SubmittedAt { get; set; }

    /// <summary>
    /// A correlation identifier used to link related artifacts across systems.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Tags associated with this artifact for categorization and search.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// The data retention policy governing this artifact's lifecycle.
    /// </summary>
    public RetentionPolicy RetentionPolicy { get; set; } = RetentionPolicy.Indefinite;

    /// <summary>
    /// The date and time when this artifact expires, based on the retention policy.
    /// A <c>null</c> value indicates no expiration.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
}
