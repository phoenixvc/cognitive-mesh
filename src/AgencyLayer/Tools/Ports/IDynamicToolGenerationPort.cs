namespace AgencyLayer.Tools.Ports;

/// <summary>
/// Type of generated tool.
/// </summary>
public enum GeneratedToolType
{
    /// <summary>API wrapper tool.</summary>
    ApiWrapper,
    /// <summary>Data transformation tool.</summary>
    DataTransformation,
    /// <summary>File operation tool.</summary>
    FileOperation,
    /// <summary>Computation tool.</summary>
    Computation,
    /// <summary>Integration tool.</summary>
    Integration,
    /// <summary>Custom logic tool.</summary>
    CustomLogic
}

/// <summary>
/// A specification for generating a tool.
/// </summary>
public class ToolSpecification
{
    /// <summary>Name for the tool.</summary>
    public required string Name { get; init; }

    /// <summary>Description of what the tool should do.</summary>
    public required string Description { get; init; }

    /// <summary>Type of tool to generate.</summary>
    public GeneratedToolType Type { get; init; } = GeneratedToolType.CustomLogic;

    /// <summary>Input parameters.</summary>
    public IReadOnlyList<ToolParameter> InputParameters { get; init; } = Array.Empty<ToolParameter>();

    /// <summary>Expected output format.</summary>
    public string? OutputFormat { get; init; }

    /// <summary>Example inputs and outputs.</summary>
    public IReadOnlyList<ToolExample> Examples { get; init; } = Array.Empty<ToolExample>();

    /// <summary>Constraints or requirements.</summary>
    public IReadOnlyList<string> Constraints { get; init; } = Array.Empty<string>();

    /// <summary>Dependencies on other tools.</summary>
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();

    /// <summary>Runtime environment (python, node, dotnet).</summary>
    public string? Runtime { get; init; }
}

/// <summary>
/// A parameter for a tool.
/// </summary>
public class ToolParameter
{
    /// <summary>Parameter name.</summary>
    public required string Name { get; init; }

    /// <summary>Parameter type.</summary>
    public required string Type { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Whether required.</summary>
    public bool Required { get; init; } = true;

    /// <summary>Default value.</summary>
    public string? DefaultValue { get; init; }

    /// <summary>Validation pattern.</summary>
    public string? ValidationPattern { get; init; }

    /// <summary>Allowed values (for enums).</summary>
    public IReadOnlyList<string>? AllowedValues { get; init; }
}

/// <summary>
/// An example for a tool.
/// </summary>
public class ToolExample
{
    /// <summary>Description of the example.</summary>
    public string? Description { get; init; }

    /// <summary>Input values.</summary>
    public Dictionary<string, string> Input { get; init; } = new();

    /// <summary>Expected output.</summary>
    public required string ExpectedOutput { get; init; }
}

/// <summary>
/// A generated tool.
/// </summary>
public class GeneratedTool
{
    /// <summary>Tool identifier.</summary>
    public string ToolId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Tool name.</summary>
    public required string Name { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>JSON Schema for input.</summary>
    public required string InputSchema { get; init; }

    /// <summary>Generated code.</summary>
    public required string GeneratedCode { get; init; }

    /// <summary>Runtime for the code.</summary>
    public required string Runtime { get; init; }

    /// <summary>Whether the tool is validated.</summary>
    public bool IsValidated { get; init; }

    /// <summary>Validation results.</summary>
    public IReadOnlyList<string> ValidationResults { get; init; } = Array.Empty<string>();

    /// <summary>When generated.</summary>
    public DateTimeOffset GeneratedAt { get; init; }

    /// <summary>Version.</summary>
    public int Version { get; init; } = 1;
}

/// <summary>
/// Result of tool generation.
/// </summary>
public class ToolGenerationResult
{
    /// <summary>Whether generation succeeded.</summary>
    public required bool Success { get; init; }

    /// <summary>The generated tool.</summary>
    public GeneratedTool? Tool { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Warnings during generation.</summary>
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

    /// <summary>Generation duration.</summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Port for dynamic tool generation.
/// Implements the "LLM-Generated Tool Definitions" and "Dynamic Plugin Loading" patterns.
/// </summary>
/// <remarks>
/// This port enables agents to generate new tools dynamically based on
/// specifications, allowing the system to extend its capabilities
/// without pre-defined tool implementations.
/// </remarks>
public interface IDynamicToolGenerationPort
{
    /// <summary>
    /// Generates a tool from a specification.
    /// </summary>
    /// <param name="specification">The tool specification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generation result.</returns>
    Task<ToolGenerationResult> GenerateToolAsync(
        ToolSpecification specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a tool from natural language description.
    /// </summary>
    /// <param name="description">Natural language description.</param>
    /// <param name="examples">Optional examples.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generation result.</returns>
    Task<ToolGenerationResult> GenerateFromDescriptionAsync(
        string description,
        IReadOnlyList<ToolExample>? examples = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a generated tool.
    /// </summary>
    /// <param name="tool">The tool to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Whether valid and any issues.</returns>
    Task<(bool IsValid, IReadOnlyList<string> Issues)> ValidateToolAsync(
        GeneratedTool tool,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests a generated tool with examples.
    /// </summary>
    /// <param name="tool">The tool to test.</param>
    /// <param name="examples">Test examples.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Test results.</returns>
    Task<IReadOnlyList<(ToolExample Example, bool Passed, string? Actual)>> TestToolAsync(
        GeneratedTool tool,
        IReadOnlyList<ToolExample> examples,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a generated tool for use.
    /// </summary>
    /// <param name="tool">The tool to register.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The registered tool ID.</returns>
    Task<string> RegisterToolAsync(
        GeneratedTool tool,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a generated tool.
    /// </summary>
    /// <param name="toolId">The tool ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tool.</returns>
    Task<GeneratedTool?> GetToolAsync(
        string toolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists generated tools.
    /// </summary>
    /// <param name="type">Filter by type (null = all).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated tools.</returns>
    Task<IReadOnlyList<GeneratedTool>> ListToolsAsync(
        GeneratedToolType? type = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invokes a generated tool.
    /// </summary>
    /// <param name="toolId">The tool ID.</param>
    /// <param name="input">Input parameters (JSON).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tool output.</returns>
    Task<string> InvokeToolAsync(
        string toolId,
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a tool with improvements.
    /// </summary>
    /// <param name="toolId">The tool ID.</param>
    /// <param name="feedback">Feedback for improvement.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated tool.</returns>
    Task<GeneratedTool> ImproveToolAsync(
        string toolId,
        string feedback,
        CancellationToken cancellationToken = default);
}
