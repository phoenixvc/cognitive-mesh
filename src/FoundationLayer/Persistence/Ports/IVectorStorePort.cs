namespace FoundationLayer.Persistence.Ports;

/// <summary>
/// Distance metric for vector similarity.
/// </summary>
public enum DistanceMetric
{
    /// <summary>Cosine similarity.</summary>
    Cosine,
    /// <summary>Euclidean distance.</summary>
    Euclidean,
    /// <summary>Dot product.</summary>
    DotProduct,
    /// <summary>Manhattan distance.</summary>
    Manhattan
}

/// <summary>
/// A vector with metadata.
/// </summary>
public class VectorRecord
{
    /// <summary>Record identifier.</summary>
    public required string Id { get; init; }

    /// <summary>The vector embedding.</summary>
    public required float[] Vector { get; init; }

    /// <summary>Metadata fields.</summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>The original text/content.</summary>
    public string? Content { get; init; }

    /// <summary>Collection this belongs to.</summary>
    public string? Collection { get; init; }

    /// <summary>When created.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// A search result.
/// </summary>
public class VectorSearchResult
{
    /// <summary>The matching record.</summary>
    public required VectorRecord Record { get; init; }

    /// <summary>Similarity/distance score.</summary>
    public double Score { get; init; }

    /// <summary>Rank in results.</summary>
    public int Rank { get; init; }
}

/// <summary>
/// Filter for vector search.
/// </summary>
public class VectorFilter
{
    /// <summary>Field name.</summary>
    public required string Field { get; init; }

    /// <summary>Operator (eq, ne, gt, lt, in, contains).</summary>
    public required string Operator { get; init; }

    /// <summary>Value to compare.</summary>
    public required object Value { get; init; }
}

/// <summary>
/// Search request.
/// </summary>
public class VectorSearchRequest
{
    /// <summary>Query vector.</summary>
    public float[]? Vector { get; init; }

    /// <summary>Query text (for hybrid search).</summary>
    public string? Text { get; init; }

    /// <summary>Collection to search.</summary>
    public string? Collection { get; init; }

    /// <summary>Maximum results.</summary>
    public int TopK { get; init; } = 10;

    /// <summary>Minimum score threshold.</summary>
    public double? MinScore { get; init; }

    /// <summary>Metadata filters.</summary>
    public IReadOnlyList<VectorFilter> Filters { get; init; } = Array.Empty<VectorFilter>();

    /// <summary>Whether to include vectors in results.</summary>
    public bool IncludeVectors { get; init; } = false;

    /// <summary>Whether to include metadata in results.</summary>
    public bool IncludeMetadata { get; init; } = true;

    /// <summary>Distance metric.</summary>
    public DistanceMetric Metric { get; init; } = DistanceMetric.Cosine;
}

/// <summary>
/// Collection configuration.
/// </summary>
public class VectorCollectionConfig
{
    /// <summary>Collection name.</summary>
    public required string Name { get; init; }

    /// <summary>Vector dimension.</summary>
    public required int Dimension { get; init; }

    /// <summary>Distance metric.</summary>
    public DistanceMetric Metric { get; init; } = DistanceMetric.Cosine;

    /// <summary>Indexed metadata fields.</summary>
    public IReadOnlyList<string> IndexedFields { get; init; } = Array.Empty<string>();

    /// <summary>Whether to enable full-text search.</summary>
    public bool EnableFullTextSearch { get; init; } = false;
}

/// <summary>
/// Port for vector store operations.
/// Implements the "RAG (Retrieval-Augmented Generation)" pattern storage.
/// </summary>
public interface IVectorStorePort
{
    /// <summary>
    /// Creates a collection.
    /// </summary>
    Task CreateCollectionAsync(
        VectorCollectionConfig config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a collection.
    /// </summary>
    Task DeleteCollectionAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists collections.
    /// </summary>
    Task<IReadOnlyList<string>> ListCollectionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts vectors.
    /// </summary>
    Task UpsertAsync(
        IEnumerable<VectorRecord> records,
        string? collection = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for similar vectors.
    /// </summary>
    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        VectorSearchRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a record by ID.
    /// </summary>
    Task<VectorRecord?> GetAsync(
        string id,
        string? collection = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes records.
    /// </summary>
    Task DeleteAsync(
        IEnumerable<string> ids,
        string? collection = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets collection statistics.
    /// </summary>
    Task<(int Count, int Dimension)> GetCollectionStatsAsync(
        string collection,
        CancellationToken cancellationToken = default);
}
