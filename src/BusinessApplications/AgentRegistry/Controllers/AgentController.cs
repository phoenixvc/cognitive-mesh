using CognitiveMesh.AgencyLayer.MultiAgentOrchestration.Ports;
using CognitiveMesh.AgencyLayer.MultiAgentOrchestration.Ports.Models;
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
        private readonly ILogger<AgentController> _logger;

        public AgentController(
            IMultiAgentOrchestrationPort orchestrationPort,
            ILogger<AgentController> logger)
        {
            _orchestrationPort = orchestrationPort ?? throw new ArgumentNullException(nameof(orchestrationPort));
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
                    return BadRequest(new { error_code = "INVALID_PAYLOAD", message = "Agent definition is invalid." });
                }
                await _orchestrationPort.RegisterAgentAsync(definition);
                // Assuming AgentId is set within the RegisterAgentAsync or its underlying logic
                return CreatedAtAction(nameof(GetAgentDetails), new { agentId = definition.AgentId }, definition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering agent type '{AgentType}'.", definition?.AgentType);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error_code = "REGISTRATION_FAILED", message = "An internal error occurred during agent registration." });
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
                var agents = await _orchestrationPort.ListAgentsAsync(includeRetired);
                return Ok(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while listing agents.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error_code = "LIST_AGENTS_FAILED", message = "An internal error occurred." });
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
                var agent = await _orchestrationPort.GetAgentByIdAsync(agentId);
                if (agent == null)
                {
                    _logger.LogWarning("Agent with ID '{AgentId}' not found.", agentId);
                    return NotFound(new { error_code = "AGENT_NOT_FOUND", message = $"Agent with ID '{agentId}' not found." });
                }
                return Ok(agent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving agent with ID '{AgentId}'.", agentId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error_code = "GET_AGENT_FAILED", message = "An internal error occurred." });
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
                await _orchestrationPort.UpdateAgentAsync(definition);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { error_code = "AGENT_NOT_FOUND", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating agent with ID '{AgentId}'.", agentId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error_code = "UPDATE_AGENT_FAILED", message = "An internal error occurred." });
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
                var success = await _orchestrationPort.RetireAgentAsync(agentId);
                if (!success)
                {
                    return NotFound(new { error_code = "AGENT_NOT_FOUND", message = $"Agent with ID '{agentId}' not found or could not be retired." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retiring agent with ID '{AgentId}'.", agentId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error_code = "RETIRE_AGENT_FAILED", message = "An internal error occurred." });
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

            await _orchestrationPort.ConfigureAgentAuthorityAsync(agentType, scope, tenantId);
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
