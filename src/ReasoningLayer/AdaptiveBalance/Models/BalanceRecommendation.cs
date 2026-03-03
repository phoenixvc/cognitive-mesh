namespace CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;

/// <summary>
/// Represents a recommendation for balancing across multiple spectrum dimensions.
/// Contains positions for each dimension along with an overall confidence score.
/// </summary>
/// <param name="RecommendationId">Unique identifier for this recommendation.</param>
/// <param name="Dimensions">Recommended positions for each spectrum dimension.</param>
/// <param name="Context">Contextual information that influenced the recommendation.</param>
/// <param name="OverallConfidence">Overall confidence in the recommendation (0.0 to 1.0).</param>
/// <param name="GeneratedAt">Timestamp when the recommendation was generated.</param>
public sealed record BalanceRecommendation(
    Guid RecommendationId,
    List<SpectrumPosition> Dimensions,
    Dictionary<string, string> Context,
    double OverallConfidence,
    DateTimeOffset GeneratedAt);
