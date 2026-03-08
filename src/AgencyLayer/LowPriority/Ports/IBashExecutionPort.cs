namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - CLI-Specific Pattern
// Reason: No bash execution in scope for web API architecture
// Reconsideration: If shell integration becomes a requirement
// ============================================================================

/// <summary>
/// Bash execution result.
/// </summary>
public class BashResult
{
    /// <summary>Exit code.</summary>
    public int ExitCode { get; init; }

    /// <summary>Standard output.</summary>
    public required string Stdout { get; init; }

    /// <summary>Standard error.</summary>
    public required string Stderr { get; init; }

    /// <summary>Execution time.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Whether successful.</summary>
    public bool Success => ExitCode == 0;
}

/// <summary>
/// [LOW PRIORITY] Port for intelligent bash tool execution.
/// Implements the "Intelligent Bash Tool Execution" pattern.
///
/// This is a low-priority pattern because bash execution is not
/// in scope for the current web API-centric architecture.
/// </summary>
public interface IBashExecutionPort
{
    /// <summary>Executes a bash command.</summary>
    Task<BashResult> ExecuteAsync(string command, string? workingDirectory = null, CancellationToken cancellationToken = default);

    /// <summary>Executes with timeout.</summary>
    Task<BashResult> ExecuteWithTimeoutAsync(string command, TimeSpan timeout, CancellationToken cancellationToken = default);

    /// <summary>Validates command before execution.</summary>
    Task<(bool Valid, string? Error)> ValidateAsync(string command, CancellationToken cancellationToken = default);

    /// <summary>Gets command suggestions.</summary>
    Task<IReadOnlyList<string>> SuggestAsync(string partialCommand, CancellationToken cancellationToken = default);
}
