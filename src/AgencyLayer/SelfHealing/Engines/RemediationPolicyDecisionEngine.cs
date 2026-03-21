using Microsoft.Extensions.Logging;

using CognitiveMesh.AgencyLayer.SelfHealing.Ports;
using CognitiveMesh.FoundationLayer.PolicyStore.Models;
using CognitiveMesh.FoundationLayer.PolicyStore.Ports;

namespace CognitiveMesh.AgencyLayer.SelfHealing.Engines;

/// <summary>
/// Decision engine that resolves remediation actions by consulting the policy store.
/// </summary>
public sealed class RemediationPolicyDecisionEngine : IRemediationDecisionPort
{
    private readonly IRemediationPolicyPort _policyPort;
    private readonly ILogger<RemediationPolicyDecisionEngine> _logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="RemediationPolicyDecisionEngine"/> class.
    /// </summary>
    /// <param name="policyPort">The remediation policy port.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is <see langword="null"/>.</exception>
    public RemediationPolicyDecisionEngine(
        IRemediationPolicyPort policyPort,
        ILogger<RemediationPolicyDecisionEngine> logger)
    {
        ArgumentNullException.ThrowIfNull(policyPort);
        ArgumentNullException.ThrowIfNull(logger);

        _policyPort = policyPort;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<(RemediationAction AllowedActions, Dictionary<string, double> RankingWeights)> GetAllowedActionsAsync(
        string incidentCategory,
        string severity,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(incidentCategory);
        ArgumentException.ThrowIfNullOrWhiteSpace(severity);

        var policy = await _policyPort.GetPolicyAsync(incidentCategory, severity, ct).ConfigureAwait(false);

        _logger.LogInformation(
            "Resolved policy {PolicyId} v{Version} for {Category}/{Severity} with actions {Actions}",
            policy.Id,
            policy.Version,
            incidentCategory,
            severity,
            policy.AllowedActions);

        return (policy.AllowedActions, policy.RankingWeights);
    }
}
