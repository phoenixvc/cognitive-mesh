using CognitiveMesh.BusinessApplications.ConvenerServices.Ports;
using CognitiveMesh.BusinessApplications.ConvenerServices.UseCases;
using MetacognitiveLayer.CommunityPulse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using static CognitiveMesh.Shared.LogSanitizer;

namespace CognitiveMesh.BusinessApplications.ConvenerServices
{
    /// <summary>
    /// The main API controller for the Convener backend.
    /// This controller acts as the primary orchestration point, exposing business-level endpoints
    /// that coordinate functionality from the Reasoning, Metacognitive, and Foundation layers.
    /// </summary>
    [ApiController]
    [Route("api/v1/convener")]
    [Authorize]
    public class ConvenerController : ControllerBase
    {
        private readonly ILogger<ConvenerController> _logger;
        private readonly DiscoverChampionsUseCase _discoverChampionsUseCase;
        private readonly CommunityPulseService _communityPulseService;
        private readonly IInnovationSpreadPort _innovationSpreadPort;
        private readonly ILearningCatalystPort _learningCatalystPort;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvenerController"/> class.
        /// </summary>
        /// <param name="logger">Logger for structured diagnostics.</param>
        /// <param name="discoverChampionsUseCase">Use case for champion discovery.</param>
        /// <param name="communityPulseService">Service for community health metrics.</param>
        /// <param name="innovationSpreadPort">Port for innovation spread tracking.</param>
        /// <param name="learningCatalystPort">Port for learning catalyst recommendations.</param>
        public ConvenerController(
            ILogger<ConvenerController> logger,
            DiscoverChampionsUseCase discoverChampionsUseCase,
            CommunityPulseService communityPulseService,
            IInnovationSpreadPort innovationSpreadPort,
            ILearningCatalystPort learningCatalystPort)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _discoverChampionsUseCase = discoverChampionsUseCase ?? throw new ArgumentNullException(nameof(discoverChampionsUseCase));
            _communityPulseService = communityPulseService ?? throw new ArgumentNullException(nameof(communityPulseService));
            _innovationSpreadPort = innovationSpreadPort ?? throw new ArgumentNullException(nameof(innovationSpreadPort));
            _learningCatalystPort = learningCatalystPort ?? throw new ArgumentNullException(nameof(learningCatalystPort));
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
        /// Conforms to NFRs: Security (1), Telemetry &amp; Audit (2), Performance (6).
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

        /// <summary>
        /// Tracks how an innovation (idea) has spread through the organization.
        /// </summary>
        /// <remarks>
        /// Returns adoption lineage, virality metrics, and current diffusion phase for a specific idea.
        /// Implements the Innovation Spread Engine from the Convener PRD.
        ///
        /// Conforms to NFRs: Security (1), Telemetry &amp; Audit (2), Performance (6).
        /// </remarks>
        /// <param name="ideaId">The unique identifier for the innovation/idea to track.</param>
        /// <returns>Innovation spread analysis including adoption lineage and metrics.</returns>
        [HttpGet("innovation/spread/{ideaId}")]
        [ProducesResponseType(typeof(InnovationSpreadResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInnovationSpread(string ideaId)
        {
            try
            {
                var tenantId = GetTenantIdFromClaims();
                if (tenantId == null)
                {
                    return Unauthorized("Tenant ID is missing from the authentication token.");
                }

                if (string.IsNullOrWhiteSpace(ideaId))
                {
                    return BadRequest("The 'ideaId' path parameter is required.");
                }

                _logger.LogInformation(
                    "Tracking innovation spread for Idea '{IdeaId}' in Tenant '{TenantId}'.",
                    Sanitize(ideaId), Sanitize(tenantId));

                var result = await _innovationSpreadPort.GetInnovationSpreadAsync(ideaId, tenantId);
                if (result == null)
                {
                    return NotFound($"No innovation data found for idea '{ideaId}'.");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during innovation spread tracking for Idea '{IdeaId}'.", Sanitize(ideaId));
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Generates personalized learning catalyst recommendations for the authenticated user.
        /// </summary>
        /// <remarks>
        /// Analyzes the user's skill profile, identifies gaps, and recommends targeted learning
        /// activities sourced from champions and curated content. Links contributions to outcomes.
        ///
        /// Conforms to NFRs: Security (1), Privacy (4), Telemetry &amp; Audit (2).
        /// </remarks>
        /// <param name="request">Optional request body with focus areas and result limits.</param>
        /// <returns>Curated learning recommendations with identified skill gaps.</returns>
        [HttpPost("learning/catalysts/recommend")]
        [ProducesResponseType(typeof(LearningCatalystResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLearningRecommendations([FromBody] LearningCatalystRequest? request)
        {
            try
            {
                var tenantId = GetTenantIdFromClaims();
                if (tenantId == null)
                {
                    return Unauthorized("Tenant ID is missing from the authentication token.");
                }

                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized("User ID is missing from the authentication token.");
                }

                request ??= new LearningCatalystRequest();
                request.TenantId = tenantId;
                request.UserId = userId;

                _logger.LogInformation(
                    "Generating learning catalyst recommendations for User '{UserId}' in Tenant '{TenantId}'.",
                    userId, tenantId);

                var response = await _learningCatalystPort.GetRecommendationsAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during learning catalyst recommendation.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Helper method to securely retrieve the Tenant ID from the user's claims.
        /// </summary>
        private string? GetTenantIdFromClaims()
        {
            return User.FindFirstValue("tenant_id");
        }
    }
}
