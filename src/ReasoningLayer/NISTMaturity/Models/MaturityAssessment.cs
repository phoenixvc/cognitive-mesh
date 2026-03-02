namespace CognitiveMesh.ReasoningLayer.NISTMaturity.Models;

/// <summary>
/// Represents a complete NIST AI RMF maturity assessment for an organization.
/// Aggregates pillar scores, individual statement scores, and an overall maturity level.
/// </summary>
public sealed class MaturityAssessment
{
    /// <summary>Gets or sets the unique identifier for this assessment.</summary>
    public Guid AssessmentId { get; set; }

    /// <summary>Gets or sets the organization being assessed.</summary>
    public string OrganizationId { get; set; } = string.Empty;

    /// <summary>Gets or sets the aggregated pillar-level scores.</summary>
    public List<PillarScore> PillarScores { get; set; } = [];

    /// <summary>Gets or sets the overall maturity score across all pillars.</summary>
    public double OverallScore { get; set; }

    /// <summary>Gets or sets the timestamp when the assessment was generated.</summary>
    public DateTimeOffset AssessedAt { get; set; }

    /// <summary>Gets or sets the individual statement-level scores.</summary>
    public List<MaturityScore> StatementScores { get; set; } = [];
}
