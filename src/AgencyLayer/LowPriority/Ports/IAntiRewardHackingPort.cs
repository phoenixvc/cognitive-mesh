namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - RL/Fine-Tuning Pattern
// Reason: No reward system to protect
// Reconsideration: If RL with rewards is implemented
// ============================================================================

/// <summary>
/// Reward hack detection result.
/// </summary>
public class HackDetectionResult
{
    /// <summary>Whether hack detected.</summary>
    public bool HackDetected { get; init; }

    /// <summary>Hack type.</summary>
    public string? HackType { get; init; }

    /// <summary>Confidence.</summary>
    public double Confidence { get; init; }

    /// <summary>Evidence.</summary>
    public IReadOnlyList<string> Evidence { get; init; } = Array.Empty<string>();
}

/// <summary>
/// [LOW PRIORITY] Port for anti-reward-hacking grader design.
/// Implements the "Anti-Reward-Hacking Grader Design" pattern.
///
/// This is a low-priority pattern because no reward system exists
/// to protect from hacking.
/// </summary>
public interface IAntiRewardHackingPort
{
    /// <summary>Detects reward hacking.</summary>
    Task<HackDetectionResult> DetectHackingAsync(string response, double claimedReward, CancellationToken cancellationToken = default);

    /// <summary>Validates grader integrity.</summary>
    Task<bool> ValidateGraderAsync(string graderId, CancellationToken cancellationToken = default);

    /// <summary>Applies anti-hacking measures.</summary>
    Task<double> ApplyMeasuresAsync(string response, double originalReward, CancellationToken cancellationToken = default);
}
