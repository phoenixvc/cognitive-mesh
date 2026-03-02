using System.Threading;
using System.Threading.Tasks;
using CognitiveMesh.ReasoningLayer.MemoryStrategy.Models;

namespace CognitiveMesh.ReasoningLayer.MemoryStrategy.Ports;

/// <summary>
/// Defines the contract for strategy adaptation. Tracks performance metrics
/// per recall strategy and recommends the best-performing strategy based on
/// historical hit rate and relevance scores.
/// </summary>
public interface IStrategyAdaptationPort
{
    /// <summary>
    /// Returns the recommended recall strategy based on historical performance data.
    /// The strategy with the highest hit rate is preferred.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The best-performing recall strategy, or Hybrid if insufficient data is available.</returns>
    Task<RecallStrategy> GetBestStrategyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a performance sample for a recall strategy, updating the running
    /// averages for relevance, latency, and hit rate.
    /// </summary>
    /// <param name="strategy">The recall strategy that was used.</param>
    /// <param name="relevanceScore">The relevance score achieved (0.0 to 1.0).</param>
    /// <param name="latencyMs">The latency in milliseconds for the recall operation.</param>
    /// <param name="wasHit">Whether the recall returned at least one result.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The updated performance metrics for the specified strategy.</returns>
    Task<StrategyPerformance> RecordPerformanceAsync(
        RecallStrategy strategy,
        double relevanceScore,
        double latencyMs,
        bool wasHit,
        CancellationToken cancellationToken = default);
}
