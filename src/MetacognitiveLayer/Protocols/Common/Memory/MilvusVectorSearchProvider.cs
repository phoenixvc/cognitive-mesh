using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// Milvus-based implementation of <see cref="IVectorSearchProvider"/> for
    /// cloud-native vector similarity search at scale. Connects via the Milvus
    /// REST API (v2) for compatibility without requiring the gRPC client.
    /// Supports billions of vectors with GPU-accelerated indexing.
    /// </summary>
    public class MilvusVectorSearchProvider : IVectorSearchProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _collectionName;
        private readonly int _vectorDimension;
        private readonly ILogger<MilvusVectorSearchProvider> _logger;
        private bool _initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvusVectorSearchProvider"/> class.
        /// </summary>
        /// <param name="endpoint">Milvus REST API endpoint (e.g., http://localhost:19530).</param>
        /// <param name="collectionName">Name of the Milvus collection for embeddings.</param>
        /// <param name="vectorDimension">Dimension of embedding vectors.</param>
        /// <param name="logger">Logger instance for structured logging.</param>
        /// <param name="apiKey">Optional API key for Zilliz Cloud managed Milvus.</param>
        public MilvusVectorSearchProvider(
            string endpoint,
            string collectionName,
            int vectorDimension,
            ILogger<MilvusVectorSearchProvider> logger,
            string? apiKey = null)
        {
            _collectionName = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
            _vectorDimension = vectorDimension;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(endpoint ?? throw new ArgumentNullException(nameof(endpoint)))
            };
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            if (!string.IsNullOrEmpty(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            }
        }

        /// <summary>
        /// Initializes the Milvus collection with an IVF_FLAT vector index
        /// if it does not already exist.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_initialized) return;

            _logger.LogInformation(
                "Initializing Milvus vector search provider, collection {Collection}",
                _collectionName);

            try
            {
                // Check if collection exists
                var listResponse = await _httpClient.GetAsync("/v2/vectordb/collections/list");
                if (listResponse.IsSuccessStatusCode)
                {
                    var listResult = await listResponse.Content.ReadFromJsonAsync<MilvusListResponse>();
                    var exists = listResult?.Data?.Contains(_collectionName) ?? false;

                    if (!exists)
                    {
                        // Create collection with schema
                        var createPayload = new
                        {
                            collectionName = _collectionName,
                            dimension = _vectorDimension,
                            metricType = "COSINE",
                            primaryFieldName = "id",
                            vectorFieldName = "embedding",
                            idType = "Int64",
                            autoId = true
                        };

                        var createResponse = await _httpClient.PostAsJsonAsync(
                            "/v2/vectordb/collections/create",
                            createPayload);

                        if (createResponse.IsSuccessStatusCode)
                        {
                            _logger.LogInformation(
                                "Created Milvus collection {Collection} with dimension {Dimension}",
                                _collectionName, _vectorDimension);
                        }
                        else
                        {
                            var error = await createResponse.Content.ReadAsStringAsync();
                            _logger.LogError("Failed to create Milvus collection: {Error}", error);
                        }
                    }
                }

                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Milvus vector search provider");
                throw;
            }
        }

        /// <summary>
        /// Searches for vectors similar to the provided embedding using Milvus ANN search.
        /// </summary>
        public async Task<IEnumerable<string>> QuerySimilarAsync(float[] embedding, float threshold)
        {
            if (!_initialized) await InitializeAsync();

            try
            {
                var searchPayload = new
                {
                    collectionName = _collectionName,
                    data = new[] { embedding },
                    limit = 10,
                    outputFields = new[] { "value", "key" },
                    @params = new { radius = threshold }
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "/v2/vectordb/entities/search",
                    searchPayload);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Milvus search failed with status {Status}", response.StatusCode);
                    return Enumerable.Empty<string>();
                }

                var result = await response.Content.ReadFromJsonAsync<MilvusSearchResponse>();
                var values = new List<string>();

                if (result?.Data != null)
                {
                    foreach (var hit in result.Data)
                    {
                        if (hit.TryGetValue("value", out var value) && value is JsonElement je)
                        {
                            values.Add(je.GetString() ?? string.Empty);
                        }
                    }
                }

                _logger.LogDebug(
                    "Milvus search returned {Count} results above threshold {Threshold}",
                    values.Count, threshold);

                return values;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying Milvus for similar vectors");
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Inserts a document with its embedding vector into the Milvus collection.
        /// </summary>
        public async Task SaveDocumentAsync(string key, Dictionary<string, object> document)
        {
            if (!_initialized) await InitializeAsync();

            try
            {
                var embedding = ExtractEmbedding(document);
                if (embedding == null)
                {
                    _logger.LogWarning("Document {Key} has no valid embedding, skipping Milvus insert", key);
                    return;
                }

                var value = document.TryGetValue("value", out var v) ? v?.ToString() ?? string.Empty : string.Empty;

                var insertPayload = new
                {
                    collectionName = _collectionName,
                    data = new[]
                    {
                        new Dictionary<string, object>
                        {
                            ["embedding"] = embedding,
                            ["key"] = key,
                            ["value"] = value
                        }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "/v2/vectordb/entities/insert",
                    insertPayload);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Document {Key} saved to Milvus collection {Collection}", key, _collectionName);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to insert document {Key} into Milvus: {Error}", key, error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving document {Key} to Milvus", key);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a document field value by querying Milvus with a key filter.
        /// </summary>
        public async Task<string> GetDocumentValueAsync(string key, string jsonPath)
        {
            if (!_initialized) await InitializeAsync();

            try
            {
                var fieldName = jsonPath.TrimStart('$', '.');

                var queryPayload = new
                {
                    collectionName = _collectionName,
                    filter = $"key == \"{key}\"",
                    outputFields = new[] { fieldName },
                    limit = 1
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "/v2/vectordb/entities/query",
                    queryPayload);

                if (!response.IsSuccessStatusCode)
                    return string.Empty;

                var result = await response.Content.ReadFromJsonAsync<MilvusSearchResponse>();
                if (result?.Data != null && result.Data.Count > 0)
                {
                    var firstHit = result.Data[0];
                    if (firstHit.TryGetValue(fieldName, out var value) && value is JsonElement je)
                    {
                        return je.GetString() ?? string.Empty;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document value from Milvus for key {Key}", key);
                return string.Empty;
            }
        }

        private static float[]? ExtractEmbedding(Dictionary<string, object> document)
        {
            if (!document.TryGetValue("embedding", out var embeddingObj))
                return null;

            return embeddingObj switch
            {
                float[] arr => arr,
                JsonElement je => JsonSerializer.Deserialize<float[]>(je.GetRawText()),
                string str => JsonSerializer.Deserialize<float[]>(str),
                _ => null
            };
        }

        /// <summary>
        /// Response model for Milvus collection list API.
        /// </summary>
        internal class MilvusListResponse
        {
            /// <summary>List of collection names.</summary>
            [JsonPropertyName("data")]
            public List<string>? Data { get; set; }
        }

        /// <summary>
        /// Response model for Milvus search/query API.
        /// </summary>
        internal class MilvusSearchResponse
        {
            /// <summary>Search result hits.</summary>
            [JsonPropertyName("data")]
            public List<Dictionary<string, object>>? Data { get; set; }
        }
    }
}
