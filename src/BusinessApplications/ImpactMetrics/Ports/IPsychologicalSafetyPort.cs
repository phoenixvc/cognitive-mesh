using CognitiveMesh.BusinessApplications.ImpactMetrics.Models;

namespace CognitiveMesh.BusinessApplications.ImpactMetrics.Ports;

/// <summary>
/// Defines the contract for calculating and retrieving psychological safety scores
/// that measure how safe a team feels about AI adoption.
/// </summary>
public interface IPsychologicalSafetyPort
{
    /// <summary>
    /// Calculates the psychological safety score for a team based on survey responses
    /// and behavioral signals.
    /// </summary>
    /// <param name="teamId">The identifier of the team to assess.</param>
    /// <param name="tenantId">The tenant to which the team belongs.</param>
    /// <param name="surveyScores">
    /// Survey-based scores per dimension, where each value is between 0 and 100.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The calculated psychological safety score.</returns>
    Task<PsychologicalSafetyScore> CalculateSafetyScoreAsync(
        string teamId,
        string tenantId,
        Dictionary<SafetyDimension, double> surveyScores,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves historical psychological safety scores for a team.
    /// </summary>
    /// <param name="teamId">The identifier of the team.</param>
    /// <param name="tenantId">The tenant to which the team belongs.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of historical safety scores ordered by calculation date.</returns>
    Task<IReadOnlyList<PsychologicalSafetyScore>> GetHistoricalScoresAsync(
        string teamId,
        string tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a breakdown of the most recent psychological safety score by dimension.
    /// </summary>
    /// <param name="teamId">The identifier of the team.</param>
    /// <param name="tenantId">The tenant to which the team belongs.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A dictionary mapping each dimension to its score, or null if no score exists.</returns>
    Task<Dictionary<SafetyDimension, double>?> GetDimensionBreakdownAsync(
        string teamId,
        string tenantId,
        CancellationToken cancellationToken = default);
}
