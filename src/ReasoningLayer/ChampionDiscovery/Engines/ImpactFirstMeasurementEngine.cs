using CognitiveMesh.ReasoningLayer.ChampionDiscovery.Ports;
using CognitiveMesh.ReasoningLayer.ChampionDiscovery.Ports.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CognitiveMesh.ReasoningLayer.ChampionDiscovery.Engines
{
    /// <summary>
    /// A pure domain engine that implements the core business logic for scoring the impact of various events.
    /// As part of a Hexagonal Architecture, this engine is completely isolated from infrastructure concerns
    /// and depends only on the contracts defined by its Port.
    /// </summary>
    public class ImpactFirstMeasurementEngine : IImpactScoringPort
    {
        private readonly ILogger<ImpactFirstMeasurementEngine> _logger;
        private const string ModelVersion = "ImpactModel-v1.1.0";

        public ImpactFirstMeasurementEngine(ILogger<ImpactFirstMeasurementEngine> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public Task<ImpactScore> ScoreImpactAsync(ImpactScoringRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.EventId))
                {
                    _logger.LogWarning("Invalid ImpactScoringRequest received: Request or EventId is null.");
                    throw new ArgumentNullException(nameof(request), "The scoring request cannot be null and must have an EventId.");
                }

                _logger.LogDebug("Scoring impact for EventId '{EventId}' of type '{EventType}'.", request.EventId, request.EventType);

                double score;
                string explanation;

                // Route to the appropriate scoring algorithm based on the event type.
                switch (request.EventType.ToLowerInvariant())
                {
                    case "social.interaction.shared":
                    case "social.interaction.commented":
                    case "social.interaction.liked":
                        (score, explanation) = CalculateSocialImpact(request);
                        break;

                    case "innovation.idea.submitted":
                    case "innovation.prototype.developed":
                        (score, explanation) = CalculateInnovationImpact(request);
                        break;

                    case "learning.course.completed":
                    case "learning.skill.applied":
                        (score, explanation) = CalculateLearningImpact(request);
                        break;
                    
                    case "workflow.step.completed":
                        (score, explanation) = CalculateWorkflowDepthImpact(request);
                        break;

                    default:
                        _logger.LogInformation("No specific scoring model found for event type '{EventType}'. Assigning default impact score.", request.EventType);
                        score = 1.0; // Default score for any tracked event
                        explanation = "Default impact score assigned for a tracked system event.";
                        break;
                }

                var result = new ImpactScore
                {
                    EventId = request.EventId,
                    Score = Math.Round(score, 4),
                    Explanation = explanation,
                    ModelVersion = ModelVersion
                };

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while scoring impact for EventId '{EventId}'.", request?.EventId);
                // In a real system, we might return a result with an error state instead of throwing.
                // For now, re-throwing allows the adapter layer to handle it.
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<BulkImpactScoringResponse> ScoreImpactBatchAsync(BulkImpactScoringRequest request)
        {
            if (request?.Requests == null || !request.Requests.Any())
            {
                _logger.LogWarning("BulkImpactScoringRequest received with no requests to process.");
                return new BulkImpactScoringResponse { Scores = Enumerable.Empty<ImpactScore>() };
            }

            _logger.LogInformation("Processing a batch of {RequestCount} impact scoring requests.", request.Requests.Count());

            // Process requests in parallel for efficiency.
            var scoringTasks = request.Requests.Select(req => ScoreImpactAsync(req));
            var scores = await Task.WhenAll(scoringTasks);

            return new BulkImpactScoringResponse { Scores = scores };
        }

        // --- Private Scoring Algorithms ---

        private (double score, string explanation) CalculateSocialImpact(ImpactScoringRequest request)
        {
            // A simple weighted model for social interactions.
            double score = 0;
            request.Context.TryGetValue("likes", out var likes);
            request.Context.TryGetValue("shares", out var shares);
            request.Context.TryGetValue("reach", out var reach);

            int likesCount = Convert.ToInt32(likes);
            int sharesCount = Convert.ToInt32(shares);
            int reachCount = Convert.ToInt32(reach);

            // Virality component: shares have the highest weight.
            score += sharesCount * 5.0;
            // Engagement component: likes are a simple form of engagement.
            score += likesCount * 0.5;
            // Reach component: how many people saw the content.
            score += reachCount * 0.1;

            string explanation = $"Social Impact Score calculated from {sharesCount} shares, {likesCount} likes, and {reachCount} reach.";
            return (score, explanation);
        }

        private (double score, string explanation) CalculateInnovationImpact(ImpactScoringRequest request)
        {
            // Innovation is highly valued, so it gets a higher base score.
            double score = 50.0;
            string explanation = "Base score for a significant innovation event.";

            if (request.Context.TryGetValue("isPrototype", out var isProto) && Convert.ToBoolean(isProto))
            {
                score *= 2; // Prototypes are more impactful than ideas.
                explanation = "Doubled score for a prototype submission.";
            }

            return (score, explanation);
        }

        private (double score, string explanation) CalculateLearningImpact(ImpactScoringRequest request)
        {
            // Score based on the difficulty or level of the course/skill.
            double score = 10.0;
            request.Context.TryGetValue("skillLevel", out var level);
            int skillLevel = Convert.ToInt32(level); // e.g., 1=Beginner, 2=Intermediate, 3=Advanced

            if (skillLevel > 0)
            {
                score *= skillLevel;
            }

            string explanation = $"Learning impact score calculated for skill level {skillLevel}.";
            return (score, explanation);
        }

        private (double score, string explanation) CalculateWorkflowDepthImpact(ImpactScoringRequest request)
        {
            // The deeper into a workflow a user gets, the higher the impact.
            double score = 2.0;
            request.Context.TryGetValue("stepCount", out var steps);
            int stepCount = Convert.ToInt32(steps);

            // Exponentially increase score with depth to reward completion of complex tasks.
            score *= Math.Pow(1.2, stepCount);

            string explanation = $"Workflow depth impact calculated for completing step {stepCount}.";
            return (score, explanation);
        }
    }
}
