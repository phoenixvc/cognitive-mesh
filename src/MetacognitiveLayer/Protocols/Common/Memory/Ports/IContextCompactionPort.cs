namespace MetacognitiveLayer.Protocols.Common.Memory.Ports;

/// <summary>
/// Compaction strategy types.
/// </summary>
public enum CompactionStrategy
{
    /// <summary>Summarize older content.</summary>
    Summarization,
    /// <summary>Keep most recent, discard oldest.</summary>
    SlidingWindow,
    /// <summary>Keep most relevant based on query.</summary>
    RelevanceBased,
    /// <summary>Hierarchical summarization (recent detail, older summary).</summary>
    Hierarchical,
    /// <summary>Extract key facts only.</summary>
    FactExtraction,
    /// <summary>Hybrid of multiple strategies.</summary>
    Hybrid
}

/// <summary>
/// Configuration for context compaction.
/// </summary>
public class CompactionConfiguration
{
    /// <summary>Target token count after compaction.</summary>
    public required int TargetTokenCount { get; init; }

    /// <summary>Strategy to use.</summary>
    public CompactionStrategy Strategy { get; init; } = CompactionStrategy.Hierarchical;

    /// <summary>Percentage of context to preserve as recent (0.0 - 1.0).</summary>
    public double RecentPreservationRatio { get; init; } = 0.3;

    /// <summary>Maximum summary length in tokens.</summary>
    public int MaxSummaryTokens { get; init; } = 500;

    /// <summary>Minimum relevance score for retention (0.0 - 1.0).</summary>
    public double MinRelevanceScore { get; init; } = 0.3;

    /// <summary>Whether to preserve system instructions.</summary>
    public bool PreserveSystemInstructions { get; init; } = true;

    /// <summary>Whether to preserve tool definitions.</summary>
    public bool PreserveToolDefinitions { get; init; } = true;

    /// <summary>Custom priority tags that should be preserved.</summary>
    public IReadOnlyList<string> PriorityTags { get; init; } = Array.Empty<string>();
}

/// <summary>
/// A segment of context with metadata.
/// </summary>
public class ContextSegment
{
    /// <summary>Unique identifier.</summary>
    public string SegmentId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>The content text.</summary>
    public required string Content { get; init; }

    /// <summary>Token count of the content.</summary>
    public int TokenCount { get; init; }

    /// <summary>Type of content (System, User, Assistant, Tool, etc.).</summary>
    public required string ContentType { get; init; }

    /// <summary>When the content was added.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Relevance score (0.0 - 1.0).</summary>
    public double RelevanceScore { get; init; }

    /// <summary>Importance score (0.0 - 1.0).</summary>
    public double ImportanceScore { get; init; }

    /// <summary>Tags for categorization.</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>Whether this segment has been summarized.</summary>
    public bool IsSummarized { get; init; }

    /// <summary>Original segment IDs if this is a summary.</summary>
    public IReadOnlyList<string> SummarizesSegments { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Result of context compaction.
/// </summary>
public class CompactionResult
{
    /// <summary>Compacted segments.</summary>
    public IReadOnlyList<ContextSegment> CompactedSegments { get; init; } = Array.Empty<ContextSegment>();

    /// <summary>Total tokens before compaction.</summary>
    public int TokensBefore { get; init; }

    /// <summary>Total tokens after compaction.</summary>
    public int TokensAfter { get; init; }

    /// <summary>Compression ratio (tokens after / tokens before).</summary>
    public double CompressionRatio { get; init; }

    /// <summary>Segments that were summarized.</summary>
    public int SegmentsSummarized { get; init; }

    /// <summary>Segments that were removed.</summary>
    public int SegmentsRemoved { get; init; }

    /// <summary>Segments that were preserved.</summary>
    public int SegmentsPreserved { get; init; }

    /// <summary>Strategy that was used.</summary>
    public CompactionStrategy StrategyUsed { get; init; }

    /// <summary>Whether compaction met the target.</summary>
    public bool MetTarget { get; init; }
}

/// <summary>
/// Request to check if compaction is needed.
/// </summary>
public class CompactionCheckRequest
{
    /// <summary>Current context segments.</summary>
    public required IReadOnlyList<ContextSegment> Segments { get; init; }

    /// <summary>Maximum allowed tokens.</summary>
    public required int MaxTokens { get; init; }

    /// <summary>Buffer to maintain below max (percentage).</summary>
    public double BufferPercentage { get; init; } = 0.1;
}

/// <summary>
/// Result of compaction check.
/// </summary>
public class CompactionCheckResult
{
    /// <summary>Whether compaction is needed.</summary>
    public required bool IsCompactionNeeded { get; init; }

    /// <summary>Current token count.</summary>
    public int CurrentTokens { get; init; }

    /// <summary>Maximum allowed tokens.</summary>
    public int MaxTokens { get; init; }

    /// <summary>Tokens over the limit.</summary>
    public int TokensOverLimit { get; init; }

    /// <summary>Percentage of limit used.</summary>
    public double UtilizationPercentage { get; init; }

    /// <summary>Recommended target token count.</summary>
    public int RecommendedTarget { get; init; }
}

/// <summary>
/// Port for context window auto-compaction.
/// Implements the "Context Window Auto-Compaction" pattern.
/// </summary>
/// <remarks>
/// This port provides automatic context window management through
/// intelligent summarization, sliding windows, and relevance-based
/// retention to keep context within token limits while preserving
/// important information.
/// </remarks>
public interface IContextCompactionPort
{
    /// <summary>
    /// Checks if compaction is needed.
    /// </summary>
    /// <param name="request">The check request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The check result.</returns>
    Task<CompactionCheckResult> CheckCompactionNeededAsync(
        CompactionCheckRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compacts context segments.
    /// </summary>
    /// <param name="segments">Segments to compact.</param>
    /// <param name="configuration">Compaction configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The compaction result.</returns>
    Task<CompactionResult> CompactAsync(
        IReadOnlyList<ContextSegment> segments,
        CompactionConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Summarizes a set of segments into a single summary.
    /// </summary>
    /// <param name="segments">Segments to summarize.</param>
    /// <param name="maxTokens">Maximum tokens for summary.</param>
    /// <param name="focusQuery">Optional query to focus summary on.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The summary segment.</returns>
    Task<ContextSegment> SummarizeAsync(
        IReadOnlyList<ContextSegment> segments,
        int maxTokens,
        string? focusQuery = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Scores segments by relevance to a query.
    /// </summary>
    /// <param name="segments">Segments to score.</param>
    /// <param name="query">The relevance query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Segments with updated relevance scores.</returns>
    Task<IReadOnlyList<ContextSegment>> ScoreRelevanceAsync(
        IReadOnlyList<ContextSegment> segments,
        string query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts key facts from segments.
    /// </summary>
    /// <param name="segments">Segments to extract from.</param>
    /// <param name="maxFacts">Maximum facts to extract.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Extracted facts as a segment.</returns>
    Task<ContextSegment> ExtractFactsAsync(
        IReadOnlyList<ContextSegment> segments,
        int maxFacts = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts tokens in content.
    /// </summary>
    /// <param name="content">Content to count.</param>
    /// <param name="model">Model to count for (affects tokenizer).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Token count.</returns>
    Task<int> CountTokensAsync(
        string content,
        string? model = null,
        CancellationToken cancellationToken = default);
}
