using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Models;

namespace CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Ports;

/// <summary>
/// Defines the contract for querying and traversing the temporal graph.
/// Supports depth-limited traversal, confidence filtering, and time range filtering
/// for downstream agents to reason about causal chains.
/// </summary>
public interface ITemporalGraphPort
{
    /// <summary>
    /// Queries the temporal graph starting from the specified event, respecting
    /// depth limits, confidence thresholds, and time range filters.
    /// </summary>
    /// <param name="query">The query parameters defining traversal constraints.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A temporal graph result containing matched edges and traversal statistics.</returns>
    Task<TemporalGraph> QueryTemporalGraphAsync(TemporalQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single temporal edge by its unique identifier.
    /// </summary>
    /// <param name="edgeId">The unique identifier of the edge to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The temporal edge if found; otherwise, null.</returns>
    Task<TemporalEdge?> GetEdgeAsync(string edgeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all temporal edges connected to a specific event, either as source or target.
    /// </summary>
    /// <param name="eventId">The event identifier to find edges for.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A read-only list of temporal edges connected to the specified event.</returns>
    Task<IReadOnlyList<TemporalEdge>> GetEdgesForEventAsync(string eventId, CancellationToken cancellationToken = default);
}
