namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Type of validation error.
/// </summary>
public enum ValidationErrorType
{
    /// <summary>Schema structure violation.</summary>
    SchemaViolation,
    /// <summary>Type mismatch.</summary>
    TypeMismatch,
    /// <summary>Required field missing.</summary>
    MissingRequired,
    /// <summary>Value out of range.</summary>
    OutOfRange,
    /// <summary>Pattern mismatch.</summary>
    PatternMismatch,
    /// <summary>Format violation.</summary>
    FormatViolation,
    /// <summary>Constraint violation.</summary>
    ConstraintViolation
}

/// <summary>
/// A validation error with learning context.
/// </summary>
public class ValidationError
{
    /// <summary>Error identifier.</summary>
    public string ErrorId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Error type.</summary>
    public required ValidationErrorType Type { get; init; }

    /// <summary>JSON path to the error.</summary>
    public required string Path { get; init; }

    /// <summary>Error message.</summary>
    public required string Message { get; init; }

    /// <summary>Expected value or pattern.</summary>
    public string? Expected { get; init; }

    /// <summary>Actual value received.</summary>
    public string? Actual { get; init; }

    /// <summary>Suggested fix.</summary>
    public string? SuggestedFix { get; init; }
}

/// <summary>
/// Learning from validation errors.
/// </summary>
public class ValidationLearning
{
    /// <summary>Error pattern that was learned.</summary>
    public required string ErrorPattern { get; init; }

    /// <summary>Correction that works.</summary>
    public required string Correction { get; init; }

    /// <summary>Times this pattern has been seen.</summary>
    public int OccurrenceCount { get; init; }

    /// <summary>Success rate of the correction.</summary>
    public double SuccessRate { get; init; }

    /// <summary>Steps this applies to.</summary>
    public IReadOnlyList<string> ApplicableSteps { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Result of schema validation.
/// </summary>
public class SchemaValidationResult
{
    /// <summary>Whether validation passed.</summary>
    public required bool IsValid { get; init; }

    /// <summary>Validation errors.</summary>
    public IReadOnlyList<ValidationError> Errors { get; init; } = Array.Empty<ValidationError>();

    /// <summary>Corrected output if auto-correction succeeded.</summary>
    public string? CorrectedOutput { get; init; }

    /// <summary>Whether auto-correction was applied.</summary>
    public bool WasCorrected { get; init; }

    /// <summary>Retry count used.</summary>
    public int RetryCount { get; init; }

    /// <summary>Learnings applied during correction.</summary>
    public IReadOnlyList<ValidationLearning> LearningsApplied { get; init; } = Array.Empty<ValidationLearning>();
}

/// <summary>
/// Configuration for schema validation with retry.
/// </summary>
public class SchemaRetryConfiguration
{
    /// <summary>Maximum retry attempts.</summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>Whether to use cross-step learning.</summary>
    public bool EnableCrossStepLearning { get; init; } = true;

    /// <summary>Whether to auto-correct where possible.</summary>
    public bool EnableAutoCorrection { get; init; } = true;

    /// <summary>Whether to provide fix hints to the model.</summary>
    public bool ProvideFixHints { get; init; } = true;

    /// <summary>Backoff multiplier between retries.</summary>
    public double BackoffMultiplier { get; init; } = 1.5;

    /// <summary>Initial backoff in milliseconds.</summary>
    public int InitialBackoffMs { get; init; } = 100;
}

/// <summary>
/// Request to validate against schema with retry.
/// </summary>
public class SchemaValidationRequest
{
    /// <summary>Request identifier.</summary>
    public string RequestId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>The output to validate.</summary>
    public required string Output { get; init; }

    /// <summary>The JSON schema to validate against.</summary>
    public required string Schema { get; init; }

    /// <summary>Step identifier for cross-step learning.</summary>
    public string? StepId { get; init; }

    /// <summary>Workflow identifier for cross-workflow learning.</summary>
    public string? WorkflowId { get; init; }

    /// <summary>Retry configuration.</summary>
    public SchemaRetryConfiguration Configuration { get; init; } = new();

    /// <summary>Context for correction (original prompt, etc.).</summary>
    public string? CorrectionContext { get; init; }
}

/// <summary>
/// Port for schema validation with retry and cross-step learning.
/// Implements the "Schema Validation Retry with Cross-Step Learning" pattern.
/// </summary>
/// <remarks>
/// This port validates outputs against JSON schemas and learns from
/// validation failures to improve corrections across steps and
/// workflows over time.
/// </remarks>
public interface ISchemaValidationRetryPort
{
    /// <summary>
    /// Validates output against schema with retry and learning.
    /// </summary>
    /// <param name="request">The validation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validation result.</returns>
    Task<SchemaValidationResult> ValidateWithRetryAsync(
        SchemaValidationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates output without retry.
    /// </summary>
    /// <param name="output">The output to validate.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation errors.</returns>
    Task<IReadOnlyList<ValidationError>> ValidateAsync(
        string output,
        string schema,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a successful validation for learning.
    /// </summary>
    /// <param name="stepId">The step ID.</param>
    /// <param name="output">The valid output.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordSuccessAsync(
        string stepId,
        string output,
        string schema,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets learnings for a step.
    /// </summary>
    /// <param name="stepId">The step ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Learnings for this step.</returns>
    Task<IReadOnlyList<ValidationLearning>> GetLearningsAsync(
        string stepId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to auto-correct output based on learnings.
    /// </summary>
    /// <param name="output">The invalid output.</param>
    /// <param name="errors">Validation errors.</param>
    /// <param name="learnings">Available learnings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Corrected output if possible.</returns>
    Task<string?> AttemptCorrectionAsync(
        string output,
        IReadOnlyList<ValidationError> errors,
        IReadOnlyList<ValidationLearning> learnings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets validation statistics.
    /// </summary>
    /// <param name="workflowId">Filter by workflow (null = all).</param>
    /// <param name="since">Start time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Statistics.</returns>
    Task<ValidationStatistics> GetStatisticsAsync(
        string? workflowId = null,
        DateTimeOffset? since = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Validation statistics.
/// </summary>
public class ValidationStatistics
{
    public int TotalValidations { get; init; }
    public int FirstAttemptSuccesses { get; init; }
    public int SuccessesAfterRetry { get; init; }
    public int Failures { get; init; }
    public double FirstAttemptSuccessRate { get; init; }
    public double OverallSuccessRate { get; init; }
    public double AverageRetries { get; init; }
    public Dictionary<ValidationErrorType, int> ErrorsByType { get; init; } = new();
    public int LearningsGenerated { get; init; }
    public int LearningsApplied { get; init; }
}
