namespace ReasoningLayer.StructuredReasoning.Ports;

/// <summary>
/// Type of chunking strategy.
/// </summary>
public enum ChunkingStrategy
{
    /// <summary>Fixed size chunks.</summary>
    FixedSize,
    /// <summary>Sentence-based chunks.</summary>
    Sentence,
    /// <summary>Paragraph-based chunks.</summary>
    Paragraph,
    /// <summary>Semantic chunking.</summary>
    Semantic,
    /// <summary>Recursive character text splitter.</summary>
    Recursive
}

/// <summary>
/// A document to be indexed.
/// </summary>
public class RAGDocument
{
    /// <summary>Document identifier.</summary>
    public required string Id { get; init; }

    /// <summary>Document content.</summary>
    public required string Content { get; init; }

    /// <summary>Document title.</summary>
    public string? Title { get; init; }

    /// <summary>Source URL or path.</summary>
    public string? Source { get; init; }

    /// <summary>Metadata.</summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// A chunk of a document.
/// </summary>
public class DocumentChunk
{
    /// <summary>Chunk identifier.</summary>
    public required string ChunkId { get; init; }

    /// <summary>Parent document ID.</summary>
    public required string DocumentId { get; init; }

    /// <summary>Chunk content.</summary>
    public required string Content { get; init; }

    /// <summary>Chunk index in document.</summary>
    public int ChunkIndex { get; init; }

    /// <summary>Start position in document.</summary>
    public int StartPosition { get; init; }

    /// <summary>End position in document.</summary>
    public int EndPosition { get; init; }

    /// <summary>Embedding vector.</summary>
    public float[]? Embedding { get; init; }

    /// <summary>Inherited metadata.</summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Configuration for RAG pipeline.
/// </summary>
public class RAGConfiguration
{
    /// <summary>Chunking strategy.</summary>
    public ChunkingStrategy ChunkingStrategy { get; init; } = ChunkingStrategy.Recursive;

    /// <summary>Chunk size in tokens.</summary>
    public int ChunkSize { get; init; } = 512;

    /// <summary>Chunk overlap in tokens.</summary>
    public int ChunkOverlap { get; init; } = 50;

    /// <summary>Number of chunks to retrieve.</summary>
    public int TopK { get; init; } = 5;

    /// <summary>Minimum similarity score.</summary>
    public double MinScore { get; init; } = 0.7;

    /// <summary>Whether to use reranking.</summary>
    public bool EnableReranking { get; init; } = true;

    /// <summary>Reranking model.</summary>
    public string? RerankingModel { get; init; }

    /// <summary>Whether to use hybrid search.</summary>
    public bool EnableHybridSearch { get; init; } = false;

    /// <summary>BM25 weight for hybrid search.</summary>
    public double BM25Weight { get; init; } = 0.3;

    /// <summary>Vector weight for hybrid search.</summary>
    public double VectorWeight { get; init; } = 0.7;
}

/// <summary>
/// Result of a RAG query.
/// </summary>
public class RAGResult
{
    /// <summary>Generated answer.</summary>
    public required string Answer { get; init; }

    /// <summary>Retrieved chunks used.</summary>
    public IReadOnlyList<DocumentChunk> RetrievedChunks { get; init; } = Array.Empty<DocumentChunk>();

    /// <summary>Confidence in the answer (0.0 - 1.0).</summary>
    public double Confidence { get; init; }

    /// <summary>Citations in the answer.</summary>
    public IReadOnlyList<Citation> Citations { get; init; } = Array.Empty<Citation>();

    /// <summary>Whether sufficient context was found.</summary>
    public bool SufficientContext { get; init; }

    /// <summary>Processing time.</summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// A citation in the answer.
/// </summary>
public class Citation
{
    /// <summary>Chunk ID cited.</summary>
    public required string ChunkId { get; init; }

    /// <summary>Document ID.</summary>
    public required string DocumentId { get; init; }

    /// <summary>Text span in the answer.</summary>
    public string? AnswerSpan { get; init; }

    /// <summary>Source text.</summary>
    public string? SourceText { get; init; }
}

/// <summary>
/// Port for RAG (Retrieval-Augmented Generation).
/// Implements the "RAG" pattern.
/// </summary>
public interface IRAGPort
{
    /// <summary>
    /// Indexes documents.
    /// </summary>
    Task IndexDocumentsAsync(
        IEnumerable<RAGDocument> documents,
        RAGConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the knowledge base.
    /// </summary>
    Task<RAGResult> QueryAsync(
        string query,
        RAGConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves relevant chunks without generation.
    /// </summary>
    Task<IReadOnlyList<DocumentChunk>> RetrieveAsync(
        string query,
        int topK = 5,
        double? minScore = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an answer from provided chunks.
    /// </summary>
    Task<RAGResult> GenerateWithContextAsync(
        string query,
        IReadOnlyList<DocumentChunk> chunks,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Chunks a document.
    /// </summary>
    Task<IReadOnlyList<DocumentChunk>> ChunkDocumentAsync(
        RAGDocument document,
        RAGConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes documents.
    /// </summary>
    Task DeleteDocumentsAsync(
        IEnumerable<string> documentIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets indexing statistics.
    /// </summary>
    Task<RAGStatistics> GetStatisticsAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// RAG statistics.
/// </summary>
public class RAGStatistics
{
    public int TotalDocuments { get; init; }
    public int TotalChunks { get; init; }
    public double AverageChunksPerDocument { get; init; }
    public DateTimeOffset LastIndexedAt { get; init; }
}
