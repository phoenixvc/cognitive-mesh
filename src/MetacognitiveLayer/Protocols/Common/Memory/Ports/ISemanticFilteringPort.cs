namespace MetacognitiveLayer.Protocols.Common.Memory.Ports;

/// <summary>
/// Filtering mode for semantic operations.
/// </summary>
public enum SemanticFilterMode
{
    /// <summary>Include only items above threshold.</summary>
    IncludeAbove,
    /// <summary>Exclude items below threshold.</summary>
    ExcludeBelow,
    /// <summary>Rank all items by relevance.</summary>
    RankAll,
    /// <summary>Return top-k most relevant.</summary>
    TopK
}

/// <summary>
/// Configuration for semantic filtering.
/// </summary>
public class SemanticFilterConfiguration
{
    /// <summary>Filtering mode to use.</summary>
    public SemanticFilterMode Mode { get; init; } = SemanticFilterMode.TopK;

    /// <summary>Relevance threshold (0.0 - 1.0).</summary>
    public double RelevanceThreshold { get; init; } = 0.5;

    /// <summary>Number of items for TopK mode.</summary>
    public int TopK { get; init; } = 10;

    /// <summary>Whether to use reranking after initial filter.</summary>
    public bool EnableReranking { get; init; } = true;

    /// <summary>Whether to diversify results.</summary>
    public bool EnableDiversification { get; init; } = false;

    /// <summary>Diversity factor (0.0 = no diversity, 1.0 = max diversity).</summary>
    public double DiversityFactor { get; init; } = 0.3;

    /// <summary>Whether to boost recent items.</summary>
    public bool BoostRecent { get; init; } = false;

    /// <summary>Recency decay factor.</summary>
    public double RecencyDecay { get; init; } = 0.1;

    /// <summary>Custom weights for different content types.</summary>
    public Dictionary<string, double> ContentTypeWeights { get; init; } = new();
}

/// <summary>
/// An item to be semantically filtered.
/// </summary>
public class FilterableItem
{
    /// <summary>Unique identifier.</summary>
    public required string ItemId { get; init; }

    /// <summary>Content for semantic matching.</summary>
    public required string Content { get; init; }

    /// <summary>Pre-computed embedding (optional).</summary>
    public float[]? Embedding { get; init; }

    /// <summary>Content type for weighting.</summary>
    public string? ContentType { get; init; }

    /// <summary>Timestamp for recency boosting.</summary>
    public DateTimeOffset? Timestamp { get; init; }

    /// <summary>Metadata for additional filtering.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Result of filtering a single item.
/// </summary>
public class FilteredItem
{
    /// <summary>Original item.</summary>
    public required FilterableItem Item { get; init; }

    /// <summary>Semantic relevance score (0.0 - 1.0).</summary>
    public double RelevanceScore { get; init; }

    /// <summary>Reranked score if reranking was applied.</summary>
    public double? RerankedScore { get; init; }

    /// <summary>Final combined score.</summary>
    public double FinalScore { get; init; }

    /// <summary>Rank in the result set (1-based).</summary>
    public int Rank { get; init; }

    /// <summary>Whether this item passed the filter.</summary>
    public bool Included { get; init; }

    /// <summary>Explanation for the score.</summary>
    public string? ScoreExplanation { get; init; }
}

/// <summary>
/// Result of semantic filtering operation.
/// </summary>
public class SemanticFilterResult
{
    /// <summary>Filtered items in ranked order.</summary>
    public IReadOnlyList<FilteredItem> Items { get; init; } = Array.Empty<FilteredItem>();

    /// <summary>Total items before filtering.</summary>
    public int TotalItemsBefore { get; init; }

    /// <summary>Items after filtering.</summary>
    public int TotalItemsAfter { get; init; }

    /// <summary>Query used for filtering.</summary>
    public required string Query { get; init; }

    /// <summary>Configuration used.</summary>
    public required SemanticFilterConfiguration Configuration { get; init; }

    /// <summary>Processing time.</summary>
    public TimeSpan ProcessingTime { get; init; }

    /// <summary>Whether reranking was applied.</summary>
    public bool RerankingApplied { get; init; }
}

/// <summary>
/// Request to minimize context while preserving relevance.
/// </summary>
public class ContextMinimizationRequest
{
    /// <summary>Items to minimize.</summary>
    public required IReadOnlyList<FilterableItem> Items { get; init; }

    /// <summary>Query to preserve relevance to.</summary>
    public required string Query { get; init; }

    /// <summary>Target token count.</summary>
    public required int TargetTokens { get; init; }

    /// <summary>Minimum relevance to include.</summary>
    public double MinRelevance { get; init; } = 0.3;

    /// <summary>Items that must be included regardless of relevance.</summary>
    public IReadOnlyList<string> MustInclude { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Result of context minimization.
/// </summary>
public class ContextMinimizationResult
{
    /// <summary>Minimized items.</summary>
    public IReadOnlyList<FilteredItem> Items { get; init; } = Array.Empty<FilteredItem>();

    /// <summary>Tokens before minimization.</summary>
    public int TokensBefore { get; init; }

    /// <summary>Tokens after minimization.</summary>
    public int TokensAfter { get; init; }

    /// <summary>Items removed.</summary>
    public int ItemsRemoved { get; init; }

    /// <summary>Average relevance of included items.</summary>
    public double AverageRelevance { get; init; }

    /// <summary>Whether target was met.</summary>
    public bool MetTarget { get; init; }
}

/// <summary>
/// Port for semantic context filtering.
/// Implements "Semantic Context Filtering" and "Context-Minimization" patterns.
/// </summary>
/// <remarks>
/// This port provides semantic filtering capabilities that select the most
/// relevant content based on query similarity, enabling efficient context
/// window usage by including only semantically relevant information.
/// </remarks>
public interface ISemanticFilteringPort
{
    /// <summary>
    /// Filters items based on semantic relevance to a query.
    /// </summary>
    /// <param name="items">Items to filter.</param>
    /// <param name="query">The relevance query.</param>
    /// <param name="configuration">Filter configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The filter result.</returns>
    Task<SemanticFilterResult> FilterAsync(
        IReadOnlyList<FilterableItem> items,
        string query,
        SemanticFilterConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Minimizes context while preserving query-relevant information.
    /// </summary>
    /// <param name="request">The minimization request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The minimization result.</returns>
    Task<ContextMinimizationResult> MinimizeContextAsync(
        ContextMinimizationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes semantic similarity between two texts.
    /// </summary>
    /// <param name="text1">First text.</param>
    /// <param name="text2">Second text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Similarity score (0.0 - 1.0).</returns>
    Task<double> ComputeSimilarityAsync(
        string text1,
        string text2,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates embedding for text.
    /// </summary>
    /// <param name="text">Text to embed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The embedding vector.</returns>
    Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reranks items using a cross-encoder model.
    /// </summary>
    /// <param name="items">Items to rerank.</param>
    /// <param name="query">The query for reranking.</param>
    /// <param name="topK">Number of items to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Reranked items.</returns>
    Task<IReadOnlyList<FilteredItem>> RerankAsync(
        IReadOnlyList<FilteredItem> items,
        string query,
        int topK,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Diversifies a result set to reduce redundancy.
    /// </summary>
    /// <param name="items">Items to diversify.</param>
    /// <param name="diversityFactor">How much to prioritize diversity (0.0 - 1.0).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Diversified items.</returns>
    Task<IReadOnlyList<FilteredItem>> DiversifyAsync(
        IReadOnlyList<FilteredItem> items,
        double diversityFactor,
        CancellationToken cancellationToken = default);
}
