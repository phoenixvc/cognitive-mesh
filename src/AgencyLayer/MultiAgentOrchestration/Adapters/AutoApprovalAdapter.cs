using AgencyLayer.MultiAgentOrchestration.Engines;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.MultiAgentOrchestration.Adapters;

/// <summary>
/// Approval adapter that automatically approves actions for pre-approved workflows
/// (governance hot path). For non-pre-approved workflows, delegates to a configurable
/// approval callback.
/// </summary>
public class AutoApprovalAdapter : IApprovalAdapter
{
    private readonly ILogger<AutoApprovalAdapter> _logger;
    private readonly Func<string, string, object, Task<bool>>? _manualApprovalCallback;
    private readonly bool _autoApproveAll;

    /// <summary>
    /// Creates an auto-approval adapter.
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="autoApproveAll">If true, auto-approves all requests (for pre-approved workflows / MAKER benchmarks)</param>
    /// <param name="manualApprovalCallback">Optional callback for manual approval when autoApproveAll is false</param>
    public AutoApprovalAdapter(
        ILogger<AutoApprovalAdapter> logger,
        bool autoApproveAll = false,
        Func<string, string, object, Task<bool>>? manualApprovalCallback = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _autoApproveAll = autoApproveAll;
        _manualApprovalCallback = manualApprovalCallback;
    }

    public async Task<bool> RequestApprovalAsync(string userId, string actionDescription, object actionPayload)
    {
        if (_autoApproveAll)
        {
            _logger.LogDebug("Auto-approved action for user {UserId}: {ActionDescription}", userId, actionDescription);
            return true;
        }

        if (_manualApprovalCallback != null)
        {
            return await _manualApprovalCallback(userId, actionDescription, actionPayload);
        }

        _logger.LogWarning("No approval mechanism configured. Defaulting to deny for user {UserId}: {ActionDescription}", userId, actionDescription);
        return false;
    }
}
