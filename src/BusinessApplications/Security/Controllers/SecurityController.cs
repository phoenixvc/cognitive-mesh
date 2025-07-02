using CognitiveMesh.FoundationLayer.Security.Ports;
using CognitiveMesh.FoundationLayer.Security.Ports.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

using CognitiveMesh.FoundationLayer.Security.Ports; // already present for ISecurityPolicyPort
using CognitiveMesh.FoundationLayer.Security.Ports.Models;
using CognitiveMesh.ReasoningLayer.SecurityReasoning.Ports;
using CognitiveMesh.ReasoningLayer.SecurityReasoning.Ports.Models;
namespace CognitiveMesh.BusinessApplications.Security.Controllers
{
    /// <summary>
    /// API controller for the Security & Zero-Trust Infrastructure Framework.
    /// This controller exposes endpoints for managing and verifying security policies,
    /// handling authentication and authorization, and generating compliance reports.
    /// </summary>
    [ApiController]
    [Route("api/v1/security")]
    [Authorize] // Secure all endpoints by default
    public class SecurityController : ControllerBase
    {
        private readonly ISecurityPolicyPort _securityPolicyPort;
        private readonly ISecretsManagementPort _secretsPort;
        private readonly IThreatIntelligencePort _threatIntelPort;
        private readonly ILogger<SecurityController> _logger;

        public SecurityController(
            ISecurityPolicyPort securityPolicyPort,
            ISecretsManagementPort secretsPort,
            IThreatIntelligencePort threatIntelligencePort,
            ILogger<SecurityController> logger)
        {
            _securityPolicyPort = securityPolicyPort ?? throw new ArgumentNullException(nameof(securityPolicyPort));
            _secretsPort        = secretsPort        ?? throw new ArgumentNullException(nameof(secretsPort));
            _threatIntelPort    = threatIntelligencePort ?? throw new ArgumentNullException(nameof(threatIntelligencePort));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Verifies an authentication token to enforce Zero-Trust principles.
        /// </summary>
        /// <param name="request">The authentication request containing the token.</param>
        /// <returns>The result of the authentication verification.</returns>
        [HttpPost("auth/verify")]
        [AllowAnonymous] // This endpoint must be public to verify tokens.
        [ProducesResponseType(typeof(AuthenticationVerificationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthenticationVerificationResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerifyAuthentication([FromBody] AuthenticationVerificationRequest request)
        {
            try
            {
                var response = await _securityPolicyPort.VerifyAuthenticationAsync(request);
                if (!response.IsAuthenticated)
                {
                    return Unauthorized(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
        #region Secrets Management

        /// <summary>
        /// Validates the status (existence / expiry) of a secret without returning its value.
        /// </summary>
        /// <param name="secretName">The unique name of the secret.</param>
        /// <param name="versionId">Optional version identifier.  If omitted, the latest version is validated.</param>
        [HttpGet("secrets/status")]
        [Authorize(Policy = "SecurityAdmin")]
        [ProducesResponseType(typeof(ValidateSecretResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidateSecretResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSecretStatus([FromQuery] string secretName, [FromQuery] string? versionId)
        {
            if (string.IsNullOrWhiteSpace(secretName))
                return BadRequest(new { error_code = "INVALID_PARAMETER", message = "secretName is required." });

            try
            {
                var result = await _secretsPort.ValidateSecretAsync(new SecretRequest
                {
                    SecretName = secretName,
                    VersionId  = versionId
                });

                if (!result.Exists)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while validating secret status.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error_code = "SECRET_STATUS_FAILED", message = "An internal error occurred." });
            }
        }

        #endregion

        #region Threat Intelligence

        /// <summary>
        /// Analyzes a collection of security events for potential threats and anomalies.
        /// </summary>
        [HttpPost("threats/analyze")]
        [Authorize(Policy = "SecurityAdmin")]
        [ProducesResponseType(typeof(ThreatAnalysisResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AnalyzeThreats([FromBody] ThreatAnalysisRequest request)
        {
            try
            {
                var result = await _threatIntelPort.AnalyzeThreatPatternsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while analyzing threat patterns.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error_code = "THREAT_ANALYSIS_FAILED", message = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Scans supplied artifacts (IPs, hashes, domains) against known Indicators of Compromise.
        /// </summary>
        [HttpPost("threats/ioc-scan")]
        [Authorize(Policy = "SecurityAdmin")]
        [ProducesResponseType(typeof(IOCDetectionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ScanIOCs([FromBody] IOCDetectionRequest request)
        {
            try
            {
                var result = await _threatIntelPort.DetectIndicatorsOfCompromiseAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while scanning for IOCs.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error_code = "IOC_SCAN_FAILED", message = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Calculates a dynamic risk score for a specific action or entity.
        /// </summary>
        [HttpPost("risk/score")]
        [Authorize] // Any authenticated subject can request a risk score (subject to policy)
        [ProducesResponseType(typeof(RiskScoringResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CalculateRiskScore([FromBody] RiskScoringRequest request)
        {
            try
            {
                var result = await _threatIntelPort.CalculateRiskScoreAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calculating risk score.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error_code = "RISK_SCORE_FAILED", message = "An internal error occurred." });
            }
        }

        #endregion
                _logger.LogError(ex, "An unexpected error occurred during authentication verification.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error_code = "VERIFICATION_FAILED", message = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Authorizes a specific action based on the subject's claims and the resource being accessed.
        /// </summary>
        /// <param name="request">The authorization request.</param>
        /// <returns>The result of the authorization decision.</returns>
        [HttpPost("auth/authorize")]
        [ProducesResponseType(typeof(AuthorizationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthorizationResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AuthorizeRequest([FromBody] AuthorizationRequest request)
        {
            try
            {
                var response = await _securityPolicyPort.AuthorizeRequestAsync(request);
                if (!response.IsAuthorized)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during authorization.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error_code = "AUTHORIZATION_FAILED", message = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Validates a security policy document. (Admin Only)
        /// </summary>
        /// <param name="request">The policy validation request.</param>
        /// <returns>The result of the policy validation.</returns>
        [HttpPost("policy/validate")]
        [Authorize(Policy = "SecurityAdmin")]
        [ProducesResponseType(typeof(PolicyValidationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PolicyValidationResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidatePolicy([FromBody] PolicyValidationRequest request)
        {
            try
            {
                var response = await _securityPolicyPort.ValidatePolicyAsync(request);
                if (!response.IsValid)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during policy validation.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error_code = "POLICY_VALIDATION_FAILED", message = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Generates a compliance report. (Compliance Officer or Admin Only)
        /// </summary>
        /// <param name="request">The compliance report request.</param>
        /// <returns>The generated compliance report.</returns>
        [HttpPost("compliance/report")]
        [Authorize(Policy = "ComplianceAccess")] // A policy that includes ComplianceOfficer and SecurityAdmin roles
        [ProducesResponseType(typeof(ComplianceReportResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateComplianceReport([FromBody] ComplianceReportRequest request)
        {
            try
            {
                var response = await _securityPolicyPort.GenerateComplianceReportAsync(request);
                if (response == null)
                {
                    return BadRequest(new { error_code = "INVALID_REPORT_TYPE", message = $"The report type '{request.ReportType}' is not supported." });
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while generating compliance report.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error_code = "REPORT_GENERATION_FAILED", message = "An internal error occurred." });
            }
        }
    }
}
