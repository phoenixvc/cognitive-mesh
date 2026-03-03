using System;

namespace CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Models;

/// <summary>
/// Parameters for querying the temporal graph. Supports filtering by confidence,
/// depth, time range, and rationale inclusion for flexible graph traversal.
/// </summary>
public sealed class TemporalQuery
{
    /// <summary>
    /// The starting event identifier from which to traverse the temporal graph.
    /// </summary>
    public string StartEventId { get; set; } = string.Empty;

    /// <summary>
    /// Minimum confidence threshold for edges to include in the query result.
    /// Edges with confidence below this value are excluded.
    /// </summary>
    public double MinConfidence { get; set; }

    /// <summary>
    /// Maximum traversal depth from the start event. Limits the number of hops
    /// in the graph to prevent unbounded traversal.
    /// </summary>
    public int MaxDepth { get; set; } = 5;

    /// <summary>
    /// Whether to include the machine-readable rationale for each edge in the result.
    /// </summary>
    public bool IncludeRationale { get; set; } = true;

    /// <summary>
    /// Optional start of the time range filter. Only edges created after this time are included.
    /// </summary>
    public DateTimeOffset? TimeRangeStart { get; set; }

    /// <summary>
    /// Optional end of the time range filter. Only edges created before this time are included.
    /// </summary>
    public DateTimeOffset? TimeRangeEnd { get; set; }
}
