namespace CognitiveMesh.ReasoningLayer.NISTMaturity.Models;

/// <summary>
/// Represents the maturity score assigned to a specific NIST AI RMF statement.
/// Scores range from 1 (initial) to 5 (optimized).
/// </summary>
/// <param name="StatementId">The statement being scored.</param>
/// <param name="Score">Maturity score from 1 (lowest) to 5 (highest).</param>
/// <param name="Rationale">Explanation of why this score was assigned.</param>
/// <param name="ScoredAt">Timestamp when the score was calculated.</param>
/// <param name="ScoredBy">Identifier of the scorer (system or human).</param>
public sealed record MaturityScore(
    string StatementId,
    int Score,
    string Rationale,
    DateTimeOffset ScoredAt,
    string ScoredBy);
