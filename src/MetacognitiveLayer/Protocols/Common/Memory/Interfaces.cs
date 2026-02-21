namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// Common interface for all mesh memory stores.
    /// </summary>
    public interface IMeshMemoryStore
    {
        /// <summary>
        /// Initializes the memory store, creating any required schemas or connections.
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Saves a context value associated with the given session and key.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="key">The context key.</param>
        /// <param name="value">The context value to store.</param>
        Task SaveContextAsync(string sessionId, string key, string value);

        /// <summary>
        /// Retrieves a context value for the given session and key.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="key">The context key.</param>
        /// <returns>The stored context value, or an empty string if not found.</returns>
        Task<string> GetContextAsync(string sessionId, string key);

        /// <summary>
        /// Queries the store for context values similar to the provided embedding.
        /// </summary>
        /// <param name="embedding">A JSON-serialized float array representing the query embedding.</param>
        /// <param name="threshold">The minimum similarity threshold for results.</param>
        /// <returns>A collection of context values that meet the similarity threshold.</returns>
        Task<IEnumerable<string>> QuerySimilarAsync(string embedding, float threshold);
    }

    /// <summary>
    /// Optional separation for initialization routines.
    /// Allows initialization to be handled independently.
    /// </summary>
    public interface IMemoryStoreInitializer
    {
        /// <summary>
        /// Initializes the memory store backend, ensuring all required resources are ready.
        /// </summary>
        Task InitializeAsync();
    }

    /// <summary>
    /// Interface for low-level vector search backends (Redis, Qdrant, etc).
    /// </summary>
    public interface IVectorSearchProvider : IMemoryStoreInitializer
    {
        /// <summary>
        /// Queries the vector index for documents similar to the provided embedding.
        /// </summary>
        /// <param name="embedding">The query embedding as a float array.</param>
        /// <param name="threshold">The minimum similarity score for returned results.</param>
        /// <returns>A collection of matching document values.</returns>
        Task<IEnumerable<string>> QuerySimilarAsync(float[] embedding, float threshold);

        /// <summary>
        /// Saves a document with the specified key and field values.
        /// </summary>
        /// <param name="key">The document key.</param>
        /// <param name="document">A dictionary of field names and their values.</param>
        Task SaveDocumentAsync(string key, Dictionary<string, object> document);

        /// <summary>
        /// Retrieves a specific value from a document using a JSON path expression.
        /// </summary>
        /// <param name="key">The document key.</param>
        /// <param name="jsonPath">The JSON path expression to extract the value.</param>
        /// <returns>The extracted value as a string, or null if not found.</returns>
        Task<string> GetDocumentValueAsync(string key, string jsonPath);
    }
}
