namespace CognitiveMesh.ReasoningLayer.NISTMaturity.Models;

/// <summary>
/// Represents a gap between the current maturity score and the target score for a NIST statement.
/// Gaps are prioritized and include recommended actions for remediation.
/// </summary>
/// <param name="StatementId">The statement where the gap exists.</param>
/// <param name="CurrentScore">The current maturity score (1-5).</param>
/// <param name="TargetScore">The target maturity score to achieve.</param>
/// <param name="Priority">Priority level based on the severity of the gap.</param>
/// <param name="RecommendedActions">List of recommended actions to close the gap.</param>
public sealed record MaturityGap(
    string StatementId,
    int CurrentScore,
    int TargetScore,
    GapPriority Priority,
    List<string> RecommendedActions);
