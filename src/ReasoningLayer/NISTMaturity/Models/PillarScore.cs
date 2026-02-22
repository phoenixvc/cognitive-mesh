namespace CognitiveMesh.ReasoningLayer.NISTMaturity.Models;

/// <summary>
/// Represents an aggregated maturity score for a NIST AI RMF pillar.
/// Pillars group related statements and provide a high-level view of organizational maturity.
/// </summary>
/// <param name="PillarId">Identifier of the pillar (e.g., "GOV", "MAP", "MEA", "MAN").</param>
/// <param name="PillarName">Human-readable name of the pillar (e.g., "Govern", "Map").</param>
/// <param name="AverageScore">Average maturity score across all statements in this pillar.</param>
/// <param name="StatementCount">Number of statements assessed in this pillar.</param>
/// <param name="EvidenceCount">Total number of evidence items submitted for this pillar.</param>
/// <param name="LastUpdated">Timestamp of the most recent score update in this pillar.</param>
public sealed record PillarScore(
    string PillarId,
    string PillarName,
    double AverageScore,
    int StatementCount,
    int EvidenceCount,
    DateTimeOffset LastUpdated);
