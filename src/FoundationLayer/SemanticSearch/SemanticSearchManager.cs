using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CognitiveMesh.FoundationLayer.LLM.Abstractions;
using CognitiveMesh.FoundationLayer.VectorDatabase;
using CognitiveMesh.Shared.Interfaces;

namespace FoundationLayer.SemanticSearch
{
    /// <summary>
    /// Provides enhanced RAG by combining vector search with contextual LLM prompts.
    /// </summary>
    public class SemanticSearchManager : ISemanticSearchManager, IDisposable
    {
        private readonly VectorDatabaseManager _vectorManager;
        private readonly ILLMClient _llmClient;
        private bool _disposed = false;

        public SemanticSearchManager(
            VectorDatabaseManager vectorManager,
            ILLMClient llmClient)
        {
            _vectorManager = vectorManager ?? throw new ArgumentNullException(nameof(vectorManager));
            _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        }

        /// <summary>
        /// Performs a semantic search using vector similarity and refines results with an LLM.
        /// </summary>
        /// <param name="indexName">Name of the vector index to search.</param>
        /// <param name="queryText">The natural language query.</param>
        /// <returns>Refined search results based on semantic understanding.</returns>
        public async Task<string> SearchAsync(string indexName, string queryText)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name cannot be null or whitespace.", nameof(indexName));
                
            if (string.IsNullOrWhiteSpace(queryText))
                throw new ArgumentException("Query text cannot be null or whitespace.", nameof(queryText));

            try
            {
                // Step 1: Generate embedding for the query
                var queryEmbedding = await _llmClient.GetEmbeddingsAsync(queryText);
                
                // Step 2: Find similar vectors
                var searchResults = await _vectorManager.QuerySimilarAsync(indexName, queryEmbedding, limit: 5);
                var similarIds = searchResults.Select(r => r.Id);
                
                // Step 3: Retrieve the actual content for the similar vectors
                // This would typically be done by looking up the IDs in a document store
                var context = await RetrieveContextForIdsAsync(similarIds);
                
                // Step 4: Generate a refined response using the LLM
                var prompt = $"""
                    Based on the following context, answer the query.
                    
                    Query: {queryText}
                    
                    Context:
                    {context}
                    
                    Answer:
                    """;
                    
                return await _llmClient.GenerateCompletionAsync(prompt);
            }
            catch (Exception ex)
            {
                // Log the error
                throw new InvalidOperationException("Semantic search failed. See inner exception for details.", ex);
            }
        }

        private async Task<string> RetrieveContextForIdsAsync(IEnumerable<string> ids)
        {
            // In a real implementation, this would retrieve the actual content
            // from a document store using the IDs
            return string.Join("\n\n", ids.Select(id => $"[Document ID: {id}]"));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _vectorManager?.Dispose();
                    (_llmClient as IDisposable)?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
