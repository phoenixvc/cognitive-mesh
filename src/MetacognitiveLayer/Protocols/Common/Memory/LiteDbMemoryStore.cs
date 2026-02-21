using System.Text.Json;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// LiteDB-based implementation of <see cref="IMeshMemoryStore"/> providing
    /// an embedded NoSQL document store with ACID transactions. Pure C# implementation
    /// with zero native dependencies, ideal for development and single-instance deployments.
    /// </summary>
    public class LiteDbMemoryStore : IMeshMemoryStore
    {
        private readonly string _dbPath;
        private readonly ILogger<LiteDbMemoryStore> _logger;
        private LiteDatabase? _database;
        private ILiteCollection<ContextDocument>? _contextCollection;
        private ILiteCollection<EmbeddingDocument>? _embeddingCollection;
        private bool _initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteDbMemoryStore"/> class.
        /// </summary>
        /// <param name="dbPath">Path to the LiteDB database file.</param>
        /// <param name="logger">Logger instance for structured logging.</param>
        public LiteDbMemoryStore(string dbPath, ILogger<LiteDbMemoryStore> logger)
        {
            _dbPath = dbPath ?? throw new ArgumentNullException(nameof(dbPath));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes the LiteDB database with context and embedding collections,
        /// creating unique indexes for efficient lookups.
        /// </summary>
        public Task InitializeAsync()
        {
            if (_initialized) return Task.CompletedTask;

            _logger.LogInformation("Initializing LiteDB memory store at {DbPath}", _dbPath);

            var directory = Path.GetDirectoryName(_dbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _database = new LiteDatabase($"Filename={_dbPath};Connection=shared");

            _contextCollection = _database.GetCollection<ContextDocument>("context");
            _contextCollection.EnsureIndex(x => x.SessionId);
            _contextCollection.EnsureIndex(x => x.SessionKey, true);

            _embeddingCollection = _database.GetCollection<EmbeddingDocument>("embeddings");
            _embeddingCollection.EnsureIndex(x => x.SessionId);

            _initialized = true;
            _logger.LogInformation("LiteDB memory store initialized successfully");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Saves a context key-value pair. Uses upsert semantics based on
        /// the composite key of session ID and context key.
        /// </summary>
        public Task SaveContextAsync(string sessionId, string key, string value)
        {
            if (!_initialized) InitializeAsync();
            if (_contextCollection == null) return Task.CompletedTask;

            var sessionKey = $"{sessionId}:{key}";
            var existing = _contextCollection.FindOne(x => x.SessionKey == sessionKey);

            if (existing != null)
            {
                existing.Value = value;
                existing.UpdatedAt = DateTime.UtcNow;
                _contextCollection.Update(existing);
            }
            else
            {
                _contextCollection.Insert(new ContextDocument
                {
                    SessionId = sessionId,
                    ContextKey = key,
                    SessionKey = sessionKey,
                    Value = value,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // Detect and store embeddings
            if (key.Contains("embedding", StringComparison.OrdinalIgnoreCase) && _embeddingCollection != null)
            {
                try
                {
                    var embedding = System.Text.Json.JsonSerializer.Deserialize<float[]>(value);
                    if (embedding != null)
                    {
                        _embeddingCollection.Insert(new EmbeddingDocument
                        {
                            SessionId = sessionId,
                            ContextKey = key,
                            Embedding = embedding,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
                catch (JsonException)
                {
                    _logger.LogWarning("Value for key {Key} could not be parsed as embedding", key);
                }
            }

            _logger.LogDebug("Context saved for session {SessionId}, key {Key}", sessionId, key);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Retrieves a context value by session ID and key.
        /// </summary>
        public Task<string> GetContextAsync(string sessionId, string key)
        {
            if (!_initialized) InitializeAsync();
            if (_contextCollection == null) return Task.FromResult(string.Empty);

            var sessionKey = $"{sessionId}:{key}";
            var doc = _contextCollection.FindOne(x => x.SessionKey == sessionKey);
            return Task.FromResult(doc?.Value ?? string.Empty);
        }

        /// <summary>
        /// Queries for similar embeddings using in-memory cosine similarity calculation.
        /// Loads all embeddings and computes similarity in C#.
        /// </summary>
        public Task<IEnumerable<string>> QuerySimilarAsync(string embedding, float threshold)
        {
            if (!_initialized) InitializeAsync();
            if (_embeddingCollection == null || _contextCollection == null)
                return Task.FromResult(Enumerable.Empty<string>());

            float[]? queryEmbedding;
            try
            {
                queryEmbedding = System.Text.Json.JsonSerializer.Deserialize<float[]>(embedding);
                if (queryEmbedding == null) return Task.FromResult(Enumerable.Empty<string>());
            }
            catch (JsonException)
            {
                return Task.FromResult(Enumerable.Empty<string>());
            }

            var allEmbeddings = _embeddingCollection.FindAll().ToList();
            var similarities = new List<(float similarity, string value)>();

            foreach (var doc in allEmbeddings)
            {
                if (doc.Embedding == null) continue;

                var similarity = CalculateCosineSimilarity(queryEmbedding, doc.Embedding);
                if (similarity >= threshold)
                {
                    var sessionKey = $"{doc.SessionId}:{doc.ContextKey}";
                    var contextDoc = _contextCollection.FindOne(x => x.SessionKey == sessionKey);
                    if (contextDoc != null)
                    {
                        similarities.Add((similarity, contextDoc.Value));
                    }
                }
            }

            IEnumerable<string> result = similarities
                .OrderByDescending(s => s.similarity)
                .Select(s => s.value);

            return Task.FromResult(result);
        }

        /// <summary>
        /// Disposes the LiteDB database connection.
        /// </summary>
        public void Dispose()
        {
            _database?.Dispose();
        }

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

        /// <summary>
        /// Internal document model for context storage in LiteDB.
        /// </summary>
        internal class ContextDocument
        {
            /// <summary>LiteDB document identifier.</summary>
            public ObjectId Id { get; set; } = ObjectId.NewObjectId();

            /// <summary>Session identifier.</summary>
            public string SessionId { get; set; } = string.Empty;

            /// <summary>Context key name.</summary>
            public string ContextKey { get; set; } = string.Empty;

            /// <summary>Composite key for unique constraint: sessionId:key.</summary>
            public string SessionKey { get; set; } = string.Empty;

            /// <summary>Context value.</summary>
            public string Value { get; set; } = string.Empty;

            /// <summary>Document creation timestamp.</summary>
            public DateTime CreatedAt { get; set; }

            /// <summary>Last update timestamp.</summary>
            public DateTime UpdatedAt { get; set; }
        }

        /// <summary>
        /// Internal document model for embedding storage in LiteDB.
        /// </summary>
        internal class EmbeddingDocument
        {
            /// <summary>LiteDB document identifier.</summary>
            public ObjectId Id { get; set; } = ObjectId.NewObjectId();

            /// <summary>Session identifier.</summary>
            public string SessionId { get; set; } = string.Empty;

            /// <summary>Context key name.</summary>
            public string ContextKey { get; set; } = string.Empty;

            /// <summary>Embedding vector.</summary>
            public float[] Embedding { get; set; } = Array.Empty<float>();

            /// <summary>Document creation timestamp.</summary>
            public DateTime CreatedAt { get; set; }
        }
    }
}
