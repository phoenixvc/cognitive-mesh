using CognitiveMesh.FoundationLayer.ConvenerData.Entities;
using CognitiveMesh.FoundationLayer.ConvenerData.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// --- Supporting Interfaces & Models ---
// Note: These contracts now logically reside in the FoundationLayer's interface definitions.
// They define the data required by the domain logic from the infrastructure layer.

namespace CognitiveMesh.FoundationLayer.ConvenerData.Interfaces
{
    /// <summary>
    /// Represents a data snapshot of a champion's activities, used for scoring.
    /// This DTO is used to pass data from Infrastructure to the Domain layer
    /// without coupling the domain logic to a specific data source.
    /// </summary>
    public class ChampionDataSnapshot
    {
        public string UserId { get; set; }
        public int InteractionCount { get; set; }
        public int EndorsementCount { get; set; }
        public DateTimeOffset LastActivityDate { get; set; }
    }

    /// <summary>
    /// Defines the contract for a repository that fetches champion activity data.
    /// This interface is part of the Core/Foundation layer and is implemented in the Infrastructure layer.
    /// </summary>
    public interface IChampionDataRepository
    {
        /// <summary>
        /// Retrieves a snapshot of activity data for a single champion.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve data for.</param>
        /// <param name="tenantId">The tenant ID to scope the data retrieval.</param>
        /// <returns>A snapshot of the champion's data.</returns>
        Task<ChampionDataSnapshot> GetChampionDataSnapshotAsync(string userId, string tenantId);

        /// <summary>
        /// Retrieves activity data snapshots for multiple champions in a single batch.
        /// </summary>
        /// <param name="userIds">A collection of user IDs.</param>
        /// <param name="tenantId">The tenant ID to scope the data retrieval.</param>
        /// <returns>A dictionary mapping user IDs to their data snapshots.</returns>
        Task<Dictionary<string, ChampionDataSnapshot>> GetChampionDataSnapshotsAsync(IEnumerable<string> userIds, string tenantId);
    }
}


// --- Domain Service ---
namespace CognitiveMesh.ReasoningLayer.ChampionDiscovery
{
    using CognitiveMesh.FoundationLayer.ConvenerData.Interfaces;

    /// <summary>
    /// Implements the core business logic for scoring and ranking knowledge champions.
    /// This is a pure domain service, containing no infrastructure-specific code.
    /// It depends only on abstractions defined in the Core/Foundation layer.
    /// </summary>
    public class ChampionScorer
    {
        private readonly IChampionDataRepository _championDataRepository;

        // Scoring weights can be tuned and potentially externalized to a config system in the future.
        private const double InteractionWeight = 1.5;
        private const double EndorsementWeight = 2.5;
        private const double RecencyWeight = 1.2;
        private const int RecencyPeriodDays = 90;

        public ChampionScorer(IChampionDataRepository championDataRepository)
        {
            _championDataRepository = championDataRepository ?? throw new ArgumentNullException(nameof(championDataRepository));
        }

        /// <summary>
        /// Calculates a champion's influence score based on their activity data.
        /// </summary>
        /// <param name="data">A snapshot of the champion's activity.</param>
        /// <returns>The calculated influence score.</returns>
        public double CalculateInfluenceScore(ChampionDataSnapshot data)
        {
            if (data == null) return 0.0;

            // Calculate a recency factor (score is higher for more recent activity)
            var daysSinceLastActivity = (DateTimeOffset.UtcNow - data.LastActivityDate).TotalDays;
            var recencyFactor = Math.Max(0, (RecencyPeriodDays - daysSinceLastActivity) / RecencyPeriodDays); // Value between 0 and 1

            // Apply weighted formula
            var rawScore = (data.InteractionCount * InteractionWeight) +
                           (data.EndorsementCount * EndorsementWeight);

            var finalScore = rawScore * (1 + (recencyFactor * RecencyWeight));

            return ValidateScoring(finalScore) ? Math.Round(finalScore, 2) : 0.0;
        }

        /// <summary>
        /// Ranks a collection of champions by calculating and updating their influence scores.
        /// </summary>
        /// <param name="champions">The list of Champion entities to rank.</param>
        /// <returns>A ranked, ordered list of champions with updated scores and provenance.</returns>
        public async Task<IEnumerable<Champion>> RankChampions(IEnumerable<Champion> champions)
        {
            if (champions == null || !champions.Any())
                return Enumerable.Empty<Champion>();

            var userIds = champions.Select(c => c.UserId).ToList();
            var tenantId = champions.First().TenantId; // Assume all champions are from the same tenant for a given ranking operation.

            // 1. Fetch all data in a single batch from the repository.
            var dataSnapshots = await _championDataRepository.GetChampionDataSnapshotsAsync(userIds, tenantId);

            // 2. Calculate scores and update entities.
            foreach (var champion in champions)
            {
                if (dataSnapshots.TryGetValue(champion.UserId, out var snapshot))
                {
                    var newScore = CalculateInfluenceScore(snapshot);

                    // 3. Create a provenance entry to explain the score update.
                    var reason = new ProvenanceEntry(
                        Source: nameof(ChampionScorer),
                        EventType: "InfluenceScoreCalculated",
                        Details: $"Score calculated based on {snapshot.InteractionCount} interactions and {snapshot.EndorsementCount} endorsements.",
                        Timestamp: DateTimeOffset.UtcNow
                    );

                    // 4. Update the champion aggregate root with the new score and provenance.
                    champion.UpdateInfluenceScore(newScore, reason);
                }
            }

            // 5. Return the champions, sorted by their new score.
            return champions.OrderByDescending(c => c.InfluenceScore);
        }

        /// <summary>
        /// Validates that a calculated score is within acceptable business rule boundaries.
        /// </summary>
        /// <param name="score">The score to validate.</param>
        /// <returns>True if the score is valid, otherwise false.</returns>
        public bool ValidateScoring(double score)
        {
            // A simple business rule: scores cannot be negative.
            // More complex rules could be added here (e.g., checking for outliers).
            return score >= 0;
        }
    }
}
