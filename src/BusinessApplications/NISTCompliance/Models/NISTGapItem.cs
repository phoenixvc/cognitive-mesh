namespace CognitiveMesh.BusinessApplications.NISTCompliance.Models;

/// <summary>
/// Represents a single compliance gap in the NIST AI RMF roadmap,
/// including the current score, target score, and recommended actions.
/// </summary>
public class NISTGapItem
{
    /// <summary>
    /// The identifier of the statement with the identified gap.
    /// </summary>
    public string StatementId { get; set; } = string.Empty;

    /// <summary>
    /// The current maturity score (1-5 scale).
    /// </summary>
    public int CurrentScore { get; set; }

    /// <summary>
    /// The target maturity score to achieve (1-5 scale).
    /// </summary>
    public int TargetScore { get; set; }

    /// <summary>
    /// The priority level of this gap (e.g., "Critical", "High", "Medium", "Low").
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Recommended actions to close this gap.
    /// </summary>
    public List<string> RecommendedActions { get; set; } = new();
}
