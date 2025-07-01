using CognitiveMesh.ConvenerLayer.Core.Entities;
using CognitiveMesh.ConvenerLayer.Core.Interfaces;
using CognitiveMesh.ConvenerLayer.Core.ValueObjects;
using CognitiveMesh.ConvenerLayer.Domain.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// --- DTOs for the Discover Champions Use Case ---
// Note: These classes define the public contract for this specific application use case.
// They ensure that the internal domain entities are not exposed to the outside world.
namespace CognitiveMesh.ConvenerLayer.Application.UseCases.ChampionDiscovery
{
    /// <summary>
    /// Represents the request to discover knowledge champions.
    /// </summary>
    public class DiscoverChampionsRequest
    {
        /// <summary>
        /// The tenant ID to scope the search, ensuring data isolation.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// An optional filter to find champions with a specific skill.
        /// </summary>
        public string SkillFilter { get; set; }

        /// <summary>
        /// The maximum number of champions to return. Defaults to 10.
        /// </summary>
        public int MaxResults { get; set; } = 10;
    }

    /// <summary>
    /// A Data Transfer Object (DTO) representing a champion's public profile.
    /// </summary>
    public class ChampionDto
    {
        public string UserId { get; set; }
        public double InfluenceScore { get; set; }
        public IEnumerable<string> Skills { get; set; }
        public DateTimeOffset LastActiveDate { get; set; }
    }

    /// <summary>
    /// Represents the response from the champion discovery use case.
    /// </summary>
    public class DiscoverChampionsResponse
    {
        /// <summary>
        /// The ranked list of discovered champions.
        /// </summary>
        public IEnumerable<ChampionDto> Champions { get; set; }
    }


    // --- Application Service / Use Case Handler ---

    /// <summary>
    /// Application service that orchestrates the champion discovery workflow.
    /// It coordinates between domain services and infrastructure (via interfaces)
    /// to fulfill the business use case.
    /// </summary>
    public class DiscoverChampionsUseCase
    {
        private readonly ILogger<DiscoverChampionsUseCase> _logger;
        private readonly IChampionRepository _championRepository; // Depends on Core interface
        private readonly ChampionScorer _championScorer;       // Depends on Domain service

        public DiscoverChampionsUseCase(
            ILogger<DiscoverChampionsUseCase> logger,
            IChampionRepository championRepository,
            ChampionScorer championScorer)
        {
            _logger = logger;
            _championRepository = championRepository;
            _championScorer = championScorer;
        }

        /// <summary>
        /// Executes the champion discovery process.
        /// </summary>
        /// <param name="request">The request containing discovery parameters.</param>
        /// <returns>A response with the ranked list of champions.</returns>
        public async Task<DiscoverChampionsResponse> ExecuteAsync(DiscoverChampionsRequest request)
        {
            // 1. Validate the incoming request
            if (!ValidateRequest(request))
            {
                _logger.LogWarning("Invalid DiscoverChampionsRequest received.");
                return new DiscoverChampionsResponse { Champions = Enumerable.Empty<ChampionDto>() };
            }

            _logger.LogInformation(
                "Executing champion discovery for Tenant '{TenantId}' with SkillFilter '{SkillFilter}'.",
                request.TenantId,
                request.SkillFilter ?? "N/A");

            // 2. Fetch potential champions from the repository (Infrastructure layer)
            var potentialChampions = await _championRepository.FindPotentialChampionsAsync(request.TenantId, request.SkillFilter);

            if (!potentialChampions.Any())
            {
                _logger.LogInformation("No potential champions found for the given criteria.");
                return new DiscoverChampionsResponse { Champions = Enumerable.Empty<ChampionDto>() };
            }

            // 3. Use the Domain Service to perform the core business logic (scoring and ranking)
            var rankedChampions = await _championScorer.RankChampions(potentialChampions);

            // 4. Map the domain entities to DTOs for the response
            var championDtos = rankedChampions
                .Take(request.MaxResults)
                .Select(c => new ChampionDto
                {
                    UserId = c.UserId,
                    InfluenceScore = c.InfluenceScore,
                    Skills = c.Skills.Select(s => s.Name),
                    LastActiveDate = c.LastActiveDate
                });

            _logger.LogInformation("Successfully discovered and ranked {Count} champions.", championDtos.Count());

            return new DiscoverChampionsResponse
            {
                Champions = championDtos
            };
        }

        private bool ValidateRequest(DiscoverChampionsRequest request)
        {
            // Basic validation. In a real application, this could use a library like FluentValidation.
            return request != null &&
                   !string.IsNullOrWhiteSpace(request.TenantId) &&
                   request.MaxResults > 0;
        }
    }
}
