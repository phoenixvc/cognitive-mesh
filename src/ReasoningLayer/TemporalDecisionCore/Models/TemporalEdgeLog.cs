using System;

namespace CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Models;

/// <summary>
/// Audit log entry for temporal edge actions. Every edge creation, rejection,
/// or expiration is recorded for compliance with the TDC PRD audit requirements.
/// </summary>
/// <param name="LogId">Unique identifier for this audit log entry.</param>
/// <param name="EdgeId">Identifier of the temporal edge this log entry pertains to.</param>
/// <param name="Action">The action taken on the edge (Created, Rejected, or Expired).</param>
/// <param name="Rationale">Machine-readable rationale explaining why this action was taken.</param>
/// <param name="Confidence">Confidence score at the time of the action.</param>
/// <param name="Timestamp">Timestamp when the action was recorded.</param>
/// <param name="ActorAgentId">Identifier of the agent that initiated the action.</param>
public sealed record TemporalEdgeLog(
    string LogId,
    string EdgeId,
    TemporalEdgeAction Action,
    string Rationale,
    double Confidence,
    DateTimeOffset Timestamp,
    string ActorAgentId);

/// <summary>
/// Enumerates the possible actions that can be taken on a temporal edge.
/// </summary>
public enum TemporalEdgeAction
{
    /// <summary>
    /// The edge was successfully created and added to the temporal graph.
    /// </summary>
    Created,

    /// <summary>
    /// The edge was rejected by the dual-circuit gate (suppressor exceeded threshold).
    /// </summary>
    Rejected,

    /// <summary>
    /// The edge expired due to time-to-live or window contraction.
    /// </summary>
    Expired
}
