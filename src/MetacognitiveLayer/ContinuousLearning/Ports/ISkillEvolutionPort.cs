namespace MetacognitiveLayer.ContinuousLearning.Ports;

/// <summary>
/// A skill definition.
/// </summary>
public class SkillDefinition
{
    /// <summary>Skill identifier.</summary>
    public required string SkillId { get; init; }

    /// <summary>Skill name.</summary>
    public required string Name { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Skill category.</summary>
    public required string Category { get; init; }

    /// <summary>Version.</summary>
    public string Version { get; init; } = "1.0.0";

    /// <summary>Prompt template.</summary>
    public required string PromptTemplate { get; init; }

    /// <summary>Input parameters.</summary>
    public IReadOnlyList<SkillParameter> Parameters { get; init; } = Array.Empty<SkillParameter>();

    /// <summary>Example usages.</summary>
    public IReadOnlyList<SkillExample> Examples { get; init; } = Array.Empty<SkillExample>();

    /// <summary>Required capabilities.</summary>
    public IReadOnlyList<string> RequiredCapabilities { get; init; } = Array.Empty<string>();

    /// <summary>Proficiency level required.</summary>
    public double ProficiencyThreshold { get; init; } = 0.7;

    /// <summary>Created at.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Updated at.</summary>
    public DateTimeOffset UpdatedAt { get; init; }

    /// <summary>Metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// A skill parameter.
/// </summary>
public class SkillParameter
{
    /// <summary>Parameter name.</summary>
    public required string Name { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Type.</summary>
    public required string Type { get; init; }

    /// <summary>Whether required.</summary>
    public bool Required { get; init; } = true;

    /// <summary>Default value.</summary>
    public string? DefaultValue { get; init; }
}

/// <summary>
/// A skill example.
/// </summary>
public class SkillExample
{
    /// <summary>Input.</summary>
    public required string Input { get; init; }

    /// <summary>Expected output.</summary>
    public required string Output { get; init; }

    /// <summary>Context.</summary>
    public string? Context { get; init; }
}

/// <summary>
/// Skill performance metrics.
/// </summary>
public class SkillPerformance
{
    /// <summary>Skill identifier.</summary>
    public required string SkillId { get; init; }

    /// <summary>Total uses.</summary>
    public int TotalUses { get; init; }

    /// <summary>Successful uses.</summary>
    public int SuccessfulUses { get; init; }

    /// <summary>Success rate.</summary>
    public double SuccessRate { get; init; }

    /// <summary>Average execution time.</summary>
    public TimeSpan AverageExecutionTime { get; init; }

    /// <summary>User satisfaction score.</summary>
    public double? UserSatisfaction { get; init; }

    /// <summary>Common failure reasons.</summary>
    public IReadOnlyList<string> CommonFailures { get; init; } = Array.Empty<string>();

    /// <summary>Period start.</summary>
    public DateTimeOffset PeriodStart { get; init; }

    /// <summary>Period end.</summary>
    public DateTimeOffset PeriodEnd { get; init; }
}

/// <summary>
/// Skill evolution proposal.
/// </summary>
public class SkillEvolutionProposal
{
    /// <summary>Proposal identifier.</summary>
    public string ProposalId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Skill identifier.</summary>
    public required string SkillId { get; init; }

    /// <summary>Evolution type.</summary>
    public EvolutionType Type { get; init; }

    /// <summary>Proposed changes.</summary>
    public required string ProposedChanges { get; init; }

    /// <summary>Rationale.</summary>
    public required string Rationale { get; init; }

    /// <summary>Expected improvement.</summary>
    public double ExpectedImprovement { get; init; }

    /// <summary>Created at.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Evolution type.
/// </summary>
public enum EvolutionType
{
    /// <summary>Improve existing prompt.</summary>
    PromptImprovement,
    /// <summary>Add new examples.</summary>
    ExampleAddition,
    /// <summary>Add new capability.</summary>
    CapabilityExpansion,
    /// <summary>Performance optimization.</summary>
    Optimization,
    /// <summary>Error handling improvement.</summary>
    ErrorHandling,
    /// <summary>Full rewrite.</summary>
    Rewrite
}

/// <summary>
/// Port for skill library evolution.
/// Implements the "Skill Library Evolution" pattern.
/// </summary>
public interface ISkillEvolutionPort
{
    /// <summary>
    /// Registers a skill.
    /// </summary>
    Task<SkillDefinition> RegisterSkillAsync(
        SkillDefinition skill,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a skill.
    /// </summary>
    Task<SkillDefinition?> GetSkillAsync(
        string skillId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists skills.
    /// </summary>
    Task<IReadOnlyList<SkillDefinition>> ListSkillsAsync(
        string? category = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets skill performance metrics.
    /// </summary>
    Task<SkillPerformance> GetPerformanceAsync(
        string skillId,
        DateTimeOffset? since = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records skill usage.
    /// </summary>
    Task RecordUsageAsync(
        string skillId,
        bool successful,
        TimeSpan executionTime,
        string? feedback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Proposes skill evolution.
    /// </summary>
    Task<SkillEvolutionProposal> ProposeEvolutionAsync(
        string skillId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies an evolution proposal.
    /// </summary>
    Task<SkillDefinition> ApplyEvolutionAsync(
        string proposalId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests a skill with examples.
    /// </summary>
    Task<IReadOnlyList<(SkillExample Example, bool Passed, string? Error)>> TestSkillAsync(
        string skillId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets skill version history.
    /// </summary>
    Task<IReadOnlyList<SkillDefinition>> GetVersionHistoryAsync(
        string skillId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reverts to a previous version.
    /// </summary>
    Task<SkillDefinition> RevertToVersionAsync(
        string skillId,
        string version,
        CancellationToken cancellationToken = default);
}
