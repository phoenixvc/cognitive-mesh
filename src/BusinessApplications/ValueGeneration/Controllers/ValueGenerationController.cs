using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CognitiveMesh.BusinessApplications.Common.Models;
using CognitiveMesh.BusinessApplications.ConvenerServices.Ports;
using CognitiveMesh.BusinessApplications.ConvenerServices.Ports.Models;
using CognitiveMesh.FoundationLayer.AuditLogging;
using CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;
using CognitiveMesh.ReasoningLayer.ValueGeneration.Ports.Models;

namespace CognitiveMesh.BusinessApplications.ValueGeneration.Controllers
{
    /// <summary>
    /// API controller for the Value Generation System.
    /// This controller exposes REST API endpoints for value diagnostics, organizational blindness detection,
    /// and employability checks, integrating with the Reasoning Layer and enforcing consent and manual review processes.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class ValueGenerationController : ControllerBase
    {
        private readonly IValueDiagnosticPort _valueDiagnosticPort;
        private readonly IOrgBlindnessDetectionPort _orgBlindnessPort;
        private readonly IEmployabilityPort _employabilityPort;
        private readonly IConsentPort _consentPort;
        private readonly IManualAdjudicationPort _manualAdjudicationPort;
        private readonly IAuditLoggingAdapter _auditLogger;
        private readonly ILogger<ValueGenerationController> _logger;

        /// <summary>
        /// Initializes a new instance of the ValueGenerationController class.
        /// </summary>
        public ValueGenerationController(
            IValueDiagnosticPort valueDiagnosticPort,
            IOrgBlindnessDetectionPort orgBlindnessPort,
            IEmployabilityPort employabilityPort,
            IConsentPort consentPort,
            IManualAdjudicationPort manualAdjudicationPort,
            IAuditLoggingAdapter auditLogger,
            ILogger<ValueGenerationController> logger)
        {
            _valueDiagnosticPort = valueDiagnosticPort ?? throw new ArgumentNullException(nameof(valueDiagnosticPort));
            _orgBlindnessPort = orgBlindnessPort ?? throw new ArgumentNullException(nameof(orgBlindnessPort));
            _employabilityPort = employabilityPort ?? throw new ArgumentNullException(nameof(employabilityPort));
            _consentPort = consentPort ?? throw new ArgumentNullException(nameof(consentPort));
            _manualAdjudicationPort = manualAdjudicationPort ?? throw new ArgumentNullException(nameof(manualAdjudicationPort));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Value Diagnostic Endpoints

        /// <summary>
        /// Runs a value diagnostic for a user or team.
        /// </summary>
        /// <param name="request">The request containing the target and context for the diagnostic.</param>
        /// <returns>The value diagnostic results, including score, profile, strengths, and development opportunities.</returns>
        /// <response code="200">Returns the value diagnostic results.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission or required consent is missing.</response>
        /// <response code="500">If an unexpected error occurs.</response>
        [HttpPost("value-diagnostic")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ValueDiagnosticResponse>> RunValueDiagnosticAsync([FromBody] ValueDiagnosticApiRequest request)
        {
            var correlationId = Guid.NewGuid().ToString();
            _logger.LogInformation("Value diagnostic requested for target {TargetId} with correlation ID {CorrelationId}", 
                request.TargetId, correlationId);

            try
            {
                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(ErrorEnvelope.InvalidPayload("Invalid request payload", correlationId));
                }

                // Check for consent if the target is a user
                if (request.TargetType == "User")
                {
                    var consentRequest = new ValidateConsentRequest
                    {
                        UserId = request.TargetId,
                        TenantId = request.TenantId,
                        RequiredConsentType = ConsentTypes.ValueDiagnosticDataCollection
                    };

                    var consentResponse = await _consentPort.ValidateConsentAsync(consentRequest);
                    if (!consentResponse.HasConsent)
                    {
                        await _auditLogger.LogEventAsync(new AuditEvent
                        {
                            EventType = "ValueDiagnostic.ConsentMissing",
                            EventCategory = "Consent",
                            UserId = GetCurrentUserId(),
                            TargetId = request.TargetId,
                            CorrelationId = correlationId,
                            EventData = JsonSerializer.Serialize(new { request.TenantId, request.TargetType })
                        });

                        return Forbid(ErrorEnvelope.ConsentMissing(
                            $"Consent '{ConsentTypes.ValueDiagnosticDataCollection}' is required for value diagnostics", 
                            correlationId));
                    }
                }

                // Create the domain request
                var domainRequest = new ValueDiagnosticRequest
                {
                    TargetId = request.TargetId,
                    TargetType = request.TargetType,
                    Provenance = new ProvenanceContext
                    {
                        TenantId = request.TenantId,
                        ActorId = GetCurrentUserId(),
                        CorrelationId = correlationId
                    }
                };

                // Call the domain service
                var result = await _valueDiagnosticPort.RunValueDiagnosticAsync(domainRequest);

                // Log the event
                await _auditLogger.LogEventAsync(new AuditEvent
                {
                    EventType = "ValueDiagnostic.Completed",
                    EventCategory = "ValueGeneration",
                    UserId = GetCurrentUserId(),
                    TargetId = request.TargetId,
                    CorrelationId = correlationId,
                    EventData = JsonSerializer.Serialize(new 
                    { 
                        request.TenantId, 
                        request.TargetType,
                        ValueScore = result.ValueScore,
                        ValueProfile = result.ValueProfile
                    })
                });

                // Check if the result indicates potential issues that require review
                if (result.ValueScore < 50)
                {
                    await TriggerValueDiagnosticReviewAsync(request.TargetId, request.TargetType, request.TenantId, result, correlationId);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running value diagnostic for target {TargetId}", request.TargetId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    ErrorEnvelope.Create("INTERNAL_ERROR", "An unexpected error occurred while processing the request", correlationId));
            }
        }

        #endregion

        #region Organizational Blindness Endpoints

        /// <summary>
        /// Detects organizational blindness by analyzing perceived versus actual value.
        /// </summary>
        /// <param name="request">The request containing the organization and context for the analysis.</param>
        /// <returns>The organizational blindness detection results, including risk score and identified blind spots.</returns>
        /// <response code="200">Returns the organizational blindness detection results.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission.</response>
        /// <response code="500">If an unexpected error occurs.</response>
        [HttpPost("org-blindness/detect")]
        [Authorize(Roles = "Admin,HR,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrgBlindnessDetectionResponse>> DetectOrganizationalBlindnessAsync([FromBody] OrgBlindnessDetectionApiRequest request)
        {
            var correlationId = Guid.NewGuid().ToString();
            _logger.LogInformation("Organizational blindness detection requested for organization {OrganizationId} with correlation ID {CorrelationId}", 
                request.OrganizationId, correlationId);

            try
            {
                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(ErrorEnvelope.InvalidPayload("Invalid request payload", correlationId));
                }

                // Create the domain request
                var domainRequest = new OrgBlindnessDetectionRequest
                {
                    OrganizationId = request.OrganizationId,
                    DepartmentFilters = request.DepartmentFilters,
                    Provenance = new ProvenanceContext
                    {
                        TenantId = request.TenantId,
                        ActorId = GetCurrentUserId(),
                        CorrelationId = correlationId
                    }
                };

                // Call the domain service
                var result = await _orgBlindnessPort.DetectOrganizationalBlindnessAsync(domainRequest);

                // Log the event
                await _auditLogger.LogEventAsync(new AuditEvent
                {
                    EventType = "OrgBlindness.Completed",
                    EventCategory = "ValueGeneration",
                    UserId = GetCurrentUserId(),
                    TargetId = request.OrganizationId,
                    CorrelationId = correlationId,
                    EventData = JsonSerializer.Serialize(new 
                    { 
                        request.TenantId, 
                        BlindnessRiskScore = result.BlindnessRiskScore,
                        BlindSpotCount = result.IdentifiedBlindSpots?.Count ?? 0
                    })
                });

                // Check if the result indicates significant blindness that requires review
                if (result.BlindnessRiskScore > 0.6)
                {
                    await TriggerOrgBlindnessReviewAsync(request.OrganizationId, request.TenantId, result, correlationId);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting organizational blindness for organization {OrganizationId}", request.OrganizationId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    ErrorEnvelope.Create("INTERNAL_ERROR", "An unexpected error occurred while processing the request", correlationId));
            }
        }

        #endregion

        #region Employability Check Endpoints

        /// <summary>
        /// Checks the employability risk for a user based on skills, creativity, and market trends.
        /// This is a sensitive HR operation that requires explicit consent and may be subject to manual review.
        /// </summary>
        /// <param name="request">The request containing the user and context for the employability check.</param>
        /// <returns>The employability check results, including risk score, risk factors, and recommended actions.</returns>
        /// <response code="200">Returns the employability check results.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission or required consent is missing.</response>
        /// <response code="500">If an unexpected error occurs.</response>
        [HttpPost("employability/check")]
        [Authorize(Roles = "Admin,HR")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EmployabilityCheckResponse>> CheckEmployabilityAsync([FromBody] EmployabilityCheckApiRequest request)
        {
            var correlationId = Guid.NewGuid().ToString();
            _logger.LogInformation("Employability check requested for user {UserId} with correlation ID {CorrelationId}", 
                request.UserId, correlationId);

            try
            {
                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(ErrorEnvelope.InvalidPayload("Invalid request payload", correlationId));
                }

                // Check for consent
                var consentRequest = new ValidateConsentRequest
                {
                    UserId = request.UserId,
                    TenantId = request.TenantId,
                    RequiredConsentType = ConsentTypes.EmployabilityAnalysis
                };

                var consentResponse = await _consentPort.ValidateConsentAsync(consentRequest);
                if (!consentResponse.HasConsent)
                {
                    await _auditLogger.LogEventAsync(new AuditEvent
                    {
                        EventType = "Employability.ConsentMissing",
                        EventCategory = "Consent",
                        UserId = GetCurrentUserId(),
                        TargetId = request.UserId,
                        CorrelationId = correlationId,
                        EventData = JsonSerializer.Serialize(new { request.TenantId })
                    });

                    return Forbid(ErrorEnvelope.ConsentMissing(
                        $"Consent '{ConsentTypes.EmployabilityAnalysis}' is required for employability analysis", 
                        correlationId));
                }

                // Create the domain request
                var domainRequest = new EmployabilityCheckRequest
                {
                    UserId = request.UserId,
                    Provenance = new ProvenanceContext
                    {
                        TenantId = request.TenantId,
                        ActorId = GetCurrentUserId(),
                        ConsentId = consentResponse.ConsentRecordId,
                        CorrelationId = correlationId
                    }
                };

                // Call the domain service
                var result = await _employabilityPort.CheckEmployabilityAsync(domainRequest);

                // Log the event
                await _auditLogger.LogEventAsync(new AuditEvent
                {
                    EventType = "Employability.Completed",
                    EventCategory = "ValueGeneration",
                    UserId = GetCurrentUserId(),
                    TargetId = request.UserId,
                    CorrelationId = correlationId,
                    EventData = JsonSerializer.Serialize(new 
                    { 
                        request.TenantId, 
                        EmployabilityRiskScore = result.EmployabilityRiskScore,
                        RiskLevel = result.RiskLevel,
                        RiskFactorCount = result.RiskFactors?.Count ?? 0
                    })
                });

                // High-risk results require manual review before being released
                if (result.RiskLevel == "High")
                {
                    var reviewResponse = await TriggerEmployabilityReviewAsync(request.UserId, request.TenantId, result, correlationId);
                    
                    // Inform the client that the result is pending review
                    return Accepted(new
                    {
                        Message = "Employability check completed but requires manual review before results are released.",
                        ReviewId = reviewResponse.ReviewId,
                        EstimatedCompletionTime = reviewResponse.EstimatedCompletionTime
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking employability for user {UserId}", request.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    ErrorEnvelope.Create("INTERNAL_ERROR", "An unexpected error occurred while processing the request", correlationId));
            }
        }

        /// <summary>
        /// Gets the status of a pending employability check review.
        /// </summary>
        /// <param name="reviewId">The ID of the review.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>The current status of the review.</returns>
        /// <response code="200">Returns the review status.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the user does not have permission.</response>
        /// <response code="404">If the review is not found.</response>
        /// <response code="500">If an unexpected error occurs.</response>
        [HttpGet("employability/review/{reviewId}")]
        [Authorize(Roles = "Admin,HR")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ReviewRecord>> GetEmployabilityReviewStatusAsync(string reviewId, [FromQuery] string tenantId)
        {
            var correlationId = Guid.NewGuid().ToString();
            _logger.LogInformation("Employability review status requested for review {ReviewId} with correlation ID {CorrelationId}", 
                reviewId, correlationId);

            try
            {
                // Validate parameters
                if (string.IsNullOrEmpty(reviewId) || string.IsNullOrEmpty(tenantId))
                {
                    return BadRequest(ErrorEnvelope.InvalidPayload("Review ID and tenant ID are required", correlationId));
                }

                // Get the review status
                var review = await _manualAdjudicationPort.GetReviewStatusAsync(reviewId, tenantId);
                if (review == null)
                {
                    return NotFound(ErrorEnvelope.Create("REVIEW_NOT_FOUND", $"Review with ID {reviewId} not found", correlationId));
                }

                return Ok(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employability review status for review {ReviewId}", reviewId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    ErrorEnvelope.Create("INTERNAL_ERROR", "An unexpected error occurred while processing the request", correlationId));
            }
        }

        #endregion

        #region Helper Methods

        private string GetCurrentUserId()
        {
            // In a real implementation, this would get the current user's ID from the claims
            return User?.Identity?.Name ?? "system";
        }

        private async Task TriggerValueDiagnosticReviewAsync(string targetId, string targetType, string tenantId, 
            ValueDiagnosticResponse result, string correlationId)
        {
            try
            {
                var reviewRequest = new ManualReviewRequest
                {
                    TenantId = tenantId,
                    RequestedBy = GetCurrentUserId(),
                    ReviewType = ReviewTypes.ValueDiagnosticAlert,
                    SubjectId = targetId,
                    SubjectType = targetType,
                    Priority = "Normal",
                    Summary = $"Low value score ({result.ValueScore}) detected for {targetType} {targetId}",
                    Details = $"Value diagnostic completed with a score of {result.ValueScore} and profile '{result.ValueProfile}', " +
                              $"which is below the threshold of 50. This may indicate potential issues that require attention.",
                    Context = new Dictionary<string, object>
                    {
                        { "valueScore", result.ValueScore },
                        { "valueProfile", result.ValueProfile },
                        { "strengths", result.Strengths },
                        { "developmentOpportunities", result.DevelopmentOpportunities },
                        { "correlationId", correlationId }
                    },
                    CorrelationId = correlationId
                };

                await _manualAdjudicationPort.SubmitForReviewAsync(reviewRequest);
                _logger.LogInformation("Value diagnostic review triggered for {TargetType} {TargetId} with correlation ID {CorrelationId}", 
                    targetType, targetId, correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering value diagnostic review for {TargetType} {TargetId}", targetType, targetId);
                // Don't rethrow - this is a background operation that shouldn't affect the main response
            }
        }

        private async Task TriggerOrgBlindnessReviewAsync(string organizationId, string tenantId, 
            OrgBlindnessDetectionResponse result, string correlationId)
        {
            try
            {
                var reviewRequest = new ManualReviewRequest
                {
                    TenantId = tenantId,
                    RequestedBy = GetCurrentUserId(),
                    ReviewType = ReviewTypes.OrgBlindnessAlert,
                    SubjectId = organizationId,
                    SubjectType = "Organization",
                    Priority = result.BlindnessRiskScore > 0.8 ? "High" : "Normal",
                    Summary = $"High blindness risk ({result.BlindnessRiskScore:P0}) detected for organization {organizationId}",
                    Details = $"Organizational blindness detection completed with a risk score of {result.BlindnessRiskScore:P0}, " +
                              $"which is above the threshold of 60%. This indicates significant value blindness that requires attention.",
                    Context = new Dictionary<string, object>
                    {
                        { "blindnessRiskScore", result.BlindnessRiskScore },
                        { "identifiedBlindSpots", result.IdentifiedBlindSpots },
                        { "correlationId", correlationId }
                    },
                    CorrelationId = correlationId
                };

                await _manualAdjudicationPort.SubmitForReviewAsync(reviewRequest);
                _logger.LogInformation("Organizational blindness review triggered for organization {OrganizationId} with correlation ID {CorrelationId}", 
                    organizationId, correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering organizational blindness review for organization {OrganizationId}", organizationId);
                // Don't rethrow - this is a background operation that shouldn't affect the main response
            }
        }

        private async Task<ManualReviewResponse> TriggerEmployabilityReviewAsync(string userId, string tenantId, 
            EmployabilityCheckResponse result, string correlationId)
        {
            try
            {
                var reviewRequest = new ManualReviewRequest
                {
                    TenantId = tenantId,
                    RequestedBy = GetCurrentUserId(),
                    ReviewType = ReviewTypes.EmployabilityHighRisk,
                    SubjectId = userId,
                    SubjectType = "User",
                    Priority = "High",
                    Summary = $"High employability risk ({result.EmployabilityRiskScore:P0}) detected for user {userId}",
                    Details = $"Employability check completed with a risk score of {result.EmployabilityRiskScore:P0} and risk level '{result.RiskLevel}'. " +
                              $"This requires manual review before results are released to ensure accuracy and appropriate handling.",
                    Context = new Dictionary<string, object>
                    {
                        { "employabilityRiskScore", result.EmployabilityRiskScore },
                        { "riskLevel", result.RiskLevel },
                        { "riskFactors", result.RiskFactors },
                        { "recommendedActions", result.RecommendedActions },
                        { "correlationId", correlationId }
                    },
                    Deadline = DateTimeOffset.UtcNow.AddHours(24), // 24-hour SLA for high-risk employability reviews
                    CorrelationId = correlationId
                };

                var response = await _manualAdjudicationPort.SubmitForReviewAsync(reviewRequest);
                _logger.LogInformation("Employability review triggered for user {UserId} with correlation ID {CorrelationId}", 
                    userId, correlationId);
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering employability review for user {UserId}", userId);
                throw; // Rethrow because this is critical for the employability check flow
            }
        }

        #endregion
    }

    #region API Request Models

    /// <summary>
    /// Represents an API request to run a value diagnostic.
    /// </summary>
    public class ValueDiagnosticApiRequest
    {
        /// <summary>
        /// The ID of the target (user or team) for the diagnostic.
        /// </summary>
        [Required]
        public string TargetId { get; set; }

        /// <summary>
        /// The type of the target ("User" or "Team").
        /// </summary>
        [Required]
        public string TargetType { get; set; }

        /// <summary>
        /// The tenant ID.
        /// </summary>
        [Required]
        public string TenantId { get; set; }
    }

    /// <summary>
    /// Represents an API request to detect organizational blindness.
    /// </summary>
    public class OrgBlindnessDetectionApiRequest
    {
        /// <summary>
        /// The ID of the organization to analyze.
        /// </summary>
        [Required]
        public string OrganizationId { get; set; }

        /// <summary>
        /// Optional filters to limit the analysis to specific departments.
        /// </summary>
        public string[] DepartmentFilters { get; set; }

        /// <summary>
        /// The tenant ID.
        /// </summary>
        [Required]
        public string TenantId { get; set; }
    }

    /// <summary>
    /// Represents an API request to check employability risk.
    /// </summary>
    public class EmployabilityCheckApiRequest
    {
        /// <summary>
        /// The ID of the user to check.
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// The tenant ID.
        /// </summary>
        [Required]
        public string TenantId { get; set; }
    }

    #endregion
}
