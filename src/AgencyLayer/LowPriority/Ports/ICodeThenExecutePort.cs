namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - Workspace-Native Pattern
// Reason: No code-gen-then-execute pipeline in scope
// Reconsideration: If code generation workflows are prioritized
// ============================================================================

/// <summary>
/// Generated code.
/// </summary>
public class GeneratedCode
{
    /// <summary>Code identifier.</summary>
    public required string CodeId { get; init; }

    /// <summary>Language.</summary>
    public required string Language { get; init; }

    /// <summary>Code content.</summary>
    public required string Code { get; init; }

    /// <summary>Is validated.</summary>
    public bool IsValidated { get; init; }
}

/// <summary>
/// Execution result.
/// </summary>
public class CodeExecuteResult
{
    /// <summary>Code identifier.</summary>
    public required string CodeId { get; init; }

    /// <summary>Output.</summary>
    public required string Output { get; init; }

    /// <summary>Success.</summary>
    public bool Success { get; init; }

    /// <summary>Errors.</summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}

/// <summary>
/// [LOW PRIORITY] Port for code-then-execute pattern.
/// Implements the "Code-Then-Execute Pattern".
///
/// This is a low-priority pattern because no code-gen-then-execute
/// pipeline is in scope for current architecture.
/// </summary>
public interface ICodeThenExecutePort
{
    /// <summary>Generates code.</summary>
    Task<GeneratedCode> GenerateAsync(string prompt, string language, CancellationToken cancellationToken = default);

    /// <summary>Validates generated code.</summary>
    Task<(bool Valid, IReadOnlyList<string> Issues)> ValidateAsync(string codeId, CancellationToken cancellationToken = default);

    /// <summary>Executes generated code.</summary>
    Task<CodeExecuteResult> ExecuteAsync(string codeId, CancellationToken cancellationToken = default);
}
