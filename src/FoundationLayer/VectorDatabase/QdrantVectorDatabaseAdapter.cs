namespace FoundationLayer.VectorDatabase
{
    /// <summary>
    /// Qdrant implementation of the IVectorDatabaseAdapter interface
    /// </summary>
    public class QdrantVectorDatabaseAdapter : IVectorDatabaseAdapter, IDisposable
    {
        private readonly ILogger<QdrantVectorDatabaseAdapter> _logger;
        private QdrantClient _qdrantClient;
        private bool _disposed = false;
        private readonly string _host;
        private readonly int _port;
        private readonly bool _useTls;
        private readonly string _apiKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="QdrantVectorDatabaseAdapter"/> class.
        /// </summary>
        /// <param name="host">The Qdrant server host.</param>
        /// <param name="port">The Qdrant server port.</param>
        /// <param name="useTls">Whether to use TLS for the connection.</param>
        /// <param name="apiKey">Optional API key for authentication.</param>
        /// <param name="logger">The logger instance.</param>
        public QdrantVectorDatabaseAdapter(
            string host = "localhost",
            int port = 6334,
            bool useTls = false,
            string apiKey = null,
            ILogger<QdrantVectorDatabaseAdapter> logger = null)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _port = port;
            _useTls = useTls;
            _apiKey = apiKey;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var builder = new QdrantClientBuilder(_host)
                    .OnPort(_port);

                if (_useTls)
                {
                    builder = builder.UseHttps();
                }

                if (!string.IsNullOrEmpty(_apiKey))
                {
                    builder = builder.WithApiKey(_apiKey);
                }

                _qdrantClient = builder.Build();

                // Test the connection
                await _qdrantClient.HealthCheckAsync(cancellationToken);
                _logger?.LogInformation("Successfully connected to Qdrant at {Host}:{Port}", _host, _port);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to connect to Qdrant at {Host}:{Port}", _host, _port);
                throw new InvalidOperationException($"Failed to connect to Qdrant: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _qdrantClient?.Dispose();
                _qdrantClient = null;
                _logger?.LogInformation("Disconnected from Qdrant");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error disconnecting from Qdrant");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task CreateCollectionAsync(string collectionName, int vectorSize, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(collectionName));

            if (vectorSize <= 0)
                throw new ArgumentException("Vector size must be greater than zero.", nameof(vectorSize));

            try
            {
                await _qdrantClient.CreateCollectionAsync(
                    collectionName,
                    new VectorParams
                    {
                        Size = (ulong)vectorSize,
                        Distance = Distance.Cosine
                    },
                    cancellationToken: cancellationToken);

                _logger?.LogInformation("Created collection {CollectionName} with vector size {VectorSize}", collectionName, vectorSize);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to create collection {CollectionName}", collectionName);
                throw new InvalidOperationException($"Failed to create collection {collectionName}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(collectionName));

            try
            {
                await _qdrantClient.DeleteCollectionAsync(collectionName, cancellationToken);
                _logger?.LogInformation("Deleted collection {CollectionName}", collectionName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to delete collection {CollectionName}", collectionName);
                throw new InvalidOperationException($"Failed to delete collection {collectionName}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> InsertVectorsAsync<T>(
            string collectionName,
            IEnumerable<T> items,
            Func<T, float[]> vectorSelector,
            Func<T, string> idSelector = null,
            CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(collectionName));

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (vectorSelector == null)
                throw new ArgumentNullException(nameof(vectorSelector));

            var itemList = items.ToList();
            if (!itemList.Any())
                return Enumerable.Empty<string>();

            var points = new List<PointStruct>();
            var ids = new List<string>();

            foreach (var item in itemList)
            {
                var vector = vectorSelector(item);
                if (vector == null)
                {
                    _logger?.LogWarning("Vector selector returned null for an item, skipping");
                    continue;
                }

                var id = idSelector?.Invoke(item) ?? Guid.NewGuid().ToString();
                ids.Add(id);

                var point = new PointStruct
                {
                    Id = new PointId { Uuid = id },
                    Vectors = new Vectors { Vector = new() { Data = { vector } } },
                    Payload = 
                    {
                        ["item"] = System.Text.Json.JsonSerializer.SerializeToNode(item)
                    }
                };

                points.Add(point);
            }

            if (!points.Any())
                return Enumerable.Empty<string>();

            try
            {
                await _qdrantClient.UpsertAsync(collectionName, points, cancellationToken: cancellationToken);
                _logger?.LogDebug("Upserted {Count} vectors to collection {CollectionName}", points.Count, collectionName);
                return ids;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to insert vectors into collection {CollectionName}", collectionName);
                throw new InvalidOperationException($"Failed to insert vectors into collection {collectionName}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<(string Id, float Score)>> SearchVectorsAsync(
            string collectionName,
            float[] vector,
            int limit = 10,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(collectionName));

            if (vector == null || vector.Length == 0)
                throw new ArgumentException("Vector cannot be null or empty.", nameof(vector));

            if (limit <= 0)
                throw new ArgumentException("Limit must be greater than zero.", nameof(limit));

            try
            {
                var searchResult = await _qdrantClient.SearchAsync(
                    collectionName,
                    vector,
                    limit: (ulong)limit,
                    cancellationToken: cancellationToken);

                return searchResult.Select(r => (r.Id.Uuid, (float)r.Score));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to search vectors in collection {CollectionName}", collectionName);
                throw new InvalidOperationException($"Failed to search vectors in collection {collectionName}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteVectorsAsync(
            string collectionName,
            IEnumerable<string> vectorIds,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(collectionName));

            if (vectorIds == null)
                throw new ArgumentNullException(nameof(vectorIds));

            var ids = vectorIds.ToList();
            if (!ids.Any())
                return;

            try
            {
                await _qdrantClient.DeleteAsync(
                    collectionName,
                    points: ids.Select(id => new PointId { Uuid = id }),
                    cancellationToken: cancellationToken);

                _logger?.LogDebug("Deleted {Count} vectors from collection {CollectionName}", ids.Count, collectionName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to delete vectors from collection {CollectionName}", collectionName);
                throw new InvalidOperationException($"Failed to delete vectors from collection {collectionName}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<T> GetVectorMetadataAsync<T>(
            string collectionName,
            string vectorId,
            CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(collectionName));

            if (string.IsNullOrWhiteSpace(vectorId))
                throw new ArgumentException("Vector ID cannot be null or whitespace.", nameof(vectorId));

            try
            {
                var point = await _qdrantClient.RetrieveAsync(
                    collectionName,
                    new PointId { Uuid = vectorId },
                    withPayload: true,
                    withVectors: false,
                    cancellationToken: cancellationToken);

                if (point == null)
                    return null;

                if (point.Payload.TryGetValue("item", out var payloadNode) && payloadNode != null)
                {
                    return payloadNode.Deserialize<T>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get vector metadata for ID {VectorId} in collection {CollectionName}", vectorId, collectionName);
                throw new InvalidOperationException($"Failed to get vector metadata for ID {vectorId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task UpsertVectorsAsync(
            string indexName,
            IEnumerable<float[]> vectors,
            IEnumerable<string> ids,
            IDictionary<string, object> metadata = null)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name cannot be null or whitespace.", nameof(indexName));

            var vectorsList = vectors?.ToList() ?? throw new ArgumentNullException(nameof(vectors));
            var idsList = ids?.ToList() ?? throw new ArgumentNullException(nameof(ids));

            if (vectorsList.Count != idsList.Count)
                throw new ArgumentException("The number of vectors must match the number of IDs.");

            var points = new List<PointStruct>();
            for (int i = 0; i < vectorsList.Count; i++)
            {
                var point = new PointStruct
                {
                    Id = new PointId { Uuid = idsList[i] },
                    Vectors = new Vectors { Vector = new() { Data = { vectorsList[i] } } }
                };

                if (metadata != null)
                {
                    foreach (var kvp in metadata)
                    {
                        point.Payload[kvp.Key] = System.Text.Json.JsonSerializer.SerializeToNode(kvp.Value);
                    }
                }

                points.Add(point);
            }

            await _qdrantClient.UpsertAsync(indexName, points);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<(string Id, float Score)>> QuerySimilarAsync(
            string indexName,
            float[] queryVector,
            int topK = 10,
            string filter = null)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name cannot be null or whitespace.", nameof(indexName));

            Filter qdrantFilter = null;
            if (!string.IsNullOrWhiteSpace(filter))
            {
                // Note: This is a simplified filter implementation
                // In a real implementation, you would need to parse the filter string
                // and convert it to Qdrant's filter format
                qdrantFilter = new Filter();
                // Add filter conditions based on the filter string
            }

            var searchResult = await _qdrantClient.SearchAsync(
                indexName,
                queryVector,
                limit: (ulong)topK,
                filter: qdrantFilter);

            return searchResult.Select(r => (r.Id.Uuid, (float)r.Score));
        }

        /// <inheritdoc/>
        public async Task CreateIndexAsync(
            string indexName,
            int dimension,
            string metricType = "COSINE")
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name cannot be null or whitespace.", nameof(indexName));

            if (dimension <= 0)
                throw new ArgumentException("Dimension must be greater than zero.", nameof(dimension));

            var distance = metricType?.ToUpper() switch
            {
                "EUCLID" => Distance.Euclid,
                "DOT" => Distance.Dot,
                _ => Distance.Cosine
            };

            await _qdrantClient.CreateCollectionAsync(
                indexName,
                new VectorParams
                {
                    Size = (ulong)dimension,
                    Distance = distance
                });
        }

        /// <inheritdoc/>
        public async Task<long> GetVectorCountAsync(string indexName)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name cannot be null or whitespace.", nameof(indexName));

            var collectionInfo = await _qdrantClient.GetCollectionInfoAsync(indexName);
            return (long)collectionInfo.VectorsCount;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _qdrantClient?.Dispose();
                }
                _disposed = true;
            }
        }

        ~QdrantVectorDatabaseAdapter()
        {
            Dispose(false);
        }
    }
}
