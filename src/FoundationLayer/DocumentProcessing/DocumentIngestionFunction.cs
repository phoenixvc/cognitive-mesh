using System.Net;
using System.Text.Json;
using FoundationLayer.DocumentProcessing.Ports;
using FoundationLayer.SemanticSearch;

namespace FoundationLayer.DocumentProcessing;

/// <summary>
/// Azure Function that handles document ingestion requests. Processes incoming documents
/// by indexing them in the RAG system and optionally enriching them through Fabric's
/// data integration services.
/// </summary>
public class DocumentIngestionFunction
{
    private readonly ILogger _logger;
    private readonly EnhancedRAGSystem _ragSystem;
    private readonly IFabricDataIntegrationPort? _fabricIntegration;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentIngestionFunction"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="ragSystem">The RAG system for document indexing and search.</param>
    /// <param name="fabricIntegration">
    /// Optional Fabric data integration port. When null, Fabric enrichment and
    /// OneLake synchronization are skipped gracefully.
    /// </param>
    public DocumentIngestionFunction(
        ILoggerFactory loggerFactory,
        EnhancedRAGSystem ragSystem,
        IFabricDataIntegrationPort? fabricIntegration = null)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<DocumentIngestionFunction>();
        _ragSystem = ragSystem ?? throw new ArgumentNullException(nameof(ragSystem));
        _fabricIntegration = fabricIntegration;
    }

    /// <summary>
    /// Processes an HTTP POST request to ingest a document. The document is indexed in the
    /// RAG system and, when Fabric integration is available, enriched and synchronized
    /// to OneLake.
    /// </summary>
    /// <param name="req">The incoming HTTP request containing a JSON-serialized <see cref="KnowledgeDocument"/>.</param>
    /// <returns>An HTTP response indicating the result of the ingestion operation.</returns>
    [Function("IngestDocument")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("Document ingestion function processing a request");

        // Parse request
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var document = JsonSerializer.Deserialize<KnowledgeDocument>(requestBody);

        if (document == null || string.IsNullOrEmpty(document.Content))
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Please provide document content");
            return badResponse;
        }

        // Process and index document in the RAG system
        await _ragSystem.IndexDocumentAsync(document);

        // Integrate with Fabric's data endpoints for enrichment and OneLake sync
        await IntegrateWithFabricDataEndpointsAsync(document);

        // Create response
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(new { id = document.Id, message = "Document indexed successfully" }));

        return response;
    }

    /// <summary>
    /// Integrates with Microsoft Fabric's data endpoints to enrich and synchronize
    /// the ingested document. If no Fabric integration is configured, the step is
    /// skipped gracefully.
    /// </summary>
    /// <param name="document">The document to enrich and synchronize.</param>
    private async Task IntegrateWithFabricDataEndpointsAsync(KnowledgeDocument document)
    {
        if (_fabricIntegration is null)
        {
            _logger.LogDebug(
                "Fabric data integration is not configured. Skipping enrichment for document '{DocumentId}'.",
                document.Id);
            return;
        }

        try
        {
            // Step 1: Enrich document via Fabric's prebuilt AI services
            _logger.LogInformation(
                "Enriching document '{DocumentId}' via Fabric AI services.", document.Id);

            var enrichmentResult = await _fabricIntegration.EnrichDocumentAsync(
                document.Id, document.Content);

            if (!enrichmentResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Fabric enrichment failed for document '{DocumentId}': {Error}. Continuing without enrichment.",
                    document.Id,
                    enrichmentResult.ErrorMessage);
            }
            else
            {
                _logger.LogInformation(
                    "Fabric enrichment completed for document '{DocumentId}'. Extracted {EntityCount} entities, {PhraseCount} key phrases.",
                    document.Id,
                    enrichmentResult.ExtractedEntities.Count,
                    enrichmentResult.KeyPhrases.Count);
            }

            // Step 2: Vectorize via Fabric's vectorization pipeline
            await _fabricIntegration.VectorizeViaFabricAsync(
                document.Id, document.Content);

            // Step 3: Sync metadata to OneLake for analytics
            var metadata = new Dictionary<string, string>
            {
                ["title"] = document.Title ?? string.Empty,
                ["source"] = document.Source ?? string.Empty,
                ["category"] = document.Category ?? string.Empty,
                ["created"] = document.Created.ToString("o"),
                ["tagCount"] = document.Tags?.Count.ToString() ?? "0"
            };

            await _fabricIntegration.SyncToOneLakeAsync(document.Id, metadata);

            _logger.LogInformation(
                "Fabric integration completed for document '{DocumentId}'.", document.Id);
        }
        catch (Exception ex)
        {
            // Fabric integration failures should not block the primary ingestion pipeline.
            // The document is already indexed in the RAG system at this point.
            _logger.LogError(
                ex,
                "Fabric integration failed for document '{DocumentId}'. The document was indexed successfully but Fabric enrichment/sync did not complete.",
                document.Id);
        }
    }
}