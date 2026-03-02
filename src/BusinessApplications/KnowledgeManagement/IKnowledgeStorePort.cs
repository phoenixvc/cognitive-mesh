using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Port interface for knowledge storage and retrieval operations.
/// Adapters implement this to integrate with specific backing stores
/// (CosmosDB, vector databases, knowledge graphs, etc.).
/// The KnowledgeManager delegates all persistence to this port,
/// removing the need for framework-specific branching.
/// </summary>
public interface IKnowledgeStorePort
{
    /// <summary>
    /// Adds knowledge content with the specified identifier.
    /// </summary>
    /// <param name="knowledgeId">The unique identifier for the knowledge entry.</param>
    /// <param name="content">The knowledge content to store.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if the knowledge was added successfully; otherwise, <c>false</c>.</returns>
    Task<bool> AddAsync(string knowledgeId, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves knowledge content by its identifier.
    /// </summary>
    /// <param name="knowledgeId">The unique identifier of the knowledge to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The knowledge content, or null if not found.</returns>
    Task<string?> GetAsync(string knowledgeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates existing knowledge content with the specified identifier.
    /// </summary>
    /// <param name="knowledgeId">The unique identifier of the knowledge to update.</param>
    /// <param name="newContent">The updated knowledge content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if the knowledge was updated successfully; otherwise, <c>false</c>.</returns>
    Task<bool> UpdateAsync(string knowledgeId, string newContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes knowledge content by its identifier.
    /// </summary>
    /// <param name="knowledgeId">The unique identifier of the knowledge to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if the knowledge was deleted successfully; otherwise, <c>false</c>.</returns>
    Task<bool> DeleteAsync(string knowledgeId, CancellationToken cancellationToken = default);
}
