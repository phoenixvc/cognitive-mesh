namespace CognitiveMesh.BusinessApplications.ImpactMetrics.Models;

/// <summary>
/// Represents the result of assessing how well an AI decision aligns with an
/// organisation's stated mission and values.
/// </summary>
/// <param name="AlignmentId">Unique identifier for this alignment assessment.</param>
/// <param name="DecisionId">The identifier of the decision being assessed.</param>
/// <param name="MissionStatementHash">A hash of the mission statement used for the assessment.</param>
/// <param name="AlignmentScore">Alignment score on a 0-1 scale where 1 is fully aligned.</param>
/// <param name="ValueMatches">List of mission values that the decision supports.</param>
/// <param name="Conflicts">List of conflicts where the decision contradicts stated values.</param>
/// <param name="AssessedAt">The timestamp when the alignment was assessed.</param>
public record MissionAlignment(
    string AlignmentId,
    string DecisionId,
    string MissionStatementHash,
    double AlignmentScore,
    List<string> ValueMatches,
    List<string> Conflicts,
    DateTimeOffset AssessedAt);
