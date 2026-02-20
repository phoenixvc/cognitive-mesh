namespace CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;

/// <summary>
/// Represents a position on a specific spectrum dimension.
/// Values range from 0.0 to 1.0, with bounds constraining the recommended range.
/// </summary>
/// <param name="Dimension">The spectrum dimension this position applies to.</param>
/// <param name="Value">The current position value (0.0 to 1.0).</param>
/// <param name="LowerBound">The recommended lower bound for this dimension.</param>
/// <param name="UpperBound">The recommended upper bound for this dimension.</param>
/// <param name="Rationale">Explanation for why this position was recommended.</param>
/// <param name="RecommendedBy">The system or user that recommended this position.</param>
public sealed record SpectrumPosition(
    SpectrumDimension Dimension,
    double Value,
    double LowerBound,
    double UpperBound,
    string Rationale,
    string RecommendedBy);
