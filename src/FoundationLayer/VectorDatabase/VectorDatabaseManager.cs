// Refactored VectorDatabaseManager.cs
// - Full SOLID + DRY pass applied

namespace FoundationLayer.VectorDatabase
{
    /// <summary>
    /// Manages vector database operations, delegating to an underlying adapter
    /// and adding logging and error handling.
    /// </summary>
    public class VectorDatabaseManager : IVectorDatabaseAdapter, IDisposable
    {
        private readonly IVectorDatabaseAdapter _adapter;
        private readonly ILogger<VectorDatabaseManager>? _logger;
        private bool _disposed;

        public VectorDatabaseManager(IVectorDatabaseAdapter adapter, ILogger<VectorDatabaseManager>? logger = null)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _logger = logger;
            LogInfo("VectorDatabaseManager initialized with {AdapterType}", _adapter.GetType().Name);
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default) =>
            await ExecuteAsync(() => _adapter.ConnectAsync(cancellationToken), "connect to vector database");

        public async Task DisconnectAsync(CancellationToken cancellationToken = default) =>
            await ExecuteAsync(() => _adapter.DisconnectAsync(cancellationToken), "disconnect from vector database");

        public async Task CreateCollectionAsync(string collectionName, int vectorSize, CancellationToken cancellationToken = default)
        {
            ThrowIfNullOrWhiteSpace(collectionName);
            if (vectorSize <= 0) throw new ArgumentOutOfRangeException(nameof(vectorSize));
            await ExecuteAsync(() => _adapter.CreateCollectionAsync(collectionName, vectorSize, cancellationToken),
                $"create collection {collectionName}");
        }

        public async Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
        {
            ThrowIfNullOrWhiteSpace(collectionName);
            await ExecuteAsync(() => _adapter.DeleteCollectionAsync(collectionName, cancellationToken),
                $"delete collection {collectionName}");
        }

        public async Task<object> GetVectorMetadataAsync(string collectionName, string vectorId, CancellationToken cancellationToken = default)
        {
            ThrowIfNullOrWhiteSpace(collectionName);
            ThrowIfNullOrWhiteSpace(vectorId);
            return await ExecuteAsync(
                () => _adapter.GetVectorMetadataAsync(collectionName, vectorId, cancellationToken),
                $"get metadata for vector {vectorId}");
        }

        public async Task<IEnumerable<string>> InsertVectorsAsync<T>(
            string collectionName, IEnumerable<T> items, Func<T, float[]> vectorSelector,
            Func<T, string>? idSelector = null, CancellationToken cancellationToken = default) where T : class
        {
            ValidateInsertParams(collectionName, items, vectorSelector);
            return await ExecuteAsync(
                () => _adapter.InsertVectorsAsync(collectionName, items, vectorSelector, idSelector, cancellationToken),
                $"insert vectors to collection {collectionName}");
        }

        public async Task<IEnumerable<string>> UpsertVectorsAsync<T>(
            string collectionName, IEnumerable<T> items, Func<T, float[]> vectorSelector,
            Func<T, string>? idSelector = null, CancellationToken cancellationToken = default) where T : class
        {
            ValidateInsertParams(collectionName, items, vectorSelector);
            return await ExecuteAsync(
                () => _adapter.UpsertVectorsAsync(collectionName, items, vectorSelector, idSelector, cancellationToken),
                $"upsert vectors to collection {collectionName}");
        }

        public async Task DeleteVectorsAsync(string collectionName, IEnumerable<string> vectorIds, CancellationToken cancellationToken = default)
        {
            ThrowIfNullOrWhiteSpace(collectionName);
            if (vectorIds == null) throw new ArgumentNullException(nameof(vectorIds));
            var ids = vectorIds.ToList();
            if (!ids.Any()) return;

            await ExecuteAsync(() => _adapter.DeleteVectorsAsync(collectionName, ids, cancellationToken),
                $"delete {ids.Count} vectors from {collectionName}");
        }

        public async Task<IEnumerable<(string Id, float Score)>> SearchVectorsAsync(string collectionName, float[] vector, int limit = 10, CancellationToken cancellationToken = default)
        {
            ThrowIfNullOrWhiteSpace(collectionName);
            if (vector == null || vector.Length == 0) throw new ArgumentException("Vector is null or empty");
            if (limit <= 0) throw new ArgumentOutOfRangeException(nameof(limit));

            return await ExecuteAsync(
                () => _adapter.SearchVectorsAsync(collectionName, vector, limit, cancellationToken),
                $"search vectors in {collectionName}");
        }

        public async Task<IEnumerable<(string Id, float Score)>> QuerySimilarAsync(string collectionName, float[] vector, int limit = 10, CancellationToken cancellationToken = default)
        {
            ThrowIfNullOrWhiteSpace(collectionName);
            if (vector == null || vector.Length == 0) throw new ArgumentException("Vector is null or empty");
            return await ExecuteAsync(
                () => _adapter.QuerySimilarAsync(collectionName, vector, limit, cancellationToken),
                $"query similar vectors in {collectionName}");
        }

        public async Task CreateIndexAsync(string collectionName, string indexType, CancellationToken cancellationToken = default)
        {
            ThrowIfNullOrWhiteSpace(collectionName);
            ThrowIfNullOrWhiteSpace(indexType);
            await ExecuteAsync(() => _adapter.CreateIndexAsync(collectionName, indexType, cancellationToken),
                $"create index {indexType} in {collectionName}");
        }

        public async Task<long> GetVectorCountAsync(string collectionName, CancellationToken cancellationToken = default)
        {
            ThrowIfNullOrWhiteSpace(collectionName);
            return await ExecuteAsync(
                () => _adapter.GetVectorCountAsync(collectionName, cancellationToken),
                $"get vector count in {collectionName}");
        }

        private async Task ExecuteAsync(Func<Task> action, string operation)
        {
            try
            {
                LogDebug("Starting {Operation}", operation);
                await action();
                LogInfo("Completed {Operation}", operation);
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to {Operation}: {Message}", operation, ex.Message);
                throw new InvalidOperationException($"Failed to {operation}", ex);
            }
        }

        private async Task<T> ExecuteAsync<T>(Func<Task<T>> func, string operation)
        {
            try
            {
                LogDebug("Starting {Operation}", operation);
                var result = await func();
                LogInfo("Completed {Operation}", operation);
                return result;
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to {Operation}: {Message}", operation, ex.Message);
                throw new InvalidOperationException($"Failed to {operation}", ex);
            }
        }

        private static void ValidateInsertParams<T>(string collectionName, IEnumerable<T> items, Func<T, float[]> selector)
        {
            ThrowIfNullOrWhiteSpace(collectionName);
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
        }

        private static void ThrowIfNullOrWhiteSpace(string? s, string? paramName = null) =>
            ArgumentNullException.ThrowIfNullOrWhiteSpace(s, paramName);

        private void LogInfo(string msg, params object?[] args) => _logger?.LogInformation(0, msg, args);
        private void LogDebug(string msg, params object?[] args) => _logger?.LogDebug(0, msg, args);
        private void LogError(Exception ex, string msg, params object?[] args) => _logger?.LogError(0, ex, msg, args);

        public void Dispose()
        {
            if (_disposed) return;
            if (_adapter is IDisposable d)
            {
                try
                {
                    LogInfo("Disposing VectorDatabaseManager");
                    d.Dispose();
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error during disposal");
                }
            }
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
