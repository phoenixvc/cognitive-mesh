namespace AgencyLayer.Tools.Ports;

/// <summary>
/// A tool template.
/// </summary>
public class ToolTemplate
{
    /// <summary>Template identifier.</summary>
    public required string TemplateId { get; init; }

    /// <summary>Template name.</summary>
    public required string Name { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Category.</summary>
    public required string Category { get; init; }

    /// <summary>Input parameters schema.</summary>
    public required string InputSchema { get; init; }

    /// <summary>Output schema.</summary>
    public required string OutputSchema { get; init; }

    /// <summary>Implementation template.</summary>
    public required string ImplementationTemplate { get; init; }

    /// <summary>Required dependencies.</summary>
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();

    /// <summary>Example usages.</summary>
    public IReadOnlyList<ToolUsageExample> Examples { get; init; } = Array.Empty<ToolUsageExample>();
}

/// <summary>
/// Tool usage example.
/// </summary>
public class ToolUsageExample
{
    /// <summary>Input.</summary>
    public required string Input { get; init; }

    /// <summary>Expected output.</summary>
    public required string Output { get; init; }

    /// <summary>Description.</summary>
    public string? Description { get; init; }
}

/// <summary>
/// A created tool.
/// </summary>
public class CreatedTool
{
    /// <summary>Tool identifier.</summary>
    public required string ToolId { get; init; }

    /// <summary>Tool name.</summary>
    public required string Name { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Input schema (JSON).</summary>
    public required string InputSchema { get; init; }

    /// <summary>Output schema (JSON).</summary>
    public required string OutputSchema { get; init; }

    /// <summary>Implementation code.</summary>
    public required string Implementation { get; init; }

    /// <summary>Template used.</summary>
    public string? TemplateId { get; init; }

    /// <summary>Created by.</summary>
    public required string CreatedBy { get; init; }

    /// <summary>Created at.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Is validated.</summary>
    public bool IsValidated { get; init; }

    /// <summary>Is published.</summary>
    public bool IsPublished { get; init; }

    /// <summary>Version.</summary>
    public string Version { get; init; } = "1.0.0";
}

/// <summary>
/// Tool creation request.
/// </summary>
public class ToolCreationRequest
{
    /// <summary>Tool name.</summary>
    public required string Name { get; init; }

    /// <summary>Description of what the tool should do.</summary>
    public required string Description { get; init; }

    /// <summary>Template to base on (optional).</summary>
    public string? TemplateId { get; init; }

    /// <summary>Example inputs/outputs.</summary>
    public IReadOnlyList<ToolUsageExample> Examples { get; init; } = Array.Empty<ToolUsageExample>();

    /// <summary>Additional constraints.</summary>
    public IReadOnlyList<string> Constraints { get; init; } = Array.Empty<string>();

    /// <summary>Category.</summary>
    public string? Category { get; init; }
}

/// <summary>
/// Port for democratization of tooling via agents.
/// Implements the "Democratization of Tooling via Agents" pattern.
/// </summary>
public interface IToolCreationPort
{
    /// <summary>
    /// Creates a tool from description.
    /// </summary>
    Task<CreatedTool> CreateToolAsync(
        ToolCreationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a tool from natural language.
    /// </summary>
    Task<CreatedTool> CreateToolFromDescriptionAsync(
        string naturalLanguageDescription,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a created tool.
    /// </summary>
    Task<(bool Valid, IReadOnlyList<string> Issues)> ValidateToolAsync(
        string toolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests a tool with examples.
    /// </summary>
    Task<IReadOnlyList<(ToolUsageExample Example, bool Passed, string? Error)>> TestToolAsync(
        string toolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a tool for use.
    /// </summary>
    Task PublishToolAsync(
        string toolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tool.
    /// </summary>
    Task<CreatedTool?> GetToolAsync(
        string toolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists available templates.
    /// </summary>
    Task<IReadOnlyList<ToolTemplate>> ListTemplatesAsync(
        string? category = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists created tools.
    /// </summary>
    Task<IReadOnlyList<CreatedTool>> ListToolsAsync(
        string? createdBy = null,
        bool publishedOnly = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a tool.
    /// </summary>
    Task DeleteToolAsync(
        string toolId,
        CancellationToken cancellationToken = default);
}
