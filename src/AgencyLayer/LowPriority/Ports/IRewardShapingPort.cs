namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - RL/Fine-Tuning Pattern
// Reason: No reward signals; also carries antipattern risk (reward hacking)
// Reconsideration: If reward signals can be made robust
// Related Antipattern: Tool Use Incentivization via Reward Shaping (Medium Risk)
// ============================================================================

/// <summary>
/// Reward signal.
/// </summary>
public class RewardSignal
{
    /// <summary>Signal identifier.</summary>
    public required string SignalId { get; init; }

    /// <summary>Action rewarded.</summary>
    public required string Action { get; init; }

    /// <summary>Reward value.</summary>
    public double Reward { get; init; }

    /// <summary>Reason.</summary>
    public string? Reason { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for tool use incentivization via reward shaping.
/// Implements the "Tool Use Incentivization via Reward Shaping" pattern.
///
/// This is a low-priority pattern because no reward signals exist
/// and it carries antipattern risk (reward hacking).
/// </summary>
public interface IRewardShapingPort
{
    /// <summary>Assigns reward for tool use.</summary>
    Task AssignRewardAsync(string toolId, string action, double reward, CancellationToken cancellationToken = default);

    /// <summary>Gets tool reward policy.</summary>
    Task<Dictionary<string, double>> GetRewardPolicyAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates reward policy.</summary>
    Task UpdatePolicyAsync(Dictionary<string, double> policy, CancellationToken cancellationToken = default);
}
