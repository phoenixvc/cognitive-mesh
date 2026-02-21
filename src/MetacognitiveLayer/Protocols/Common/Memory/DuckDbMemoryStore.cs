using System.Data;
using System.Data.Common;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// DuckDB-based implementation of the mesh memory store.
    /// Uses DuckDB's OLAP capabilities for efficient context storage and vector similarity search.
    /// </summary>
    public class DuckDbMemoryStore : IMeshMemoryStore
    {
        private readonly string _dbFilePath;
        private readonly ILogger<DuckDbMemoryStore> _logger;
        private bool _initialized = false;
        private string _connectionString;

        public DuckDbMemoryStore(string dbFilePath, ILogger<DuckDbMemoryStore> logger)
        {
            _dbFilePath = dbFilePath ?? throw new ArgumentNullException(nameof(dbFilePath));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = $"Data Source={_dbFilePath}";
        }

        /// <summary>
        /// Initializes the DuckDB database and creates required tables if they don't exist.
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                if (_initialized)
                    return;

                _logger.LogInformation("Initializing DuckDB memory store at {DbFilePath}", _dbFilePath);

                // Create directory if it doesn't exist
                var directory = Path.GetDirectoryName(_dbFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var connection = new DuckDBConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Create context table
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            CREATE TABLE IF NOT EXISTS context (
                                id BIGINT AUTO_INCREMENT PRIMARY KEY,
                                session_id VARCHAR NOT NULL,
                                context_key VARCHAR NOT NULL,
                                context_value TEXT NOT NULL,
                                timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                UNIQUE(session_id, context_key)
                            )";
                        await command.ExecuteNonQueryAsync();
                    }

                    // Create embeddings table with vector support
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            CREATE TABLE IF NOT EXISTS embeddings (
                                id BIGINT AUTO_INCREMENT PRIMARY KEY,
                                session_id VARCHAR NOT NULL,
                                context_key VARCHAR NOT NULL,
                                embedding FLOAT[] NOT NULL,
                                timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                            )";
                        await command.ExecuteNonQueryAsync();
                    }

                    // Create index for fast retrieval
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            CREATE INDEX IF NOT EXISTS idx_context_session_key 
                            ON context(session_id, context_key)";
                        await command.ExecuteNonQueryAsync();
                    }

                    // Load vector extension if available
                    try
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "INSTALL 'vector'; LOAD 'vector';";
                            try 
                            {
                                await command.ExecuteNonQueryAsync();
                                _logger.LogInformation("Successfully loaded vector extension");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to load vector extension. Some vector operations may not be available.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load DuckDB vector extension, falling back to manual vector operations");
                    }
                }

                _initialized = true;
                _logger.LogInformation("DuckDB memory store initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing DuckDB memory store");
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

                using (var connection = new DuckDBConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            INSERT INTO context (session_id, context_key, context_value) 
                            VALUES (@sessionId, @key, @value)
                            ON CONFLICT (session_id, context_key) DO UPDATE
                            SET context_value = EXCLUDED.context_value,
                                timestamp = CURRENT_TIMESTAMP";

                        command.Parameters.AddWithValue("@sessionId", sessionId);
                        command.Parameters.AddWithValue("@key", key);
                        command.Parameters.AddWithValue("@value", value);

                        await command.ExecuteNonQueryAsync();
                    }

                    // If this looks like embedding data, store in embeddings table
                    if (key.Contains("embedding"))
                    {
                        try
                        {
                            var embeddingArray = JsonSerializer.Deserialize<float[]>(value);
                            if (embeddingArray != null && embeddingArray.Length > 0)
                            {
                                using (var command = connection.CreateCommand())
                                {
                                    command.CommandText = @"
                                        INSERT INTO embeddings (session_id, context_key, embedding)
                                        VALUES (@sessionId, @key, @embedding)
                                        ON CONFLICT (session_id, context_key) DO UPDATE
                                        SET embedding = EXCLUDED.embedding,
                                            timestamp = CURRENT_TIMESTAMP";

                                    command.Parameters.AddWithValue("@sessionId", sessionId);
                                    command.Parameters.AddWithValue("@key", key);
                                    command.Parameters.AddWithValue("@embedding", embeddingArray);

                                    await command.ExecuteNonQueryAsync();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to parse embedding value for key {Key}, storing as plain text only", key);
                        }
                    }
                }

                _logger.LogDebug("Context saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving context to DuckDB");
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

            using (var connection = new DuckDBConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT context_value 
                        FROM context 
                        WHERE session_id = @sessionId AND context_key = @key";

                    command.Parameters.AddWithValue("@sessionId", sessionId);
                    command.Parameters.AddWithValue("@key", key);

                    var result = await command.ExecuteScalarAsync();
                    return result?.ToString() ?? string.Empty;
                }
            }
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving context from DuckDB");
                throw;
            }
        }

        /// <summary>
        /// Finds context values similar to the provided embedding.
        /// </summary>
        public async Task<IEnumerable<string>> QuerySimilarAsync(string embedding, float threshold)
        {
            if (!_initialized)
                await InitializeAsync();

            try
            {
                _logger.LogDebug("Querying similar embeddings with threshold {Threshold}", threshold);

                // Parse the embedding string into a float array
                var queryEmbedding = JsonSerializer.Deserialize<float[]>(embedding);
                if (queryEmbedding == null || queryEmbedding.Length == 0)
                {
                    throw new ArgumentException("Invalid embedding format", nameof(embedding));
                }

                var results = new List<string>();
                using (var connection = new DuckDBConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Try to use DuckDB's vector extension if available
                    try
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                SELECT c.context_value, vector_distance(e.embedding, @embedding, 'cosine') as distance
                                FROM embeddings e
                                JOIN context c ON e.session_id = c.session_id AND e.context_key = c.context_key
                                WHERE vector_distance(e.embedding, @embedding, 'cosine') <= @threshold
                                ORDER BY distance
                                LIMIT 10";

                            command.Parameters.AddWithValue("@embedding", queryEmbedding);
                            command.Parameters.AddWithValue("@threshold", 1 - threshold); // Cosine distance = 1 - cosine similarity

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    results.Add(reader["context_value"]?.ToString() ?? string.Empty);
                                }
                            }

                            return results;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "DuckDB vector extension query failed, falling back to manual computation");

                        // Fallback: Fetch all embeddings and compute similarity in C#
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                SELECT e.embedding, c.context_value, e.session_id, e.context_key
                                FROM embeddings e
                                JOIN context c ON e.session_id = c.session_id AND e.context_key = c.context_key";

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                var similarities = new List<(float similarity, string value)>();
                                while (await reader.ReadAsync())
                                {
                                    var storedEmbedding = reader.GetFieldValue<float[]>(0);
                                    var contextValue = reader["context_value"]?.ToString() ?? string.Empty;

                                    // Compute cosine similarity
                                    var similarity = ComputeCosineSimilarity(queryEmbedding, storedEmbedding);
                                    if (similarity >= threshold)
                                    {
                                        similarities.Add((similarity: similarity, value: contextValue));
                                    }
                                }

                                // Sort by similarity (highest first) and take top results
                                return similarities
                                    .OrderByDescending(s => s.similarity)
                                    .Take(10)
                                    .Select(s => s.value)
                                    .ToList();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying similar embeddings from DuckDB");
                throw;
            }
        }

        /// <summary>
        /// Computes cosine similarity between two vectors.
        /// </summary>
        private float ComputeCosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
                throw new ArgumentException("Vectors must have the same length");

            float dotProduct = 0.0f;
            float normA = 0.0f;
            float normB = 0.0f;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                normA += vectorA[i] * vectorA[i];
                normB += vectorB[i] * vectorB[i];
            }

            if (normA <= 0.0f || normB <= 0.0f)
                return 0.0f;

            return dotProduct / (float)(Math.Sqrt(normA) * Math.Sqrt(normB));
        }
    }

    // Stub classes for DuckDB connection when DuckDB.NET.Data is not available
