namespace CognitiveMesh.BusinessApplications.ImpactMetrics.Models;

/// <summary>
/// Represents a calculated psychological safety score for a team,
/// measuring how safe the team feels about AI adoption across multiple dimensions.
/// </summary>
/// <param name="ScoreId">Unique identifier for this safety score record.</param>
/// <param name="TeamId">The identifier of the team being assessed.</param>
/// <param name="TenantId">The tenant to which the team belongs.</param>
/// <param name="OverallScore">The aggregate psychological safety score on a 0-100 scale.</param>
/// <param name="Dimensions">A breakdown of scores by individual safety dimension.</param>
/// <param name="SurveyResponseCount">The number of survey responses used in the calculation.</param>
/// <param name="BehavioralSignalCount">The number of behavioral signals used in the calculation.</param>
/// <param name="CalculatedAt">The timestamp when this score was calculated.</param>
/// <param name="ConfidenceLevel">The statistical confidence level based on data volume.</param>
public record PsychologicalSafetyScore(
    string ScoreId,
    string TeamId,
    string TenantId,
    double OverallScore,
    Dictionary<SafetyDimension, double> Dimensions,
    int SurveyResponseCount,
    int BehavioralSignalCount,
    DateTimeOffset CalculatedAt,
    ConfidenceLevel ConfidenceLevel);
