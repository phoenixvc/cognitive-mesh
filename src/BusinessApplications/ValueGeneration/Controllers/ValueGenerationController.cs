using CognitiveMesh.BusinessApplications.ConvenerServices.Ports;
using CognitiveMesh.BusinessApplications.ConvenerServices.Ports.Models;
using CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;
using CognitiveMesh.ReasoningLayer.ValueGeneration.Ports.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CognitiveMesh.BusinessApplications.ValueGeneration.Controllers
{
    /// <summary>
    /// API controller for the Value Generation backend services.
    /// This controller serves as the primary REST/gRPC adapter for the Value Generation domain,
    /// orchestrating calls to the Reasoning Layer's engines while enforcing consent, authorization,
    * and auditability as required by the PRD and Global NFRs.
    /// </summary>
    [ApiController]
    [Route("api/v1/value-generation")]
    [Authorize] // All endpoints require authentication by default.
    public class ValueGenerationController : ControllerBase
    {
        private readonly ILogger<ValueGenerationController> _logger;
        private readonly IValueGenerationPort _valueGenerationPort;
        private readonly IConsentPort _consentPort;

        public ValueGenerationController(
            ILogger<ValueGenerationController> logger,
            IValueGenerationPort valueGenerationPort,
            IConsentPort consentPort)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _valueGenerationPort = valueGenerationPort ?? throw new ArgumentNullException(nameof(valueGenerationPort));
            _consentPort = consentPort ?? throw new ArgumentNullException(nameof(consentPort));
        }

        /// <summary>
        /// Runs a diagnostic to assess the value generation profile of a user or team.
        /// </summary>
        /// <remarks>
        /// This endpoint triggers the ValueGenerationDiagnosticEngine to calculate metrics like the "$200 Test" score.
        /// It requires standard user authentication and is fully audited.
        /// Conforms to NFRs: Security (1), Telemetry & Audit (2), Performance (6).
        /// </remarks>
        /// <param name="request">The request containing the target for the diagnostic.</param>
        /// <returns>A response containing the value diagnostic scores and insights.</returns>
        [HttpPost("diagnostic")]
        [ProducesResponseType(typeof(ValueDiagnosticResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RunValueDiagnostic([FromBody] ValueDiagnosticRequest request)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                var (tenantId, actorId) = GetAuthContextFromClaims();
                if (tenantId == null) return Unauthorized("Tenant ID is missing or invalid.");

                request.Provenance = new ProvenanceContext { TenantId = tenantId, ActorId = actorId, CorrelationId = correlationId };

                _logger.LogInformation("Initiating value diagnostic for Target '{TargetId}' with CorrelationId '{CorrelationId}'.", request.TargetId, correlationId);
                var response = await _valueGenerationPort.RunValueDiagnosticAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during value diagnostic with CorrelationId '{CorrelationId}'.", correlationId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error_code = "INTERNAL_SERVER_ERROR", message = "An internal error occurred.", correlationID = correlationId });
            }
        }

        /// <summary>
        /// Analyzes organizational data to detect "value blindness".
        /// </summary>
        /// <remarks>
        /// This endpoint triggers the OrganizationalValueBlindnessEngine to identify areas where value creation is overlooked.
        /// Requires administrative privileges.
        /// Conforms to NFRs: Security (1), Telemetry & Audit (2), Performance (6).
        /// </remarks>
        /// <param name="request">The request specifying the organization or departments to analyze.</param>
        /// <returns>A response containing the blindness risk score and identified blind spots.</returns>
        [HttpPost("org-blindness/detect")]
        [Authorize(Policy = "AdminAccess")] // Requires elevated permissions.
        [ProducesResponseType(typeof(OrgBlindnessDetectionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DetectOrganizationalBlindness([FromBody] OrgBlindnessDetectionRequest request)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                var (tenantId, actorId) = GetAuthContextFromClaims();
                if (tenantId == null) return Unauthorized("Tenant ID is missing or invalid.");

                request.Provenance = new ProvenanceContext { TenantId = tenantId, ActorId = actorId, CorrelationId = correlationId };

                _logger.LogInformation("Initiating organizational blindness detection for Org '{OrgId}' with CorrelationId '{CorrelationId}'.", request.OrganizationId, correlationId);
                var response = await _valueGenerationPort.DetectOrganizationalBlindnessAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during organizational blindness detection with CorrelationId '{CorrelationId}'.", correlationId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error_code = "INTERNAL_SERVER_ERROR", message = "An internal error occurred.", correlationID = correlationId });
            }
        }

        /// <summary>
        /// Assesses an individual's employability risk. This is a sensitive HR operation.
        /// </summary>
        /// <remarks>
        /// This endpoint triggers the EmployabilityPredictorEngine. Access is strictly controlled by the 'HRAdminAccess' policy
        /// and requires explicit, logged user consent before execution.
        /// Conforms to NFRs: Security (1), Privacy (4), Compliance (5).
        /// </remarks>
        /// <param name="request">The request containing the user to be assessed.</param>
        /// <returns>A response containing the employability risk analysis.</returns>
        [HttpPost("employability/check")]
        [Authorize(Policy = "HRAdminAccess")] // Strict authorization for HR-sensitive data.
        [ProducesResponseType(typeof(EmployabilityCheckResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckEmployability([FromBody] EmployabilityCheckRequest request)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                var (tenantId, actorId) = GetAuthContextFromClaims();
                if (tenantId == null) return Unauthorized("Tenant ID is missing or invalid.");
                
                request.Provenance = new ProvenanceContext { TenantId = tenantId, ActorId = actorId, CorrelationId = correlationId };

                // **Consent Gating** - as required by the PRD.
                var consentValidationRequest = new ValidateConsentRequest
                {
                    UserId = request.UserId,
                    TenantId = tenantId,
                    RequiredConsentType = "EmployabilityAnalysis"
                };
                var consentResponse = await _consentPort.ValidateConsentAsync(consentValidationRequest);

                if (!consentResponse.HasConsent)
                {
                    _logger.LogWarning("Employability check for User '{UserId}' blocked due to missing consent. CorrelationId: '{CorrelationId}'.", request.UserId, correlationId);
                    return StatusCode(StatusCodes.Status403Forbidden, new { error_code = "CONSENT_MISSING", message = "Explicit user consent for employability analysis is required but was not found.", correlationID = correlationId });
                }

                // Attach the consent ID to the provenance context for a complete audit trail.
                request.Provenance.ConsentId = consentResponse.ConsentRecordId;

                _logger.LogInformation("Consent validated. Proceeding with employability check for User '{UserId}' with CorrelationId '{CorrelationId}'.", request.UserId, correlationId);
                var response = await _valueGenerationPort.CheckEmployabilityAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during employability check with CorrelationId '{CorrelationId}'.", correlationId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error_code = "INTERNAL_SERVER_ERROR", message = "An internal error occurred.", correlationID = correlationId });
            }
        }

        /// <summary>
        /// Helper method to securely retrieve the Tenant and Actor IDs from the user's claims.
        /// </summary>
        private (string TenantId, string ActorId) GetAuthContextFromClaims()
        {
            // In a real application, the claim types would be constants.
            var tenantId = User.FindFirstValue("tenant_id");
            var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return (tenantId, actorId);
        }
    }
}
