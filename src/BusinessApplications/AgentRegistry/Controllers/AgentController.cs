using CognitiveMesh.AgencyLayer.MultiAgentOrchestration.Ports;
using CognitiveMesh.AgencyLayer.MultiAgentOrchestration.Ports.Models;
using CognitiveMesh.BusinessApplications.AgentRegistry.Ports;
using CognitiveMesh.BusinessApplications.Common.Models;
using CognitiveMesh.FoundationLayer.AuditLogging;
using CognitiveMesh.FoundationLayer.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CognitiveMesh.BusinessApplications.AgentRegistry.Controllers
{
    /// <summary>
    /// API controller for the Agentic AI System.
    /// This controller serves as the primary REST/gRPC adapter for agent lifecycle management,
    /// task orchestration, and authority configuration, adhering to the Hexagonal Architecture pattern.
    /// </summary>
    [ApiController]
    [Route("api/v1/agent")]
    [Authorize] // All endpoints require authentication by default.
    public class AgentController : ControllerBase
    {
        private readonly IMultiAgentOrchestrationPort _orchestrationPort;
        private readonly IAgentRegistryPort _registryPort;
        private readonly IAuthorityPort _authorityPort;
        private readonly IAgentConsentPort _consentPort;
        private readonly IAuditLoggingAdapter _audit;
        private readonly INotificationAdapter _notify;
        private readonly ILogger<AgentController> _logger;

        public AgentController(
            IMultiAgentOrchestrationPort orchestrationPort,
            IAgentRegistryPort registryPort,
            IAuthorityPort authorityPort,
            IAgentConsentPort consentPort,
            IAuditLoggingAdapter auditLoggingAdapter,
            INotificationAdapter notificationAdapter,
            ILogger<AgentController> logger)
        {
            _orchestrationPort = orchestrationPort ?? throw new ArgumentNullException(nameof(orchestrationPort));
            _registryPort      = registryPort      ?? throw new ArgumentNullException(nameof(registryPort));
            _authorityPort     = authorityPort     ?? throw new ArgumentNullException(nameof(authorityPort));
            _consentPort       = consentPort       ?? throw new ArgumentNullException(nameof(consentPort));
            _audit             = auditLoggingAdapter ?? throw new ArgumentNullException(nameof(auditLoggingAdapter));
            _notify            = notificationAdapter ?? throw new ArgumentNullException(nameof(notificationAdapter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // --- Agent Registry Endpoints ---

        /// <summary>
        /// Registers a new agent definition in the system. (Admin Only)
        /// </summary>
        [HttpPost("registry")]
        [Authorize(Policy = "AdminAccess")]
        [ProducesResponseType(typeof(AgentDefinition), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterAgent([FromBody] AgentDefinition definition)
        {
            try
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.AgentType))
                {
                    return BadRequest(ErrorEnvelope.InvalidPayload());
                }

                await _registryPort.RegisterAgentAsync(definition);

                // audit + notify (best-effort)
                _ = _audit.LogAgentRegisteredAsync(definition.AgentId, definition.AgentType, User?.Identity?.Name ?? "system");
                _ = _notify.SendAgentRegistrationNotificationAsync(definition.AgentId, definition.AgentType, User?.Identity?.Name ?? "system", new[] { User?.Identity?.Name });

                // Assuming AgentId is set within the RegisterAgentAsync or its underlying logic
                return CreatedAtAction(nameof(GetAgentDetails), new { agentId = definition.AgentId }, definition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering agent type '{AgentType}'.", definition?.AgentType);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ErrorEnvelope.ServiceUnavailable("AgentRegistry"));
            }
        }

        /// <summary>
        /// Retrieves a list of all registered agents.
        /// </summary>
        [HttpGet("registry")]
        [ProducesResponseType(typeof(IEnumerable<AgentDefinition>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListAgents([FromQuery] bool includeRetired = false)
        {
            try
            {
                var agents = await _registryPort.ListAgentsAsync(includeRetired);
                return Ok(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while listing agents.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ErrorEnvelope.ServiceUnavailable("AgentRegistry"));
            }
        }

        /// <summary>
        /// Retrieves the details of a specific agent definition.
        /// </summary>
        [HttpGet("registry/{agentId:guid}")]
        [ProducesResponseType(typeof(AgentDefinition), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAgentDetails(Guid agentId)
        {
            try
            {
                var agent = await _registryPort.GetAgentByIdAsync(agentId);
                if (agent == null)
                {
                    _logger.LogWarning("Agent with ID '{AgentId}' not found.", agentId);
                    return NotFound(ErrorEnvelope.AgentNotFound(agentId.ToString()));
                }
                return Ok(agent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving agent with ID '{AgentId}'.", agentId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ErrorEnvelope.ServiceUnavailable("AgentRegistry"));
            }
        }

        /// <summary>
        /// Updates an existing agent definition. (Admin Only)
        /// </summary>
        [HttpPut("registry/{agentId:guid}")]
        [Authorize(Policy = "AdminAccess")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAgent(Guid agentId, [FromBody] AgentDefinition definition)
        {
            try
            {
                if (agentId != definition.AgentId)
                {
                    return BadRequest(new { error_code = "ID_MISMATCH", message = "The agent ID in the URL does not match the ID in the request body." });
                }
                await _registryPort.UpdateAgentAsync(definition);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ErrorEnvelope.AgentNotFound(agentId.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating agent with ID '{AgentId}'.", agentId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ErrorEnvelope.ServiceUnavailable("AgentRegistry"));
            }
        }

        /// <summary>
        /// Retires an agent, making it unavailable for new tasks. (Admin Only)
        /// </summary>
        [HttpDelete("registry/{agentId:guid}")]
        [Authorize(Policy = "AdminAccess")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RetireAgent(Guid agentId)
        {
            try
            {
                var success = await _registryPort.RetireAgentAsync(agentId);
                if (!success)
                {
                    return NotFound(ErrorEnvelope.AgentNotFound(agentId.ToString()));
                }

                _ = _audit.LogAgentRetiredAsync(agentId, User?.Identity?.Name ?? "system", "Manual retirement");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retiring agent with ID '{AgentId}'.", agentId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ErrorEnvelope.ServiceUnavailable("AgentRegistry"));
            }
        }

        // --- Agent Orchestration Endpoint ---

        /// <summary>
        /// Executes a complex task by assembling and coordinating a team of agents.
        /// </summary>
        [HttpPost("orchestrate")]
        [ProducesResponseType(typeof(AgentExecutionResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExecuteTask([FromBody] AgentExecutionRequest request)
        {
            var (tenantId, userId) = GetAuthContextFromClaims();
            if (tenantId == null) return Unauthorized("Tenant ID is missing.");

            request.TenantId = tenantId;
            request.RequestingUserId = userId;

            var response = await _orchestrationPort.ExecuteTaskAsync(request);
            return Ok(response);
        }

        // --- Authority & Consent Endpoints ---

        /// <summary>
        /// Configures the authority scope for a specific agent type. (Admin Only)
        /// </summary>
        [HttpPut("authority/{agentType}")]
        [Authorize(Policy = "AdminAccess")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ConfigureAuthority(string agentType, [FromBody] AuthorityScope scope)
        {
            var (tenantId, _) = GetAuthContextFromClaims();
            if (tenantId == null) return Unauthorized("Tenant ID is missing.");

            await _authorityPort.UpdateAgentAuthorityAsync(Guid.Empty /* TBD: resolve agentId by type */, scope, tenantId, "Admin API update");
            return NoContent();
        }

        /// <summary>
        /// Helper method to securely retrieve the Tenant and Actor IDs from the user's claims.
        /// </summary>
        private (string TenantId, string UserId) GetAuthContextFromClaims()
        {
            var tenantId = User.FindFirstValue("tenant_id");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return (tenantId, userId);
        }
    }
}
