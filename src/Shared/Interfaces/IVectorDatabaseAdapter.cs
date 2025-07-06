using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CognitiveMesh.Shared.Interfaces
{
    /// <summary>
    /// Defines the interface for vector database operations
    /// </summary>
    public interface IVectorDatabaseAdapter : IDisposable
    {
        /// <summary>
        /// Connects to the vector database
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous connection operation.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        Task ConnectAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Disconnects from the vector database
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous disconnection operation.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        Task DisconnectAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new collection in the vector database
        /// </summary>
        /// <param name="collectionName">The name of the collection to create.</param>
        /// <param name="vectorSize">The size of the vectors that will be stored in this collection.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous create collection operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collectionName"/> is null or whitespace.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="vectorSize"/> is less than or equal to zero.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        Task CreateCollectionAsync(string collectionName, int vectorSize, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a collection from the vector database
        /// </summary>
        /// <param name="collectionName">The name of the collection to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous delete collection operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collectionName"/> is null or whitespace.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts vectors into the specified collection
        /// </summary>
        /// <typeparam name="T">The type of items containing the vectors to insert.</typeparam>
        /// <param name="collectionName">The name of the collection to insert vectors into.</param>
        /// <param name="items">The items containing the vectors to insert.</param>
        /// <param name="vectorSelector">A function to extract the vector from each item.</param>
        /// <param name="idSelector">An optional function to extract the ID from each item. If not provided, IDs will be generated.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous insert operation. The task result contains the IDs of the inserted vectors.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collectionName"/>, <paramref name="items"/>, or <paramref name="vectorSelector"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="collectionName"/> is empty or whitespace.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        Task<IEnumerable<string>> InsertVectorsAsync<T>(string collectionName, IEnumerable<T> items, Func<T, float[]> vectorSelector, Func<T, string>? idSelector = null, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Searches for similar vectors in the specified collection
        /// </summary>
        /// <param name="collectionName">The name of the collection to search in.</param>
        /// <param name="vector">The query vector to search for similar vectors.</param>
        /// <param name="limit">The maximum number of similar vectors to return. Default is 10.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous search operation. The task result contains a collection of tuples containing the ID and similarity score of the matching vectors.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collectionName"/> or <paramref name="vector"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="collectionName"/> is empty or whitespace, or <paramref name="vector"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="limit"/> is less than or equal to zero.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        Task<IEnumerable<(string Id, float Score)>> SearchVectorsAsync(string collectionName, float[] vector, int limit = 10, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes vectors by their IDs
        /// </summary>
        /// <param name="collectionName">The name of the collection containing the vectors to delete.</param>
        /// <param name="vectorIds">The IDs of the vectors to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collectionName"/> or <paramref name="vectorIds"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="collectionName"/> is empty or whitespace, or <paramref name="vectorIds"/> is empty.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        Task DeleteVectorsAsync(string collectionName, IEnumerable<string> vectorIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets metadata for a vector
        /// </summary>
        /// <param name="collectionName">The name of the collection containing the vector.</param>
        /// <param name="vectorId">The ID of the vector to get metadata for.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the vector metadata as a dictionary of key-value pairs, or null if the vector is not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collectionName"/> or <paramref name="vectorId"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="collectionName"/> or <paramref name="vectorId"/> is empty or whitespace.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        Task<Dictionary<string, object>?> GetVectorMetadataAsync(string collectionName, string vectorId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Upserts vectors into the specified collection
        /// </summary>
        /// <typeparam name="T">The type of items containing the vectors to upsert.</typeparam>
        /// <param name="collectionName">The name of the collection to upsert vectors into.</param>
        /// <param name="items">The items containing the vectors to upsert.</param>
        /// <param name="vectorSelector">A function to extract the vector from each item.</param>
        /// <param name="idSelector">A function to extract the ID from each item. If not provided, IDs will be generated for new vectors.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous upsert operation. The task result contains the IDs of the upserted vectors.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collectionName"/>, <paramref name="items"/>, or <paramref name="vectorSelector"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="collectionName"/> is empty or whitespace.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        Task<IEnumerable<string>> UpsertVectorsAsync<T>(string collectionName, IEnumerable<T> items, Func<T, float[]> vectorSelector, Func<T, string>? idSelector = null, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Queries for similar vectors in the specified collection and returns the matching items
        /// </summary>
        /// <typeparam name="T">The type of items to return.</typeparam>
        /// <param name="collectionName">The name of the collection to query.</param>
        /// <param name="vector">The query vector to search for similar vectors.</param>
        /// <param name="limit">The maximum number of similar vectors to return. Default is 10.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous query operation. The task result contains a collection of tuples containing the ID, similarity score, and the matching item.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collectionName"/> or <paramref name="vector"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="collectionName"/> is empty or whitespace, or <paramref name="vector"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="limit"/> is less than or equal to zero.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        Task<IEnumerable<(string Id, float Score, T Item)>> QueryVectorsAsync<T>(string collectionName, float[] vector, int limit = 10, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Creates an index for the specified collection
        /// </summary>
        /// <param name="collectionName">The name of the collection to create an index for.</param>
        /// <param name="indexType">The type of index to create.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous create index operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collectionName"/> or <paramref name="indexType"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="collectionName"/> or <paramref name="indexType"/> is empty or whitespace.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        Task CreateIndexAsync(string collectionName, string indexType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the count of vectors in the specified collection
        /// </summary>
        Task<long> GetVectorCountAsync(string collectionName, CancellationToken cancellationToken = default);
    }
}
