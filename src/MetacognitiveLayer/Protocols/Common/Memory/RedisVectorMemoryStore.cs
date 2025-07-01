using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using StackExchange.Redis;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using NRedisStack.Search.Literals.Enums;

namespace CognitiveMesh.MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// Redis-based implementation of the mesh memory store.
    /// Uses Redis Vector Search for high-performance embedding similarity searches.
    /// </summary>
    public class RedisVectorMemoryStore : IMeshMemoryStore
    {
        private readonly string _redisConnectionString;
        private readonly ILogger<RedisVectorMemoryStore> _logger;
        private readonly string _indexName = "mesh_embeddings";
        private readonly int _defaultVectorDimension = 768;
        private ConnectionMultiplexer _redis;
        private IDatabase _db;
        private bool _initialized = false;

        public RedisVectorMemoryStore(string redisConnectionString, ILogger<RedisVectorMemoryStore> logger)
        {
            _redisConnectionString = redisConnectionString ?? throw new ArgumentNullException(nameof(redisConnectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes the Redis connection and creates required vector indices if they don't exist.
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                if (_initialized)
                    return;

                _logger.LogInformation("Initializing Redis vector memory store");
                
                // Connect to Redis
                _redis = await ConnectionMultiplexer.ConnectAsync(_redisConnectionString);
                _db = _redis.GetDatabase();

                // Check if index exists
                var searchCommands = _db.FT();
                try
                {
                    await searchCommands.InfoAsync(_indexName);
                    _logger.LogInformation("Redis vector index {IndexName} already exists", _indexName);
                }
                catch
                {
                    // Index doesn't exist, create it
                    _logger.LogInformation("Creating Redis vector index {IndexName}", _indexName);

                    // Create schema for the index
                    var schema = new Schema()
                        .AddTextField("$.session_id", "session_id")
                        .AddTextField("$.key", "key")
                        .AddTextField("$.value", "value")
                        .AddVectorField("$.embedding", VectorField.VectorAlgo.HNSW, new Dictionary<string, object>
                        {
                            { "TYPE", "FLOAT32" },
                            { "DIM", _defaultVectorDimension },
                            { "DISTANCE_METRIC", "COSINE" }
                        }, "embedding");

                    // Create the index with JSON data
                    await searchCommands.CreateAsync(_indexName, new FTCreateParams()
                        .On(IndexDataType.JSON)
                        .Prefix("mesh:"),
                        schema);
                }

                _initialized = true;
                _logger.LogInformation("Redis vector memory store initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Redis vector memory store");
                throw;
            }
        }

        /// <summary>
        /// Saves context data for a session.
        /// </summary>
        public async Task SaveContextAsync(string sessionId, string key, string value)
        {
            if (!_initialized)
                await InitializeAsync();

            try
            {
                _logger.LogDebug("Saving context for session {SessionId}, key {Key}", sessionId, key);

                // Create Redis key
                string redisKey = $"mesh:{sessionId}:{key}";
                
                // Create document to store
                var document = new
                {
                    session_id = sessionId,
                    key = key,
                    value = value,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                // Store in Redis using JSON
                var jsonCommands = _db.JSON();
                await jsonCommands.SetAsync(redisKey, "$", JsonConvert.SerializeObject(document));

                // If this is an embedding, store with vector data
                if (key.Contains("embedding"))
            {
                    try
                    {
                        // Try to parse as vector data and store with embedding
                        var vectorData = JsonConvert.DeserializeObject<float[]>(value);
                        if (vectorData != null && vectorData.Length > 0)
        {
                            var docWithEmbedding = new
            {
                                session_id = sessionId,
                                key = key,
                                value = value,
                                embedding = vectorData,
                                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                            };

                            // Update with embedding field
                            await jsonCommands.SetAsync(redisKey, "$", JsonConvert.SerializeObject(docWithEmbedding));
                        }
                    }
            catch (Exception ex)
            {
                        _logger.LogWarning(ex, "Failed to parse embedding value for key {Key}, storing as plain text", key);
            }
        }
                
                _logger.LogDebug("Context saved successfully to Redis");
    }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving context to Redis");
                throw;
}
        }

        /// <summary>
        /// Retrieves context data for a session.
        /// </summary>
        public async Task<string> GetContextAsync(string sessionId, string key)
        {
            if (!_initialized)
                await InitializeAsync();

            try
            {
                _logger.LogDebug("Retrieving context for session {SessionId}, key {Key}", sessionId, key);

                // Create Redis key
                string redisKey = $"mesh:{sessionId}:{key}";
                
                // Get value from Redis JSON
                var jsonCommands = _db.JSON();
                var result = await jsonCommands.GetAsync(redisKey, "$.value");
                
                if (result.IsNull)
                    return null;
                
                return result[0].ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving context from Redis");
                throw;
            }
        }

        /// <summary>
        /// Finds context values similar to the provided embedding using Redis Vector Search.
        /// </summary>
        public async Task<IEnumerable<string>> QuerySimilarAsync(string embedding, float threshold)
        {
            if (!_initialized)
                await InitializeAsync();

            try
            {
                _logger.LogDebug("Querying similar embeddings with threshold {Threshold}", threshold);

                // Parse the embedding string into a float array
                var embeddingArray = JsonConvert.DeserializeObject<float[]>(embedding);
                if (embeddingArray == null || embeddingArray.Length == 0)
                {
                    throw new ArgumentException("Invalid embedding format", nameof(embedding));
                }

                var results = new List<string>();
                var searchCommands = _db.FT();

                // Use vector search with KNN
                var query = $"*=>[KNN {10} @embedding $vector AS score]";
                var parameters = new Dictionary<string, object>
                {
                    { "vector", embeddingArray }
                };

                // Execute the vector search
                var searchResult = await searchCommands.SearchAsync(
                    _indexName, 
                    new Query(query)
                        .AddParam("vector", embeddingArray)
                        .ReturnFields("value", "score")
                        .Dialect(2)
                );

                foreach (var doc in searchResult.Documents)
                {
                    // Get score and check against threshold
                    if (doc.TryGetValue("score", out var scoreObj) && 
                        double.TryParse(scoreObj.ToString(), out var score) && 
                        score >= threshold)
                    {
                        if (doc.TryGetValue("value", out var valueObj))
                        {
                            results.Add(valueObj.ToString());
                        }
                    }
                }

                _logger.LogDebug("Found {Count} similar embeddings", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying similar embeddings from Redis");
                throw;
            }
        }
    }
}