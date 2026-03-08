namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - RL/Fine-Tuning Pattern
// Reason: No reward-based review system; carries antipattern risk
// Reconsideration: If robust reward signals can be developed
// Related Antipattern: Inference-Healed Code Review Reward (Medium Risk)
// ============================================================================

/// <summary>
/// Code review reward.
/// </summary>
public class CodeReviewReward
{
    /// <summary>Review identifier.</summary>
    public required string ReviewId { get; init; }

    /// <summary>Original reward.</summary>
    public double OriginalReward { get; init; }

    /// <summary>Healed reward.</summary>
    public double HealedReward { get; init; }

    /// <summary>Healing rationale.</summary>
    public string? Rationale { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for inference-healed code review reward.
/// Implements the "Inference-Healed Code Review Reward" pattern.
///
/// This is a low-priority pattern because no reward-based review
/// system exists and it carries antipattern risk.
/// </summary>
public interface IInferenceHealedRewardPort
{
    /// <summary>Heals reward signal.</summary>
    Task<CodeReviewReward> HealRewardAsync(string reviewId, double originalReward, CancellationToken cancellationToken = default);

    /// <summary>Validates reward correctness.</summary>
    Task<bool> ValidateRewardAsync(string reviewId, double reward, CancellationToken cancellationToken = default);

    /// <summary>Gets healed rewards.</summary>
    Task<IReadOnlyList<CodeReviewReward>> GetHealedRewardsAsync(int limit, CancellationToken cancellationToken = default);
}
