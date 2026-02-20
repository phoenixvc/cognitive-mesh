using System.Collections.Generic;

namespace CognitiveMesh.ReasoningLayer.MemoryStrategy.Models;

/// <summary>
/// Overall statistics about the memory store, including record counts,
/// average importance, and per-strategy performance metrics.
/// </summary>
public sealed class MemoryStatistics
{
    /// <summary>
    /// Total number of memory records in the store.
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Number of records that have been consolidated (promoted to long-term memory).
    /// </summary>
    public int ConsolidatedCount { get; set; }

    /// <summary>
    /// Average importance score across all records.
    /// </summary>
    public double AvgImportance { get; set; }

    /// <summary>
    /// Performance metrics for each recall strategy, keyed by strategy type.
    /// </summary>
    public IReadOnlyDictionary<RecallStrategy, StrategyPerformance> StrategyPerformanceMap { get; set; }
        = new Dictionary<RecallStrategy, StrategyPerformance>();
}
