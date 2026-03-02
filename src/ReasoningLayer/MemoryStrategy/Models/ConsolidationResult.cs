namespace CognitiveMesh.ReasoningLayer.MemoryStrategy.Models;

/// <summary>
/// Represents the result of a memory consolidation operation. Consolidation
/// promotes frequently-accessed important records to long-term memory and
/// prunes old, unaccessed records to manage memory size.
/// </summary>
/// <param name="PromotedCount">Number of records promoted from short-term to long-term memory.</param>
/// <param name="PrunedCount">Number of records removed due to low importance and no access.</param>
/// <param name="RetainedCount">Number of records retained without changes.</param>
/// <param name="DurationMs">Time in milliseconds taken to complete the consolidation.</param>
public sealed record ConsolidationResult(
    int PromotedCount,
    int PrunedCount,
    int RetainedCount,
    double DurationMs);
