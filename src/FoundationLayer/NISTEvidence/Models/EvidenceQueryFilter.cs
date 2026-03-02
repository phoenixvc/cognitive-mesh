namespace CognitiveMesh.FoundationLayer.NISTEvidence.Models;

/// <summary>
/// Filter criteria for querying NIST evidence records.
/// All filter properties are optional; when set, they are combined with AND logic.
/// </summary>
public class EvidenceQueryFilter
{
    /// <summary>
    /// Filter by NIST AI RMF statement identifier.
    /// </summary>
    public string? StatementId { get; set; }

    /// <summary>
    /// Filter by NIST AI RMF pillar identifier (e.g., "GOVERN", "MAP", "MEASURE", "MANAGE").
    /// </summary>
    public string? PillarId { get; set; }

    /// <summary>
    /// Filter by NIST AI RMF topic identifier.
    /// </summary>
    public string? TopicId { get; set; }

    /// <summary>
    /// Filter by review status.
    /// </summary>
    public EvidenceReviewStatus? ReviewStatus { get; set; }

    /// <summary>
    /// Filter to include only records submitted after this date.
    /// </summary>
    public DateTimeOffset? SubmittedAfter { get; set; }

    /// <summary>
    /// Filter to include only records submitted before this date.
    /// </summary>
    public DateTimeOffset? SubmittedBefore { get; set; }

    /// <summary>
    /// The maximum number of results to return. Defaults to 100.
    /// </summary>
    public int MaxResults { get; set; } = 100;
}
