namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// Common interface for all mesh memory stores.
    /// </summary>
    public interface IMeshMemoryStore
    {
        Task InitializeAsync();
        Task SaveContextAsync(string sessionId, string key, string value);
        Task<string> GetContextAsync(string sessionId, string key);
        Task<IEnumerable<string>> QuerySimilarAsync(string embedding, float threshold);
    }

    /// <summary>
    /// Optional separation for initialization routines.
    /// Allows initialization to be handled independently.
    /// </summary>
    public interface IMemoryStoreInitializer
    {
        Task InitializeAsync();
    }

    /// <summary>
    /// Interface for low-level vector search backends (Redis, Qdrant, etc).
    /// </summary>
    public interface IVectorSearchProvider : IMemoryStoreInitializer
    {
        Task<IEnumerable<string>> QuerySimilarAsync(float[] embedding, float threshold);
        Task SaveDocumentAsync(string key, Dictionary<string, object> document);
        Task<string> GetDocumentValueAsync(string key, string jsonPath);
    }
}
