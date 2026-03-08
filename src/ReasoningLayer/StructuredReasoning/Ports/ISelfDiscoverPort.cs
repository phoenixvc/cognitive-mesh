namespace ReasoningLayer.StructuredReasoning.Ports;

/// <summary>
/// A reasoning structure discovered by the LLM.
/// </summary>
public class DiscoveredStructure
{
    /// <summary>Structure identifier.</summary>
    public string StructureId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Structure name.</summary>
    public required string Name { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Reasoning steps.</summary>
    public IReadOnlyList<ReasoningStepTemplate> Steps { get; init; } = Array.Empty<ReasoningStepTemplate>();

    /// <summary>When to use this structure.</summary>
    public IReadOnlyList<string> UseCases { get; init; } = Array.Empty<string>();

    /// <summary>Confidence in the structure.</summary>
    public double Confidence { get; init; }

    /// <summary>Source problem type.</summary>
    public string? SourceProblemType { get; init; }

    /// <summary>Discovered at.</summary>
    public DateTimeOffset DiscoveredAt { get; init; }
}

/// <summary>
/// A template for a reasoning step.
/// </summary>
public class ReasoningStepTemplate
{
    /// <summary>Step order.</summary>
    public int Order { get; init; }

    /// <summary>Step name.</summary>
    public required string Name { get; init; }

    /// <summary>Step prompt.</summary>
    public required string Prompt { get; init; }

    /// <summary>Expected output format.</summary>
    public string? OutputFormat { get; init; }

    /// <summary>Whether step is optional.</summary>
    public bool IsOptional { get; init; }

    /// <summary>Dependencies on other steps.</summary>
    public IReadOnlyList<int> DependsOn { get; init; } = Array.Empty<int>();
}

/// <summary>
/// Self-discovery configuration.
/// </summary>
public class SelfDiscoveryConfiguration
{
    /// <summary>Number of candidate structures to generate.</summary>
    public int CandidateCount { get; init; } = 3;

    /// <summary>Whether to evaluate candidates.</summary>
    public bool EvaluateCandidates { get; init; } = true;

    /// <summary>Model for discovery.</summary>
    public string? DiscoveryModel { get; init; }

    /// <summary>Maximum steps per structure.</summary>
    public int MaxSteps { get; init; } = 10;

    /// <summary>Whether to save discovered structures.</summary>
    public bool SaveStructures { get; init; } = true;
}

/// <summary>
/// Self-discovery result.
/// </summary>
public class SelfDiscoveryResult
{
    /// <summary>Problem analyzed.</summary>
    public required string Problem { get; init; }

    /// <summary>Selected structure.</summary>
    public required DiscoveredStructure SelectedStructure { get; init; }

    /// <summary>All candidate structures.</summary>
    public IReadOnlyList<DiscoveredStructure> Candidates { get; init; } = Array.Empty<DiscoveredStructure>();

    /// <summary>Reasoning output.</summary>
    public required string ReasoningOutput { get; init; }

    /// <summary>Final answer.</summary>
    public required string Answer { get; init; }

    /// <summary>Step outputs.</summary>
    public IReadOnlyList<StepOutput> StepOutputs { get; init; } = Array.Empty<StepOutput>();

    /// <summary>Duration.</summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Output from a reasoning step.
/// </summary>
public class StepOutput
{
    /// <summary>Step name.</summary>
    public required string StepName { get; init; }

    /// <summary>Output content.</summary>
    public required string Output { get; init; }

    /// <summary>Duration.</summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Port for Self-Discover LLM self-composed reasoning.
/// Implements the "Self-Discover: LLM Self-Composed Reasoning" pattern.
/// </summary>
public interface ISelfDiscoverPort
{
    /// <summary>
    /// Discovers and applies a reasoning structure.
    /// </summary>
    Task<SelfDiscoveryResult> DiscoverAndReasonAsync(
        string problem,
        SelfDiscoveryConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Discovers a reasoning structure without applying it.
    /// </summary>
    Task<DiscoveredStructure> DiscoverStructureAsync(
        string problemType,
        SelfDiscoveryConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies an existing structure to a problem.
    /// </summary>
    Task<SelfDiscoveryResult> ApplyStructureAsync(
        string problem,
        string structureId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a saved structure.
    /// </summary>
    Task<DiscoveredStructure?> GetStructureAsync(
        string structureId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists saved structures.
    /// </summary>
    Task<IReadOnlyList<DiscoveredStructure>> ListStructuresAsync(
        string? problemType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates candidate structures.
    /// </summary>
    Task<IReadOnlyList<(DiscoveredStructure Structure, double Score)>> EvaluateStructuresAsync(
        string problem,
        IEnumerable<DiscoveredStructure> structures,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a discovered structure.
    /// </summary>
    Task SaveStructureAsync(
        DiscoveredStructure structure,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a saved structure.
    /// </summary>
    Task DeleteStructureAsync(
        string structureId,
        CancellationToken cancellationToken = default);
}