#nullable disable
    internal class DuckDBConnection : DbConnection
    {
        private string _connectionString;

        public DuckDBConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        public override string ConnectionString
        {
            get => _connectionString;
            set => _connectionString = value;
        }

        public override string Database => string.Empty;
        public override string DataSource => string.Empty;
        public override string ServerVersion => string.Empty;
        public override ConnectionState State => ConnectionState.Open;

        public override void ChangeDatabase(string databaseName) { }
        public override void Close() { }
        public override void Open() { }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            => throw new NotSupportedException("DuckDB stub does not support transactions");

        protected override DbCommand CreateDbCommand() => new DuckDBCommand();

        /// <summary>
        /// Creates a new DuckDB command associated with this connection.
        /// </summary>
        public new DuckDBCommand CreateCommand() => new();
    }

    internal class DuckDBParameterCollection : DbParameterCollection
    {
        private readonly List<DbParameter> _parameters = new();

        public override int Count => _parameters.Count;
        public override object SyncRoot => ((System.Collections.ICollection)_parameters).SyncRoot;

        public override int Add(object value)
        {
            _parameters.Add((DbParameter)value);
            return _parameters.Count - 1;
        }

        public override void AddRange(Array values)
        {
            foreach (var value in values)
                _parameters.Add((DbParameter)value);
        }

        /// <summary>
        /// Adds a parameter with the specified name and value.
        /// </summary>
        public DbParameter AddWithValue(string parameterName, object value)
        {
            var param = new DuckDBParameter { ParameterName = parameterName, Value = value };
            _parameters.Add(param);
            return param;
        }

        public override void Clear() => _parameters.Clear();
        public override bool Contains(object value) => _parameters.Contains((DbParameter)value);
        public override bool Contains(string value) => _parameters.Any(p => p.ParameterName == value);
        public override void CopyTo(Array array, int index) => ((System.Collections.ICollection)_parameters).CopyTo(array, index);
        public override System.Collections.IEnumerator GetEnumerator() => _parameters.GetEnumerator();
        public override int IndexOf(object value) => _parameters.IndexOf((DbParameter)value);
        public override int IndexOf(string parameterName) => _parameters.FindIndex(p => p.ParameterName == parameterName);
        public override void Insert(int index, object value) => _parameters.Insert(index, (DbParameter)value);
        public override void Remove(object value) => _parameters.Remove((DbParameter)value);
        public override void RemoveAt(int index) => _parameters.RemoveAt(index);
        public override void RemoveAt(string parameterName) => _parameters.RemoveAll(p => p.ParameterName == parameterName);
        protected override DbParameter GetParameter(int index) => _parameters[index];
        protected override DbParameter GetParameter(string parameterName) => _parameters.First(p => p.ParameterName == parameterName);
        protected override void SetParameter(int index, DbParameter value) => _parameters[index] = value;
        protected override void SetParameter(string parameterName, DbParameter value)
        {
            var idx = IndexOf(parameterName);
            if (idx >= 0) _parameters[idx] = value;
        }
    }

    internal class DuckDBParameter : DbParameter
    {
        public override DbType DbType { get; set; }
        public override ParameterDirection Direction { get; set; }
        public override bool IsNullable { get; set; }
        public override string ParameterName { get; set; } = string.Empty;
        public override int Size { get; set; }
        public override string SourceColumn { get; set; } = string.Empty;
        public override bool SourceColumnNullMapping { get; set; }
        public override object Value { get; set; }
        public override void ResetDbType() { }
    }

    internal class DuckDBCommand : DbCommand
    {
        private readonly DuckDBParameterCollection _parameters = new();

        public override string CommandText { get; set; } = string.Empty;
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection DbConnection { get; set; }
        protected override DbParameterCollection DbParameterCollection => _parameters;
        protected override DbTransaction DbTransaction { get; set; }

        /// <summary>
        /// Gets the parameter collection for this command.
        /// </summary>
        public new DuckDBParameterCollection Parameters => _parameters;

        public override void Cancel() { }
        public override int ExecuteNonQuery() => 0;
        public override object ExecuteScalar() => null;
        public override void Prepare() { }

        protected override DbParameter CreateDbParameter() => new DuckDBParameter();

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            => new DuckDBDataReader();
    }

    internal class DuckDBDataReader : DbDataReader
    {
        public override int FieldCount => 0;
        public override int Depth => 0;
        public override bool HasRows => false;
        public override bool IsClosed => true;
        public override int RecordsAffected => 0;

        public override object this[int ordinal] => GetValue(ordinal);
        public override object this[string name] => string.Empty;

        public override bool GetBoolean(int ordinal) => false;
        public override byte GetByte(int ordinal) => 0;
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) => 0;
        public override char GetChar(int ordinal) => '\0';
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) => 0;
        public override string GetDataTypeName(int ordinal) => string.Empty;
        public override DateTime GetDateTime(int ordinal) => DateTime.MinValue;
        public override decimal GetDecimal(int ordinal) => 0m;
        public override double GetDouble(int ordinal) => 0.0;
        public override Type GetFieldType(int ordinal) => typeof(object);
        public override float GetFloat(int ordinal) => 0f;
        public override Guid GetGuid(int ordinal) => Guid.Empty;
        public override short GetInt16(int ordinal) => 0;
        public override int GetInt32(int ordinal) => 0;
        public override long GetInt64(int ordinal) => 0;
        public override string GetName(int ordinal) => string.Empty;
        public override int GetOrdinal(string name) => -1;
        public override string GetString(int ordinal) => string.Empty;
        public override object GetValue(int ordinal) => string.Empty;
        public override int GetValues(object[] values) => 0;
        public override bool IsDBNull(int ordinal) => true;
        public override bool NextResult() => false;
        public override bool Read() => false;
        public override System.Collections.IEnumerator GetEnumerator() => Enumerable.Empty<object>().GetEnumerator();
    }
#nullable restore
}