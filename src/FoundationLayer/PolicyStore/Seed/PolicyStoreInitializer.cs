using Microsoft.Extensions.Logging;

using CognitiveMesh.FoundationLayer.PolicyStore.Ports;

namespace CognitiveMesh.FoundationLayer.PolicyStore.Seed;

/// <summary>
/// Seeds the policy store with default remediation policies when the store is empty.
/// </summary>
public sealed class PolicyStoreInitializer
{
    private readonly IRemediationPolicyPort _policyPort;
    private readonly ILogger<PolicyStoreInitializer> _logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="PolicyStoreInitializer"/> class.
    /// </summary>
    /// <param name="policyPort">The remediation policy port used for persistence.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is <see langword="null"/>.</exception>
    public PolicyStoreInitializer(
        IRemediationPolicyPort policyPort,
        ILogger<PolicyStoreInitializer> logger)
    {
        ArgumentNullException.ThrowIfNull(policyPort);
        ArgumentNullException.ThrowIfNull(logger);

        _policyPort = policyPort;
        _logger = logger;
    }

    /// <summary>
    /// Seeds the policy store with default policies if no policies currently exist.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    public async Task InitialiseAsync(CancellationToken ct = default)
    {
        var existing = await _policyPort.ListPoliciesAsync(ct).ConfigureAwait(false);

        if (existing.Any())
        {
            _logger.LogInformation("Policy store already contains {Count} policies; skipping seed", existing.Count());
            return;
        }

        _logger.LogInformation("Policy store is empty; seeding default policies");

        var defaults = DefaultPolicySeed.GetDefaultPolicies();
        foreach (var policy in defaults)
        {
            await _policyPort.UpsertPolicyAsync(policy, ct).ConfigureAwait(false);
        }

        _logger.LogInformation("Seeded {Count} default remediation policies", defaults.Count);
    }
}
