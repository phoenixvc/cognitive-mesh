using CognitiveMesh.BusinessApplications.AdaptiveBalance.Models;
using CognitiveMesh.BusinessApplications.AdaptiveBalance.Ports;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.BusinessApplications.AdaptiveBalance.Controllers;

/// <summary>
/// Controller for adaptive balance management, providing operations for
/// spectrum dimension retrieval, manual overrides, history tracking,
/// learning evidence submission, and reflexion status monitoring.
/// </summary>
public class AdaptiveBalanceController
{
    private readonly ILogger<AdaptiveBalanceController> _logger;
    private readonly IAdaptiveBalanceServicePort _servicePort;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdaptiveBalanceController"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for structured logging.</param>
    /// <param name="servicePort">The adaptive balance service port.</param>
    public AdaptiveBalanceController(
        ILogger<AdaptiveBalanceController> logger,
        IAdaptiveBalanceServicePort servicePort)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _servicePort = servicePort ?? throw new ArgumentNullException(nameof(servicePort));
    }

    /// <summary>
    /// Retrieves the current adaptive balance positions for all spectrum dimensions.
    /// </summary>
    /// <param name="request">The balance request with optional context.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The current balance positions with confidence metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    public async Task<BalanceResponse> GetBalanceAsync(BalanceRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Retrieving adaptive balance with {ContextCount} context entries", request.Context.Count);

        return await _servicePort.GetBalanceAsync(request, cancellationToken);
    }

    /// <summary>
    /// Applies a manual override to a specific spectrum dimension.
    /// </summary>
    /// <param name="request">The override request with new value and rationale.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The override response confirming the change.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="ArgumentException">Thrown when required fields are missing.</exception>
    public async Task<OverrideResponse> ApplyOverrideAsync(OverrideRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Dimension))
        {
            throw new ArgumentException("Dimension is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.OverriddenBy))
        {
            throw new ArgumentException("OverriddenBy is required.", nameof(request));
        }

        _logger.LogInformation(
            "Applying override to dimension {Dimension} by {OverriddenBy}",
            request.Dimension, request.OverriddenBy);

        return await _servicePort.ApplyOverrideAsync(request, cancellationToken);
    }

    /// <summary>
    /// Retrieves the history of changes for a specific spectrum dimension.
    /// </summary>
    /// <param name="dimension">The name of the dimension to retrieve history for.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The change history for the specified dimension.</returns>
    /// <exception cref="ArgumentException">Thrown when dimension is null or whitespace.</exception>
    public async Task<SpectrumHistoryResponse> GetSpectrumHistoryAsync(string dimension, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dimension);

        _logger.LogInformation("Retrieving spectrum history for dimension {Dimension}", dimension);

        return await _servicePort.GetSpectrumHistoryAsync(dimension, cancellationToken);
    }

    /// <summary>
    /// Submits learning evidence for the continuous improvement loop.
    /// </summary>
    /// <param name="request">The learning evidence request.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The learning evidence submission response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    public async Task<LearningEvidenceResponse> SubmitLearningEvidenceAsync(LearningEvidenceRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Submitting learning evidence from agent {SourceAgentId}: pattern {PatternType}",
            request.SourceAgentId, request.PatternType);

        return await _servicePort.SubmitLearningEvidenceAsync(request, cancellationToken);
    }

    /// <summary>
    /// Retrieves the current reflexion (self-evaluation) system status.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The current reflexion status with aggregate metrics.</returns>
    public async Task<ReflexionStatusResponse> GetReflexionStatusAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving reflexion status");

        return await _servicePort.GetReflexionStatusAsync(cancellationToken);
    }
}
