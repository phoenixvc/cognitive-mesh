namespace CognitiveMesh.MetacognitiveLayer.Protocols.Common.Memory
{
    public interface IVectorSearchProvider
    {
        Task InitializeAsync();
        Task<IEnumerable<string>> QuerySimilarAsync(float[] embedding, float threshold);
        Task SaveDocumentAsync(string key, Dictionary<string, object> document);
        Task<string> GetDocumentValueAsync(string key, string jsonPath);
    }
}
