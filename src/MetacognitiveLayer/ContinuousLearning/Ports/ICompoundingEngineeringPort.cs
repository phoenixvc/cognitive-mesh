namespace MetacognitiveLayer.ContinuousLearning.Ports;

/// <summary>
/// An engineering insight.
/// </summary>
public class EngineeringInsight
{
    /// <summary>Insight identifier.</summary>
    public string InsightId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Insight category.</summary>
    public required string Category { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Confidence.</summary>
    public double Confidence { get; init; }

    /// <summary>Evidence supporting this insight.</summary>
    public IReadOnlyList<Evidence> Evidence { get; init; } = Array.Empty<Evidence>();

    /// <summary>Times applied.</summary>
    public int ApplicationCount { get; init; }

    /// <summary>Success rate when applied.</summary>
    public double SuccessRate { get; init; }

    /// <summary>Related patterns.</summary>
    public IReadOnlyList<string> RelatedPatterns { get; init; } = Array.Empty<string>();

    /// <summary>Tags.</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>Discovered at.</summary>
    public DateTimeOffset DiscoveredAt { get; init; }

    /// <summary>Last applied.</summary>
    public DateTimeOffset? LastApplied { get; init; }
}

/// <summary>
/// Evidence for an insight.
/// </summary>
public class Evidence
{
    /// <summary>Evidence type.</summary>
    public required string Type { get; init; }

    /// <summary>Source.</summary>
    public required string Source { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Strength (0.0 - 1.0).</summary>
    public double Strength { get; init; }

    /// <summary>When observed.</summary>
    public DateTimeOffset ObservedAt { get; init; }
}

/// <summary>
/// Knowledge compounding result.
/// </summary>
public class CompoundingResult
{
    /// <summary>New insights discovered.</summary>
    public IReadOnlyList<EngineeringInsight> NewInsights { get; init; } = Array.Empty<EngineeringInsight>();

    /// <summary>Updated insights.</summary>
    public IReadOnlyList<EngineeringInsight> UpdatedInsights { get; init; } = Array.Empty<EngineeringInsight>();

    /// <summary>Deprecated insights.</summary>
    public IReadOnlyList<string> DeprecatedInsightIds { get; init; } = Array.Empty<string>();

    /// <summary>Connections discovered.</summary>
    public IReadOnlyList<InsightConnection> Connections { get; init; } = Array.Empty<InsightConnection>();

    /// <summary>Processing duration.</summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Connection between insights.
/// </summary>
public class InsightConnection
{
    /// <summary>Source insight ID.</summary>
    public required string SourceId { get; init; }

    /// <summary>Target insight ID.</summary>
    public required string TargetId { get; init; }

    /// <summary>Relationship type.</summary>
    public required string RelationType { get; init; }

    /// <summary>Strength.</summary>
    public double Strength { get; init; }
}

/// <summary>
/// Port for compounding engineering pattern.
/// Implements the "Compounding Engineering Pattern" for knowledge accumulation.
/// </summary>
public interface ICompoundingEngineeringPort
{
    /// <summary>
    /// Records an observation for insight extraction.
    /// </summary>
    Task RecordObservationAsync(
        string category,
        string observation,
        string? source = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes observations and compounds knowledge.
    /// </summary>
    Task<CompoundingResult> CompoundKnowledgeAsync(
        DateTimeOffset? since = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets insights for a category.
    /// </summary>
    Task<IReadOnlyList<EngineeringInsight>> GetInsightsAsync(
        string? category = null,
        double minConfidence = 0.0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets top insights.
    /// </summary>
    Task<IReadOnlyList<EngineeringInsight>> GetTopInsightsAsync(
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies an insight and records outcome.
    /// </summary>
    Task RecordApplicationAsync(
        string insightId,
        bool successful,
        string? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches insights.
    /// </summary>
    Task<IReadOnlyList<EngineeringInsight>> SearchInsightsAsync(
        string query,
        int limit = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets insight connections.
    /// </summary>
    Task<IReadOnlyList<InsightConnection>> GetConnectionsAsync(
        string insightId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates recommendations based on context.
    /// </summary>
    Task<IReadOnlyList<EngineeringInsight>> GetRecommendationsAsync(
        string context,
        int limit = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports insights.
    /// </summary>
    Task<string> ExportInsightsAsync(
        string format = "json",
        CancellationToken cancellationToken = default);
}
