using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// ChromaDB-based implementation of <see cref="IVectorSearchProvider"/> for
    /// AI-native embedding storage and retrieval. Connects via ChromaDB's REST API
    /// and supports automatic embedding generation, metadata filtering, and
    /// both embedded and client-server deployment modes.
    /// </summary>
    public class ChromaVectorSearchProvider : IVectorSearchProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _collectionName;
        private readonly ILogger<ChromaVectorSearchProvider> _logger;
        private string? _collectionId;
        private bool _initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChromaVectorSearchProvider"/> class.
        /// </summary>
        /// <param name="endpoint">ChromaDB REST API endpoint (e.g., http://localhost:8000).</param>
        /// <param name="collectionName">Name of the ChromaDB collection for embeddings.</param>
        /// <param name="logger">Logger instance for structured logging.</param>
        public ChromaVectorSearchProvider(
            string endpoint,
            string collectionName,
            ILogger<ChromaVectorSearchProvider> logger)
        {
            _collectionName = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(endpoint ?? throw new ArgumentNullException(nameof(endpoint)))
            };
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        /// <summary>
        /// Initializes the ChromaDB collection. Creates the collection if it does not
        /// exist, using cosine distance as the similarity metric.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_initialized) return;

            _logger.LogInformation(
                "Initializing ChromaDB vector search provider, collection {Collection}",
                _collectionName);

            try
            {
                // Get or create collection
                var payload = new
                {
                    name = _collectionName,
                    metadata = new Dictionary<string, string>
                    {
                        ["hnsw:space"] = "cosine"
                    },
                    get_or_create = true
                };

                var response = await _httpClient.PostAsJsonAsync("/api/v1/collections", payload);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ChromaCollectionResponse>();
                    _collectionId = result?.Id;
                    _logger.LogInformation(
                        "ChromaDB collection {Collection} ready with ID {Id}",
                        _collectionName, _collectionId);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to create ChromaDB collection: {Error}", error);
                    throw new InvalidOperationException($"Failed to initialize ChromaDB collection: {error}");
                }

                _initialized = true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to ChromaDB server");
                throw;
            }
        }

        /// <summary>
        /// Queries for vectors similar to the provided embedding using ChromaDB's
        /// nearest neighbor search with cosine distance.
        /// </summary>
        public async Task<IEnumerable<string>> QuerySimilarAsync(float[] embedding, float threshold)
        {
            if (!_initialized) await InitializeAsync();
            if (_collectionId == null) return Enumerable.Empty<string>();

            try
            {
                var queryPayload = new
                {
                    query_embeddings = new[] { embedding },
                    n_results = 10,
                    include = new[] { "documents", "distances", "metadatas" }
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"/api/v1/collections/{_collectionId}/query",
                    queryPayload);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("ChromaDB query failed with status {Status}", response.StatusCode);
                    return Enumerable.Empty<string>();
                }

                var result = await response.Content.ReadFromJsonAsync<ChromaQueryResponse>();

                var values = new List<string>();
                if (result?.Documents != null && result.Distances != null)
                {
                    for (int i = 0; i < result.Documents.Count; i++)
                    {
                        var docs = result.Documents[i];
                        var dists = result.Distances[i];

                        for (int j = 0; j < docs.Count; j++)
                        {
                            // ChromaDB cosine distance = 1 - similarity
                            var similarity = 1.0f - dists[j];
                            if (similarity >= threshold && docs[j] != null)
                            {
                                values.Add(docs[j]!);
                            }
                        }
                    }
                }

                _logger.LogDebug(
                    "ChromaDB query returned {Count} results above threshold {Threshold}",
                    values.Count, threshold);

                return values;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying ChromaDB for similar vectors");
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Saves a document with its embedding to the ChromaDB collection.
        /// </summary>
        public async Task SaveDocumentAsync(string key, Dictionary<string, object> document)
        {
            if (!_initialized) await InitializeAsync();
            if (_collectionId == null) return;

            try
            {
                var embedding = ExtractEmbedding(document);
                if (embedding == null)
                {
                    _logger.LogWarning("Document {Key} has no valid embedding, skipping ChromaDB upsert", key);
                    return;
                }

                var value = document.TryGetValue("value", out var v) ? v?.ToString() ?? string.Empty : string.Empty;

                // Build metadata from non-embedding fields
                var metadata = new Dictionary<string, string>();
                foreach (var kvp in document)
                {
                    if (kvp.Key == "embedding") continue;
                    metadata[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
                }

                var upsertPayload = new
                {
                    ids = new[] { key },
                    embeddings = new[] { embedding },
                    documents = new[] { value },
                    metadatas = new[] { metadata }
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"/api/v1/collections/{_collectionId}/upsert",
                    upsertPayload);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Document {Key} saved to ChromaDB collection", key);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to upsert document {Key} in ChromaDB: {Error}", key, error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving document {Key} to ChromaDB", key);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a document value from ChromaDB by its ID.
        /// </summary>
        public async Task<string> GetDocumentValueAsync(string key, string jsonPath)
        {
            if (!_initialized) await InitializeAsync();
            if (_collectionId == null) return string.Empty;

            try
            {
                var getPayload = new
                {
                    ids = new[] { key },
                    include = new[] { "documents", "metadatas" }
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"/api/v1/collections/{_collectionId}/get",
                    getPayload);

                if (!response.IsSuccessStatusCode)
                    return string.Empty;

                var result = await response.Content.ReadFromJsonAsync<ChromaGetResponse>();

                var fieldName = jsonPath.TrimStart('$', '.');

                // First check metadata for the field
                if (result?.Metadatas != null && result.Metadatas.Count > 0)
                {
                    var meta = result.Metadatas[0];
                    if (meta != null && meta.TryGetValue(fieldName, out var metaValue))
                    {
                        return metaValue ?? string.Empty;
                    }
                }

                // Fall back to document content for "value" field
                if (fieldName == "value" && result?.Documents != null && result.Documents.Count > 0)
                {
                    return result.Documents[0] ?? string.Empty;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document value from ChromaDB for key {Key}", key);
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
        /// Response model for ChromaDB collection creation.
        /// </summary>
        internal class ChromaCollectionResponse
        {
            /// <summary>Collection unique identifier.</summary>
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            /// <summary>Collection name.</summary>
            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }

        /// <summary>
        /// Response model for ChromaDB query operation.
        /// </summary>
        internal class ChromaQueryResponse
        {
            /// <summary>Nested list of matching document contents.</summary>
            [JsonPropertyName("documents")]
            public List<List<string?>>? Documents { get; set; }

            /// <summary>Nested list of distances (cosine distance = 1 - similarity).</summary>
            [JsonPropertyName("distances")]
            public List<List<float>>? Distances { get; set; }

            /// <summary>Nested list of metadata dictionaries.</summary>
            [JsonPropertyName("metadatas")]
            public List<List<Dictionary<string, string>?>>? Metadatas { get; set; }
        }

        /// <summary>
        /// Response model for ChromaDB get operation.
        /// </summary>
        internal class ChromaGetResponse
        {
            /// <summary>List of document contents.</summary>
            [JsonPropertyName("documents")]
            public List<string?>? Documents { get; set; }

            /// <summary>List of metadata dictionaries.</summary>
            [JsonPropertyName("metadatas")]
            public List<Dictionary<string, string>?>? Metadatas { get; set; }
        }
    }
}
