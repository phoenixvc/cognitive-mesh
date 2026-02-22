using System;
using System.Collections.Generic;

namespace CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Models;

/// <summary>
/// Represents a temporal event recorded in the Temporal Decision Core.
/// Events are the fundamental units of observation that may be linked
/// through temporal edges when causal relationships are detected.
/// </summary>
/// <param name="EventId">Unique identifier for this temporal event.</param>
/// <param name="Timestamp">The point in time when the event occurred.</param>
/// <param name="Salience">Salience score from 0.0 (low) to 1.0 (high), indicating the event's significance.</param>
/// <param name="Context">Key-value context metadata associated with the event (e.g., actor, type, category).</param>
/// <param name="SourceAgentId">Identifier of the agent that recorded this event.</param>
/// <param name="TenantId">Tenant identifier for multi-tenant isolation.</param>
public sealed record TemporalEvent(
    string EventId,
    DateTimeOffset Timestamp,
    double Salience,
    IReadOnlyDictionary<string, string> Context,
    string SourceAgentId,
    string TenantId);
