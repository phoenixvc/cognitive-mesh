using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.CommunityPulse;

// --- Supporting Interfaces & Models ---
// Note: These contracts define the data required by the domain logic from the infrastructure layer.
// The repository interface would reside in the FoundationLayer's interface definitions.
/// <summary>
/// Represents a raw interaction event from a communication channel.
/// </summary>
public class RawInteractionEvent
{
    public string EventId { get; set; }
    public string UserId { get; set; }
    public string ChannelId { get; set; }
    public string TenantId { get; set; }
    public string Message { get; set; }
    public int ReactionCount { get; set; }
    public double? SentimentScore { get; set; } // e.g., -1.0 (negative) to 1.0 (positive)
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Defines the contract for a repository that fetches raw community interaction data.
/// </summary>
public interface ICommunityDataRepository
{
    /// <summary>
    /// Retrieves raw interaction events for a specific channel within a given timeframe.
    /// </summary>
    /// <param name="tenantId">The tenant ID to scope the query.</param>
    /// <param name="channelId">The ID of the communication channel.</param>
    /// <param name="startTime">The start of the time window.</param>
    /// <param name="endTime">The end of the time window.</param>
    /// <returns>A collection of raw interaction events.</returns>
    Task<IEnumerable<RawInteractionEvent>> GetInteractionEventsAsync(string tenantId, string channelId, DateTimeOffset startTime, DateTimeOffset endTime);
}

// --- DTOs for the Community Pulse Service ---
public class CommunityPulseRequest
{
    public string TenantId { get; set; }
    public string ChannelId { get; set; }
    public int TimeframeInDays { get; set; } = 30; // Default to the last 30 days
}

public class EngagementMetrics
{
    public int TotalMessages { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalReactions { get; set; }
    public string EngagementTrend { get; set; } // "Increasing", "Decreasing", "Stable"
}

public class SentimentMetrics
{
    public double AverageSentiment { get; set; }
    public double PositiveRatio { get; set; }
    public double NegativeRatio { get; set; }
    public double NeutralRatio { get; set; }
}

public class PsychologicalSafetyMetrics
{
    public double SafetyScore { get; set; } // A calculated score from 0 to 100
    public string RiskLevel { get; set; } // "Low", "Medium", "High"
}

public class CommunityPulseResponse
{
    public string ChannelId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public EngagementMetrics Engagement { get; set; }
    public SentimentMetrics Sentiment { get; set; }
    public PsychologicalSafetyMetrics PsychologicalSafety { get; set; }
}


// --- Metacognitive Service ---
/// <summary>
/// A metacognitive service that monitors and analyzes community health by aggregating
/// engagement, sentiment, and psychological safety metrics.
/// </summary>
public class CommunityPulseService
{
    private readonly ILogger<CommunityPulseService> _logger;
    private readonly ICommunityDataRepository _communityDataRepository;

    public CommunityPulseService(ILogger<CommunityPulseService> logger, ICommunityDataRepository communityDataRepository)
    {
        _logger = logger;
        _communityDataRepository = communityDataRepository;
    }

    /// <summary>
    /// Gets the current community pulse for a specified channel.
    /// </summary>
    public async Task<CommunityPulseResponse> GetCommunityPulseAsync(CommunityPulseRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.TenantId) || string.IsNullOrWhiteSpace(request.ChannelId))
        {
            _logger.LogWarning("GetCommunityPulseAsync called with an invalid request.");
            return null;
        }

        var endTime = DateTimeOffset.UtcNow;
        var startTime = endTime.AddDays(-request.TimeframeInDays);

        _logger.LogInformation(
            "Fetching community pulse for Tenant '{TenantId}', Channel '{ChannelId}' from {StartTime} to {EndTime}.",
            request.TenantId, request.ChannelId, startTime, endTime);

        // 1. Aggregate Metrics
        var events = await AggregateMetricsAsync(request.TenantId, request.ChannelId, startTime, endTime);
        if (!events.Any())
        {
            _logger.LogInformation("No interaction events found for the specified criteria.");
            return new CommunityPulseResponse { ChannelId = request.ChannelId, StartDate = startTime, EndDate = endTime };
        }

