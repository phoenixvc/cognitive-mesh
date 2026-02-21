using System.Text;
using FoundationLayer.SemanticSearch.Ports;

namespace FoundationLayer.SemanticSearch;

/// <summary>
/// Provides an enhanced Retrieval-Augmented Generation (RAG) system that combines
/// Azure AI Search vector indexing with OpenAI embeddings and completions.
/// Supports optional integration with Microsoft Fabric data endpoints and
/// Azure Data Factory pipelines through the <see cref="IDataPipelinePort"/>.
/// </summary>
public class EnhancedRAGSystem
{
    private readonly SearchClient _searchClient;
    private readonly SearchIndexClient _indexClient;
    private readonly OpenAIClient _openAIClient;
    private readonly string _indexName;
    private readonly string _embeddingDeployment;
    private readonly string _completionDeployment;
    private readonly IDataPipelinePort? _dataPipeline;
    private readonly ILogger<EnhancedRAGSystem>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnhancedRAGSystem"/> class.
    /// </summary>
    /// <param name="searchClient">The Azure AI Search client for querying the index.</param>
    /// <param name="indexClient">The Azure AI Search index management client.</param>
    /// <param name="openAIClient">The Azure OpenAI client for embeddings and completions.</param>
    /// <param name="indexName">The name of the search index.</param>
    /// <param name="embeddingDeployment">The OpenAI deployment name for generating embeddings.</param>
    /// <param name="completionDeployment">The OpenAI deployment name for chat completions.</param>
    /// <param name="dataPipeline">
    /// Optional data pipeline port for Fabric and Data Factory integration.
    /// When null, pipeline operations are skipped gracefully.
    /// </param>
    /// <param name="logger">Optional logger for structured diagnostics.</param>
    public EnhancedRAGSystem(
        SearchClient searchClient,
        SearchIndexClient indexClient,
        OpenAIClient openAIClient,
        string indexName,
        string embeddingDeployment,
        string completionDeployment,
        IDataPipelinePort? dataPipeline = null,
        ILogger<EnhancedRAGSystem>? logger = null)
    {
        _searchClient = searchClient;
        _indexClient = indexClient;
        _openAIClient = openAIClient;
        _indexName = indexName;
        _embeddingDeployment = embeddingDeployment;
        _completionDeployment = completionDeployment;
        _dataPipeline = dataPipeline;
        _logger = logger;
    }

