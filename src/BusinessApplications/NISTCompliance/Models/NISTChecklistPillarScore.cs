namespace CognitiveMesh.BusinessApplications.NISTCompliance.Models;

/// <summary>
/// Represents the maturity score for a single NIST AI RMF pillar.
/// </summary>
public class NISTChecklistPillarScore
{
    /// <summary>
    /// The unique identifier of the pillar.
    /// </summary>
    public string PillarId { get; set; } = string.Empty;

    /// <summary>
    /// The human-readable name of the pillar.
    /// </summary>
    public string PillarName { get; set; } = string.Empty;

    /// <summary>
    /// The average maturity score for this pillar (0-5 scale).
    /// </summary>
    public double AverageScore { get; set; }

    /// <summary>
    /// The number of statements within this pillar.
    /// </summary>
    public int StatementCount { get; set; }
}
