namespace CognitiveMesh.BusinessApplications.NISTCompliance.Models;

/// <summary>
/// Represents a single NIST AI RMF compliance statement within a pillar,
/// including its completion status and evidence count.
/// </summary>
public class NISTChecklistStatement
{
    /// <summary>
    /// The unique identifier of the statement.
    /// </summary>
    public string StatementId { get; set; } = string.Empty;

    /// <summary>
    /// A human-readable description of the compliance statement.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this statement has been satisfied with sufficient evidence.
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// The number of evidence items submitted for this statement.
    /// </summary>
    public int EvidenceCount { get; set; }

    /// <summary>
    /// The current maturity score for this statement, if assessed (1-5 scale).
    /// </summary>
    public int? CurrentScore { get; set; }
}
