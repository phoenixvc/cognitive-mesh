namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - CLI-Specific Pattern
// Reason: ASP.NET Core web API architecture; CLI not planned
// Reconsideration: If developer productivity requires shell integration
// ============================================================================

/// <summary>
/// CLI orchestration session.
/// </summary>
public class CLISession
{
    /// <summary>Session identifier.</summary>
    public required string SessionId { get; init; }

    /// <summary>Working directory.</summary>
    public required string WorkingDirectory { get; init; }

    /// <summary>Environment variables.</summary>
    public Dictionary<string, string> Environment { get; init; } = new();

    /// <summary>Command history.</summary>
    public IReadOnlyList<string> History { get; init; } = Array.Empty<string>();
}

/// <summary>
/// [LOW PRIORITY] Port for CLI-native agent orchestration.
/// Implements the "CLI-Native Agent Orchestration" pattern.
///
/// This is a low-priority pattern because cognitive-mesh uses ASP.NET Core
/// web API architecture. CLI orchestration is not currently planned.
/// </summary>
public interface ICLIOrchestrationPort
{
    /// <summary>Creates a CLI session.</summary>
    Task<CLISession> CreateSessionAsync(string workingDirectory, CancellationToken cancellationToken = default);

    /// <summary>Executes a command in session.</summary>
    Task<(int ExitCode, string Output, string Error)> ExecuteAsync(string sessionId, string command, CancellationToken cancellationToken = default);

    /// <summary>Gets session history.</summary>
    Task<IReadOnlyList<string>> GetHistoryAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>Ends a session.</summary>
    Task EndSessionAsync(string sessionId, CancellationToken cancellationToken = default);
}
