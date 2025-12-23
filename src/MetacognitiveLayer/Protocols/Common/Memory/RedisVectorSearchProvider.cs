using System.Text.Json;
using StackExchange.Redis;

namespace MetacognitiveLayer.Protocols.Common.Memory
{
    public class RedisVectorSearchProvider : IVectorSearchProvider
    {
        private readonly string _connectionString;
        private readonly ILogger<RedisVectorSearchProvider> _logger;
        private readonly string _indexName = "mesh_embeddings";
        private readonly int _vectorDim = 768;
        private ConnectionMultiplexer _redis;
        private IDatabase _db;
        private bool _initialized;

        public RedisVectorSearchProvider(string connectionString, ILogger<RedisVectorSearchProvider> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;

            _redis = await ConnectionMultiplexer.ConnectAsync(_connectionString);
            _db = _redis.GetDatabase();

            try
            {
                await _db.ExecuteAsync("FT.INFO", _indexName);
                _logger.LogInformation("Redis index {IndexName} already exists", _indexName);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("Unknown Index name"))
            {
                await CreateIndexAsync();
                _logger.LogInformation("Created new Redis index {IndexName}", _indexName);
            }

            _initialized = true;
        }

        private async Task CreateIndexAsync()
        {
            var args = new List<RedisValue>
            {
                _indexName,
                "ON", "JSON",
                "PREFIX", "1", "mesh:",
                "SCHEMA",
                "$.session_id", "AS", "session_id", "TEXT",
                "$.key", "AS", "key", "TEXT",
                "$.value", "AS", "value", "TEXT",
                "$.embedding", "AS", "embedding", "VECTOR", "HNSW", "6",
                "TYPE", "FLOAT32",
                "DIM", _vectorDim.ToString(),
                "DISTANCE_METRIC", "COSINE"
            };

            await _db.ExecuteAsync("FT.CREATE", args.ToArray());
        }

        public async Task SaveDocumentAsync(string key, Dictionary<string, object> document)
        {
            string json = JsonSerializer.Serialize(document);
            await _db.ExecuteAsync("JSON.SET", key, "$", json);
        }

        public async Task<string> GetDocumentValueAsync(string key, string jsonPath)
        {
            var result = await _db.ExecuteAsync("JSON.GET", key, jsonPath);
            return result.IsNull ? null : result.ToString();
        }

        public async Task<IEnumerable<string>> QuerySimilarAsync(float[] embedding, float threshold)
        {
            var vectorBytes = new byte[embedding.Length * sizeof(float)];
            Buffer.BlockCopy(embedding, 0, vectorBytes, 0, vectorBytes.Length);

            var args = new List<RedisValue>
            {
                _indexName,
                "*=>[KNN 10 @embedding $vector AS score]",
                "PARAMS", "2", "vector", vectorBytes,
                "RETURN", "2", "value", "score",
                "DIALECT", "2"
            };

            var raw = (RedisResult[])await _db.ExecuteAsync("FT.SEARCH", args.ToArray());

            var results = new List<string>();
            for (int i = 1; i < raw.Length; i += 2)
            {
                var fields = (RedisResult[])raw[i + 1];
                string value = null;
                double score = 0;

                for (int j = 0; j < fields.Length; j += 2)
                {
                    if (fields[j].ToString() == "value")
                        value = fields[j + 1].ToString();
                    if (fields[j].ToString() == "score" && double.TryParse(fields[j + 1].ToString(), out var s))
                        score = s;
                }

                if (score >= threshold && value != null)
                    results.Add(value);
            }

            return results;
        }
    }
}
