using CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;

namespace CognitiveMesh.ReasoningLayer.AdaptiveBalance.Ports;

/// <summary>
/// Defines the contract for the adaptive balance engine.
/// Provides capabilities for generating balance recommendations across spectrum dimensions,
/// applying manual overrides, and tracking spectrum position history.
/// </summary>
public interface IAdaptiveBalancePort
{
    /// <summary>
    /// Generates a balance recommendation based on the provided context.
    /// Analyzes contextual signals to recommend optimal positions across all spectrum dimensions.
    /// </summary>
    /// <param name="context">Context key-value pairs influencing the recommendation (e.g., "threat_level", "revenue_target").</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A balance recommendation with positions for all spectrum dimensions.</returns>
    Task<BalanceRecommendation> GetBalanceAsync(Dictionary<string, string> context, CancellationToken cancellationToken);

    /// <summary>
    /// Applies a manual override to a specific spectrum dimension.
    /// Validates the override value is within bounds and returns an updated recommendation.
    /// </summary>
    /// <param name="balanceOverride">The override to apply.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>An updated balance recommendation reflecting the override.</returns>
    Task<BalanceRecommendation> ApplyOverrideAsync(BalanceOverride balanceOverride, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the history of spectrum position changes for a specific dimension.
    /// </summary>
    /// <param name="dimension">The spectrum dimension to retrieve history for.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of historical spectrum positions, ordered by time.</returns>
    Task<IReadOnlyList<SpectrumPosition>> GetSpectrumHistoryAsync(SpectrumDimension dimension, CancellationToken cancellationToken);
}
