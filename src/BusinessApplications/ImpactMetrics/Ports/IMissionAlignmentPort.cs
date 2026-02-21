using CognitiveMesh.BusinessApplications.ImpactMetrics.Models;

namespace CognitiveMesh.BusinessApplications.ImpactMetrics.Ports;

/// <summary>
/// Defines the contract for assessing how well AI-driven decisions align with
/// an organisation's stated mission and values.
/// </summary>
public interface IMissionAlignmentPort
{
    /// <summary>
    /// Assesses the alignment of a specific decision with the provided mission statement.
    /// </summary>
    /// <param name="decisionId">The identifier of the decision being assessed.</param>
    /// <param name="decisionContext">A description of the decision and its context.</param>
    /// <param name="missionStatement">The organisation's mission statement to compare against.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The mission alignment assessment result.</returns>
    Task<MissionAlignment> AssessAlignmentAsync(
        string decisionId,
        string decisionContext,
        string missionStatement,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the alignment trend over time for a given tenant.
    /// </summary>
    /// <param name="tenantId">The tenant whose alignment trend to retrieve.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of historical alignment records ordered by assessment date.</returns>
    Task<IReadOnlyList<MissionAlignment>> GetAlignmentTrendAsync(
        string tenantId,
        CancellationToken cancellationToken = default);
}
