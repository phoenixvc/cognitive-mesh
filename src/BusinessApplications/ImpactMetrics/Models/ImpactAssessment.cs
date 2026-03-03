namespace CognitiveMesh.BusinessApplications.ImpactMetrics.Models;

/// <summary>
/// Represents an impact assessment for a tenant over a specific time period,
/// aggregating safety, alignment, adoption, and productivity metrics.
/// </summary>
/// <param name="AssessmentId">Unique identifier for this assessment.</param>
/// <param name="TenantId">The tenant being assessed.</param>
/// <param name="PeriodStart">Start of the assessment period.</param>
/// <param name="PeriodEnd">End of the assessment period.</param>
/// <param name="ProductivityDelta">Change in productivity as a percentage (-1 to 1).</param>
/// <param name="QualityDelta">Change in quality as a percentage (-1 to 1).</param>
/// <param name="TimeToDecisionDelta">Change in time-to-decision as a percentage (-1 to 1, negative means faster).</param>
/// <param name="UserSatisfactionScore">User satisfaction score on a 0-100 scale.</param>
/// <param name="AdoptionRate">AI tool adoption rate as a percentage (0-1).</param>
/// <param name="ResistanceIndicators">List of detected resistance patterns.</param>
public record ImpactAssessment(
    string AssessmentId,
    string TenantId,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    double ProductivityDelta,
    double QualityDelta,
    double TimeToDecisionDelta,
    double UserSatisfactionScore,
    double AdoptionRate,
    List<ResistanceIndicator> ResistanceIndicators);
