using CognitiveMesh.Application.UseCases.ChampionDiscovery;
using CognitiveMesh.MetacognitiveLayer.CommunityPulse;
using CognitiveMesh.MetacognitiveLayer.CommunityPulse.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CognitiveMesh.BusinessApplications.ConvenerServices
{
    /// <summary>
    /// The main API controller for the Convener backend.
    /// This controller acts as the primary orchestration point, exposing business-level endpoints
    /// that coordinate functionality from the Reasoning, Metacognitive, and Foundation layers.
    /// </summary>
    [ApiController]
    [Route("api/v1/convener")]
    [Authorize] // All endpoints require authentication by default.
    public class ConvenerController : ControllerBase
    {
        private readonly ILogger<ConvenerController> _logger;
        private readonly DiscoverChampionsUseCase _discoverChampionsUseCase;
        private readonly CommunityPulseService _communityPulseService;

        public ConvenerController(
            ILogger<ConvenerController> logger,
            DiscoverChampionsUseCase discoverChampionsUseCase,
            CommunityPulseService communityPulseService)
        {
            _logger = logger;
            _discoverChampionsUseCase = discoverChampionsUseCase;
            _communityPulseService = communityPulseService;
        }

        /// <summary>
        /// Discovers and ranks knowledge champions based on specified criteria.
        /// </summary>
        /// <remarks>
        /// This endpoint queries the Reasoning Layer to find users who are influential experts
        /// in a given skill area. The results are scored and ranked based on their interactions,
        /// endorsements, and recent activity. All data access is strictly scoped to the
        /// authenticated user's tenant.
        ///
        /// Conforms to NFRs: Security (1), Telemetry & Audit (2), Performance (6).
        /// </remarks>
        /// <param name="skill" example="MLOps">An optional skill to filter champions by.</param>
        /// <param name="maxResults" example="10">The maximum number of champions to return.</param>
        /// <returns>A ranked list of discovered champions.</returns>
        [HttpGet("discover/champions")]
        [ProducesResponseType(typeof(DiscoverChampionsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DiscoverChampions([FromQuery] string skill, [FromQuery] int maxResults = 10)
        {
            try
            {
                var tenantId = GetTenantIdFromClaims();
                if (tenantId == null)
                {
                    return Unauthorized("Tenant ID is missing from the authentication token.");
                }

                var request = new DiscoverChampionsRequest
                {
                    TenantId = tenantId,
                    SkillFilter = skill,
                    MaxResults = maxResults
                };

                _logger.LogInformation(
                    "Initiating champion discovery for Tenant '{TenantId}' with SkillFilter '{SkillFilter}'.",
                    request.TenantId,
                    request.SkillFilter ?? "N/A");

                var response = await _discoverChampionsUseCase.ExecuteAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during champion discovery.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets the current community pulse for a specified channel.
        /// </summary>
        /// <remarks>
        /// This endpoint queries the Metacognitive Layer to analyze and aggregate community health
        /// metrics, including engagement, sentiment, and psychological safety.
        ///
        /// Conforms to NFRs: Security (1), Privacy (4), Performance (6).
        /// </remarks>
        /// <param name="channelId" example="C024BE91L">The unique identifier for the communication channel.</param>
        /// <param name="timeframeInDays" example="30">The number of days to look back for the analysis.</param>
        /// <returns>A report containing community pulse metrics.</returns>
        [HttpGet("pulse/community")]
        [ProducesResponseType(typeof(CommunityPulseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCommunityPulse([FromQuery] string channelId, [FromQuery] int timeframeInDays = 30)
        {
            try
            {
                var tenantId = GetTenantIdFromClaims();
                if (tenantId == null)
                {
                    return Unauthorized("Tenant ID is missing from the authentication token.");
                }

                if (string.IsNullOrWhiteSpace(channelId))
                {
                    return BadRequest("The 'channelId' query parameter is required.");
                }

                var request = new CommunityPulseRequest
                {
                    TenantId = tenantId,
                    ChannelId = channelId,
                    TimeframeInDays = timeframeInDays
                };

                _logger.LogInformation(
                    "Fetching community pulse for Tenant '{TenantId}', Channel '{ChannelId}'.",
                    request.TenantId,
                    request.ChannelId);

                var response = await _communityPulseService.GetCommunityPulseAsync(request);
                if (response == null)
                {
                    return NotFound($"No community data found for channel '{channelId}'.");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during community pulse analysis.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred while processing your request.");
            }
        }

        // --- Placeholder Endpoints for Future Implementation ---

        [HttpGet("innovation/spread/{ideaId}")]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult GetInnovationSpread(string ideaId)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, "Innovation Spread tracking is not yet implemented.");
        }

        [HttpPost("learning/catalysts/recommend")]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult GetLearningRecommendations()
        {
            return StatusCode(StatusCodes.Status501NotImplemented, "Learning Catalyst recommendations are not yet implemented.");
        }

        /// <summary>
        /// Helper method to securely retrieve the Tenant ID from the user's claims.
        /// </summary>
        private string GetTenantIdFromClaims()
        {
            // In a real application, the claim type would be a constant.
            return User.FindFirstValue("tenant_id");
        }
    }
}
