using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SemanticSearchManager
{
    private readonly SearchClient _searchClient;
    private readonly OpenAIClient _openAIClient;
    private readonly string _embeddingDeployment;
    private readonly string _completionDeployment;
    private readonly FeatureFlagManager _featureFlagManager;

    public SemanticSearchManager(SearchClient searchClient, OpenAIClient openAIClient, string embeddingDeployment, string completionDeployment, FeatureFlagManager featureFlagManager)
    {
        _searchClient = searchClient;
        _openAIClient = openAIClient;
        _embeddingDeployment = embeddingDeployment;
        _completionDeployment = completionDeployment;
        _featureFlagManager = featureFlagManager;
    }

    public async Task<List<KnowledgeDocument>> PerformSemanticSearchAsync(string query, int limit = 5, string filter = null)
    {
        if (!_featureFlagManager.EnableSemanticKernel)
        {
            return new List<KnowledgeDocument> { new KnowledgeDocument { Title = "Feature not enabled.", Content = "The Semantic Kernel feature is not enabled." } };
        }

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

    public async Task<string> GenerateResponseWithSemanticSearchAsync(string query, string systemPrompt = null)
    {
        // Perform semantic search
        var documents = await PerformSemanticSearchAsync(query, 5);

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
}
