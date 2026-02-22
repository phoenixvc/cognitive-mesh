using CognitiveMesh.BusinessApplications.ImpactMetrics.Models;

namespace CognitiveMesh.BusinessApplications.ImpactMetrics.Ports;

/// <summary>
/// Defines the contract for generating comprehensive impact assessments and reports
/// that aggregate psychological safety, mission alignment, and adoption metrics.
/// </summary>
public interface IImpactAssessmentPort
{
    /// <summary>
    /// Generates an impact assessment for a tenant over a specified time period.
    /// </summary>
    /// <param name="tenantId">The tenant to assess.</param>
    /// <param name="periodStart">Start of the assessment period.</param>
    /// <param name="periodEnd">End of the assessment period.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The generated impact assessment.</returns>
    Task<ImpactAssessment> GenerateAssessmentAsync(
        string tenantId,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a comprehensive impact report for a tenant, including safety scores,
    /// alignment metrics, adoption rates, and actionable recommendations.
    /// </summary>
    /// <param name="tenantId">The tenant for which to generate the report.</param>
    /// <param name="periodStart">Start of the reporting period.</param>
    /// <param name="periodEnd">End of the reporting period.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The comprehensive impact report.</returns>
    Task<ImpactReport> GenerateReportAsync(
        string tenantId,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        CancellationToken cancellationToken = default);
}
