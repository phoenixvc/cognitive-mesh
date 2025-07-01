using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.Extensions.Options;

namespace CognitiveMesh.MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// Hybrid implementation of the mesh memory store that combines DuckDB and Redis.
    /// Uses Redis for fast, in-memory operations and DuckDB for persistence and complex queries.
    /// </summary>
    public class HybridMemoryStore : IMeshMemoryStore
    {
        private readonly DuckDbMemoryStore _duckDbStore;
        private readonly RedisVectorMemoryStore _redisStore;
        private readonly ILogger<HybridMemoryStore> _logger;
        private readonly bool _preferRedisForRetrieval;
        private bool _initialized = false;

        public HybridMemoryStore(
            DuckDbMemoryStore duckDbStore, 
            RedisVectorMemoryStore redisStore, 
            ILogger<HybridMemoryStore> logger,
            bool preferRedisForRetrieval = true)
        {
            _duckDbStore = duckDbStore ?? throw new ArgumentNullException(nameof(duckDbStore));
            _redisStore = redisStore ?? throw new ArgumentNullException(nameof(redisStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _preferRedisForRetrieval = preferRedisForRetrieval;
        }

        /// <summary>
        /// Initializes both storage systems.
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                if (_initialized)
                    return;

                _logger.LogInformation("Initializing hybrid memory store");
                
                // Initialize both stores
                await Task.WhenAll(
                    _duckDbStore.InitializeAsync(),
                    _redisStore.InitializeAsync()
                );
                
                _initialized = true;
                _logger.LogInformation("Hybrid memory store initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing hybrid memory store");
                throw;
            }
        }

        /// <summary>
        /// Saves context data to both stores.
        /// </summary>
        public async Task SaveContextAsync(string sessionId, string key, string value)
        {
            if (!_initialized)
                await InitializeAsync();

            try
            {
                _logger.LogDebug("Saving context to hybrid store for session {SessionId}, key {Key}", sessionId, key);
                
                // Save to both stores concurrently
                await Task.WhenAll(
                    _redisStore.SaveContextAsync(sessionId, key, value),
                    _duckDbStore.SaveContextAsync(sessionId, key, value)
                );
                
                _logger.LogDebug("Context saved successfully to hybrid store");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving context to hybrid store");
                throw;
            }
        }

        /// <summary>
        /// Retrieves context data, preferring Redis for speed if configured.
        /// </summary>
        public async Task<string> GetContextAsync(string sessionId, string key)
        {
            if (!_initialized)
                await InitializeAsync();

            try
            {
                _logger.LogDebug("Retrieving context from hybrid store for session {SessionId}, key {Key}", sessionId, key);
                
                string value = null;
                
                // Try primary store first
                if (_preferRedisForRetrieval)
                {
                    value = await _redisStore.GetContextAsync(sessionId, key);
                    if (value != null)
                    {
                        return value;
    }
                    
                    // Fall back to DuckDB if not found in Redis
                    _logger.LogDebug("Context not found in Redis, falling back to DuckDB");
                    return await _duckDbStore.GetContextAsync(sessionId, key);
}
                else
                {
                    value = await _duckDbStore.GetContextAsync(sessionId, key);
                    if (value != null)
                    {
                        return value;
                    }
                    
                    // Fall back to Redis if not found in DuckDB
                    _logger.LogDebug("Context not found in DuckDB, falling back to Redis");
                    return await _redisStore.GetContextAsync(sessionId, key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving context from hybrid store");
                throw;
            }
        }

        /// <summary>
        /// Finds context values similar to the provided embedding, preferring Redis for vector search.
        /// </summary>
        public async Task<IEnumerable<string>> QuerySimilarAsync(string embedding, float threshold)
        {
            if (!_initialized)
                await InitializeAsync();

            try
            {
                _logger.LogDebug("Querying similar embeddings from hybrid store with threshold {Threshold}", threshold);
                
                // Prefer Redis for vector similarity search as it's typically faster for this operation
                IEnumerable<string> results = await _redisStore.QuerySimilarAsync(embedding, threshold);
                
                // If no results from Redis or insufficient results, supplement with DuckDB
                if (results == null || !results.Any())
                {
                    _logger.LogDebug("No similar embeddings found in Redis, falling back to DuckDB");
                    results = await _duckDbStore.QuerySimilarAsync(embedding, threshold);
                }
                
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying similar embeddings from hybrid store");
                throw;
            }
        }
    }
}