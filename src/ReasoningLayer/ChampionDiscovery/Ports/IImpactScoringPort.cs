using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// --- DTOs for the Impact Scoring Port ---
namespace CognitiveMesh.ReasoningLayer.ChampionDiscovery.Ports.Models
{
    /// <summary>
    /// Represents the input for a request to score the impact of a single event.
    /// </summary>
    public class ImpactScoringRequest
    {
        /// <summary>
        /// A unique identifier for the event being scored.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// The type of the event (e.g., "MessageSent", "FileShared", "EndorsementGiven").
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// The ID of the user or system that initiated the event.
        /// </summary>
        public string ActorId { get; set; }

        /// <summary>
        /// The tenant ID, used for scoping and applying tenant-specific scoring models.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// A flexible dictionary containing the payload or context of the event.
        /// </summary>
        public Dictionary<string, object> Context { get; set; }
    }

    /// <summary>
    /// Represents the calculated impact score for a single event.
    /// </summary>
    public class ImpactScore
    {
        /// <summary>
        /// The identifier of the event that was scored, used for correlation.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// The calculated impact score, typically a normalized value.
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// An optional explanation of how the score was derived.
        /// </summary>
        public string Explanation { get; set; }

        /// <summary>
        /// The version of the scoring model that was used.
        /// </summary>
        public string ModelVersion { get; set; }
    }

    /// <summary>
    /// Represents a request to score a batch of events asynchronously.
    /// </summary>
    public class BulkImpactScoringRequest
    {
        /// <summary>
        /// A collection of individual scoring requests to be processed in a single batch.
        /// </summary>
        public IEnumerable<ImpactScoringRequest> Requests { get; set; }
    }

    /// <summary>
    /// Represents the response from a bulk scoring operation.
    /// </summary>
    public class BulkImpactScoringResponse
    {
        /// <summary>
        /// A collection of impact scores corresponding to the batch request.
        /// </summary>
        public IEnumerable<ImpactScore> Scores { get; set; }
    }
}


// --- Port Interface ---
namespace CognitiveMesh.ReasoningLayer.ChampionDiscovery.Ports
{
    using CognitiveMesh.ReasoningLayer.ChampionDiscovery.Ports.Models;

    /// <summary>
    /// Defines the contract for the Impact Scoring Port in the Reasoning Layer.
    /// This port is the primary entry point for all impact measurement and scoring logic,
    /// adhering to the Hexagonal Architecture pattern.
    /// </summary>
    public interface IImpactScoringPort
    {
        /// <summary>
        /// Scores the impact of a single event.
        /// This is a high-priority (Must) operation with a strict performance SLA.
        /// </summary>
        /// <param name="request">The request containing the event data to be scored.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the
        /// calculated <see cref="ImpactScore"/>.
        /// </returns>
        /// <remarks>
        /// **SLA:** This operation must return a result in less than 200ms (P95).
        /// **Acceptance Criteria:** Given a valid event, when scored, the result is returned within the SLA.
        /// </remarks>
        Task<ImpactScore> ScoreImpactAsync(ImpactScoringRequest request);

        /// <summary>
        /// Scores a batch of events asynchronously.
        /// This is a secondary-priority (Should) operation designed for bulk processing and efficiency.
        /// </summary>
        /// <param name="request">The request containing a collection of events to be scored.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the
        /// <see cref="BulkImpactScoringResponse"/> with scores for all processed events.
        /// </returns>
        Task<BulkImpactScoringResponse> ScoreImpactBatchAsync(BulkImpactScoringRequest request);
    }
}
