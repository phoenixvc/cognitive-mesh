namespace AgencyLayer.Tools.Ports;

/// <summary>
/// Type of validation to perform.
/// </summary>
public enum ValidationLevel
{
    /// <summary>Basic syntax and structure check.</summary>
    Syntax,
    /// <summary>Semantic correctness check.</summary>
    Semantic,
    /// <summary>Compilation or type checking.</summary>
    Compilation,
    /// <summary>Runtime execution test.</summary>
    Runtime,
    /// <summary>Full validation including all levels.</summary>
    Full
}

/// <summary>
/// Result of a validation check.
/// </summary>
public enum ValidationResultStatus
{
    /// <summary>Validation passed.</summary>
    Passed,
    /// <summary>Validation passed with warnings.</summary>
    PassedWithWarnings,
    /// <summary>Validation failed.</summary>
    Failed,
    /// <summary>Validation could not be performed.</summary>
    Skipped
}

/// <summary>
/// A validation issue found.
/// </summary>
public class ValidationIssue
{
    /// <summary>Severity of the issue.</summary>
    public required string Severity { get; init; }

    /// <summary>Issue code or identifier.</summary>
    public string? Code { get; init; }

    /// <summary>Description of the issue.</summary>
    public required string Message { get; init; }

    /// <summary>Location in the output (line number, etc.).</summary>
    public string? Location { get; init; }

    /// <summary>Suggested fix.</summary>
    public string? SuggestedFix { get; init; }
}

/// <summary>
/// Request to validate subagent output.
/// </summary>
public class SubagentValidationRequest
{
    /// <summary>Unique request identifier.</summary>
    public string RequestId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>The subagent that produced the output.</summary>
    public required string SubagentId { get; init; }

    /// <summary>Type of output (Code, JSON, Text, etc.).</summary>
    public required string OutputType { get; init; }

    /// <summary>The output content to validate.</summary>
    public required string Output { get; init; }

    /// <summary>Language or format (e.g., "csharp", "json", "markdown").</summary>
    public string? Language { get; init; }

    /// <summary>Validation level to perform.</summary>
    public ValidationLevel Level { get; init; } = ValidationLevel.Semantic;

    /// <summary>Schema to validate against (for JSON).</summary>
    public string? Schema { get; init; }

    /// <summary>Context from the original request.</summary>
    public string? Context { get; init; }

    /// <summary>Expected output characteristics.</summary>
    public Dictionary<string, string> Expectations { get; init; } = new();

    /// <summary>Timeout for validation in milliseconds.</summary>
    public int TimeoutMs { get; init; } = 30000;
}

/// <summary>
/// Result of subagent output validation.
/// </summary>
public class SubagentValidationResult
{
    /// <summary>The request ID.</summary>
    public required string RequestId { get; init; }

    /// <summary>Overall status.</summary>
    public required ValidationResultStatus Status { get; init; }

    /// <summary>Whether the output is valid.</summary>
    public required bool IsValid { get; init; }

    /// <summary>Confidence in the validation (0.0 - 1.0).</summary>
    public double Confidence { get; init; }

    /// <summary>Issues found during validation.</summary>
    public IReadOnlyList<ValidationIssue> Issues { get; init; } = Array.Empty<ValidationIssue>();

    /// <summary>Errors (severity = error).</summary>
    public int ErrorCount { get; init; }

    /// <summary>Warnings (severity = warning).</summary>
    public int WarningCount { get; init; }

    /// <summary>Validation levels that were performed.</summary>
    public IReadOnlyList<ValidationLevel> LevelsPerformed { get; init; } = Array.Empty<ValidationLevel>();

    /// <summary>Corrected output if auto-correction was possible.</summary>
    public string? CorrectedOutput { get; init; }

    /// <summary>Whether auto-correction was applied.</summary>
    public bool WasCorrected { get; init; }

    /// <summary>Validation duration in milliseconds.</summary>
    public double DurationMs { get; init; }

    /// <summary>Detailed validation report.</summary>
    public string? DetailedReport { get; init; }
}

/// <summary>
/// Configuration for validation behavior.
/// </summary>
public class ValidationConfiguration
{
    /// <summary>Default validation level.</summary>
    public ValidationLevel DefaultLevel { get; init; } = ValidationLevel.Semantic;

    /// <summary>Whether to attempt auto-correction.</summary>
    public bool EnableAutoCorrection { get; init; } = true;

    /// <summary>Maximum correction attempts.</summary>
    public int MaxCorrectionAttempts { get; init; } = 3;

    /// <summary>Whether to cache validation results.</summary>
    public bool EnableCaching { get; init; } = true;

    /// <summary>Custom validators by output type.</summary>
    public Dictionary<string, string> CustomValidators { get; init; } = new();

    /// <summary>Severity levels that cause failure.</summary>
    public IReadOnlyList<string> FailOnSeverities { get; init; } = new[] { "error", "critical" };
}

/// <summary>
/// Port for subagent output validation.
/// Implements the "Subagent Compilation Checker" pattern.
/// </summary>
/// <remarks>
/// This port validates outputs from subagents before they are used,
/// ensuring code compiles, JSON is valid, and outputs meet expected
/// formats. It can auto-correct minor issues.
/// </remarks>
public interface ISubagentValidationPort
{
    /// <summary>
    /// Validates subagent output.
    /// </summary>
    /// <param name="request">The validation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validation result.</returns>
    Task<SubagentValidationResult> ValidateAsync(
        SubagentValidationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates code output specifically.
    /// </summary>
    /// <param name="code">The code to validate.</param>
    /// <param name="language">Programming language.</param>
    /// <param name="level">Validation level.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validation result.</returns>
    Task<SubagentValidationResult> ValidateCodeAsync(
        string code,
        string language,
        ValidationLevel level = ValidationLevel.Compilation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates JSON output against a schema.
    /// </summary>
    /// <param name="json">The JSON to validate.</param>
    /// <param name="schema">JSON Schema to validate against.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validation result.</returns>
    Task<SubagentValidationResult> ValidateJsonAsync(
        string json,
        string? schema = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to correct invalid output.
    /// </summary>
    /// <param name="output">The invalid output.</param>
    /// <param name="outputType">Type of output.</param>
    /// <param name="issues">Known issues to fix.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Corrected output if successful.</returns>
    Task<(bool Success, string? CorrectedOutput)> AttemptCorrectionAsync(
        string output,
        string outputType,
        IReadOnlyList<ValidationIssue> issues,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or sets validation configuration.
    /// </summary>
    /// <param name="configuration">Configuration to set (null = get current).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current configuration.</returns>
    Task<ValidationConfiguration> ConfigureAsync(
        ValidationConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets validation statistics.
    /// </summary>
    /// <param name="subagentId">Filter by subagent (null = all).</param>
    /// <param name="since">Start time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation statistics.</returns>
    Task<ValidationStatistics> GetStatisticsAsync(
        string? subagentId = null,
        DateTimeOffset? since = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Validation statistics.
/// </summary>
public class ValidationStatistics
{
    public int TotalValidations { get; init; }
    public int Passed { get; init; }
    public int Failed { get; init; }
    public int Corrected { get; init; }
    public double PassRate { get; init; }
    public double CorrectionRate { get; init; }
    public Dictionary<string, int> ByOutputType { get; init; } = new();
    public Dictionary<string, double> PassRateBySubagent { get; init; } = new();
    public IReadOnlyList<string> CommonIssues { get; init; } = Array.Empty<string>();
}
