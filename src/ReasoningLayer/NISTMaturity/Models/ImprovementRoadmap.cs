namespace CognitiveMesh.ReasoningLayer.NISTMaturity.Models;

/// <summary>
/// Represents an improvement roadmap generated from a NIST AI RMF maturity assessment.
/// Identifies gaps between current and target maturity levels with prioritized recommendations.
/// </summary>
/// <param name="RoadmapId">Unique identifier for this roadmap.</param>
/// <param name="OrganizationId">The organization this roadmap is for.</param>
/// <param name="Gaps">List of identified maturity gaps with recommended actions.</param>
/// <param name="GeneratedAt">Timestamp when the roadmap was generated.</param>
public sealed record ImprovementRoadmap(
    Guid RoadmapId,
    string OrganizationId,
    List<MaturityGap> Gaps,
    DateTimeOffset GeneratedAt);
