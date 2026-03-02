using System.Collections.Generic;

namespace CognitiveMesh.ReasoningLayer.MemoryStrategy.Models;

/// <summary>
/// Represents the result of a memory recall operation, including the matched
/// records, the strategy used, and performance metrics.
/// </summary>
public sealed class RecallResult
{
    /// <summary>
    /// The memory records matching the recall query, ordered by relevance.
    /// </summary>
    public IReadOnlyList<MemoryRecord> Records { get; init; } = [];

    /// <summary>
    /// The recall strategy that was used to produce these results.
    /// </summary>
    public RecallStrategy StrategyUsed { get; init; }

    /// <summary>
    /// The time in milliseconds taken to execute the recall query.
    /// </summary>
    public double QueryDurationMs { get; init; }

    /// <summary>
    /// The total number of candidate records considered before filtering.
    /// </summary>
    public int TotalCandidates { get; init; }

    /// <summary>
    /// Relevance scores for each returned record, keyed by record ID.
    /// </summary>
    public IReadOnlyDictionary<string, double> RelevanceScores { get; init; } = new Dictionary<string, double>();
}
