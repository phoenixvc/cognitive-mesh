namespace CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;

/// <summary>
/// Represents a manual override applied to a spectrum dimension position.
/// Captures the original and new values along with the rationale for the change.
/// </summary>
/// <param name="OverrideId">Unique identifier for this override.</param>
/// <param name="Dimension">The spectrum dimension being overridden.</param>
/// <param name="OriginalValue">The value before the override was applied.</param>
/// <param name="NewValue">The new value being set (must be between 0.0 and 1.0).</param>
/// <param name="Rationale">Explanation for why the override was applied.</param>
/// <param name="OverriddenBy">Identifier of the user or system applying the override.</param>
/// <param name="OverriddenAt">Timestamp when the override was applied.</param>
public sealed record BalanceOverride(
    Guid OverrideId,
    SpectrumDimension Dimension,
    double OriginalValue,
    double NewValue,
    string Rationale,
    string OverriddenBy,
    DateTimeOffset OverriddenAt);
