using CognitiveMesh.ReasoningLayer.AgencyRouter.Ports;
using CognitiveMesh.ReasoningLayer.AgencyRouter.Ports.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CognitiveMesh.BusinessApplications.AgencyRouter.Controllers
{
    /// <summary>
    /// API controller for the Contextual Adaptive Agency/Sovereignty Router.
    /// This controller serves as the primary REST/gRPC adapter for the Agency Router domain,
    /// orchestrating calls to the Reasoning Layer's engines while enforcing consent, authorization,
    /// and auditability as required by the PRD and Global NFRs.
    /// </summary>
    [ApiController]
    [Route("api/v1/agency")]
    [Authorize] // All endpoints require authentication by default.
    public class AgencyRouterController : ControllerBase
    {
        private readonly ILogger<AgencyRouterController> _logger;
        private readonly IAgencyRouterPort _agencyRouterPort;

        public AgencyRouterController(
            ILogger<AgencyRouterController> logger,
            IAgencyRouterPort agencyRouterPort)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _agencyRouterPort = agencyRouterPort ?? throw new ArgumentNullException(nameof(agencyRouterPort));
        }

        /// <summary>
        /// Dynamically routes a task to the appropriate agency mode based on its context.
        /// </summary>
        /// <remarks>
        /// This endpoint triggers the ContextualAdaptiveAgencyEngine to analyze the task's
        /// Cognitive Impact Assessment (CIA) and Cognitive Sovereignty Index (CSI) scores
        /// against organizational policies to determine the optimal level of agent autonomy.
        /// Conforms to NFRs: Security (1), Telemetry & Audit (2), Performance (6).
        /// </remarks>
        /// <param name="context">The context of the task to be routed.</param>
        /// <returns>A decision specifying the autonomy level and authority scope for the task.</returns>
        [HttpPost("route")]
        [ProducesResponseType(typeof(AgencyModeDecision), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RouteTask([FromBody] TaskContext context)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                var (tenantId, actorId) = GetAuthContextFromClaims();
                if (tenantId == null) return Unauthorized(new { error_code = "UNAUTHORIZED", message = "Tenant ID is missing or invalid.", correlationID = correlationId });

                context.Provenance = new ProvenanceContext { TenantId = tenantId, ActorId = actorId, CorrelationId = correlationId };

                _logger.LogInformation("Initiating agency routing for Task '{TaskId}' with CorrelationId '{CorrelationId}'.", context.TaskId, correlationId);
                var response = await _agencyRouterPort.RouteTaskAsync(context);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for RouteTask with CorrelationId '{CorrelationId}'.", correlationId);
                return BadRequest(new { error_code = "INVALID_PAYLOAD", message = ex.Message, correlationID = correlationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during agency routing with CorrelationId '{CorrelationId}'.", correlationId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error_code = "INTERNAL_SERVER_ERROR", message = "An internal error occurred.", correlationID = correlationId });
            }
        }

        /// <summary>
        /// Applies a user or admin-initiated override to a task's agency mode.
        /// </summary>
        /// <remarks>
        /// This endpoint allows for real-time human intervention in agentic workflows,
        /// ensuring that users can reclaim control or adjust autonomy levels as needed.
        /// All overrides are immutably logged for compliance.
        /// Conforms to NFRs: Security (1), Audit Logging (2), Governance (7).
        /// </remarks>
        /// <param name="request">The request containing the override details.</param>
        /// <returns>An HTTP 204 No Content response if the override was successful.</returns>
        [HttpPost("override")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ApplyOverride([FromBody] OverrideRequest request)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                var (tenantId, actorId) = GetAuthContextFromClaims();
                if (tenantId == null) return Unauthorized(new { error_code = "UNAUTHORIZED", message = "Tenant ID is missing or invalid.", correlationID = correlationId });

                request.Provenance = new ProvenanceContext { TenantId = tenantId, ActorId = actorId, CorrelationId = correlationId };

                _logger.LogInformation("Applying agency override for Task '{TaskId}' with CorrelationId '{CorrelationId}'.", request.TaskId, correlationId);
                var success = await _agencyRouterPort.ApplyOverrideAsync(request);

                return success ? NoContent() : StatusCode(StatusCodes.Status500InternalServerError, new { error_code = "OVERRIDE_FAILED", message = "The override could not be applied.", correlationID = correlationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while applying an override with CorrelationId '{CorrelationId}'.", correlationId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error_code = "INTERNAL_SERVER_ERROR", message = "An internal error occurred.", correlationID = correlationId });
            }
        }

        /// <summary>
        /// Retrieves the current policy configuration for a given tenant. (Admin Only)
        /// </summary>
        [HttpGet("policy/{tenantId}")]
        [Authorize(Policy = "AdminAccess")]
        [ProducesResponseType(typeof(PolicyConfiguration), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPolicy(string tenantId)
        {
            var policy = await _agencyRouterPort.GetPolicyAsync(tenantId);
            return policy != null ? Ok(policy) : NotFound();
        }

        /// <summary>
        /// Updates the policy configuration for a given tenant. (Admin Only)
        /// </summary>
        [HttpPut("policy")]
        [Authorize(Policy = "AdminAccess")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdatePolicy([FromBody] PolicyConfiguration policy)
        {
            var success = await _agencyRouterPort.UpdatePolicyAsync(policy);
            return success ? NoContent() : BadRequest("Failed to update policy.");
        }

        /// <summary>
        /// Provides introspection into the scores and context for a given task.
        /// </summary>
        [HttpGet("introspection/{taskId}")]
        [ProducesResponseType(typeof(TaskContext), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetIntrospectionData(string taskId)
        {
            var tenantId = GetTenantIdFromClaims().TenantId;
            if (tenantId == null) return Unauthorized();

            var context = await _agencyRouterPort.GetIntrospectionDataAsync(taskId, tenantId);
            return context != null ? Ok(context) : NotFound();
        }

        /// <summary>
        /// Helper method to securely retrieve the Tenant and Actor IDs from the user's claims.
        /// </summary>
        private (string TenantId, string ActorId) GetAuthContextFromClaims()
        {
            var tenantId = User.FindFirstValue("tenant_id");
            var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return (tenantId, actorId);
        }
    }
}
