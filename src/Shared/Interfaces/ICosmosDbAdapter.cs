using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CognitiveMesh.Shared.Interfaces
{
    /// <summary>
    /// Defines the interface for Cosmos DB operations
    /// </summary>
    public interface ICosmosDbAdapter : IDisposable
    {
        /// <summary>
        /// Initializes the Cosmos DB client and creates the database and container if they don't exist
        /// </summary>
        Task InitializeAsync(string databaseId, string containerId, string partitionKeyPath, 
            int throughput = 400, bool createIfNotExists = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an item by its ID and partition key
        /// </summary>
        Task<T?> GetItemAsync<T>(string id, string partitionKey, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Gets all items of a specific type from the container
        /// </summary>
        IAsyncEnumerable<T> GetItemsAsync<T>(string query, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Adds or updates an item in the container
        /// </summary>
        Task<T> UpsertItemAsync<T>(T item, string partitionKey, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Deletes an item by its ID and partition key
        /// </summary>
        Task DeleteItemAsync<T>(string id, string partitionKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries the container using SQL-like syntax
        /// </summary>
        IAsyncEnumerable<T> QueryItemsAsync<T>(string query, Dictionary<string, object>? parameters = null, 
            string? partitionKey = null, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Executes a stored procedure
        /// </summary>
        Task<T?> ExecuteStoredProcedureAsync<T>(string storedProcedureId, string partitionKey, 
            dynamic[] procedureParams, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a stored procedure if it doesn't exist
        /// </summary>
        Task CreateStoredProcedureIfNotExistsAsync(string storedProcedureId, string storedProcedureBody, 
            CancellationToken cancellationToken = default);
    }
}
