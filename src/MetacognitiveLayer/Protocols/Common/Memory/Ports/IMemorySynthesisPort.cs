namespace MetacognitiveLayer.Protocols.Common.Memory.Ports;

/// <summary>
/// Type of insight extracted from logs.
/// </summary>
public enum InsightType
{
    /// <summary>A pattern that repeats across executions.</summary>
    RecurringPattern,
    /// <summary>A common failure mode.</summary>
    FailurePattern,
    /// <summary>A successful strategy.</summary>
    SuccessPattern,
    /// <summary>A performance optimization opportunity.</summary>
    PerformanceInsight,
    /// <summary>A behavioral anomaly.</summary>
    Anomaly,
    /// <summary>A tool usage pattern.</summary>
    ToolUsagePattern,
    /// <summary>An agent interaction pattern.</summary>
    AgentInteractionPattern,
    /// <summary>A user preference or behavior.</summary>
    UserBehavior
}

/// <summary>
/// Confidence level for an insight.
/// </summary>
public enum ConfidenceLevel
{
    /// <summary>Low confidence, needs more data.</summary>
    Low,
    /// <summary>Medium confidence, pattern is emerging.</summary>
    Medium,
    /// <summary>High confidence, well-established pattern.</summary>
    High,
    /// <summary>Very high confidence, strongly validated.</summary>
    VeryHigh
}

/// <summary>
/// An insight synthesized from execution logs.
/// </summary>
public class SynthesizedInsight
{
    /// <summary>Unique identifier.</summary>
    public string InsightId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Type of insight.</summary>
    public required InsightType Type { get; init; }

    /// <summary>Description of the insight.</summary>
    public required string Description { get; init; }

    /// <summary>Detailed explanation.</summary>
    public string? DetailedExplanation { get; init; }

    /// <summary>Confidence level.</summary>
    public ConfidenceLevel Confidence { get; init; }

    /// <summary>Confidence score (0.0 - 1.0).</summary>
    public double ConfidenceScore { get; init; }

    /// <summary>Number of occurrences supporting this insight.</summary>
    public int OccurrenceCount { get; init; }

    /// <summary>Sample log entries supporting this insight.</summary>
    public IReadOnlyList<string> SupportingEvidence { get; init; } = Array.Empty<string>();

    /// <summary>Agents this insight applies to.</summary>
    public IReadOnlyList<string> RelevantAgents { get; init; } = Array.Empty<string>();

    /// <summary>Tools this insight relates to.</summary>
    public IReadOnlyList<string> RelevantTools { get; init; } = Array.Empty<string>();

    /// <summary>Recommended actions based on insight.</summary>
    public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();

    /// <summary>When the insight was first detected.</summary>
    public DateTimeOffset FirstDetectedAt { get; init; }

    /// <summary>When the insight was last updated.</summary>
    public DateTimeOffset LastUpdatedAt { get; init; }

    /// <summary>Tags for categorization.</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>Whether this insight has been acknowledged.</summary>
    public bool IsAcknowledged { get; init; }

    /// <summary>Whether this insight led to an action.</summary>
    public bool IsActedUpon { get; init; }
}

/// <summary>
/// Configuration for memory synthesis.
/// </summary>
public class SynthesisConfiguration
{
    /// <summary>Minimum occurrences to consider a pattern.</summary>
    public int MinOccurrences { get; init; } = 3;

    /// <summary>Time window for analysis.</summary>
    public TimeSpan AnalysisWindow { get; init; } = TimeSpan.FromDays(7);

    /// <summary>Insight types to look for.</summary>
    public IReadOnlyList<InsightType> TargetInsightTypes { get; init; } = Array.Empty<InsightType>();

    /// <summary>Minimum confidence to report.</summary>
    public ConfidenceLevel MinConfidence { get; init; } = ConfidenceLevel.Medium;

    /// <summary>Maximum insights to return.</summary>
    public int MaxInsights { get; init; } = 20;

    /// <summary>Agents to focus on (empty = all).</summary>
    public IReadOnlyList<string> FocusAgents { get; init; } = Array.Empty<string>();

