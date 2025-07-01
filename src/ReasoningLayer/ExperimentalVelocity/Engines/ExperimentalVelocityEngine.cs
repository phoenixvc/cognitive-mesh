using CognitiveMesh.ReasoningLayer.ExperimentalVelocity.Ports;
using CognitiveMesh.ReasoningLayer.ExperimentalVelocity.Ports.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CognitiveMesh.ReasoningLayer.ExperimentalVelocity.Engines
{
    /// <summary>
    /// A pure domain engine that implements the core business logic for the Experimental Velocity Port.
    /// This engine is responsible for velocity recalibration, innovation theater detection, and competitive reality checks.
    /// As part of a Hexagonal Architecture, this engine is completely isolated from infrastructure concerns.
    /// </summary>
    public class ExperimentalVelocityEngine : IExperimentalVelocityPort
    {
        private readonly ILogger<ExperimentalVelocityEngine> _logger;

        // Model versions for provenance tracking, as required by the PRD.
        private const string RecalibrationModelVersion = "VelocityRecalibrator-v1.0";
        private const string TheaterModelVersion = "InnovationTheaterDetector-v1.2";
        private const string RealityCheckModelVersion = "CompetitiveRealityCheck-v1.1";

        public ExperimentalVelocityEngine(ILogger<ExperimentalVelocityEngine> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public Task<VelocityRecalibrationResponse> RecalibrateAsync(VelocityRecalibrationRequest request)
        {
            if (request?.Provenance == null || string.IsNullOrWhiteSpace(request.ProjectId))
            {
                throw new ArgumentException("Request, Provenance, and ProjectId are required.", nameof(request));
            }

            _logger.LogInformation(
                "Recalibrating velocity for ProjectId '{ProjectId}' with CorrelationId '{CorrelationId}'.",
                request.ProjectId, request.Provenance.CorrelationId);

            // Core "divide by 100" logic.
            var recalibratedEffort = request.OriginalEffortHours / 100.0;
            var recalibratedCost = request.OriginalCost / 100.0;

            var response = new VelocityRecalibrationResponse
            {
                ProjectId = request.ProjectId,
                OriginalEffortHours = request.OriginalEffortHours,
                RecalibratedEffortHours = Math.Round(recalibratedEffort, 2),
                OriginalCost = request.OriginalCost,
                RecalibratedCost = Math.Round(recalibratedCost, 2),
                Explanation = "Velocity recalibrated based on the '100x Experimentation' model, assuming AI-native tooling and processes.",
                ModelVersion = RecalibrationModelVersion,
                CorrelationId = request.Provenance.CorrelationId
            };

            return Task.FromResult(response);
        }

        /// <inheritdoc />
        public Task<TheaterDetectionResponse> DetectInnovationTheaterAsync(TheaterDetectionRequest request)
        {
            if (request?.Provenance == null || string.IsNullOrWhiteSpace(request.ProjectId) || request.ProjectMetadata == null)
            {
                throw new ArgumentException("Request, Provenance, ProjectId, and Metadata are required.", nameof(request));
            }

            _logger.LogInformation(
                "Detecting innovation theater for ProjectId '{ProjectId}' with CorrelationId '{CorrelationId}'.",
                request.ProjectId, request.Provenance.CorrelationId);
            
            var factors = new Dictionary<string, double>();
            double score = 0.0;

            // Analyze metadata based on a simple weighted model.
            // High meeting count is a negative signal.
            if (request.ProjectMetadata.TryGetValue("meeting_count", out var meetings) && Convert.ToInt32(meetings) > 10)
            {
                score += 0.4;
                factors.Add("Excessive Meetings", 0.4);
            }

            // Low decision velocity is a negative signal.
            if (request.ProjectMetadata.TryGetValue("decision_velocity", out var velocity) && Convert.ToDouble(velocity) < 0.2)
            {
                score += 0.3;
                factors.Add("Low Decision Velocity", 0.3);
            }

            // Low stakeholder alignment is a strong negative signal.
            if (request.ProjectMetadata.TryGetValue("stakeholder_alignment_score", out var alignment) && Convert.ToDouble(alignment) < 0.5)
            {
                score += 0.5;
                factors.Add("Poor Stakeholder Alignment", 0.5);
            }

            // Clamp the score to be between 0.0 and 1.0.
            var finalScore = Math.Min(1.0, score);
            
            string riskLevel;
            if (finalScore > 0.7) riskLevel = "High";
            else if (finalScore > 0.4) riskLevel = "Medium";
            else riskLevel = "Low";

            var response = new TheaterDetectionResponse
            {
                ProjectId = request.ProjectId,
                TheaterRiskScore = Math.Round(finalScore, 2),
                RiskLevel = riskLevel,
                ContributingFactors = factors.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToList(),
                ModelVersion = TheaterModelVersion,
                CorrelationId = request.Provenance.CorrelationId
            };

            return Task.FromResult(response);
        }

        /// <inheritdoc />
        public Task<CompetitiveRealityCheckResponse> PerformRealityCheckAsync(CompetitiveRealityCheckRequest request)
        {
            if (request?.Provenance == null || string.IsNullOrWhiteSpace(request.ProjectId) || request.CurrentProjectMetrics == null)
            {
                throw new ArgumentException("Request, Provenance, ProjectId, and Metrics are required.", nameof(request));
            }

            _logger.LogInformation(
                "Performing competitive reality check for ProjectId '{ProjectId}' with CorrelationId '{CorrelationId}'.",
                request.ProjectId, request.Provenance.CorrelationId);

            var benchmarks = GetIndustryBenchmarks(request.IndustrySector);
            var gapAnalysis = new Dictionary<string, double>();
            bool isAtRisk = false;

            foreach (var metric in request.CurrentProjectMetrics)
            {
                if (benchmarks.TryGetValue(metric.Key, out var benchmarkValue))
                {
                    // Positive gap means the project is slower/worse than the benchmark.
                    var gap = ((metric.Value - benchmarkValue) / benchmarkValue);
                    gapAnalysis.Add(metric.Key, Math.Round(gap, 2));
                    if (gap > 0.25) // 25% worse than benchmark is a risk flag
                    {
                        isAtRisk = true;
                    }
                }
            }

            var response = new CompetitiveRealityCheckResponse
            {
                ProjectId = request.ProjectId,
                IsAtCompetitiveRisk = isAtRisk,
                RiskSummary = isAtRisk
                    ? "Project metrics lag significantly behind AI-enabled industry benchmarks."
                    : "Project metrics are within competitive industry benchmarks.",
                GapAnalysis = gapAnalysis,
                ModelVersion = RealityCheckModelVersion,
                CorrelationId = request.Provenance.CorrelationId
            };

            return Task.FromResult(response);
        }

        /// <summary>
        /// Mock/placeholder for fetching competitive benchmarks. In a real system, this would
        /// come from a dedicated data source updated regularly.
        /// </summary>
        private Dictionary<string, double> GetIndustryBenchmarks(string sector)
        {
            // Lower is better for these metrics.
            return sector.ToLowerInvariant() switch
            {
                "fintech" => new Dictionary<string, double>
                {
                    { "time_to_market_days", 30.0 },
                    { "feature_velocity_per_sprint", 5.0 }
                },
                "healthcare_ai" => new Dictionary<string, double>
                {
                    { "time_to_market_days", 90.0 },
                    { "feature_velocity_per_sprint", 2.0 }
                },
                _ => new Dictionary<string, double> // Default/generic tech benchmarks
                {
                    { "time_to_market_days", 60.0 },
                    { "feature_velocity_per_sprint", 3.0 }
                }
            };
        }
    }
}
