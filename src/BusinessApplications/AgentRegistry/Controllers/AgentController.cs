using AgencyLayer.MultiAgentOrchestration.Ports;
using CognitiveMesh.BusinessApplications.AgentRegistry.Ports;
using CognitiveMesh.BusinessApplications.AgentRegistry.Ports.Models;
using CognitiveMesh.BusinessApplications.Common.Models;
using RegistryAuthorityScope = CognitiveMesh.BusinessApplications.AgentRegistry.Ports.Models.AuthorityScope;
using FoundationLayer.AuditLogging;
using FoundationLayer.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Initializes a new instance of the AgentController class.
        /// </summary>
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
        [ProducesResponseType(typeof(AgentRegistrationRequest), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterAgent([FromBody] AgentRegistrationRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.AgentType))
                {
                    return BadRequest(ErrorEnvelope.InvalidPayload("Agent type is required."));
                }

                var agent = await _registryPort.RegisterAgentAsync(request);

                // audit + notify (best-effort, exceptions logged but not propagated)
                var actorName = User?.Identity?.Name ?? "system";
                _ = Task.Run(async () =>
                {
                    try { await _audit.LogAgentRegisteredAsync(agent.AgentId, agent.AgentType, actorName); }
                    catch (InvalidOperationException ex) { _logger.LogWarning(ex, "Failed to log agent registration audit for {AgentId}.", agent.AgentId); }
                    catch (HttpRequestException ex) { _logger.LogWarning(ex, "Failed to log agent registration audit for {AgentId}.", agent.AgentId); }
                });
                _ = Task.Run(async () =>
                {
                    try { await _notify.SendAgentRegistrationNotificationAsync(agent.AgentId, agent.AgentType, actorName, new[] { actorName }); }
                    catch (InvalidOperationException ex) { _logger.LogWarning(ex, "Failed to send registration notification for {AgentId}.", agent.AgentId); }
                    catch (HttpRequestException ex) { _logger.LogWarning(ex, "Failed to send registration notification for {AgentId}.", agent.AgentId); }
                });

                return CreatedAtAction(nameof(GetAgentDetails), new { agentId = agent.AgentId }, agent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering agent type '{AgentType}'.", request?.AgentType);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ErrorEnvelope.Create("SERVICE_UNAVAILABLE", "AgentRegistry service is unavailable."));
            }
        }

        /// <summary>
        /// Retrieves the details of a specific agent definition.
        /// </summary>
        [HttpGet("registry/{agentId:guid}")]
        [ProducesResponseType(typeof(Agent), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAgentDetails(Guid agentId)
        {
            try
            {
                var (tenantId, _) = GetAuthContextFromClaims();
                if (tenantId == null) return Unauthorized("Tenant ID is missing.");

                var agent = await _registryPort.GetAgentByIdAsync(agentId, tenantId);
                if (agent == null)
                {
                    _logger.LogWarning("Agent with ID '{AgentId}' not found.", agentId);
                    return NotFound(ErrorEnvelope.Create("AGENT_NOT_FOUND", $"Agent with ID '{agentId}' was not found."));
                }
                return Ok(agent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving agent with ID '{AgentId}'.", agentId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ErrorEnvelope.Create("SERVICE_UNAVAILABLE", "AgentRegistry service is unavailable."));
            }
        }

        /// <summary>
        /// Deactivates an agent, making it unavailable for new tasks. (Admin Only)
        /// </summary>
        [HttpDelete("registry/{agentId:guid}")]
        [Authorize(Policy = "AdminAccess")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeactivateAgent(Guid agentId, [FromQuery] string? reason = null)
        {
            try
            {
                var (tenantId, userId) = GetAuthContextFromClaims();
                if (tenantId == null) return Unauthorized("Tenant ID is missing.");

                var success = await _registryPort.DeactivateAgentAsync(agentId, tenantId, userId ?? "system", reason ?? "Manual deactivation");
                if (!success)
                {
                    return NotFound(ErrorEnvelope.Create("AGENT_NOT_FOUND", $"Agent with ID '{agentId}' was not found."));
                }

                _ = Task.Run(async () =>
                {
                    try { await _audit.LogAgentRetiredAsync(agentId, User?.Identity?.Name ?? "system", reason ?? "Manual deactivation"); }
                    catch (InvalidOperationException ex) { _logger.LogWarning(ex, "Failed to log agent retirement audit for {AgentId}.", agentId); }
                    catch (HttpRequestException ex) { _logger.LogWarning(ex, "Failed to log agent retirement audit for {AgentId}.", agentId); }
                });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deactivating agent with ID '{AgentId}'.", agentId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ErrorEnvelope.Create("SERVICE_UNAVAILABLE", "AgentRegistry service is unavailable."));
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
            request.RequestingUserId = userId ?? string.Empty;

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
        public async Task<IActionResult> ConfigureAuthority(string agentType, [FromBody] RegistryAuthorityScope scope)
        {
            var (tenantId, userId) = GetAuthContextFromClaims();
            if (tenantId == null) return Unauthorized("Tenant ID is missing.");

            // Look up agent by type to resolve the agentId
            var agents = await _registryPort.GetAgentsByTypeAsync(agentType, tenantId);
            var targetAgent = agents.FirstOrDefault();
            if (targetAgent == null)
            {
                return NotFound(ErrorEnvelope.Create("AGENT_NOT_FOUND", $"No agent of type '{agentType}' was found."));
            }

            await _authorityPort.ConfigureAgentAuthorityAsync(targetAgent.AgentId, scope, userId ?? "system", tenantId);
            return NoContent();
        }

        /// <summary>
        /// Helper method to securely retrieve the Tenant and Actor IDs from the user's claims.
        /// </summary>
        private (string? TenantId, string? UserId) GetAuthContextFromClaims()
        {
            var tenantId = User.FindFirstValue("tenant_id");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return (tenantId, userId);
        }
    }
}
