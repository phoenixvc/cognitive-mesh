using System.Text.Json;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// Qdrant-based implementation of <see cref="IVectorSearchProvider"/> for
    /// production-grade vector similarity search. Connects to a Qdrant server
    /// and manages collections for mesh memory embeddings.
    /// </summary>
    public class QdrantVectorSearchProvider : IVectorSearchProvider
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _collectionName;
        private readonly int _vectorDimension;
        private readonly ILogger<QdrantVectorSearchProvider> _logger;
        private QdrantClient? _client;
        private bool _initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="QdrantVectorSearchProvider"/> class.
        /// </summary>
        /// <param name="host">Qdrant server hostname.</param>
        /// <param name="port">Qdrant gRPC port (default 6334).</param>
        /// <param name="collectionName">Name of the Qdrant collection for embeddings.</param>
        /// <param name="vectorDimension">Dimension of embedding vectors.</param>
        /// <param name="logger">Logger instance for structured logging.</param>
        public QdrantVectorSearchProvider(
            string host,
            int port,
            string collectionName,
            int vectorDimension,
            ILogger<QdrantVectorSearchProvider> logger)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _port = port;
            _collectionName = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
            _vectorDimension = vectorDimension;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes the Qdrant client and ensures the collection exists
        /// with the configured vector parameters.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_initialized) return;

            _logger.LogInformation(
                "Initializing Qdrant vector search provider at {Host}:{Port}, collection {Collection}",
                _host, _port, _collectionName);

            _client = new QdrantClient(_host, _port);

            try
            {
                // Check if collection exists, create if not
                var collections = await _client.ListCollectionsAsync();
                var exists = collections.Any(c => c == _collectionName);

                if (!exists)
                {
                    await _client.CreateCollectionAsync(
                        _collectionName,
                        new VectorParams
                        {
                            Size = (ulong)_vectorDimension,
                            Distance = Distance.Cosine
                        });

                    _logger.LogInformation(
                        "Created Qdrant collection {Collection} with dimension {Dimension}",
                        _collectionName, _vectorDimension);
                }
                else
                {
                    _logger.LogInformation(
                        "Qdrant collection {Collection} already exists",
                        _collectionName);
                }

                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Qdrant vector search provider");
                throw;
            }
        }

        /// <summary>
        /// Queries for vectors similar to the provided embedding using Qdrant's
        /// HNSW index for sub-millisecond similarity search.
        /// </summary>
        public async Task<IEnumerable<string>> QuerySimilarAsync(float[] embedding, float threshold)
        {
            if (!_initialized) await InitializeAsync();
            if (_client == null) return Enumerable.Empty<string>();

            try
            {
                var searchResults = await _client.SearchAsync(
                    _collectionName,
                    embedding,
                    limit: 10,
                    scoreThreshold: threshold);

                var results = new List<string>();
                foreach (var result in searchResults)
                {
                    if (result.Payload.TryGetValue("value", out var value))
                    {
                        results.Add(value.StringValue);
                    }
                }

                _logger.LogDebug(
                    "Qdrant search returned {Count} results above threshold {Threshold}",
                    results.Count, threshold);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying Qdrant for similar vectors");
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Saves a document with its embedding vector to the Qdrant collection.
        /// </summary>
        public async Task SaveDocumentAsync(string key, Dictionary<string, object> document)
        {
            if (!_initialized) await InitializeAsync();
            if (_client == null) return;

            try
            {
                // Extract embedding from document
                float[]? embedding = null;
                if (document.TryGetValue("embedding", out var embeddingObj))
                {
                    embedding = embeddingObj switch
                    {
                        float[] arr => arr,
                        JsonElement je => JsonSerializer.Deserialize<float[]>(je.GetRawText()),
                        string str => JsonSerializer.Deserialize<float[]>(str),
                        _ => null
                    };
                }

                if (embedding == null)
                {
                    _logger.LogWarning("Document {Key} has no valid embedding, skipping Qdrant upsert", key);
                    return;
                }

                // Build payload from document fields (excluding embedding)
                var payload = new Dictionary<string, Value>();
                foreach (var kvp in document)
                {
                    if (kvp.Key == "embedding") continue;
                    payload[kvp.Key] = new Value { StringValue = kvp.Value?.ToString() ?? string.Empty };
                }

                // Generate a deterministic point ID from the key
                var pointId = (ulong)key.GetHashCode(StringComparison.Ordinal) & 0x7FFFFFFFFFFFFFFF;

                await _client.UpsertAsync(
                    _collectionName,
                    new List<PointStruct>
                    {
                        new()
                        {
                            Id = new PointId { Num = pointId },
                            Vectors = embedding,
                            Payload = { payload }
                        }
                    });

                _logger.LogDebug("Document {Key} saved to Qdrant collection {Collection}", key, _collectionName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving document {Key} to Qdrant", key);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specific field value from a document in Qdrant by point ID.
        /// </summary>
        public async Task<string> GetDocumentValueAsync(string key, string jsonPath)
        {
            if (!_initialized) await InitializeAsync();
            if (_client == null) return string.Empty;

            try
            {
                var pointId = (ulong)key.GetHashCode(StringComparison.Ordinal) & 0x7FFFFFFFFFFFFFFF;

                var points = await _client.GetAsync(
                    _collectionName,
                    new List<PointId> { new() { Num = pointId } },
                    withPayload: true);

                var point = points.FirstOrDefault();
                if (point == null) return string.Empty;

                // Extract field name from jsonPath (e.g., "$.value" -> "value")
                var fieldName = jsonPath.TrimStart('$', '.');

                if (point.Payload.TryGetValue(fieldName, out var value))
                {
                    return value.StringValue;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document value from Qdrant for key {Key}", key);
                return string.Empty;
            }
        }
    }
}
