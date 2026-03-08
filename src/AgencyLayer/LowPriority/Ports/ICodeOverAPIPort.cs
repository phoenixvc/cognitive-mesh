namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - CLI-Specific Pattern
// Reason: API-centric design is intentional for cognitive-mesh
// Reconsideration: If code-first patterns become preferred
// ============================================================================

/// <summary>
/// Code execution request.
/// </summary>
public class CodeExecutionRequest
{
    /// <summary>Code to execute.</summary>
    public required string Code { get; init; }

    /// <summary>Language.</summary>
    public required string Language { get; init; }

    /// <summary>Dependencies.</summary>
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();

    /// <summary>Environment.</summary>
    public Dictionary<string, string> Environment { get; init; } = new();
}

/// <summary>
/// Code execution result.
/// </summary>
public class CodeExecutionResult
{
    /// <summary>Output.</summary>
    public required string Output { get; init; }

    /// <summary>Errors.</summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    /// <summary>Success.</summary>
    public bool Success { get; init; }

    /// <summary>Duration.</summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for code-over-API pattern.
/// Implements the "Code-Over-API Pattern".
///
/// This is a low-priority pattern because API-centric design is
/// intentional for cognitive-mesh architecture.
/// </summary>
public interface ICodeOverAPIPort
{
    /// <summary>Executes code directly.</summary>
    Task<CodeExecutionResult> ExecuteCodeAsync(CodeExecutionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Validates code syntax.</summary>
    Task<(bool Valid, IReadOnlyList<string> Errors)> ValidateSyntaxAsync(string code, string language, CancellationToken cancellationToken = default);

    /// <summary>Gets available languages.</summary>
    Task<IReadOnlyList<string>> GetLanguagesAsync(CancellationToken cancellationToken = default);
}
