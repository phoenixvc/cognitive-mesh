namespace CognitiveMesh.BusinessApplications.NISTCompliance.Models;

/// <summary>
/// Represents the NIST AI RMF maturity scores for an organization,
/// including overall and per-pillar scores.
/// </summary>
public class NISTScoreResponse
{
    /// <summary>
    /// The identifier of the organization being scored.
    /// </summary>
    public string OrganizationId { get; set; } = string.Empty;

    /// <summary>
    /// The overall maturity score across all pillars (0-5 scale).
    /// </summary>
    public double OverallScore { get; set; }

    /// <summary>
    /// The maturity scores broken down by pillar.
    /// </summary>
    public List<NISTChecklistPillarScore> PillarScores { get; set; } = new();

    /// <summary>
    /// The timestamp when this score was assessed.
    /// </summary>
    public DateTimeOffset AssessedAt { get; set; }
}
