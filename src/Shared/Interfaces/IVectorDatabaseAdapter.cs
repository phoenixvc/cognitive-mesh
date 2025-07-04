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
        Task ConnectAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Disconnects from the vector database
        /// </summary>
        Task DisconnectAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new collection in the vector database
        /// </summary>
        Task CreateCollectionAsync(string collectionName, int vectorSize, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a collection from the vector database
        /// </summary>
        Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts vectors into the specified collection
        /// </summary>
        Task<IEnumerable<string>> InsertVectorsAsync<T>(string collectionName, IEnumerable<T> items, Func<T, float[]> vectorSelector, Func<T, string> idSelector = null, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Searches for similar vectors in the specified collection
        /// </summary>
        Task<IEnumerable<(string Id, float Score)>> SearchVectorsAsync(string collectionName, float[] vector, int limit = 10, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes vectors by their IDs
        /// </summary>
        Task DeleteVectorsAsync(string collectionName, IEnumerable<string> vectorIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets metadata for a vector
        /// </summary>
        Task<object> GetVectorMetadataAsync(string collectionName, string vectorId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Upserts vectors into the specified collection
        /// </summary>
        Task<IEnumerable<string>> UpsertVectorsAsync<T>(string collectionName, IEnumerable<T> items, Func<T, float[]> vectorSelector, Func<T, string> idSelector = null, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Queries for similar vectors in the specified collection
        /// </summary>
        Task<IEnumerable<(string Id, float Score)>> QuerySimilarAsync(string collectionName, float[] vector, int limit = 10, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates an index for the specified collection
        /// </summary>
        Task CreateIndexAsync(string collectionName, string indexType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the count of vectors in the specified collection
        /// </summary>
        Task<long> GetVectorCountAsync(string collectionName, CancellationToken cancellationToken = default);
    }
}
