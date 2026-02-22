using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CognitiveMesh.ReasoningLayer.MemoryStrategy.Models;

namespace CognitiveMesh.ReasoningLayer.MemoryStrategy.Ports;

/// <summary>
/// Defines the contract for recalling memories from the episodic memory store
/// using various strategies (exact match, fuzzy, semantic, temporal, hybrid).
/// </summary>
public interface IRecallPort
{
    /// <summary>
    /// Recalls memory records using the specified query and recall strategy.
    /// </summary>
    /// <param name="query">The recall query with strategy, text, embedding, and filter parameters.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A recall result containing matched records, scores, and performance metrics.</returns>
    Task<RecallResult> RecallAsync(RecallQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recalls memory records that match any of the specified tags.
    /// </summary>
    /// <param name="tags">The tags to match against.</param>
    /// <param name="maxResults">Maximum number of records to return.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A read-only list of memory records matching the specified tags.</returns>
    Task<IReadOnlyList<MemoryRecord>> RecallByTagsAsync(
        IReadOnlyList<string> tags,
        int maxResults = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recalls the most recently created or accessed memory records.
    /// </summary>
    /// <param name="count">Maximum number of records to return.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A read-only list of the most recent memory records.</returns>
    Task<IReadOnlyList<MemoryRecord>> RecallRecentAsync(int count = 10, CancellationToken cancellationToken = default);
}
