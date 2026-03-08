namespace AgencyLayer.Tools.Ports;

/// <summary>
/// A steering directive for tool use.
/// </summary>
public class ToolSteeringDirective
{
    /// <summary>Directive identifier.</summary>
    public string DirectiveId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Directive type.</summary>
    public SteeringType Type { get; init; }

    /// <summary>Target tool ID (if specific).</summary>
    public string? TargetToolId { get; init; }

    /// <summary>Target tool category.</summary>
    public string? TargetCategory { get; init; }

    /// <summary>Steering instruction.</summary>
    public required string Instruction { get; init; }

    /// <summary>Priority (higher = more important).</summary>
    public int Priority { get; init; } = 100;

    /// <summary>Condition for activation.</summary>
    public string? Condition { get; init; }

    /// <summary>Whether active.</summary>
    public bool IsActive { get; init; } = true;

    /// <summary>Created at.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Type of steering.
/// </summary>
public enum SteeringType
{
    /// <summary>Prefer this tool.</summary>
    Prefer,
    /// <summary>Avoid this tool.</summary>
    Avoid,
    /// <summary>Require this tool.</summary>
    Require,
    /// <summary>Parameter guidance.</summary>
    ParameterGuidance,
    /// <summary>Ordering guidance.</summary>
    Ordering,
    /// <summary>Conditional use.</summary>
    Conditional
}

/// <summary>
/// Steering context for a request.
/// </summary>
public class SteeringContext
{
    /// <summary>User prompt.</summary>
    public required string UserPrompt { get; init; }

    /// <summary>Task type.</summary>
    public string? TaskType { get; init; }

    /// <summary>Available tools.</summary>
    public IReadOnlyList<string> AvailableTools { get; init; } = Array.Empty<string>();

    /// <summary>Previous tool calls in session.</summary>
    public IReadOnlyList<string> PreviousToolCalls { get; init; } = Array.Empty<string>();

    /// <summary>Session metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Steering recommendation.
/// </summary>
public class SteeringRecommendation
{
    /// <summary>Recommended tools in order.</summary>
    public IReadOnlyList<string> RecommendedTools { get; init; } = Array.Empty<string>();

    /// <summary>Tools to avoid.</summary>
    public IReadOnlyList<string> AvoidTools { get; init; } = Array.Empty<string>();

    /// <summary>Parameter suggestions per tool.</summary>
    public Dictionary<string, Dictionary<string, string>> ParameterSuggestions { get; init; } = new();

    /// <summary>Steering prompt to inject.</summary>
    public string? SteeringPrompt { get; init; }

    /// <summary>Applied directives.</summary>
    public IReadOnlyList<string> AppliedDirectives { get; init; } = Array.Empty<string>();

    /// <summary>Confidence.</summary>
    public double Confidence { get; init; }
}

/// <summary>
/// Port for tool use steering via prompting.
/// Implements the "Tool Use Steering via Prompting" pattern.
/// </summary>
public interface IToolSteeringPort
{
    /// <summary>
    /// Gets steering recommendations for a context.
    /// </summary>
    Task<SteeringRecommendation> GetSteeringAsync(
        SteeringContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a steering directive.
    /// </summary>
    Task<ToolSteeringDirective> AddDirectiveAsync(
        ToolSteeringDirective directive,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a directive.
    /// </summary>
    Task<ToolSteeringDirective> UpdateDirectiveAsync(
        ToolSteeringDirective directive,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a directive.
    /// </summary>
    Task RemoveDirectiveAsync(
        string directiveId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists directives.
    /// </summary>
    Task<IReadOnlyList<ToolSteeringDirective>> ListDirectivesAsync(
        string? toolId = null,
        SteeringType? type = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a steering prompt.
    /// </summary>
    Task<string> GenerateSteeringPromptAsync(
        SteeringContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records steering outcome for learning.
    /// </summary>
    Task RecordOutcomeAsync(
        string directiveId,
        bool successful,
        string? feedback = null,
        CancellationToken cancellationToken = default);
}
