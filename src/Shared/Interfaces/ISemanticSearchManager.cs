using System.Threading.Tasks;

namespace CognitiveMesh.Shared.Interfaces
{
    /// <summary>
    /// Interface for semantic search operations.
    /// </summary>
    public interface ISemanticSearchManager
    {
        /// <summary>
        /// Performs a semantic search using vector similarity and refines results with an LLM.
        /// </summary>
        /// <param name="indexName">Name of the vector index to search.</param>
        /// <param name="queryText">The natural language query.</param>
        /// <returns>Refined search results based on semantic understanding.</returns>
        Task<string> SearchAsync(string indexName, string queryText);
    }
}
