namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - Workspace-Native Pattern
// Reason: No code-gen-then-execute pipeline in scope
// Reconsideration: If code generation workflows are prioritized
// ============================================================================

/// <summary>
/// Generated code with immutable revision tracking.
/// </summary>
public class GeneratedCode
{
    /// <summary>Code identifier.</summary>
    public required string CodeId { get; init; }

    /// <summary>
    /// Immutable revision identifier (content hash).
    /// Must be used in ValidateAsync/ExecuteAsync to ensure TOCTOU safety.
    /// </summary>
    public required string RevisionId { get; init; }

    /// <summary>Language.</summary>
    public required string Language { get; init; }

    /// <summary>Code content.</summary>
    public required string Code { get; init; }

    /// <summary>Is validated.</summary>
    public bool IsValidated { get; init; }

    /// <summary>Revision ID that was validated (if validated).</summary>
    public string? ValidatedRevisionId { get; init; }
}

/// <summary>
/// Execution result.
/// </summary>
public class CodeExecuteResult
{
    /// <summary>Code identifier.</summary>
    public required string CodeId { get; init; }

    /// <summary>Revision ID that was executed (immutable snapshot).</summary>
    public required string RevisionId { get; init; }

    /// <summary>Output.</summary>
    public required string Output { get; init; }

    /// <summary>Success.</summary>
    public bool Success { get; init; }

    /// <summary>Errors.</summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Validation result with revision tracking.
/// </summary>
public class CodeValidationResult
{
    /// <summary>Code identifier.</summary>
    public required string CodeId { get; init; }

    /// <summary>Revision ID that was validated.</summary>
    public required string RevisionId { get; init; }

    /// <summary>Whether the code is valid.</summary>
    public bool Valid { get; init; }

    /// <summary>Validation issues found.</summary>
    public IReadOnlyList<string> Issues { get; init; } = Array.Empty<string>();
}

/// <summary>
/// [LOW PRIORITY] Port for code-then-execute pattern.
/// Implements the "Code-Then-Execute Pattern".
///
/// This is a low-priority pattern because no code-gen-then-execute
/// pipeline is in scope for current architecture.
///
/// Security: All operations use immutable revision IDs to prevent TOCTOU attacks.
/// The same revisionId from GenerateAsync must be passed to ValidateAsync and ExecuteAsync
/// to ensure the validated code is the same code that gets executed.
/// </summary>
public interface ICodeThenExecutePort
{
    /// <summary>
    /// Generates code and returns an immutable snapshot with a revision ID.
    /// The revisionId must be used in subsequent Validate/Execute calls.
    /// </summary>
    Task<GeneratedCode> GenerateAsync(
        string prompt,
        string language,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates generated code for a specific immutable revision.
    /// </summary>
    /// <param name="codeId">Code identifier.</param>
    /// <param name="revisionId">Immutable revision ID from GenerateAsync.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with the validated revision ID.</returns>
    Task<CodeValidationResult> ValidateAsync(
        string codeId,
        string revisionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes generated code for a specific immutable revision.
    /// Should only execute revisions that have been validated.
    /// </summary>
    /// <param name="codeId">Code identifier.</param>
    /// <param name="revisionId">Immutable revision ID (should match validated revision).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Execution result with the executed revision ID.</returns>
    Task<CodeExecuteResult> ExecuteAsync(
        string codeId,
        string revisionId,
        CancellationToken cancellationToken = default);
}
