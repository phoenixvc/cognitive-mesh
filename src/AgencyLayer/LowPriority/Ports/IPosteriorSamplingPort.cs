namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - Niche/Specialized Pattern
// Reason: Probabilistic planning not required for current use cases
// Reconsideration: If advanced probabilistic planning is needed
// ============================================================================

/// <summary>
/// Posterior sample.
/// </summary>
public class PosteriorSample
{
    /// <summary>Sample identifier.</summary>
    public required string SampleId { get; init; }

    /// <summary>Plan.</summary>
    public required string Plan { get; init; }

    /// <summary>Probability.</summary>
    public double Probability { get; init; }

    /// <summary>Expected utility.</summary>
    public double ExpectedUtility { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for explicit posterior-sampling planner.
/// Implements the "Explicit Posterior-Sampling Planner" pattern.
///
/// This is a low-priority pattern because probabilistic planning
/// is not required for current use cases.
/// </summary>
public interface IPosteriorSamplingPort
{
    /// <summary>Samples from posterior.</summary>
    Task<IReadOnlyList<PosteriorSample>> SampleAsync(string goal, int count, CancellationToken cancellationToken = default);

    /// <summary>Updates posterior with evidence.</summary>
    Task UpdatePosteriorAsync(string evidence, CancellationToken cancellationToken = default);

    /// <summary>Selects best plan.</summary>
    Task<PosteriorSample> SelectBestAsync(IEnumerable<PosteriorSample> samples, CancellationToken cancellationToken = default);
}