    private async Task EnsureIndexExists()
    {
        if (!await _indexClient.GetIndexesAsync().AnyAsync(i => i.Name == _indexName))
        {
            // Define index fields
            var fields = new List<SearchField>
            {
                new SearchField("id", SearchFieldDataType.String) { IsKey = true },
                new SearchField("title", SearchFieldDataType.String) { IsSearchable = true, IsSortable = true },
                new SearchField("content", SearchFieldDataType.String) { IsSearchable = true },
                new SearchField("source", SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                new SearchField("category", SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                new SearchField("created", SearchFieldDataType.DateTimeOffset) { IsFilterable = true, IsSortable = true },
                new SearchField("tags", SearchFieldDataType.Collection(SearchFieldDataType.String)) { IsFilterable = true, IsFacetable = true },
                new SearchField("embedding", SearchFieldDataType.Collection(SearchFieldDataType.Single))
                    { IsSearchable = true, VectorSearchDimensions = 1536, VectorSearchProfile = "embedding-profile" }
            };

            // Define vector search profile
            var vectorSearchProfiles = new List<VectorSearchProfile>
            {
                new VectorSearchProfile("embedding-profile", "hnsw-index")
            };

            // Define vector search algorithm
            var vectorSearchAlgorithms = new List<VectorSearchAlgorithmConfiguration>
            {
                new HnswAlgorithmConfiguration("hnsw-index")
                {
                    Parameters = new HnswParameters
                    {
                        Metric = VectorSearchAlgorithmMetric.Cosine,
                        M = 4,
                        EfConstruction = 400,
                        EfSearch = 500
                    }
                }
            };

            // Create index
            var indexDefinition = new SearchIndex(_indexName)
            {
                Fields = fields,
                VectorSearch = new VectorSearch
                {
                    Profiles = vectorSearchProfiles,
                    Algorithms = vectorSearchAlgorithms
                }
            };

            await _indexClient.CreateIndexAsync(indexDefinition);
        }
    }

    public async Task IndexDocumentAsync(KnowledgeDocument document)
    {
        // Generate embedding for document content
        var embeddingResponse = await _openAIClient.GetEmbeddingsAsync(
            _embeddingDeployment, 
            new EmbeddingsOptions(document.Content));
            
        float[] embedding = embeddingResponse.Value.Data[0].Embedding.ToArray();
        
        // Create search document
        var searchDocument = new Dictionary<string, object>
        {
            { "id", document.Id },
            { "title", document.Title },
            { "content", document.Content },
            { "source", document.Source },
            { "category", document.Category },
            { "created", document.Created },
            { "tags", document.Tags },
            { "embedding", embedding }
        };
        
        // Upload document to index
        await _searchClient.IndexDocumentsAsync(IndexDocumentsBatch.Upload(new[] { searchDocument }));
    }

    public async Task<List<KnowledgeDocument>> SearchAsync(string query, int limit = 5, string filter = null)
    {
        // Generate embedding for query
        var embeddingResponse = await _openAIClient.GetEmbeddingsAsync(
            _embeddingDeployment, 
            new EmbeddingsOptions(query));
            
        float[] queryEmbedding = embeddingResponse.Value.Data[0].Embedding.ToArray();
        
        // Create vector query
        var vectorQuery = new VectorizedQuery(queryEmbedding)
        {
            KNearestNeighborsCount = limit,
            Fields = { "embedding" }
        };
        
        // Execute search
        var searchOptions = new SearchOptions
        {
            VectorSearch = new VectorSearchOptions
            {
                Queries = { vectorQuery }
            },
            Size = limit
        };
        
        // Add filter if provided
        if (!string.IsNullOrEmpty(filter))
        {
            searchOptions.Filter = filter;
        }
        
        var searchResults = await _searchClient.SearchAsync<SearchDocument>(null, searchOptions);
        
        // Process results
        var documents = new List<KnowledgeDocument>();
        await foreach (var result in searchResults.GetResultsAsync())
        {
            var doc = new KnowledgeDocument
            {
                Id = result.Document["id"].ToString(),
                Title = result.Document["title"].ToString(),
                Content = result.Document["content"].ToString(),
                Source = result.Document["source"].ToString(),
                Category = result.Document["category"].ToString(),
                Created = (DateTimeOffset)result.Document["created"],
                Tags = ((string[])result.Document["tags"]).ToList()
            };
            
            documents.Add(doc);
        }
        
        return documents;
    }

    public async Task<string> GenerateResponseWithRAGAsync(string query, string systemPrompt = null)
    {
        // Search for relevant documents
        var documents = await SearchAsync(query, 5);
        
        if (documents.Count == 0)
        {
            return "I don't have enough information to answer that question.";
        }
        
        // Format documents as context
        var context = new StringBuilder();
        foreach (var doc in documents)
        {
            context.AppendLine($"--- Document: {doc.Title} ---");
            context.AppendLine($"Source: {doc.Source}");
            context.AppendLine(doc.Content);
            context.AppendLine();
        }
        
        // Create system prompt if not provided
        if (string.IsNullOrEmpty(systemPrompt))
        {
            systemPrompt = "You are a helpful assistant that answers questions based on the provided context. " +
                           "If the answer cannot be found in the context, say that you don't know.";
        }
        
        // Create completion options
        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.3f,
            MaxTokens = 800,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage($"Context:\n{context}\n\nQuestion: {query}")
            }
        };
        
        // Generate response
        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
        
        return response.Value.Choices[0].Message.Content;
    }

