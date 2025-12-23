using System.Text;

namespace FoundationLayer.SemanticSearch;

public class EnhancedRAGSystem
{
    private readonly SearchClient _searchClient;
    private readonly SearchIndexClient _indexClient;
    private readonly OpenAIClient _openAIClient;
    private readonly string _indexName;
    private readonly string _embeddingDeployment;
    private readonly string _completionDeployment;

    public EnhancedRAGSystem(SearchClient searchClient, SearchIndexClient indexClient, OpenAIClient openAIClient, string indexName, string embeddingDeployment, string completionDeployment)
    {
        _searchClient = searchClient;
        _indexClient = indexClient;
        _openAIClient = openAIClient;
        _indexName = indexName;
        _embeddingDeployment = embeddingDeployment;
        _completionDeployment = completionDeployment;
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

    public async Task ConnectToFabricDataEndpointsAsync()
    {
        // Implement logic to connect to Fabric data endpoints
        await Task.CompletedTask;
    }

    public async Task OrchestrateDataFactoryPipelinesAsync()
    {
        // Implement logic to orchestrate Data Factory pipelines
        await Task.CompletedTask;
    }
}