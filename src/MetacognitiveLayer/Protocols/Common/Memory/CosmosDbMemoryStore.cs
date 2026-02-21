using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// Azure Cosmos DB implementation of <see cref="IMeshMemoryStore"/> providing
    /// globally distributed, multi-model persistence with automatic scaling.
    /// Uses the SQL (NoSQL) API with partition key isolation by session ID.
    /// </summary>
    public class CosmosDbMemoryStore : IMeshMemoryStore
    {
        private readonly string _connectionString;
        private readonly string _databaseId;
        private readonly string _containerId;
        private readonly ILogger<CosmosDbMemoryStore> _logger;
        private CosmosClient? _client;
        private Container? _container;
        private bool _initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbMemoryStore"/> class.
        /// </summary>
        /// <param name="connectionString">Cosmos DB connection string.</param>
        /// <param name="databaseId">Database name.</param>
        /// <param name="containerId">Container name.</param>
        /// <param name="logger">Logger instance for structured logging.</param>
        public CosmosDbMemoryStore(
            string connectionString,
            string databaseId,
            string containerId,
            ILogger<CosmosDbMemoryStore> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _databaseId = databaseId ?? throw new ArgumentNullException(nameof(databaseId));
            _containerId = containerId ?? throw new ArgumentNullException(nameof(containerId));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes the Cosmos DB client and ensures the database and container exist.
        /// Configures partition key on session_id for efficient query isolation.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_initialized) return;

            _logger.LogInformation(
                "Initializing Cosmos DB memory store, database {Database}, container {Container}",
                _databaseId, _containerId);

            _client = new CosmosClient(_connectionString, new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Direct,
                MaxRetryAttemptsOnRateLimitedRequests = 9,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(60),
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });

            var database = await _client.CreateDatabaseIfNotExistsAsync(_databaseId);
            var containerResponse = await database.Database.CreateContainerIfNotExistsAsync(
                new ContainerProperties(_containerId, "/sessionId")
                {
                    DefaultTimeToLive = -1 // No default TTL, items persist indefinitely
                });

            _container = containerResponse.Container;
            _initialized = true;
            _logger.LogInformation("Cosmos DB memory store initialized successfully");
        }

        /// <summary>
        /// Saves a context key-value pair, using upsert semantics with the session ID
        /// as the partition key for efficient isolation and querying.
        /// </summary>
        public async Task SaveContextAsync(string sessionId, string key, string value)
        {
            if (!_initialized) await InitializeAsync();
            if (_container == null) return;

            var document = new CosmosContextDocument
            {
                Id = $"{sessionId}:{key}",
                SessionId = sessionId,
                ContextKey = key,
                Value = value,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            // Detect and store embeddings inline
            if (key.Contains("embedding", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var embedding = JsonSerializer.Deserialize<float[]>(value);
                    if (embedding != null)
                    {
                        document.Embedding = embedding;
                    }
                }
                catch (JsonException)
                {
                    _logger.LogWarning("Value for key {Key} could not be parsed as embedding", key);
                }
            }

            await _container.UpsertItemAsync(
                document,
                new PartitionKey(sessionId));

            _logger.LogDebug("Context saved to Cosmos DB for session {SessionId}, key {Key}", sessionId, key);
        }

        /// <summary>
        /// Retrieves a context value by session ID and key using a point read
        /// (most efficient Cosmos DB operation at 1 RU).
        /// </summary>
        public async Task<string> GetContextAsync(string sessionId, string key)
        {
            if (!_initialized) await InitializeAsync();
            if (_container == null) return string.Empty;

            try
            {
                var response = await _container.ReadItemAsync<CosmosContextDocument>(
                    $"{sessionId}:{key}",
                    new PartitionKey(sessionId));

                return response.Resource.Value;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Queries for similar embeddings by scanning documents with stored embeddings
        /// and computing cosine similarity. For production workloads with many embeddings,
        /// consider using Cosmos DB's vector search preview or an external vector database.
        /// </summary>
        public async Task<IEnumerable<string>> QuerySimilarAsync(string embedding, float threshold)
        {
            if (!_initialized) await InitializeAsync();
            if (_container == null) return Enumerable.Empty<string>();

            float[]? queryEmbedding;
            try
            {
                queryEmbedding = JsonSerializer.Deserialize<float[]>(embedding);
                if (queryEmbedding == null) return Enumerable.Empty<string>();
            }
            catch (JsonException)
            {
                return Enumerable.Empty<string>();
            }

            // Query documents that have embeddings
            var query = new QueryDefinition(
                "SELECT c.value, c.embedding FROM c WHERE IS_ARRAY(c.embedding)");

            var similarities = new List<(float similarity, string value)>();
            using var iterator = _container.GetItemQueryIterator<CosmosContextDocument>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                foreach (var doc in response)
                {
                    if (doc.Embedding == null) continue;

                    var similarity = CalculateCosineSimilarity(queryEmbedding, doc.Embedding);
                    if (similarity >= threshold)
                    {
                        similarities.Add((similarity, doc.Value));
                    }
                }
            }

            return similarities
                .OrderByDescending(s => s.similarity)
                .Select(s => s.value);
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
        /// Cosmos DB document model for context storage.
        /// Partitioned by sessionId for efficient cross-partition isolation.
        /// </summary>
        internal class CosmosContextDocument
        {
            /// <summary>Document ID: "{sessionId}:{contextKey}".</summary>
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            /// <summary>Session identifier (partition key).</summary>
            [JsonPropertyName("sessionId")]
            public string SessionId { get; set; } = string.Empty;

            /// <summary>Context key name.</summary>
            [JsonPropertyName("contextKey")]
            public string ContextKey { get; set; } = string.Empty;

            /// <summary>Context value.</summary>
            [JsonPropertyName("value")]
            public string Value { get; set; } = string.Empty;

            /// <summary>Optional embedding vector for similarity search.</summary>
            [JsonPropertyName("embedding")]
            public float[]? Embedding { get; set; }

            /// <summary>Unix timestamp of last update.</summary>
            [JsonPropertyName("timestamp")]
            public long Timestamp { get; set; }
        }
    }
}
