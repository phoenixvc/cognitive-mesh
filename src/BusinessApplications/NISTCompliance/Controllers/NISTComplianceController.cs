using CognitiveMesh.BusinessApplications.NISTCompliance.Models;
using CognitiveMesh.BusinessApplications.NISTCompliance.Ports;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.BusinessApplications.NISTCompliance.Controllers;

/// <summary>
/// Controller for NIST AI RMF compliance management, providing operations for
/// evidence submission, checklist tracking, maturity scoring, reviews,
/// roadmap generation, and audit logging.
/// </summary>
public class NISTComplianceController
{
    private readonly ILogger<NISTComplianceController> _logger;
    private readonly INISTComplianceServicePort _servicePort;

    /// <summary>
    /// Initializes a new instance of the <see cref="NISTComplianceController"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for structured logging.</param>
    /// <param name="servicePort">The NIST compliance service port.</param>
    public NISTComplianceController(
        ILogger<NISTComplianceController> logger,
        INISTComplianceServicePort servicePort)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _servicePort = servicePort ?? throw new ArgumentNullException(nameof(servicePort));
    }

    /// <summary>
    /// Submits evidence for a NIST AI RMF compliance statement.
    /// </summary>
    /// <param name="request">The evidence submission request.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The evidence submission response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="ArgumentException">Thrown when required fields are missing.</exception>
    public async Task<NISTEvidenceResponse> SubmitEvidenceAsync(NISTEvidenceRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.StatementId))
        {
            throw new ArgumentException("StatementId is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.ArtifactType))
        {
            throw new ArgumentException("ArtifactType is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Content is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.SubmittedBy))
        {
            throw new ArgumentException("SubmittedBy is required.", nameof(request));
        }

        _logger.LogInformation(
            "Submitting evidence for statement {StatementId} by {SubmittedBy}",
            request.StatementId, request.SubmittedBy);

        var response = await _servicePort.SubmitEvidenceAsync(request, cancellationToken);

        _logger.LogInformation(
            "Evidence {EvidenceId} submitted successfully for statement {StatementId}",
            response.EvidenceId, request.StatementId);

        return response;
    }

    /// <summary>
    /// Retrieves the complete NIST AI RMF compliance checklist for an organization.
    /// </summary>
    /// <param name="organizationId">The identifier of the organization.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The compliance checklist grouped by pillar.</returns>
    /// <exception cref="ArgumentException">Thrown when organizationId is null or whitespace.</exception>
    public async Task<NISTChecklistResponse> GetChecklistAsync(string organizationId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(organizationId);

        _logger.LogInformation("Retrieving checklist for organization {OrganizationId}", organizationId);

        return await _servicePort.GetChecklistAsync(organizationId, cancellationToken);
    }

    /// <summary>
    /// Retrieves the current NIST AI RMF maturity scores for an organization.
    /// </summary>
    /// <param name="organizationId">The identifier of the organization.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The maturity scores by pillar and overall.</returns>
    /// <exception cref="ArgumentException">Thrown when organizationId is null or whitespace.</exception>
    public async Task<NISTScoreResponse> GetScoreAsync(string organizationId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(organizationId);

        _logger.LogInformation("Retrieving score for organization {OrganizationId}", organizationId);

        return await _servicePort.GetScoreAsync(organizationId, cancellationToken);
    }

    /// <summary>
    /// Submits a review decision for previously submitted evidence.
    /// </summary>
    /// <param name="request">The review request containing the decision.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The review response with updated status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="ArgumentException">Thrown when required fields are missing.</exception>
    public async Task<NISTReviewResponse> SubmitReviewAsync(NISTReviewRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.EvidenceId == Guid.Empty)
        {
            throw new ArgumentException("EvidenceId is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.ReviewerId))
        {
            throw new ArgumentException("ReviewerId is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Decision))
        {
            throw new ArgumentException("Decision is required.", nameof(request));
        }

        _logger.LogInformation(
            "Submitting review for evidence {EvidenceId} by {ReviewerId}",
            request.EvidenceId, request.ReviewerId);

        var response = await _servicePort.SubmitReviewAsync(request, cancellationToken);

        _logger.LogInformation(
            "Review for evidence {EvidenceId} completed with status {NewStatus}",
            response.EvidenceId, response.NewStatus);

        return response;
    }

    /// <summary>
    /// Generates an improvement roadmap identifying compliance gaps and prioritized actions.
    /// </summary>
    /// <param name="organizationId">The identifier of the organization.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The improvement roadmap with identified gaps.</returns>
    /// <exception cref="ArgumentException">Thrown when organizationId is null or whitespace.</exception>
    public async Task<NISTRoadmapResponse> GetRoadmapAsync(string organizationId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(organizationId);

        _logger.LogInformation("Generating roadmap for organization {OrganizationId}", organizationId);

        return await _servicePort.GetRoadmapAsync(organizationId, cancellationToken);
    }

    /// <summary>
    /// Retrieves the audit log of compliance activities for an organization.
    /// </summary>
    /// <param name="organizationId">The identifier of the organization.</param>
    /// <param name="maxResults">The maximum number of audit entries to return.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The audit log entries.</returns>
    /// <exception cref="ArgumentException">Thrown when organizationId is null or whitespace.</exception>
    public async Task<NISTAuditLogResponse> GetAuditLogAsync(string organizationId, int maxResults, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(organizationId);

        if (maxResults <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxResults), "maxResults must be greater than zero.");
        }

        _logger.LogInformation(
            "Retrieving audit log for organization {OrganizationId} (max {MaxResults})",
            organizationId, maxResults);

        return await _servicePort.GetAuditLogAsync(organizationId, maxResults, cancellationToken);
    }
}
