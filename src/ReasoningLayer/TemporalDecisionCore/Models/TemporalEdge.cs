using System;

namespace CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Models;

/// <summary>
/// Represents a temporal edge linking two events in the Temporal Decision Core graph.
/// Each edge carries the dual-circuit gate scores, confidence, and machine-readable rationale
/// as required by the PRD for full explainability.
/// </summary>
/// <param name="EdgeId">Unique identifier for this temporal edge.</param>
/// <param name="SourceEventId">Identifier of the source (earlier) event.</param>
/// <param name="TargetEventId">Identifier of the target (later) event.</param>
/// <param name="Confidence">Overall confidence score from 0.0 to 1.0 for this temporal link.</param>
/// <param name="Rationale">Machine-readable rationale explaining why this edge was created.</param>
/// <param name="CreatedAt">Timestamp when this edge was created.</param>
/// <param name="PromoterScore">Score from the CA1 promoter circuit (higher means stronger causal signal).</param>
/// <param name="SuppressorScore">Score from the L2 suppressor circuit (higher means more likely spurious).</param>
/// <param name="WindowSizeMs">The adaptive window size in milliseconds at the time of edge creation.</param>
public sealed record TemporalEdge(
    string EdgeId,
    string SourceEventId,
    string TargetEventId,
    double Confidence,
    string Rationale,
    DateTimeOffset CreatedAt,
    double PromoterScore,
    double SuppressorScore,
    double WindowSizeMs);
