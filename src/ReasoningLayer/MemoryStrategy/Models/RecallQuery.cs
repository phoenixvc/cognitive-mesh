using System;

namespace CognitiveMesh.ReasoningLayer.MemoryStrategy.Models;

/// <summary>
/// Parameters for querying the episodic memory store. Supports multiple recall strategies,
/// relevance thresholds, and optional time-based filtering.
/// </summary>
public sealed class RecallQuery
{
    /// <summary>
    /// The text query to search for in memory records.
    /// </summary>
    public string QueryText { get; set; } = string.Empty;

    /// <summary>
    /// Optional embedding vector for semantic similarity searches.
    /// If null, semantic similarity scoring is skipped.
    /// </summary>
    public float[]? QueryEmbedding { get; set; }

    /// <summary>
    /// The recall strategy to use for this query.
    /// </summary>
    public RecallStrategy Strategy { get; set; } = RecallStrategy.Hybrid;

    /// <summary>
    /// Maximum number of records to return.
    /// </summary>
    public int MaxResults { get; set; } = 10;

    /// <summary>
    /// Minimum relevance score (0.0 to 1.0) for records to be included in results.
    /// </summary>
    public double MinRelevance { get; set; }

    /// <summary>
    /// Optional time window constraining how far back to search.
    /// If null, no time restriction is applied.
    /// </summary>
    public TimeSpan? TimeWindow { get; set; }
}