    /// <summary>
    /// Connects to Microsoft Fabric data endpoints for real-time data synchronization
    /// with the RAG index. Validates endpoint availability and authentication.
    /// </summary>
    /// <param name="configuration">The Fabric endpoint configuration. Required when a data pipeline port is configured.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>
    /// A <see cref="PipelineConnectionResult"/> indicating whether the connection was established.
    /// Returns a disconnected result when no <see cref="IDataPipelinePort"/> is configured.
    /// </returns>
    public async Task<PipelineConnectionResult> ConnectToFabricDataEndpointsAsync(
        FabricEndpointConfiguration? configuration = null,
        CancellationToken cancellationToken = default)
    {
        if (_dataPipeline is null)
        {
            _logger?.LogDebug(
                "No data pipeline port configured. Fabric endpoint connection skipped.");
            return new PipelineConnectionResult
            {
                IsConnected = false,
                ErrorMessage = "Data pipeline integration is not configured."
            };
        }

        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration),
                "Fabric endpoint configuration is required when a data pipeline port is registered.");
        }

        _logger?.LogInformation(
            "Connecting to Fabric data endpoints for workspace '{WorkspaceId}', data store '{DataStoreName}'.",
            configuration.WorkspaceId,
            configuration.DataStoreName);

        try
        {
            var result = await _dataPipeline.ConnectToFabricEndpointsAsync(configuration, cancellationToken);

            if (result.IsConnected)
            {
                _logger?.LogInformation(
                    "Successfully connected to Fabric endpoint at '{EndpointUrl}'.",
                    result.ResolvedEndpointUrl);
            }
            else
            {
                _logger?.LogWarning(
                    "Failed to connect to Fabric endpoint: {Error}",
                    result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Exception while connecting to Fabric data endpoints.");
            return new PipelineConnectionResult
            {
                IsConnected = false,
                ErrorMessage = $"Connection failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Triggers an Azure Data Factory pipeline to perform batch ETL operations that
    /// feed data into the RAG search index.
    /// </summary>
    /// <param name="request">The pipeline execution request specifying which pipeline to trigger and its parameters.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>
    /// A <see cref="PipelineRunResult"/> with the pipeline run identifier and initial status.
    /// Returns an unknown-status result when no <see cref="IDataPipelinePort"/> is configured.
    /// </returns>
    public async Task<PipelineRunResult> OrchestrateDataFactoryPipelinesAsync(
        DataFactoryPipelineRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        if (_dataPipeline is null)
        {
            _logger?.LogDebug(
                "No data pipeline port configured. Data Factory pipeline orchestration skipped.");
            return new PipelineRunResult
            {
                RunId = string.Empty,
                Status = PipelineRunStatus.Unknown,
                ErrorMessage = "Data pipeline integration is not configured."
            };
        }

        if (request is null)
        {
            throw new ArgumentNullException(nameof(request),
                "Pipeline request is required when a data pipeline port is registered.");
        }

        _logger?.LogInformation(
            "Triggering Data Factory pipeline '{PipelineName}' in factory '{FactoryName}'.",
            request.PipelineName,
            request.FactoryName);

        try
        {
            var result = await _dataPipeline.TriggerDataFactoryPipelineAsync(request, cancellationToken);

            _logger?.LogInformation(
                "Data Factory pipeline '{PipelineName}' triggered. Run ID: '{RunId}', Status: {Status}.",
                request.PipelineName,
                result.RunId,
                result.Status);

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Exception while orchestrating Data Factory pipeline '{PipelineName}'.",
                request.PipelineName);

            return new PipelineRunResult
            {
                RunId = string.Empty,
                Status = PipelineRunStatus.Failed,
                ErrorMessage = $"Pipeline orchestration failed: {ex.Message}"
            };
        }
    }
}