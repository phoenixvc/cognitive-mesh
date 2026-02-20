using System.Collections.Generic;

namespace CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Models;

/// <summary>
/// Represents the result of a temporal graph query, containing the matched edges
/// and summary statistics about the traversal.
/// </summary>
public sealed class TemporalGraph
{
    /// <summary>
    /// The temporal edges matching the query criteria, ordered by creation time.
    /// </summary>
    public IReadOnlyList<TemporalEdge> Edges { get; init; } = [];

    /// <summary>
    /// The number of distinct event nodes touched by the query.
    /// </summary>
    public int NodeCount { get; init; }

    /// <summary>
    /// The total number of edges in the query result.
    /// </summary>
    public int EdgeCount { get; init; }

    /// <summary>
    /// The time in milliseconds taken to execute the graph query.
    /// </summary>
    public double QueryDurationMs { get; init; }
}
