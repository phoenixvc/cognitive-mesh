using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Models;

namespace CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Ports;

/// <summary>
/// Defines the contract for the temporal edge audit trail.
/// All edge actions (creation, rejection, expiration) are logged with
/// machine-readable rationale and confidence for compliance and debugging.
/// </summary>
public interface ITemporalAuditPort
{
    /// <summary>
    /// Logs an action taken on a temporal edge (created, rejected, or expired).
    /// </summary>
    /// <param name="log">The audit log entry to record.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the asynchronous logging operation.</returns>
    Task LogEdgeActionAsync(TemporalEdgeLog log, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the audit trail for a specific temporal edge.
    /// </summary>
    /// <param name="edgeId">The edge identifier to retrieve the audit trail for.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A read-only list of audit log entries for the specified edge, ordered by timestamp.</returns>
    Task<IReadOnlyList<TemporalEdgeLog>> GetAuditTrailAsync(string edgeId, CancellationToken cancellationToken = default);
}
