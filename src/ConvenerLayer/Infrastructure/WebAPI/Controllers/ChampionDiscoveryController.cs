using CognitiveMesh.ConvenerLayer.Application.UseCases.ChampionDiscovery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CognitiveMesh.ConvenerLayer.Infrastructure.WebAPI.Controllers
{
    /// <summary>
    /// API controller for discovering knowledge champions within the Cognitive Mesh.
    /// This controller exposes the Champion Discovery use case through RESTful endpoints,
    /// adhering to the defined API contract and security policies.
    /// </summary>
    [ApiController]
    [Route("api/v1/champions")]
    [Authorize(Policy = "UserAccess")] // Enforces that only authenticated users can access these endpoints.
    public class ChampionDiscoveryController : ControllerBase
    {
        private readonly DiscoverChampionsUseCase _discoverChampionsUseCase;
        private readonly ILogger<ChampionDiscoveryController> _logger;

        public ChampionDiscoveryController(
            DiscoverChampionsUseCase discoverChampionsUseCase,
            ILogger<ChampionDiscoveryController> logger)
        {
            _discoverChampionsUseCase = discoverChampionsUseCase ?? throw new ArgumentNullException(nameof(discoverChampionsUseCase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Discovers and ranks knowledge champions based on specified criteria.
        /// </summary>
        /// <remarks>
        /// This endpoint queries the Cognitive Mesh to find users who are influential experts
        /// in a given skill area. The results are scored and ranked based on their interactions,
        /// endorsements, and recent activity. All data access is strictly scoped to the
        /// authenticated user's tenant.
        ///
        /// Conforms to NFRs: Security (1), Telemetry & Audit (2), Performance (6).
        /// </remarks>
        /// <param name="skill" example="MLOps">An optional skill to filter champions by.</param>
        /// <param name="maxResults" example="10">The maximum number of champions to return.</param>
        /// <returns>A ranked list of discovered champions.</returns>
        [HttpGet("discover")]
        [ProducesResponseType(typeof(DiscoverChampionsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Discover([FromQuery] string skill, [FromQuery] int maxResults = 10)
        {
            try
            {
                // Enforce tenant isolation by extracting TenantId from the authenticated user's claims.
                var tenantId = User.FindFirstValue("tenant_id");
                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    _logger.LogWarning("Tenant ID claim is missing for the authenticated user.");
                    return BadRequest("Unable to determine tenant context for the request.");
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
                // Log the full exception details for internal diagnostics.
                _logger.LogError(ex, "An unhandled exception occurred while executing the DiscoverChampions use case.");

                // Return a generic, safe error message to the client.
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred. Please try again later.");
            }
        }
    }
}
