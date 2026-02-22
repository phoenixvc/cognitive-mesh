namespace CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Models;

/// <summary>
/// Represents the result of the dual-circuit temporal gate evaluation.
/// Contains the promoter and suppressor scores, the final decision on whether
/// to link two events, and a machine-readable rationale for audit compliance.
/// </summary>
/// <param name="ShouldLink">Whether the gate decided to create a temporal edge between the two events.</param>
/// <param name="PromoterScore">Score from the CA1 promoter circuit indicating causal evidence strength.</param>
/// <param name="SuppressorScore">Score from the L2 suppressor circuit indicating spurious association risk.</param>
/// <param name="Confidence">Overall confidence in the gating decision, derived from promoter and suppressor scores.</param>
/// <param name="Rationale">Machine-readable rationale explaining the gating decision.</param>
/// <param name="EvaluationDurationMs">Time in milliseconds taken to evaluate the gate (target: P95 &lt;= 1 ms).</param>
public sealed record GatingDecision(
    bool ShouldLink,
    double PromoterScore,
    double SuppressorScore,
    double Confidence,
    string Rationale,
    double EvaluationDurationMs);
