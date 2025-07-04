using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CognitiveMesh.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.FoundationLayer.AzureCosmosDB
{
    /// <summary>
    /// High-level manager for Azure Cosmos DB operations.
    /// </summary>
    public class CosmosDBManager : IDisposable
    {
        private readonly ICosmosDbAdapter _cosmosDbAdapter;
        private readonly ILogger<CosmosDBManager>? _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDBManager"/> class.
        /// </summary>
        /// <param name="cosmosDbAdapter">The Cosmos DB adapter.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="jsonOptions">Optional JSON serializer options.</param>
        public CosmosDBManager(
            ICosmosDbAdapter cosmosDbAdapter,
            ILogger<CosmosDBManager>? logger = null,
            JsonSerializerOptions? jsonOptions = null)
        {
            _cosmosDbAdapter = cosmosDbAdapter ?? throw new ArgumentNullException(nameof(cosmosDbAdapter));
            _logger = logger;
            _jsonOptions = jsonOptions ?? new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        /// <summary>
        /// Initializes the Cosmos DB client and creates the database and container if they don't exist.
        /// </summary>
        /// <param name="databaseId">The database ID.</param>
        /// <param name="containerId">The container ID.</param>
        /// <param name="partitionKeyPath">The partition key path.</param>
        /// <param name="throughput">The throughput to configure.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task InitializeAsync(
            string databaseId,
            string containerId,
            string partitionKeyPath = "/id",
            int throughput = 400,
            CancellationToken cancellationToken = default)
        {
            await _cosmosDbAdapter.InitializeAsync(databaseId, containerId, partitionKeyPath, throughput, true, cancellationToken);
        }

        /// <summary>
        /// Upserts a document into the specified container.
        /// </summary>
        /// <typeparam name="T">The type of document to upsert.</typeparam>
        /// <param name="document">The document to upsert.</param>
        /// <param name="partitionKey">The partition key for the document.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The upserted document.</returns>
        public async Task<T> UpsertDocumentAsync<T>(
            T document,
            string partitionKey = null,
            CancellationToken cancellationToken = default) where T : class
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            try
            {
                // If no partition key is provided, try to get it from the document
                var pk = partitionKey ?? GetPartitionKeyFromDocument(document);
                
                return await _cosmosDbAdapter.UpsertItemAsync(document, pk, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(0, ex, "Error upserting document: {Message}", ex.Message);
                throw new InvalidOperationException("Failed to upsert document", ex);
            }
        }

        /// <summary>
        /// Reads a document by ID and partition key.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the document into.</typeparam>
        /// <param name="id">The document ID.</param>
        /// <param name="partitionKey">The partition key for the document.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The deserialized document, or null if not found.</returns>
        public async Task<T?> ReadDocumentAsync<T>(
            string id,
            string? partitionKey,
            CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be null or whitespace.", nameof(id));
                
            if (string.IsNullOrWhiteSpace(partitionKey))
                throw new ArgumentException("Partition key cannot be null or whitespace.", nameof(partitionKey));

            try
            {
                return await _cosmosDbAdapter.GetItemAsync<T>(id, partitionKey, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(0, ex, "Failed to read document {DocumentId}: {Message}", id ?? "[null]", ex.Message);
                throw new InvalidOperationException($"Failed to read document {id}", ex);
            }
        }

        /// <summary>
        /// Queries documents using a SQL query.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the results into.</typeparam>
        /// <param name="query">The SQL query string.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <param name="partitionKey">Optional partition key for cross-partition queries.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async enumerable of matching documents.</returns>
        public IAsyncEnumerable<T> QueryDocumentsAsync<T>(
            string query,
            Dictionary<string, object>? parameters = default,
            string? partitionKey = default,
            CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or whitespace.", nameof(query));

            try
            {
                return _cosmosDbAdapter.QueryItemsAsync<T>(query, parameters, partitionKey, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(0, ex, "Error executing query: {Query}: {Message}", query ?? "[null]", ex.Message);
                throw new InvalidOperationException("Failed to execute query", ex);
            }
        }

        /// <summary>
        /// Deletes a document by ID and partition key.
        /// </summary>
        /// <param name="id">The document ID.</param>
        /// <param name="partitionKey">The partition key for the document.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task DeleteDocumentAsync<T>(
            string id,
            string partitionKey,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be null or whitespace.", nameof(id));
                
            if (string.IsNullOrWhiteSpace(partitionKey))
                throw new ArgumentException("Partition key cannot be null or whitespace.", nameof(partitionKey));

            try
            {
                await _cosmosDbAdapter.DeleteItemAsync<T>(id, partitionKey, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(0, ex, "Error deleting document {DocumentId}: {Message}", id ?? "[null]", ex.Message);
                throw new InvalidOperationException($"Failed to delete document {id}", ex);
            }
        }

        /// <summary>
        /// Executes a stored procedure.
        /// </summary>
        /// <typeparam name="T">The return type of the stored procedure.</typeparam>
        /// <param name="storedProcedureId">The ID of the stored procedure.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="procedureParams">The parameters to pass to the stored procedure.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the stored procedure execution.</returns>
        public async Task<T> ExecuteStoredProcedureAsync<T>(
            string storedProcedureId,
            string? partitionKey,
            dynamic[]? procedureParams = default,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storedProcedureId))
                throw new ArgumentException("Stored procedure ID cannot be null or whitespace.", nameof(storedProcedureId));

            try
            {
                return await _cosmosDbAdapter.ExecuteStoredProcedureAsync<T>(
                    storedProcedureId, 
                    partitionKey, 
                    procedureParams, 
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(0, ex, "Error executing stored procedure {StoredProcedureId}: {Message}", storedProcedureId ?? "[null]", ex.Message);
                throw new InvalidOperationException($"Failed to execute stored procedure {storedProcedureId}", ex);
            }
        }

        private string GetPartitionKeyFromDocument<T>(T? document)
        {
            if (document == null)
                return "default";

            // Try to get the partition key from the document using reflection
            var property = typeof(T).GetProperty("PartitionKey") ?? 
                         typeof(T).GetProperty("partitionKey") ??
                         typeof(T).GetProperty("id") ??
                         typeof(T).GetProperty("Id");
                         
            if (property != null)
            {
                var value = property.GetValue(document);
                if (value != null)
                {
                    return value.ToString();
                }
            }
            
            // If no partition key is found, use a default
            return "default";
        }

        /// <summary>
        /// Disposes the Cosmos DB client.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the Cosmos DB client.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cosmosDbAdapter?.Dispose();
                }
                _disposed = true;
            }
        }
        
        ~CosmosDBManager()
        {
            Dispose(false);
        }
    }
}
