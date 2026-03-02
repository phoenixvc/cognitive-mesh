using CognitiveMesh.BusinessApplications.AdaptiveBalance.Models;

namespace CognitiveMesh.BusinessApplications.AdaptiveBalance.Ports;

/// <summary>
/// Defines the contract for adaptive balance services, providing operations
/// for spectrum dimension management, manual overrides, learning evidence,
/// and reflexion status monitoring.
/// </summary>
public interface IAdaptiveBalanceServicePort
{
    /// <summary>
    /// Retrieves the current adaptive balance positions for all spectrum dimensions.
    /// </summary>
    /// <param name="request">The balance request with optional context.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The current balance positions with confidence metrics.</returns>
    Task<BalanceResponse> GetBalanceAsync(BalanceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a manual override to a specific spectrum dimension.
    /// </summary>
    /// <param name="request">The override request with new value and rationale.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The override response confirming the change.</returns>
    Task<OverrideResponse> ApplyOverrideAsync(OverrideRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the history of changes for a specific spectrum dimension.
    /// </summary>
    /// <param name="dimension">The name of the dimension to retrieve history for.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The change history for the specified dimension.</returns>
    Task<SpectrumHistoryResponse> GetSpectrumHistoryAsync(string dimension, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits learning evidence for the continuous improvement loop.
    /// </summary>
    /// <param name="request">The learning evidence request.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The learning evidence submission response.</returns>
    Task<LearningEvidenceResponse> SubmitLearningEvidenceAsync(LearningEvidenceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the current reflexion (self-evaluation) system status.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The current reflexion status with aggregate metrics.</returns>
    Task<ReflexionStatusResponse> GetReflexionStatusAsync(CancellationToken cancellationToken = default);
}
