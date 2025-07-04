using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CognitiveMesh.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.FoundationLayer.VectorDatabase
{
    /// <summary>
    /// Manages vector storage and retrieval for semantic search and embeddings.
    /// Provides a high-level interface over the IVectorDatabaseAdapter implementation.
    /// </summary>
    public class VectorDatabaseManager : IVectorDatabaseAdapter, IDisposable
    {
        private readonly IVectorDatabaseAdapter _adapter;
        private readonly ILogger<VectorDatabaseManager>? _logger;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorDatabaseManager"/> class.
        /// </summary>
        /// <param name="adapter">The vector database adapter to use.</param>
        /// <param name="logger">Optional logger instance.</param>
        public VectorDatabaseManager(
            IVectorDatabaseAdapter adapter,
            ILogger<VectorDatabaseManager>? logger = null)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _logger = logger;
            _logger?.LogInformation("VectorDatabaseManager initialized with {AdapterType}", adapter?.GetType().Name ?? "[unknown]");
        }

        #region IVectorDatabaseAdapter Implementation (CognitiveMesh.Shared.Interfaces)

        /// <inheritdoc/>
        public async Task<object> GetVectorMetadataAsync(string collectionName, string vectorId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(collectionName));

            if (string.IsNullOrWhiteSpace(vectorId))
                throw new ArgumentException("Vector ID cannot be null or whitespace.", nameof(vectorId));

            try
            {
                _logger?.LogDebug("Getting metadata for vector {VectorId} in collection {CollectionName}", vectorId, collectionName);
                var metadata = await _adapter.GetVectorMetadataAsync(collectionName, vectorId, cancellationToken);
                _logger?.LogDebug("Successfully retrieved metadata for vector {VectorId}", vectorId);
                return metadata;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get metadata for vector {VectorId} in collection {CollectionName}: {Message}", 
                    vectorId, collectionName, ex.Message);
                throw new InvalidOperationException($"Failed to get metadata for vector {vectorId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation("Connecting to vector database...");
                await _adapter.ConnectAsync(cancellationToken);
                _logger?.LogInformation("Successfully connected to vector database");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to connect to vector database: {Message}", ex.Message);
                throw new InvalidOperationException("Failed to connect to vector database", ex);
            }
        }

        /// <inheritdoc/>
        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation("Disconnecting from vector database...");
                await _adapter.DisconnectAsync(cancellationToken);
                _logger?.LogInformation("Successfully disconnected from vector database");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error disconnecting from vector database: {Message}", ex.Message);
                throw new InvalidOperationException("Error disconnecting from vector database", ex);
            }
        }

        /// <inheritdoc/>
        public async Task CreateCollectionAsync(string collectionName, int vectorSize, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(collectionName));

            if (vectorSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(vectorSize), "Vector size must be greater than zero.");

            try
            {
                _logger?.LogInformation("Creating collection {CollectionName} with vector size {VectorSize}", collectionName, vectorSize);
                await _adapter.CreateCollectionAsync(collectionName, vectorSize, cancellationToken);
                _logger?.LogInformation("Successfully created collection {CollectionName}", collectionName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to create collection {CollectionName}: {Message}", collectionName, ex.Message);
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
                _logger?.LogInformation("Deleting collection {CollectionName}", collectionName);
                await _adapter.DeleteCollectionAsync(collectionName, cancellationToken);
                _logger?.LogInformation("Successfully deleted collection {CollectionName}", collectionName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to delete collection {CollectionName}: {Message}", collectionName, ex.Message);
                throw new InvalidOperationException($"Failed to delete collection {collectionName}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> InsertVectorsAsync<T>(
            string collectionName,
            IEnumerable<T> items,
            Func<T, float[]> vectorSelector,
            Func<T, string>? idSelector = null,
            CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(collectionName));

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (vectorSelector == null)
                throw new ArgumentNullException(nameof(vectorSelector));

            try
            {
                _logger?.LogDebug("Inserting {Count} vectors into collection {CollectionName}", items.Count(), collectionName);
                var result = await _adapter.InsertVectorsAsync(collectionName, items, vectorSelector, idSelector, cancellationToken);
                _logger?.LogInformation("Successfully inserted {Count} vectors into collection {CollectionName}", items.Count(), collectionName);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to insert vectors into collection {CollectionName}: {Message}", collectionName, ex.Message);
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
                throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be greater than zero.");

            try
            {
                _logger?.LogDebug("Searching for similar vectors in collection {CollectionName}", collectionName);
                var results = await _adapter.SearchVectorsAsync(collectionName, vector, limit, cancellationToken);
                _logger?.LogDebug("Found {Count} similar vectors in collection {CollectionName}", results.Count(), collectionName);
                return results;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to search vectors in collection {CollectionName}: {Message}", collectionName, ex.Message);
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
                _logger?.LogDebug("Deleting {Count} vectors from collection {CollectionName}", ids.Count, collectionName);
                await _adapter.DeleteVectorsAsync(collectionName, ids, cancellationToken);
                _logger?.LogInformation("Successfully deleted {Count} vectors from collection {CollectionName}", ids.Count, collectionName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to delete vectors from collection {CollectionName}: {Message}", collectionName, ex.Message);
                throw new InvalidOperationException($"Failed to delete vectors from collection {collectionName}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> UpsertVectorsAsync<T>(
            string collectionName, 
            IEnumerable<T> items, 
            Func<T, float[]> vectorSelector, 
            Func<T, string>? idSelector = null, 
            CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(collectionName));

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (vectorSelector == null)
                throw new ArgumentNullException(nameof(vectorSelector));

            try
            {
                _logger?.LogDebug("Upserting {Count} vectors to collection {CollectionName}", items.Count(), collectionName);
                var result = await _adapter.UpsertVectorsAsync(collectionName, items, vectorSelector, idSelector, cancellationToken);
                _logger?.LogInformation("Successfully upserted {Count} vectors to collection {CollectionName}", items.Count(), collectionName);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to upsert vectors to collection {CollectionName}: {Message}", collectionName, ex.Message);
                throw new InvalidOperationException($"Failed to upsert vectors to collection {collectionName}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<(string Id, float Score)>> QuerySimilarAsync(
            string collectionName,
            float[] vector,
            int limit = 10,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(collectionName));

            if (vector == null || vector.Length == 0)
                throw new ArgumentException("Vector cannot be null or empty.", nameof(vector));

            try
            {
                _logger?.LogDebug("Querying for similar vectors in collection {CollectionName}", collectionName);
                var results = await _adapter.QuerySimilarAsync(collectionName, vector, limit, cancellationToken);
                _logger?.LogDebug("Found {Count} similar vectors in collection {CollectionName}", results.Count(), collectionName);
                return results;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to query similar vectors in collection {CollectionName}: {Message}", collectionName, ex.Message);
                throw new InvalidOperationException($"Failed to query similar vectors in collection {collectionName}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task CreateIndexAsync(
            string collectionName, 
            string indexType, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(collectionName));

            if (string.IsNullOrWhiteSpace(indexType))
                throw new ArgumentException("Index type cannot be null or whitespace.", nameof(indexType));

            try
            {
                _logger?.LogInformation("Creating index of type {IndexType} for collection {CollectionName}", indexType, collectionName);
                await _adapter.CreateIndexAsync(collectionName, indexType, cancellationToken);
                _logger?.LogInformation("Successfully created index for collection {CollectionName}", collectionName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to create index for collection {CollectionName}: {Message}", collectionName, ex.Message);
                throw new InvalidOperationException($"Failed to create index for collection {collectionName}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<long> GetVectorCountAsync(
            string collectionName, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(collectionName));

            try
            {
                _logger?.LogDebug("Getting vector count for collection {CollectionName}", collectionName);
                var count = await _adapter.GetVectorCountAsync(collectionName, cancellationToken);
                _logger?.LogDebug("Collection {CollectionName} contains {Count} vectors", collectionName, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get vector count for collection {CollectionName}: {Message}", collectionName, ex.Message);
                throw new InvalidOperationException($"Failed to get vector count for collection {collectionName}", ex);
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Disposes the vector database manager and its resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the vector database manager and its resources.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        _logger?.LogInformation("Disposing VectorDatabaseManager");
                        (_adapter as IDisposable)?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error disposing VectorDatabaseManager");
                    }
                }
                _disposed = true;
            }
        }

        ~VectorDatabaseManager()
        {
            Dispose(false);
        }

        #endregion
    }
}
