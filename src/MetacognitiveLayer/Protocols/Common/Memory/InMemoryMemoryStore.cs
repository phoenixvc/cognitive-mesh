using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// In-memory implementation of <see cref="IMeshMemoryStore"/> using
    /// <see cref="ConcurrentDictionary{TKey,TValue}"/> for thread-safe storage.
    /// Ideal for development, testing, and single-process deployments where
    /// persistence across restarts is not required.
    /// </summary>
    public class InMemoryMemoryStore : IMeshMemoryStore
    {
        private readonly ConcurrentDictionary<string, string> _contextStore = new();
        private readonly ConcurrentDictionary<string, float[]> _embeddingStore = new();
        private readonly ILogger<InMemoryMemoryStore> _logger;
        private bool _initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryMemoryStore"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for structured logging.</param>
        public InMemoryMemoryStore(ILogger<InMemoryMemoryStore> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes the in-memory store (no-op since no external resources are needed).
        /// </summary>
        public Task InitializeAsync()
        {
            if (_initialized) return Task.CompletedTask;

            _logger.LogInformation("In-memory memory store initialized (ConcurrentDictionary-backed)");
            _initialized = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Saves a context key-value pair to the in-memory dictionary.
        /// Thread-safe via ConcurrentDictionary.
        /// </summary>
        public Task SaveContextAsync(string sessionId, string key, string value)
        {
            var compositeKey = $"{sessionId}:{key}";
            _contextStore.AddOrUpdate(compositeKey, value, (_, _) => value);

            if (key.Contains("embedding", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var embedding = JsonSerializer.Deserialize<float[]>(value);
                    if (embedding != null)
                    {
                        _embeddingStore.AddOrUpdate(compositeKey, embedding, (_, _) => embedding);
                    }
                }
                catch (JsonException)
                {
                    _logger.LogDebug("Value for key {Key} is not a valid embedding vector", key);
                }
            }

            _logger.LogDebug("Context saved in-memory for session {SessionId}, key {Key}", sessionId, key);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Retrieves a context value from the in-memory dictionary.
        /// </summary>
        public Task<string> GetContextAsync(string sessionId, string key)
        {
            var compositeKey = $"{sessionId}:{key}";
            _contextStore.TryGetValue(compositeKey, out var value);
            return Task.FromResult(value ?? string.Empty);
        }

        /// <summary>
        /// Queries for similar embeddings using brute-force cosine similarity
        /// across all stored vectors. Suitable for small to medium datasets.
        /// </summary>
        public Task<IEnumerable<string>> QuerySimilarAsync(string embedding, float threshold)
        {
            float[]? queryEmbedding;
            try
            {
                queryEmbedding = JsonSerializer.Deserialize<float[]>(embedding);
                if (queryEmbedding == null) return Task.FromResult(Enumerable.Empty<string>());
            }
            catch (JsonException)
            {
                return Task.FromResult(Enumerable.Empty<string>());
            }

            var results = _embeddingStore
                .Select(kvp => (key: kvp.Key, similarity: CalculateCosineSimilarity(queryEmbedding, kvp.Value)))
                .Where(x => x.similarity >= threshold)
                .OrderByDescending(x => x.similarity)
                .Select(x => _contextStore.TryGetValue(x.key, out var val) ? val : string.Empty)
                .Where(v => !string.IsNullOrEmpty(v));

            return Task.FromResult(results);
        }

        /// <summary>
        /// Clears all stored data. Useful for test isolation.
        /// </summary>
        public void Clear()
        {
            _contextStore.Clear();
            _embeddingStore.Clear();
            _logger.LogInformation("In-memory store cleared");
        }

        /// <summary>
        /// Returns the current count of stored context entries.
        /// </summary>
        public int Count => _contextStore.Count;

        private static float CalculateCosineSimilarity(float[] a, float[] b)
        {
            if (a.Length != b.Length) return 0f;

            float dotProduct = 0f, normA = 0f, normB = 0f;
            for (int i = 0; i < a.Length; i++)
            {
                dotProduct += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }

            var denominator = MathF.Sqrt(normA) * MathF.Sqrt(normB);
            return denominator == 0f ? 0f : dotProduct / denominator;
        }
    }
}
