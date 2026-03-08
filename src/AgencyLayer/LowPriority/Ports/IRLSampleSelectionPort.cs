namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - RL/Fine-Tuning Pattern
// Reason: No RL components in current architecture
// Reconsideration: If RL training infrastructure is built
// ============================================================================

/// <summary>
/// Sample for RL training.
/// </summary>
public class RLSample
{
    /// <summary>Sample identifier.</summary>
    public required string SampleId { get; init; }

    /// <summary>State.</summary>
    public required string State { get; init; }

    /// <summary>Action.</summary>
    public required string Action { get; init; }

    /// <summary>Reward.</summary>
    public double Reward { get; init; }

    /// <summary>Variance score.</summary>
    public double Variance { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for variance-based RL sample selection.
/// Implements the "Variance-Based RL Sample Selection" pattern.
///
/// This is a low-priority pattern because no RL components exist
/// in the current architecture.
/// </summary>
public interface IRLSampleSelectionPort
{
    /// <summary>Selects high-variance samples.</summary>
    Task<IReadOnlyList<RLSample>> SelectHighVarianceAsync(IEnumerable<RLSample> samples, int count, CancellationToken cancellationToken = default);

    /// <summary>Computes variance for samples.</summary>
    Task<IReadOnlyList<(string SampleId, double Variance)>> ComputeVarianceAsync(IEnumerable<RLSample> samples, CancellationToken cancellationToken = default);

    /// <summary>Prioritizes samples for training.</summary>
    Task<IReadOnlyList<RLSample>> PrioritizeAsync(IEnumerable<RLSample> samples, CancellationToken cancellationToken = default);
}
