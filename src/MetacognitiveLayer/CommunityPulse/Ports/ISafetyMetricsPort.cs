// --- DTOs for the Safety Metrics Port ---

namespace MetacognitiveLayer.CommunityPulse.Ports;

/// <summary>
/// Represents a request to retrieve psychological safety metrics for a specific scope.
/// </summary>
public class SafetyMetricsRequest
{
    /// <summary>
    /// The tenant ID to scope the query.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// The specific scope ID (e.g., a channel ID, team ID, or organization unit ID).
    /// </summary>
    public string ScopeId { get; set; } = string.Empty;

    /// <summary>
    /// The type of scope being queried (e.g., "Channel", "Team").
    /// </summary>
    public string ScopeType { get; set; } = string.Empty;

    /// <summary>
    /// The number of days of historical data to include in the calculation.
    /// </summary>
    public int TimeframeInDays { get; set; } = 30;
}

/// <summary>
/// Contains the calculated psychological safety and culture health metrics.
/// </summary>
public class SafetyMetrics
{
    /// <summary>
    /// A calculated score from 0 to 100 representing the overall psychological safety.
    /// </summary>
    public double SafetyScore { get; set; }

    /// <summary>
    /// The assessed risk level ("Low", "Medium", "High").
    /// </summary>
    public string RiskLevel { get; set; } = string.Empty;

    /// <summary>
    /// A dictionary of factors that contributed to the score (e.g., "PositiveSentimentRatio", "EngagementTrend").
    /// </summary>
    public Dictionary<string, double> ContributingFactors { get; set; } = new();
}

/// <summary>
/// The response object containing the calculated safety metrics for a given request.
/// </summary>
public class SafetyMetricsResponse
{
    public string TenantId { get; set; } = string.Empty;
    public string ScopeId { get; set; } = string.Empty;
    public DateTimeOffset AsOfTimestamp { get; set; }
    public SafetyMetrics Metrics { get; set; } = null!;
}

/// <summary>
/// Represents a request to analyze safety metric trends over time.
/// </summary>
public class TrendAnalysisRequest
{
    public string TenantId { get; set; } = string.Empty;
    public string ScopeId { get; set; } = string.Empty;
    public string ScopeType { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
}

/// <summary>
/// Represents a specific alert generated when a safety metric crosses a critical threshold.
/// </summary>
public class SafetyAlert
{
    public string AlertId { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset Timestamp { get; set; }
    public string AlertType { get; set; } = string.Empty; // e.g., "NegativeSentimentSpike", "SustainedEngagementDrop"
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // "High", "Medium", "Low"
}

/// <summary>
/// The response from a trend analysis operation, containing trend data and any triggered alerts.
/// </summary>
public class TrendAnalysisResponse
{
    public List<SafetyMetricsResponse> HistoricalMetrics { get; set; } = new();
    public List<SafetyAlert> TriggeredAlerts { get; set; } = new();
}


// --- Port Interface ---
/// <summary>
/// Defines the contract for the Safety Metrics Port in the Metacognitive Layer.
/// This port is the primary entry point for all psychological safety and culture
/// health monitoring and analysis, adhering to the Hexagonal Architecture pattern.
/// </summary>
public interface ISafetyMetricsPort
{
    /// <summary>
    /// Retrieves the current psychological safety metrics for a given scope.
    /// This is a high-priority (Must) operation for real-time dashboard widgets.
    /// </summary>
    /// <param name="request">The request specifying the scope and timeframe for the metrics.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the
    /// calculated <see cref="SafetyMetricsResponse"/>.
    /// </returns>
    Task<SafetyMetricsResponse> GetCurrentMetricsAsync(SafetyMetricsRequest request);

    /// <summary>
    /// Analyzes safety metrics over a specified period to identify trends and surface alerts.
    /// This operation is crucial for proactive community health management.
    /// </summary>
    /// <param name="request">The request defining the scope and time window for the trend analysis.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the
    /// <see cref="TrendAnalysisResponse"/>, including historical data and any triggered alerts.
    /// </returns>
    /// <remarks>
    /// **SLA:** This operation must complete in less than 60 seconds for trend roll-ups.
    /// **Acceptance Criteria:** Given an hourly or weekly event flow, when rolled up, this method must surface a trend alert if thresholds are met.
    /// </remarks>
    Task<TrendAnalysisResponse> AnalyzeTrendsAsync(TrendAnalysisRequest request);
}