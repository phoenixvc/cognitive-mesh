namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - Niche/Specialized Pattern
// Reason: No perception pipeline needed for current architecture
// Reconsideration: If multimodal perception becomes a requirement
// ============================================================================

/// <summary>
/// Perception stage.
/// </summary>
public enum PerceptionStage
{
    /// <summary>Raw input processing.</summary>
    Raw,
    /// <summary>Feature extraction.</summary>
    Features,
    /// <summary>Semantic understanding.</summary>
    Semantic
}

/// <summary>
/// Perception result.
/// </summary>
public class PerceptionResult
{
    /// <summary>Stage.</summary>
    public PerceptionStage Stage { get; init; }

    /// <summary>Output.</summary>
    public required string Output { get; init; }

    /// <summary>Confidence.</summary>
    public double Confidence { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for three-stage perception architecture.
/// Implements the "Three-Stage Perception Architecture" pattern.
///
/// This is a low-priority pattern because no perception pipeline
/// is needed for current text-centric architecture.
/// </summary>
public interface IPerceptionArchitecturePort
{
    /// <summary>Processes through all stages.</summary>
    Task<IReadOnlyList<PerceptionResult>> ProcessAsync(string input, CancellationToken cancellationToken = default);

    /// <summary>Processes specific stage.</summary>
    Task<PerceptionResult> ProcessStageAsync(string input, PerceptionStage stage, CancellationToken cancellationToken = default);

    /// <summary>Gets stage configuration.</summary>
    Task<Dictionary<string, string>> GetStageConfigAsync(PerceptionStage stage, CancellationToken cancellationToken = default);
}
