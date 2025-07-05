using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// Redis-based implementation of the mesh memory store.
    /// Now uses IVectorSearchProvider to abstract Redis vector capabilities.
    /// </summary>
    public class RedisVectorMemoryStore : IMeshMemoryStore
    {
        private readonly IVectorSearchProvider _vectorSearch;
        private readonly ILogger<RedisVectorMemoryStore> _logger;
        private readonly Lazy<Task> _lazyInit;

        public RedisVectorMemoryStore(IVectorSearchProvider vectorSearch, ILogger<RedisVectorMemoryStore> logger)
        {
            _vectorSearch = vectorSearch ?? throw new ArgumentNullException(nameof(vectorSearch));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lazyInit = new Lazy<Task>(() => _vectorSearch.InitializeAsync());
        }

        public async Task InitializeAsync() => await _lazyInit.Value;

        public async Task SaveContextAsync(string sessionId, string key, string value)
        {
            await InitializeAsync();
            var redisKey = MeshKeyFormatter.Format(sessionId, key);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var document = new Dictionary<string, object>
            {
                ["session_id"] = sessionId,
                ["key"] = key,
                ["value"] = value,
                ["timestamp"] = timestamp
            };

            var embedding = EmbeddingParser.TryParse(value, _logger, key);
            if (embedding != null)
            {
                document["embedding"] = embedding;
            }

            await _vectorSearch.SaveDocumentAsync(redisKey, document);
            _logger.LogDebug("Saved context to Redis for key {Key}", redisKey);
        }

        public async Task<string> GetContextAsync(string sessionId, string key)
        {
            await InitializeAsync();
            var redisKey = MeshKeyFormatter.Format(sessionId, key);
            return await _vectorSearch.GetDocumentValueAsync(redisKey, "$.value");
        }

        public async Task<IEnumerable<string>> QuerySimilarAsync(string embedding, float threshold)
        {
            await InitializeAsync();
            var parsed = EmbeddingParser.TryParse(embedding, _logger);
            if (parsed == null || parsed.Length == 0)
                throw new ArgumentException("Invalid embedding format", nameof(embedding));

            return await _vectorSearch.QuerySimilarAsync(parsed, threshold);
        }
    }
}
