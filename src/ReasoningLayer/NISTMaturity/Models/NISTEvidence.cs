namespace CognitiveMesh.ReasoningLayer.NISTMaturity.Models;

/// <summary>
/// Represents a piece of evidence submitted to support a NIST AI RMF maturity assessment.
/// Evidence is associated with a specific statement and undergoes a review process.
/// </summary>
/// <param name="EvidenceId">Unique identifier for this evidence artifact.</param>
/// <param name="StatementId">The statement this evidence supports.</param>
/// <param name="ArtifactType">Type of artifact (e.g., "Document", "Policy", "Screenshot").</param>
/// <param name="Content">The content or description of the evidence.</param>
/// <param name="SubmittedBy">Identifier of the person who submitted the evidence.</param>
/// <param name="SubmittedAt">Timestamp when the evidence was submitted.</param>
/// <param name="Tags">Tags for categorization and searchability.</param>
/// <param name="FileSizeBytes">Size of the evidence file in bytes.</param>
/// <param name="ReviewerId">Identifier of the reviewer, if assigned.</param>
/// <param name="ReviewStatus">Current review status of the evidence.</param>
public sealed record NISTEvidence(
    Guid EvidenceId,
    string StatementId,
    string ArtifactType,
    string Content,
    string SubmittedBy,
    DateTimeOffset SubmittedAt,
    List<string> Tags,
    long FileSizeBytes,
    string? ReviewerId,
    ReviewStatus ReviewStatus);
