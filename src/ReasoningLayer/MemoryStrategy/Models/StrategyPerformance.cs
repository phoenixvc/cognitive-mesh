namespace CognitiveMesh.ReasoningLayer.MemoryStrategy.Models;

/// <summary>
/// Tracks the effectiveness of a recall strategy over time.
/// Used by the strategy adaptation system to recommend the best-performing
/// strategy for a given context.
/// </summary>
public sealed class StrategyPerformance
{
    /// <summary>
    /// The recall strategy being tracked.
    /// </summary>
    public RecallStrategy Strategy { get; set; }

    /// <summary>
    /// Average relevance score achieved by this strategy across all samples.
    /// </summary>
    public double AvgRelevanceScore { get; set; }

    /// <summary>
    /// Average latency in milliseconds for this strategy across all samples.
    /// </summary>
    public double AvgLatencyMs { get; set; }

    /// <summary>
    /// Hit rate: proportion of queries that returned at least one result (0.0 to 1.0).
    /// </summary>
    public double HitRate { get; set; }

    /// <summary>
    /// Number of performance samples collected for this strategy.
    /// </summary>
    public int SampleCount { get; set; }
}