    /// <summary>Whether to include historical insights.</summary>
    public bool IncludeHistorical { get; init; } = false;
}

/// <summary>
/// Result of memory synthesis.
/// </summary>
public class SynthesisResult
{
    /// <summary>Insights discovered.</summary>
    public IReadOnlyList<SynthesizedInsight> Insights { get; init; } = Array.Empty<SynthesizedInsight>();

    /// <summary>Total log entries analyzed.</summary>
    public int LogsAnalyzed { get; init; }

    /// <summary>Analysis start time.</summary>
    public DateTimeOffset AnalysisStart { get; init; }

    /// <summary>Analysis end time.</summary>
    public DateTimeOffset AnalysisEnd { get; init; }

    /// <summary>New insights discovered in this run.</summary>
    public int NewInsights { get; init; }

    /// <summary>Existing insights that were updated.</summary>
    public int UpdatedInsights { get; init; }

    /// <summary>Processing time.</summary>
    public TimeSpan ProcessingTime { get; init; }
}

/// <summary>
/// An execution log entry for analysis.
/// </summary>
public class ExecutionLogEntry
{
    /// <summary>Log identifier.</summary>
    public required string LogId { get; init; }

    /// <summary>Agent identifier.</summary>
    public required string AgentId { get; init; }

    /// <summary>Session identifier.</summary>
    public string? SessionId { get; init; }

    /// <summary>Event type.</summary>
    public required string EventType { get; init; }

    /// <summary>Event data.</summary>
    public required string EventData { get; init; }

    /// <summary>Whether successful.</summary>
    public bool Success { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Duration in milliseconds.</summary>
    public double? DurationMs { get; init; }

    /// <summary>Tool identifier.</summary>
    public string? ToolId { get; init; }

    /// <summary>Timestamp.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Additional metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Port for memory synthesis from execution logs.
/// Implements the "Memory Synthesis from Execution Logs" pattern.
/// </summary>
/// <remarks>
/// This port analyzes execution logs to extract patterns, insights, and
/// learnings that can improve agent performance over time. It identifies
/// recurring successes, failures, and behavioral patterns.
/// </remarks>
public interface IMemorySynthesisPort
{
    /// <summary>
    /// Synthesizes insights from execution logs.
    /// </summary>
    /// <param name="configuration">Synthesis configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The synthesis result.</returns>
    Task<SynthesisResult> SynthesizeAsync(
        SynthesisConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ingests execution logs for analysis.
    /// </summary>
    /// <param name="logs">Logs to ingest.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of logs ingested.</returns>
    Task<int> IngestLogsAsync(
        IEnumerable<ExecutionLogEntry> logs,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets existing insights.
    /// </summary>
    /// <param name="type">Filter by type (null = all).</param>
    /// <param name="minConfidence">Minimum confidence level.</param>
    /// <param name="limit">Maximum to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of insights.</returns>
    Task<IReadOnlyList<SynthesizedInsight>> GetInsightsAsync(
        InsightType? type = null,
        ConfidenceLevel minConfidence = ConfidenceLevel.Low,
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledges an insight.
    /// </summary>
    /// <param name="insightId">The insight ID.</param>
    /// <param name="acknowledgedBy">Who acknowledged.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AcknowledgeInsightAsync(
        string insightId,
        string acknowledgedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an insight as acted upon.
    /// </summary>
    /// <param name="insightId">The insight ID.</param>
    /// <param name="action">What action was taken.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task MarkActedUponAsync(
        string insightId,
        string action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dismisses an insight as not relevant.
    /// </summary>
    /// <param name="insightId">The insight ID.</param>
    /// <param name="reason">Why it was dismissed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DismissInsightAsync(
        string insightId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets insights relevant to a specific agent or context.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <param name="context">Current context for relevance matching.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Relevant insights.</returns>
    Task<IReadOnlyList<SynthesizedInsight>> GetRelevantInsightsAsync(
        string agentId,
        string? context = null,
        CancellationToken cancellationToken = default);
}
