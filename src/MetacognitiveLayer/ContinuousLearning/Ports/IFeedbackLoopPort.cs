namespace MetacognitiveLayer.ContinuousLearning.Ports;

/// <summary>
/// Type of feedback.
/// </summary>
public enum FeedbackType
{
    /// <summary>User rating.</summary>
    Rating,
    /// <summary>User text feedback.</summary>
    TextFeedback,
    /// <summary>Implicit signal (e.g., user continued, user abandoned).</summary>
    ImplicitSignal,
    /// <summary>Correction provided.</summary>
    Correction,
    /// <summary>A/B test result.</summary>
    ABTestResult,
    /// <summary>Automated evaluation.</summary>
    AutomatedEval
}

/// <summary>
/// Feedback from user or system.
/// </summary>
public class Feedback
{
    /// <summary>Feedback identifier.</summary>
    public string FeedbackId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Feedback type.</summary>
    public required FeedbackType Type { get; init; }

    /// <summary>Target (what the feedback is about).</summary>
    public required string TargetId { get; init; }

    /// <summary>Target type (response, agent, workflow, etc.).</summary>
    public required string TargetType { get; init; }

    /// <summary>Rating value (if Rating type).</summary>
    public int? Rating { get; init; }

    /// <summary>Text content.</summary>
    public string? Content { get; init; }

    /// <summary>Signal name (if ImplicitSignal).</summary>
    public string? SignalName { get; init; }

    /// <summary>Correction content (if Correction).</summary>
    public string? Correction { get; init; }

    /// <summary>Who provided the feedback.</summary>
    public string? ProvidedBy { get; init; }

    /// <summary>When provided.</summary>
    public DateTimeOffset ProvidedAt { get; init; }

    /// <summary>Session context.</summary>
    public string? SessionId { get; init; }

    /// <summary>Metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Aggregated feedback metrics.
/// </summary>
public class FeedbackMetrics
{
    /// <summary>Target ID.</summary>
    public required string TargetId { get; init; }

    /// <summary>Average rating.</summary>
    public double? AverageRating { get; init; }

    /// <summary>Total feedback count.</summary>
    public int TotalFeedbackCount { get; init; }

    /// <summary>Positive feedback count.</summary>
    public int PositiveFeedbackCount { get; init; }

    /// <summary>Negative feedback count.</summary>
    public int NegativeFeedbackCount { get; init; }

    /// <summary>Sentiment score (-1.0 to 1.0).</summary>
    public double SentimentScore { get; init; }

    /// <summary>Trends.</summary>
    public FeedbackTrend Trend { get; init; }

    /// <summary>Common themes in feedback.</summary>
    public IReadOnlyList<string> CommonThemes { get; init; } = Array.Empty<string>();

    /// <summary>Period start.</summary>
    public DateTimeOffset PeriodStart { get; init; }

    /// <summary>Period end.</summary>
    public DateTimeOffset PeriodEnd { get; init; }
}

/// <summary>
/// Feedback trend direction.
/// </summary>
public enum FeedbackTrend
{
    Improving,
    Stable,
    Declining,
    Insufficient
}

/// <summary>
/// A learning derived from feedback.
/// </summary>
public class FeedbackLearning
{
    /// <summary>Learning identifier.</summary>
    public string LearningId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>What was learned.</summary>
    public required string Insight { get; init; }

    /// <summary>Confidence (0.0 - 1.0).</summary>
    public double Confidence { get; init; }

    /// <summary>Supporting feedback count.</summary>
    public int SupportingFeedbackCount { get; init; }

    /// <summary>Recommended action.</summary>
    public string? RecommendedAction { get; init; }

    /// <summary>Targets affected.</summary>
    public IReadOnlyList<string> AffectedTargets { get; init; } = Array.Empty<string>();

    /// <summary>When derived.</summary>
    public DateTimeOffset DerivedAt { get; init; }
}

/// <summary>
/// Port for continuous feedback loops.
/// Implements the "Continuous Feedback Loop" pattern.
/// </summary>
public interface IFeedbackLoopPort
{
    /// <summary>
    /// Records feedback.
    /// </summary>
    Task RecordFeedbackAsync(
        Feedback feedback,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a rating.
    /// </summary>
    Task RecordRatingAsync(
        string targetId,
        string targetType,
        int rating,
        string? comment = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records an implicit signal.
    /// </summary>
    Task RecordSignalAsync(
        string targetId,
        string targetType,
        string signalName,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets feedback for a target.
    /// </summary>
    Task<IReadOnlyList<Feedback>> GetFeedbackAsync(
        string targetId,
        FeedbackType? type = null,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets aggregated metrics.
    /// </summary>
    Task<FeedbackMetrics> GetMetricsAsync(
        string targetId,
        DateTimeOffset? since = null,
        DateTimeOffset? until = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes feedback and extracts learnings.
    /// </summary>
    Task<IReadOnlyList<FeedbackLearning>> AnalyzeFeedbackAsync(
        string? targetId = null,
        DateTimeOffset? since = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets top performers based on feedback.
    /// </summary>
    Task<IReadOnlyList<(string TargetId, FeedbackMetrics Metrics)>> GetTopPerformersAsync(
        string targetType,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets underperformers needing attention.
    /// </summary>
    Task<IReadOnlyList<(string TargetId, FeedbackMetrics Metrics)>> GetUnderperformersAsync(
        string targetType,
        double belowRating = 3.0,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to real-time feedback.
    /// </summary>
    IAsyncEnumerable<Feedback> SubscribeAsync(
        string? targetId = null,
        CancellationToken cancellationToken = default);
}
