namespace FoundationLayer.DocumentProcessing.Ports;

/// <summary>
/// Defines the contract for integrating with Microsoft Fabric's data services.
/// Implementations of this port handle document enrichment, vectorization, and
/// synchronization with Fabric's prebuilt Azure AI services and OneLake endpoints.
/// </summary>
/// <remarks>
/// <para>
/// This port abstracts the Fabric integration layer so that document ingestion
/// can operate independently of the specific Fabric SDK or REST API version.
/// </para>
/// <para>
/// Implementors should handle authentication, retry policies, and circuit-breaking
/// for Fabric endpoint calls. The Foundation layer's circuit breaker pattern
/// (3-state: Closed, Open, HalfOpen) should be applied to all external calls.
/// </para>
/// </remarks>
public interface IFabricDataIntegrationPort
{
    /// <summary>
    /// Enriches a document by sending it through Fabric's prebuilt Azure AI services
    /// for entity extraction, key phrase identification, and language detection.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document to enrich.</param>
    /// <param name="content">The raw document content to process.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>
    /// A <see cref="FabricEnrichmentResult"/> containing the enrichment metadata
    /// produced by Fabric's AI services.
    /// </returns>
    Task<FabricEnrichmentResult> EnrichDocumentAsync(
        string documentId,
        string content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes a processed document and its metadata to Fabric's OneLake storage
    /// for downstream analytics and reporting pipelines.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document to sync.</param>
    /// <param name="metadata">Key-value metadata associated with the document.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous sync operation.</returns>
    Task SyncToOneLakeAsync(
        string documentId,
        IDictionary<string, string> metadata,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers Fabric's vectorization pipeline to generate and store embeddings
    /// for the specified document, enabling semantic search via Fabric endpoints.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document to vectorize.</param>
    /// <param name="content">The document content to generate embeddings for.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous vectorization operation.</returns>
    Task VectorizeViaFabricAsync(
        string documentId,
        string content,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of document enrichment performed by Fabric's AI services.
/// </summary>
public class FabricEnrichmentResult
{
    /// <summary>
    /// The document identifier that was enriched.
    /// </summary>
    public string DocumentId { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the enrichment operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Entities extracted from the document content (e.g., people, organizations, locations).
    /// </summary>
    public IReadOnlyList<string> ExtractedEntities { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Key phrases identified in the document content.
    /// </summary>
    public IReadOnlyList<string> KeyPhrases { get; init; } = Array.Empty<string>();

    /// <summary>
    /// The detected language of the document content.
    /// </summary>
    public string? DetectedLanguage { get; init; }

    /// <summary>
    /// An error message if enrichment failed; null on success.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
