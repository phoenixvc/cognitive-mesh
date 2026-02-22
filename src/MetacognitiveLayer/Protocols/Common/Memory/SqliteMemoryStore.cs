using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// SQLite-based implementation of <see cref="IMeshMemoryStore"/> that provides
    /// ACID-compliant persistent storage with embedded vector similarity search.
    /// Replaces DuckDB to eliminate native library dependencies.
    /// </summary>
    public class SqliteMemoryStore : IMeshMemoryStore
    {
        private readonly string _connectionString;
        private readonly ILogger<SqliteMemoryStore> _logger;
        private bool _initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteMemoryStore"/> class.
        /// </summary>
        /// <param name="dbPath">Path to the SQLite database file.</param>
        /// <param name="logger">Logger instance for structured logging.</param>
        public SqliteMemoryStore(string dbPath, ILogger<SqliteMemoryStore> logger)
        {
            _connectionString = $"Data Source={dbPath ?? throw new ArgumentNullException(nameof(dbPath))}";
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes the SQLite database, creating tables and indexes if they do not exist.
        /// Uses WAL journal mode for concurrent read support.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_initialized) return;

            _logger.LogInformation("Initializing SQLite memory store");

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Enable WAL mode for concurrent reads
            await using (var walCmd = connection.CreateCommand())
            {
                walCmd.CommandText = "PRAGMA journal_mode=WAL;";
                await walCmd.ExecuteNonQueryAsync();
            }

            // Create context table with UPSERT support
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS context (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        session_id TEXT NOT NULL,
                        context_key TEXT NOT NULL,
                        context_value TEXT NOT NULL,
                        timestamp INTEGER NOT NULL DEFAULT (strftime('%s', 'now')),
                        UNIQUE(session_id, context_key)
                    );
                    CREATE INDEX IF NOT EXISTS idx_context_session_key
                        ON context(session_id, context_key);
                ";
                await cmd.ExecuteNonQueryAsync();
            }

            // Create embeddings table for vector storage
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS embeddings (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        session_id TEXT NOT NULL,
                        context_key TEXT NOT NULL,
                        embedding TEXT NOT NULL,
                        timestamp INTEGER NOT NULL DEFAULT (strftime('%s', 'now'))
                    );
                    CREATE INDEX IF NOT EXISTS idx_embeddings_session
                        ON embeddings(session_id);
                ";
                await cmd.ExecuteNonQueryAsync();
            }

            _initialized = true;
            _logger.LogInformation("SQLite memory store initialized successfully");
        }

        /// <summary>
        /// Saves a context key-value pair for a session. Uses UPSERT semantics
        /// to update existing entries. If the value contains an embedding (float array),
        /// it is also stored in the embeddings table.
        /// </summary>
        public async Task SaveContextAsync(string sessionId, string key, string value)
        {
            if (!_initialized) await InitializeAsync();

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Upsert context value
                await using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = (SqliteTransaction)transaction;
                    cmd.CommandText = @"
                        INSERT INTO context (session_id, context_key, context_value, timestamp)
                        VALUES (@sessionId, @key, @value, strftime('%s', 'now'))
                        ON CONFLICT(session_id, context_key)
                        DO UPDATE SET context_value = @value, timestamp = strftime('%s', 'now');
                    ";
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
                        if (embedding != null)
                        {
                            await using var embCmd = connection.CreateCommand();
                            embCmd.Transaction = (SqliteTransaction)transaction;
                            embCmd.CommandText = @"
                                INSERT INTO embeddings (session_id, context_key, embedding, timestamp)
                                VALUES (@sessionId, @key, @embedding, strftime('%s', 'now'));
                            ";
                            embCmd.Parameters.AddWithValue("@sessionId", sessionId);
                            embCmd.Parameters.AddWithValue("@key", key);
                            embCmd.Parameters.AddWithValue("@embedding", JsonSerializer.Serialize(embedding));
                            await embCmd.ExecuteNonQueryAsync();
                        }
                    }
                    catch (JsonException)
                    {
                        _logger.LogWarning(
                            "Value for key {Key} appears to be an embedding but could not be deserialized",
                            key);
                    }
                }

                await transaction.CommitAsync();
                _logger.LogDebug("Context saved for session {SessionId}, key {Key}", sessionId, key);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a context value for a session and key.
        /// </summary>
        public async Task<string> GetContextAsync(string sessionId, string key)
        {
            if (!_initialized) await InitializeAsync();

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT context_value FROM context
                WHERE session_id = @sessionId AND context_key = @key;
            ";
            cmd.Parameters.AddWithValue("@sessionId", sessionId);
            cmd.Parameters.AddWithValue("@key", key);

            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Queries for context values with embeddings similar to the provided embedding vector.
        /// Uses cosine similarity computed in C# for compatibility without native extensions.
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
                _logger.LogWarning("Could not parse query embedding from string");
                return Enumerable.Empty<string>();
            }

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Load all embeddings and compute cosine similarity in C#
            var similarities = new List<(float similarity, string value)>();

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT e.embedding, c.context_value
                FROM embeddings e
                INNER JOIN context c ON e.session_id = c.session_id AND e.context_key = c.context_key;
            ";

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var storedEmbeddingJson = reader.GetString(0);
                var contextValue = reader.GetString(1);

                try
                {
                    var storedEmbedding = JsonSerializer.Deserialize<float[]>(storedEmbeddingJson);
                    if (storedEmbedding != null)
                    {
                        var similarity = CalculateCosineSimilarity(queryEmbedding, storedEmbedding);
                        if (similarity >= threshold)
                        {
                            similarities.Add((similarity: similarity, value: contextValue));
                        }
                    }
                }
                catch (JsonException)
                {
                    // Skip malformed embeddings
                }
            }

            return similarities
                .OrderByDescending(s => s.similarity)
                .Select(s => s.value);
        }

        /// <summary>
        /// Computes cosine similarity between two embedding vectors.
        /// </summary>
        internal static float CalculateCosineSimilarity(float[] a, float[] b)
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
    }
}
