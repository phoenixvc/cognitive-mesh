using System.Threading;
using System.Threading.Tasks;
using CognitiveMesh.ReasoningLayer.MemoryStrategy.Models;

namespace CognitiveMesh.ReasoningLayer.MemoryStrategy.Ports;

/// <summary>
/// Defines the contract for memory consolidation operations.
/// Consolidation promotes frequently-accessed, important short-term memories
/// to long-term storage and prunes old, unused records to manage capacity.
/// </summary>
public interface IConsolidationPort
{
    /// <summary>
    /// Runs the consolidation process: promotes important, frequently-accessed
    /// records to long-term memory and prunes old, unaccessed records.
    /// </summary>
    /// <param name="accessCountThreshold">Minimum access count required for promotion.</param>
    /// <param name="importanceThreshold">Minimum importance score required for promotion.</param>
    /// <param name="pruneAge">Records older than this duration with zero access count are pruned.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A consolidation result with promotion, pruning, and retention counts.</returns>
    Task<ConsolidationResult> ConsolidateAsync(
        int accessCountThreshold = 3,
        double importanceThreshold = 0.5,
        System.TimeSpan? pruneAge = null,
        CancellationToken cancellationToken = default);
}
