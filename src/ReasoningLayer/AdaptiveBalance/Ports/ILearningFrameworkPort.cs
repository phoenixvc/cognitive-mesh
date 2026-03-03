using CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;

namespace CognitiveMesh.ReasoningLayer.AdaptiveBalance.Ports;

/// <summary>
/// Defines the contract for the learning framework engine.
/// Captures learning events, identifies patterns, and provides mistake prevention insights
/// to support continuous improvement across the system.
/// </summary>
public interface ILearningFrameworkPort
{
    /// <summary>
    /// Records a learning event for future pattern analysis.
    /// Events are classified by pattern type and outcome.
    /// </summary>
    /// <param name="learningEvent">The learning event to record.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous recording operation.</returns>
    Task RecordEventAsync(LearningEvent learningEvent, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves learning events matching a specific pattern type.
    /// </summary>
    /// <param name="patternType">The pattern type to filter by.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of learning events matching the pattern type.</returns>
    Task<IReadOnlyList<LearningEvent>> GetPatternsAsync(string patternType, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves insights from past failures to help prevent future mistakes.
    /// Returns failure events sorted by recency to prioritize recent patterns.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of failure events ordered by most recent first.</returns>
    Task<IReadOnlyList<LearningEvent>> GetMistakePreventionInsightsAsync(CancellationToken cancellationToken);
}
