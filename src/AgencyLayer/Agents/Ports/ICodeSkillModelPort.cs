namespace AgencyLayer.Agents.Ports;

/// <summary>
/// A code-specialized skill model.
/// </summary>
public class CodeSkillModel
{
    /// <summary>Model identifier.</summary>
    public required string ModelId { get; init; }

    /// <summary>Model name.</summary>
    public required string Name { get; init; }

    /// <summary>Supported languages.</summary>
    public IReadOnlyList<string> SupportedLanguages { get; init; } = Array.Empty<string>();

    /// <summary>Capabilities.</summary>
    public IReadOnlyList<CodeCapability> Capabilities { get; init; } = Array.Empty<CodeCapability>();

    /// <summary>Whether active.</summary>
    public bool IsActive { get; init; } = true;

    /// <summary>Model endpoint.</summary>
    public required string Endpoint { get; init; }

    /// <summary>Configuration.</summary>
    public Dictionary<string, string> Configuration { get; init; } = new();
}

/// <summary>
/// Code capability.
/// </summary>
public enum CodeCapability
{
    /// <summary>Code completion.</summary>
    Completion,
    /// <summary>Code explanation.</summary>
    Explanation,
    /// <summary>Code review.</summary>
    Review,
    /// <summary>Bug fixing.</summary>
    BugFix,
    /// <summary>Refactoring.</summary>
    Refactoring,
    /// <summary>Test generation.</summary>
    TestGeneration,
    /// <summary>Documentation.</summary>
    Documentation,
    /// <summary>Translation between languages.</summary>
    Translation
}

/// <summary>
/// Code task request.
/// </summary>
public class CodeTaskRequest
{
    /// <summary>Task type.</summary>
    public CodeCapability TaskType { get; init; }

    /// <summary>Code input.</summary>
    public required string Code { get; init; }

    /// <summary>Language.</summary>
    public required string Language { get; init; }

    /// <summary>Additional context.</summary>
    public string? Context { get; init; }

    /// <summary>Instructions.</summary>
    public string? Instructions { get; init; }

    /// <summary>Target language (for translation).</summary>
    public string? TargetLanguage { get; init; }
}

/// <summary>
/// Code task result.
/// </summary>
public class CodeTaskResult
{
    /// <summary>Output code or text.</summary>
    public required string Output { get; init; }

    /// <summary>Model used.</summary>
    public required string ModelId { get; init; }

    /// <summary>Confidence.</summary>
    public double Confidence { get; init; }

    /// <summary>Explanation.</summary>
    public string? Explanation { get; init; }

    /// <summary>Suggestions.</summary>
    public IReadOnlyList<string> Suggestions { get; init; } = Array.Empty<string>();

    /// <summary>Execution time.</summary>
    public TimeSpan ExecutionTime { get; init; }
}

/// <summary>
/// Port for merged code + language skill model.
/// Implements the "Merged Code + Language Skill Model" pattern.
/// </summary>
public interface ICodeSkillModelPort
{
    /// <summary>
    /// Executes a code task.
    /// </summary>
    Task<CodeTaskResult> ExecuteTaskAsync(
        CodeTaskRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available models.
    /// </summary>
    Task<IReadOnlyList<CodeSkillModel>> GetModelsAsync(
        string? language = null,
        CodeCapability? capability = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a code skill model.
    /// </summary>
    Task RegisterModelAsync(
        CodeSkillModel model,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Selects best model for a task.
    /// </summary>
    Task<CodeSkillModel?> SelectModelAsync(
        string language,
        CodeCapability capability,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes code.
    /// </summary>
    Task<string> CompleteCodeAsync(
        string code,
        string language,
        string? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Explains code.
    /// </summary>
    Task<string> ExplainCodeAsync(
        string code,
        string language,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates tests for code.
    /// </summary>
    Task<string> GenerateTestsAsync(
        string code,
        string language,
        string? framework = null,
        CancellationToken cancellationToken = default);
}
