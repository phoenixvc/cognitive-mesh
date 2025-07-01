using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveMesh.MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// Interface for mesh memory storage systems that manage context, embeddings and retrieval across the protocol system.
    /// </summary>
    public interface IMeshMemoryStore
    {
        /// <summary>
        /// Saves context data for a session.
        /// </summary>
        /// <param name="sessionId">The unique session identifier</param>
        /// <param name="key">The context key</param>
        /// <param name="value">The context value (serialized as string)</param>
        Task SaveContextAsync(string sessionId, string key, string value);

        /// <summary>
        /// Retrieves context data for a session.
        /// </summary>
        /// <param name="sessionId">The unique session identifier</param>
        /// <param name="key">The context key</param>
        /// <returns>The context value, or null if not found</returns>
        Task<string> GetContextAsync(string sessionId, string key);

        /// <summary>
        /// Finds context values similar to the provided embedding.
        /// </summary>
        /// <param name="embedding">The embedding vector as a string</param>
        /// <param name="threshold">Similarity threshold (0.0 to 1.0)</param>
        /// <returns>List of similar context values</returns>
        Task<IEnumerable<string>> QuerySimilarAsync(string embedding, float threshold);
    }
}