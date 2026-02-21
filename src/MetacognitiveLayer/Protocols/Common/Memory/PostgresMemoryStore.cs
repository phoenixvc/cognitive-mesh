using System.Text.Json;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// PostgreSQL-based implementation of <see cref="IMeshMemoryStore"/> with pgvector
    /// extension support for production-grade vector similarity search. Provides ACID
    /// transactions, MVCC concurrency, and horizontal read scaling.
    /// </summary>
    public class PostgresMemoryStore : IMeshMemoryStore, IVectorSearchProvider
    {
        private readonly string _connectionString;
        private readonly int _vectorDimension;
        private readonly ILogger<PostgresMemoryStore> _logger;
        private bool _initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresMemoryStore"/> class.
        /// </summary>
        /// <param name="connectionString">PostgreSQL connection string.</param>
        /// <param name="vectorDimension">Dimension of embedding vectors.</param>
        /// <param name="logger">Logger instance for structured logging.</param>
        public PostgresMemoryStore(
            string connectionString,
            int vectorDimension,
            ILogger<PostgresMemoryStore> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _vectorDimension = vectorDimension;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes the database schema including the pgvector extension,
        /// context table, and embeddings table with HNSW index.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_initialized) return;

            _logger.LogInformation("Initializing PostgreSQL memory store with pgvector");

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Enable pgvector extension
            await using (var cmd = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS vector;", connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            // Create context table
            await using (var cmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS mesh_context (
                    id BIGSERIAL PRIMARY KEY,
                    session_id TEXT NOT NULL,
                    context_key TEXT NOT NULL,
                    context_value TEXT NOT NULL,
                    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                    UNIQUE(session_id, context_key)
                );
                CREATE INDEX IF NOT EXISTS idx_mesh_context_session_key
                    ON mesh_context(session_id, context_key);
            ", connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            // Create embeddings table with vector column
            await using (var cmd = new NpgsqlCommand($@"
                CREATE TABLE IF NOT EXISTS mesh_embeddings (
                    id BIGSERIAL PRIMARY KEY,
                    session_id TEXT NOT NULL,
                    context_key TEXT NOT NULL,
                    embedding vector({_vectorDimension}),
                    context_value TEXT,
                    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
                );
                CREATE INDEX IF NOT EXISTS idx_mesh_embeddings_session
                    ON mesh_embeddings(session_id);
            ", connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            // Create HNSW index for cosine similarity search
            try
            {
                await using var hnsw = new NpgsqlCommand(@"
                    CREATE INDEX IF NOT EXISTS idx_mesh_embeddings_hnsw
                        ON mesh_embeddings USING hnsw (embedding vector_cosine_ops)
                        WITH (m = 16, ef_construction = 64);
                ", connection);
                await hnsw.ExecuteNonQueryAsync();
                _logger.LogInformation("HNSW index created for pgvector embeddings");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not create HNSW index, falling back to sequential scan");
            }

            _initialized = true;
            _logger.LogInformation("PostgreSQL memory store initialized successfully");
        }

        /// <summary>
        /// Saves a context key-value pair using UPSERT semantics.
        /// Automatically detects and stores embeddings.
        /// </summary>
        public async Task SaveContextAsync(string sessionId, string key, string value)
        {
            if (!_initialized) await InitializeAsync();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Upsert context
                await using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO mesh_context (session_id, context_key, context_value, updated_at)
                    VALUES (@sessionId, @key, @value, NOW())
                    ON CONFLICT (session_id, context_key)
                    DO UPDATE SET context_value = @value, updated_at = NOW();
                ", connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@sessionId", sessionId);
                    cmd.Parameters.AddWithValue("@key", key);
                    cmd.Parameters.AddWithValue("@value", value);
                    await cmd.ExecuteNonQueryAsync();
                }

                // Detect and store embeddings
                if (key.Contains("embedding", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var embedding = JsonSerializer.Deserialize<float[]>(value);
                        if (embedding != null && embedding.Length == _vectorDimension)
                        {
                            var vectorStr = $"[{string.Join(",", embedding)}]";
                            await using var embCmd = new NpgsqlCommand(@"
                                INSERT INTO mesh_embeddings (session_id, context_key, embedding, context_value)
                                VALUES (@sessionId, @key, @embedding::vector, @value);
                            ", connection, transaction);
                            embCmd.Parameters.AddWithValue("@sessionId", sessionId);
                            embCmd.Parameters.AddWithValue("@key", key);
                            embCmd.Parameters.AddWithValue("@embedding", vectorStr);
                            embCmd.Parameters.AddWithValue("@value", value);
                            await embCmd.ExecuteNonQueryAsync();
                        }
                    }
                    catch (JsonException)
                    {
                        _logger.LogWarning("Value for key {Key} could not be parsed as embedding", key);
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a context value by session ID and key.
        /// </summary>
        public async Task<string> GetContextAsync(string sessionId, string key)
        {
            if (!_initialized) await InitializeAsync();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand(@"
                SELECT context_value FROM mesh_context
                WHERE session_id = @sessionId AND context_key = @key;
            ", connection);
            cmd.Parameters.AddWithValue("@sessionId", sessionId);
            cmd.Parameters.AddWithValue("@key", key);

            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Queries for similar embeddings using pgvector's cosine distance operator
        /// with HNSW index acceleration.
        /// </summary>
        public async Task<IEnumerable<string>> QuerySimilarAsync(string embedding, float threshold)
        {
            if (!_initialized) await InitializeAsync();

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

            return await QuerySimilarCoreAsync(queryEmbedding, threshold);
        }

        /// <summary>
        /// Queries for similar vectors using pgvector's cosine distance with HNSW index.
        /// </summary>
        public async Task<IEnumerable<string>> QuerySimilarAsync(float[] embedding, float threshold)
        {
            if (!_initialized) await InitializeAsync();
            return await QuerySimilarCoreAsync(embedding, threshold);
        }

        /// <summary>
        /// Saves a document with its embedding to the embeddings table.
        /// </summary>
        public async Task SaveDocumentAsync(string key, Dictionary<string, object> document)
        {
            if (!_initialized) await InitializeAsync();

            float[]? embedding = ExtractEmbedding(document);
            if (embedding == null) return;

            var value = document.TryGetValue("value", out var v) ? v?.ToString() ?? string.Empty : string.Empty;
            var sessionId = document.TryGetValue("session_id", out var s) ? s?.ToString() ?? string.Empty : string.Empty;

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var vectorStr = $"[{string.Join(",", embedding)}]";
            await using var cmd = new NpgsqlCommand(@"
                INSERT INTO mesh_embeddings (session_id, context_key, embedding, context_value)
                VALUES (@sessionId, @key, @embedding::vector, @value);
            ", connection);
            cmd.Parameters.AddWithValue("@sessionId", sessionId);
            cmd.Parameters.AddWithValue("@key", key);
            cmd.Parameters.AddWithValue("@embedding", vectorStr);
            cmd.Parameters.AddWithValue("@value", value);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Retrieves a document field value by key.
        /// </summary>
        public async Task<string> GetDocumentValueAsync(string key, string jsonPath)
        {
            if (!_initialized) await InitializeAsync();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand(@"
                SELECT context_value FROM mesh_embeddings
                WHERE context_key = @key
                ORDER BY created_at DESC
                LIMIT 1;
            ", connection);
            cmd.Parameters.AddWithValue("@key", key);

            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString() ?? string.Empty;
        }

        private async Task<IEnumerable<string>> QuerySimilarCoreAsync(float[] embedding, float threshold)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // pgvector cosine distance: 1 - cosine_similarity
            // threshold is similarity, so distance <= (1 - threshold)
            var maxDistance = 1.0f - threshold;
            var vectorStr = $"[{string.Join(",", embedding)}]";

            await using var cmd = new NpgsqlCommand(@"
                SELECT context_value, 1 - (embedding <=> @embedding::vector) AS similarity
                FROM mesh_embeddings
                WHERE embedding IS NOT NULL
                AND (embedding <=> @embedding::vector) <= @maxDistance
                ORDER BY embedding <=> @embedding::vector
                LIMIT 10;
            ", connection);
            cmd.Parameters.AddWithValue("@embedding", vectorStr);
            cmd.Parameters.AddWithValue("@maxDistance", (double)maxDistance);

            var results = new List<string>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var value = reader.GetString(0);
                if (!string.IsNullOrEmpty(value))
                    results.Add(value);
            }

            return results;
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
    }
}
