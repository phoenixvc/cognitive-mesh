using CognitiveMesh.BusinessApplications.NISTCompliance.Models;

namespace CognitiveMesh.BusinessApplications.NISTCompliance.Ports;

/// <summary>
/// Defines the contract for NIST AI RMF compliance services, including evidence management,
/// checklist tracking, scoring, review workflows, roadmap generation, and audit logging.
/// </summary>
public interface INISTComplianceServicePort
{
    /// <summary>
    /// Submits evidence for a NIST AI RMF compliance statement.
    /// </summary>
    /// <param name="request">The evidence submission request.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The evidence submission response with assigned identifier and status.</returns>
    Task<NISTEvidenceResponse> SubmitEvidenceAsync(NISTEvidenceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the complete NIST AI RMF compliance checklist for an organization,
    /// including all pillars and statements with their completion status.
    /// </summary>
    /// <param name="organizationId">The identifier of the organization.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The compliance checklist grouped by pillar.</returns>
    Task<NISTChecklistResponse> GetChecklistAsync(string organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the current NIST AI RMF maturity scores for an organization,
    /// broken down by pillar and overall.
    /// </summary>
    /// <param name="organizationId">The identifier of the organization.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The maturity scores by pillar and overall.</returns>
    Task<NISTScoreResponse> GetScoreAsync(string organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits a review decision for previously submitted evidence.
    /// </summary>
    /// <param name="request">The review request containing the decision.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The review response with updated status.</returns>
    Task<NISTReviewResponse> SubmitReviewAsync(NISTReviewRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an improvement roadmap identifying compliance gaps and prioritized actions.
    /// </summary>
    /// <param name="organizationId">The identifier of the organization.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The improvement roadmap with identified gaps.</returns>
    Task<NISTRoadmapResponse> GetRoadmapAsync(string organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the audit log of compliance activities for an organization.
    /// </summary>
    /// <param name="organizationId">The identifier of the organization.</param>
    /// <param name="maxResults">The maximum number of audit entries to return.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The audit log entries.</returns>
    Task<NISTAuditLogResponse> GetAuditLogAsync(string organizationId, int maxResults, CancellationToken cancellationToken = default);
}
