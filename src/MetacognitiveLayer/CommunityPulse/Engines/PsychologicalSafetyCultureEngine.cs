using CognitiveMesh.FoundationLayer.ConvenerData.Interfaces;
using CognitiveMesh.MetacognitiveLayer.CommunityPulse.Ports;
using CognitiveMesh.MetacognitiveLayer.CommunityPulse.Ports.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CognitiveMesh.MetacognitiveLayer.CommunityPulse.Engines
{
    /// <summary>
    /// A pure domain engine that implements the core business logic for analyzing
    /// psychological safety and culture health. It aggregates signals, performs trend
    /// analysis, and surfaces alerts, adhering to the Hexagonal Architecture pattern.
    /// </summary>
    public class PsychologicalSafetyCultureEngine : ISafetyMetricsPort
    {
        private readonly ILogger<PsychologicalSafetyCultureEngine> _logger;
        private readonly ICommunityDataRepository _communityDataRepository;

        // Configuration for scoring and alerting. In a real system, these would be externalized.
        private static class Thresholds
        {
            public const double HighRiskSafetyScore = 40.0;
            public const double MediumRiskSafetyScore = 70.0;
            public const double NegativeSentimentAlert = 0.30; // 30%
            public const double EngagementDropAlertSlope = -0.5;
            public const int MinEventsForTrendAnalysis = 10;
        }

        public PsychologicalSafetyCultureEngine(
            ILogger<PsychologicalSafetyCultureEngine> logger,
            ICommunityDataRepository communityDataRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _communityDataRepository = communityDataRepository ?? throw new ArgumentNullException(nameof(communityDataRepository));
        }

        /// <inheritdoc />
        public async Task<SafetyMetricsResponse> GetCurrentMetricsAsync(SafetyMetricsRequest request)
        {
            var endTime = DateTimeOffset.UtcNow;
            var startTime = endTime.AddDays(-request.TimeframeInDays);

            var events = await _communityDataRepository.GetInteractionEventsAsync(request.TenantId, request.ScopeId, startTime, endTime);
            
            var metrics = CalculateSafetyMetricsForPeriod(events.ToList());

            return new SafetyMetricsResponse
            {
                TenantId = request.TenantId,
                ScopeId = request.ScopeId,
                AsOfTimestamp = endTime,
                Metrics = metrics
            };
        }

        /// <inheritdoc />
        public async Task<TrendAnalysisResponse> AnalyzeTrendsAsync(TrendAnalysisRequest request)
        {
            _logger.LogInformation("Starting trend analysis for Scope '{ScopeId}' from {StartTime} to {EndTime}.", request.ScopeId, request.StartTime, request.EndTime);

            var historicalMetrics = new List<SafetyMetricsResponse>();
            var allEvents = (await _communityDataRepository.GetInteractionEventsAsync(request.TenantId, request.ScopeId, request.StartTime, request.EndTime)).ToList();

            if (allEvents.Count < Thresholds.MinEventsForTrendAnalysis)
            {
                _logger.LogWarning("Insufficient data for trend analysis. Found only {EventCount} events.", allEvents.Count);
                return new TrendAnalysisResponse();
            }

            // Group events by day to create a time series
            var eventsByDay = allEvents
                .GroupBy(e => e.Timestamp.Date)
                .OrderBy(g => g.Key);

            foreach (var dayGroup in eventsByDay)
            {
                var dailyMetrics = CalculateSafetyMetricsForPeriod(dayGroup.ToList());
                historicalMetrics.Add(new SafetyMetricsResponse
                {
                    TenantId = request.TenantId,
                    ScopeId = request.ScopeId,
                    AsOfTimestamp = new DateTimeOffset(dayGroup.Key, TimeSpan.Zero),
                    Metrics = dailyMetrics
                });
            }

            var triggeredAlerts = GenerateAlerts(historicalMetrics);

            return new TrendAnalysisResponse
            {
                HistoricalMetrics = historicalMetrics,
                TriggeredAlerts = triggeredAlerts
            };
        }

        private SafetyMetrics CalculateSafetyMetricsForPeriod(List<RawInteractionEvent> events)
        {
            if (events == null || !events.Any())
            {
                return new SafetyMetrics { SafetyScore = 50, RiskLevel = "Unknown", ContributingFactors = { { "DataPoints", 0 } } };
            }
            
            var sentiment = CalculateSentimentMetrics(events);
            var engagement = CalculateEngagementMetrics(events);

            // Sophisticated safety scoring model
            var contributingFactors = new Dictionary<string, double>
            {
                { "SentimentBalance", (sentiment.PositiveRatio - sentiment.NegativeRatio) },
                { "EngagementVolume", engagement.TotalMessages },
                { "UserParticipation", engagement.ActiveUsers }
            };
            
            double score = 50.0; // Start from a neutral baseline
            score += contributingFactors["SentimentBalance"] * 50; // Sentiment is a primary driver
            score += Math.Log10(Math.Max(1, engagement.TotalMessages)) * 5; // Logarithmic scale for volume
            
            score = Math.Max(0, Math.Min(100, score)); // Clamp score between 0 and 100

            string riskLevel = "Low";
            if (score < Thresholds.HighRiskSafetyScore) riskLevel = "High";
            else if (score < Thresholds.MediumRiskSafetyScore) riskLevel = "Medium";

            return new SafetyMetrics
            {
                SafetyScore = Math.Round(score, 2),
                RiskLevel = riskLevel,
                ContributingFactors = contributingFactors
            };
        }

        private List<SafetyAlert> GenerateAlerts(List<SafetyMetricsResponse> historicalMetrics)
        {
            var alerts = new List<SafetyAlert>();
            if (historicalMetrics.Count < 2) return alerts;

            var latest = historicalMetrics.Last();

            // Alert 1: High negative sentiment
            if (latest.Metrics.ContributingFactors.TryGetValue("NegativeSentimentRatio", out var negRatio) && negRatio > Thresholds.NegativeSentimentAlert)
            {
                alerts.Add(new SafetyAlert
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    AlertType = "NegativeSentimentSpike",
                    Description = $"Negative sentiment ratio reached {negRatio:P0}, exceeding the threshold of {Thresholds.NegativeSentimentAlert:P0}.",
                    Severity = "High"
                });
            }

            // Alert 2: Sustained engagement drop (using linear regression)
            var engagementDataPoints = historicalMetrics
                .Select((m, i) => new { Index = (double)i, Value = m.Metrics.ContributingFactors["EngagementVolume"] })
                .ToList();

            if (engagementDataPoints.Count > 3)
            {
                double slope = CalculateLinearRegressionSlope(engagementDataPoints.Select(dp => dp.Index).ToArray(), engagementDataPoints.Select(dp => dp.Value).ToArray());
                if (slope < Thresholds.EngagementDropAlertSlope)
                {
                    alerts.Add(new SafetyAlert
                    {
                        Timestamp = DateTimeOffset.UtcNow,
                        AlertType = "SustainedEngagementDrop",
                        Description = $"Engagement volume shows a significant downward trend (slope: {slope:F2}).",
                        Severity = "Medium"
                    });
                }
            }

            return alerts;
        }

        private SentimentMetrics CalculateSentimentMetrics(List<RawInteractionEvent> events)
        {
            var validEvents = events.Where(e => e.SentimentScore.HasValue).ToList();
            if (!validEvents.Any()) return new SentimentMetrics();
            
            double total = validEvents.Count;
            double positive = validEvents.Count(e => e.SentimentScore > 0.2);
            double negative = validEvents.Count(e => e.SentimentScore < -0.2);
            
            return new SentimentMetrics
            {
                AverageSentiment = Math.Round(validEvents.Average(e => e.SentimentScore.Value), 2),
                PositiveRatio = Math.Round(positive / total, 2),
                NegativeRatio = Math.Round(negative / total, 2),
                NeutralRatio = Math.Round((total - positive - negative) / total, 2)
            };
        }

        private EngagementMetrics CalculateEngagementMetrics(List<RawInteractionEvent> events)
        {
            return new EngagementMetrics
            {
                TotalMessages = events.Count,
                ActiveUsers = events.Select(e => e.UserId).Distinct().Count(),
                TotalReactions = events.Sum(e => e.ReactionCount)
            };
        }

        private double CalculateLinearRegressionSlope(double[] xVals, double[] yVals)
        {
            if (xVals.Length != yVals.Length || xVals.Length < 2)
            {
                return 0;
            }

            double sumX = 0, sumY = 0, sumXy = 0, sumX2 = 0;
            int n = xVals.Length;

            for (int i = 0; i < n; i++)
            {
                sumX += xVals[i];
                sumY += yVals[i];
                sumXy += xVals[i] * yVals[i];
                sumX2 += xVals[i] * xVals[i];
            }

            double numerator = n * sumXy - sumX * sumY;
            double denominator = n * sumX2 - sumX * sumX;

            return denominator == 0 ? 0 : numerator / denominator;
        }
    }
}
