using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace CognitiveMesh.FoundationLayer.AzureCosmosDB.Abstractions
{
    /// <summary>
    /// Defines the contract for Cosmos DB operations.
    /// </summary>
    public interface ICosmosDbAdapter : IDisposable
    {
        /// <summary>
        /// Gets a container reference.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <returns>A Container instance.</returns>
        Container GetContainer(string containerName);

        /// <summary>
        /// Creates a container if it doesn't exist.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="partitionKeyPath">The partition key path.</param>
        /// <param name="throughput">The throughput to configure.</param>
        /// <returns>The created container.</returns>
        Task<Container> CreateContainerIfNotExistsAsync(
            string containerName, 
            string partitionKeyPath = "/id",
            int? throughput = null);

        /// <summary>
        /// Executes a query and returns the results.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>An enumerable of results.</returns>
        IAsyncEnumerable<T> QueryItemsAsync<T>(
            string query,
            string containerName,
            string partitionKey = null);

        /// <summary>
        /// Reads an item by ID.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="id">The ID of the item.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>The item if found; otherwise, null.</returns>
        Task<T> ReadItemAsync<T>(
            string id,
            string containerName,
            string partitionKey = null);

        /// <summary>
        /// Creates an item if it doesn't exist, or replaces it if it does.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="item">The item to create or replace.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>The created or replaced item.</returns>
        Task<T> UpsertItemAsync<T>(
            T item,
            string containerName,
            string partitionKey = null);

        /// <summary>
        /// Deletes an item by ID.
        /// </summary>
        /// <param name="id">The ID of the item to delete.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>True if the item was deleted; false if it didn't exist.</returns>
        Task<bool> DeleteItemAsync(
            string id,
            string containerName,
            string partitionKey = null);
    }
}
