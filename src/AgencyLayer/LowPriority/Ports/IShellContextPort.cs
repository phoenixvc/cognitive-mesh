namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - CLI-Specific Pattern
// Reason: No shell command execution in scope
// Reconsideration: If shell integration becomes a requirement
// ============================================================================

/// <summary>
/// Shell context.
/// </summary>
public class ShellContext
{
    /// <summary>Current directory.</summary>
    public required string CurrentDirectory { get; init; }

    /// <summary>Shell type (bash, zsh, powershell).</summary>
    public required string ShellType { get; init; }

    /// <summary>Environment variables.</summary>
    public Dictionary<string, string> Environment { get; init; } = new();

    /// <summary>Available commands.</summary>
    public IReadOnlyList<string> AvailableCommands { get; init; } = Array.Empty<string>();
}

/// <summary>
/// [LOW PRIORITY] Port for shell command contextualization.
/// Implements the "Shell Command Contextualization" pattern.
///
/// This is a low-priority pattern because shell command execution
/// is not in scope for the current architecture.
/// </summary>
public interface IShellContextPort
{
    /// <summary>Gets current shell context.</summary>
    Task<ShellContext> GetContextAsync(CancellationToken cancellationToken = default);

    /// <summary>Contextualizes a command.</summary>
    Task<string> ContextualizeCommandAsync(string command, ShellContext context, CancellationToken cancellationToken = default);

    /// <summary>Suggests commands based on context.</summary>
    Task<IReadOnlyList<string>> SuggestCommandsAsync(string intent, ShellContext context, CancellationToken cancellationToken = default);

    /// <summary>Validates command safety.</summary>
    Task<(bool Safe, string? Warning)> ValidateCommandAsync(string command, CancellationToken cancellationToken = default);
}