        // 2. Calculate Metrics
        var engagementMetrics = DetectEngagementTrends(events, request.TimeframeInDays);
        var sentimentMetrics = CalculateSentimentMetrics(events);
        var safetyMetrics = CalculateSafetyScore(sentimentMetrics, engagementMetrics);

        // 3. Assemble Response
        return new CommunityPulseResponse
        {
            ChannelId = request.ChannelId,
            StartDate = startTime,
            EndDate = endTime,
            Engagement = engagementMetrics,
            Sentiment = sentimentMetrics,
            PsychologicalSafety = safetyMetrics
        };
    }

    /// <summary>
    /// Fetches and aggregates raw metrics from the data repository.
    /// </summary>
    private async Task<List<RawInteractionEvent>> AggregateMetricsAsync(string tenantId, string channelId, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        var events = await _communityDataRepository.GetInteractionEventsAsync(tenantId, channelId, startTime, endTime);
        return events.ToList();
    }

    /// <summary>
    /// Calculates psychological safety based on sentiment and engagement patterns.
    /// </summary>
    private PsychologicalSafetyMetrics CalculateSafetyScore(SentimentMetrics sentiment, EngagementMetrics engagement)
    {
        // Placeholder logic: a healthy community has high positive sentiment and stable/increasing engagement.
        // A score of 100 is perfect.
        double score = 50.0;

        // Sentiment is a major factor. High positive ratio is good. High negative is bad.
        score += (sentiment.PositiveRatio - sentiment.NegativeRatio) * 40;

        // Engagement trends also matter. Decreasing engagement is a risk sign.
        if (engagement.EngagementTrend == "Increasing") score += 10;
        if (engagement.EngagementTrend == "Decreasing") score -= 20;

        // Normalize score to be within 0-100.
        score = Math.Max(0, Math.Min(100, score));

        string riskLevel = "Low";
        if (score < 40) riskLevel = "High";
        else if (score < 70) riskLevel = "Medium";

        return new PsychologicalSafetyMetrics
        {
            SafetyScore = Math.Round(score, 2),
            RiskLevel = riskLevel
        };
    }

    /// <summary>
    /// Analyzes engagement trends over the period.
    /// </summary>
    private EngagementMetrics DetectEngagementTrends(List<RawInteractionEvent> events, int timeframeInDays)
    {
        int totalMessages = events.Count;
        int activeUsers = events.Select(e => e.UserId).Distinct().Count();
        int totalReactions = events.Sum(e => e.ReactionCount);

        // Simple trend detection: compare the first half of the period to the second half.
        var midpoint = events.Min(e => e.Timestamp).AddDays(timeframeInDays / 2.0);
        int firstHalfMessages = events.Count(e => e.Timestamp < midpoint);
        int secondHalfMessages = events.Count(e => e.Timestamp >= midpoint);

        string trend = "Stable";
        double changeRatio = (firstHalfMessages > 0) ? ((double)secondHalfMessages - firstHalfMessages) / firstHalfMessages : (secondHalfMessages > 0 ? 1.0 : 0.0);

        if (changeRatio > 0.15) trend = "Increasing";
        if (changeRatio < -0.15) trend = "Decreasing";

        return new EngagementMetrics
        {
            TotalMessages = totalMessages,
            ActiveUsers = activeUsers,
            TotalReactions = totalReactions,
            EngagementTrend = trend
        };
    }
        
    /// <summary>
    /// Calculates sentiment distribution from raw events.
    /// </summary>
    private SentimentMetrics CalculateSentimentMetrics(List<RawInteractionEvent> events)
    {
        var validEvents = events.Where(e => e.SentimentScore.HasValue).ToList();
        if (!validEvents.Any())
        {
            return new SentimentMetrics();
        }

        double totalEvents = validEvents.Count;
        double positiveCount = validEvents.Count(e => e.SentimentScore > 0.2);
        double negativeCount = validEvents.Count(e => e.SentimentScore < -0.2);
        double neutralCount = totalEvents - positiveCount - negativeCount;

        return new SentimentMetrics
        {
            AverageSentiment = Math.Round(validEvents.Average(e => e.SentimentScore.Value), 2),
            PositiveRatio = Math.Round(positiveCount / totalEvents, 2),
            NegativeRatio = Math.Round(negativeCount / totalEvents, 2),
            NeutralRatio = Math.Round(neutralCount / totalEvents, 2)
        };
    }
}