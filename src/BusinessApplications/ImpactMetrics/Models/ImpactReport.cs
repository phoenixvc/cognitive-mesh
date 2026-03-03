namespace CognitiveMesh.BusinessApplications.ImpactMetrics.Models;

/// <summary>
/// Represents a comprehensive impact report for a tenant, aggregating psychological
/// safety, mission alignment, adoption rates, and overall impact into a single
/// document with actionable recommendations.
/// </summary>
/// <param name="ReportId">Unique identifier for this report.</param>
/// <param name="TenantId">The tenant for which the report was generated.</param>
/// <param name="PeriodStart">Start of the reporting period.</param>
/// <param name="PeriodEnd">End of the reporting period.</param>
/// <param name="SafetyScore">The aggregate psychological safety score (0-100).</param>
/// <param name="AlignmentScore">The aggregate mission alignment score (0-1).</param>
/// <param name="AdoptionRate">The AI tool adoption rate (0-1).</param>
/// <param name="OverallImpactScore">The weighted overall impact score (0-100).</param>
/// <param name="Recommendations">Actionable recommendations based on the analysis.</param>
/// <param name="GeneratedAt">The timestamp when this report was generated.</param>
public record ImpactReport(
    string ReportId,
    string TenantId,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    double SafetyScore,
    double AlignmentScore,
    double AdoptionRate,
    double OverallImpactScore,
    List<string> Recommendations,
    DateTimeOffset GeneratedAt);
