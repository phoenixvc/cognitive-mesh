using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// Hybrid implementation of the mesh memory store that combines a persistent store
    /// (SQLite or DuckDB) with a fast cache layer (Redis). Uses the cache for
    /// low-latency reads and dual-writes to both stores for durability.
    /// </summary>
    public class HybridMemoryStore : IMeshMemoryStore
    {
        private readonly IMeshMemoryStore _persistentStore;
        private readonly IMeshMemoryStore _cacheStore;
        private readonly ILogger<HybridMemoryStore> _logger;
        private readonly bool _preferCacheForRetrieval;
        private bool _initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="HybridMemoryStore"/> class.
        /// </summary>
        /// <param name="persistentStore">The durable store (SQLite, DuckDB).</param>
        /// <param name="cacheStore">The fast cache store (Redis).</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="preferCacheForRetrieval">When true, reads try cache first.</param>
        public HybridMemoryStore(
            IMeshMemoryStore persistentStore,
            IMeshMemoryStore cacheStore,
            ILogger<HybridMemoryStore> logger,
            bool preferCacheForRetrieval = true)
        {
            _persistentStore = persistentStore ?? throw new ArgumentNullException(nameof(persistentStore));
            _cacheStore = cacheStore ?? throw new ArgumentNullException(nameof(cacheStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _preferCacheForRetrieval = preferCacheForRetrieval;
        }

        /// <summary>
        /// Initializes both storage systems in parallel.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_initialized) return;

            try
            {
                _logger.LogInformation("Initializing hybrid memory store");

                await Task.WhenAll(
                    _persistentStore.InitializeAsync(),
                    _cacheStore.InitializeAsync()
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
        /// Saves context data to both stores concurrently.
        /// </summary>
        public async Task SaveContextAsync(string sessionId, string key, string value)
        {
            if (!_initialized) await InitializeAsync();

            try
            {
                _logger.LogDebug("Saving context to hybrid store for session {SessionId}, key {Key}", sessionId, key);

                await Task.WhenAll(
                    _cacheStore.SaveContextAsync(sessionId, key, value),
                    _persistentStore.SaveContextAsync(sessionId, key, value)
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
        /// Retrieves context data, trying the preferred store first then falling back.
        /// </summary>
        public async Task<string> GetContextAsync(string sessionId, string key)
        {
            if (!_initialized) await InitializeAsync();

            try
            {
                _logger.LogDebug("Retrieving context from hybrid store for session {SessionId}, key {Key}", sessionId, key);

                if (_preferCacheForRetrieval)
                {
                    var value = await _cacheStore.GetContextAsync(sessionId, key);
                    if (!string.IsNullOrEmpty(value))
                        return value;

                    _logger.LogDebug("Context not found in cache, falling back to persistent store");
                    return await _persistentStore.GetContextAsync(sessionId, key);
                }
                else
                {
                    var value = await _persistentStore.GetContextAsync(sessionId, key);
                    if (!string.IsNullOrEmpty(value))
                        return value;

                    _logger.LogDebug("Context not found in persistent store, falling back to cache");
                    return await _cacheStore.GetContextAsync(sessionId, key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving context from hybrid store");
                throw;
            }
        }

        /// <summary>
        /// Queries for similar embeddings, preferring the cache store for speed
        /// and falling back to the persistent store if needed.
        /// </summary>
        public async Task<IEnumerable<string>> QuerySimilarAsync(string embedding, float threshold)
        {
            if (!_initialized) await InitializeAsync();

            try
            {
                _logger.LogDebug("Querying similar embeddings from hybrid store with threshold {Threshold}", threshold);

                var results = await _cacheStore.QuerySimilarAsync(embedding, threshold);

                if (results == null || !results.Any())
                {
                    _logger.LogDebug("No similar embeddings found in cache, falling back to persistent store");
                    results = await _persistentStore.QuerySimilarAsync(embedding, threshold);
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
