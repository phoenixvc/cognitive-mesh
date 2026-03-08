namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - Workspace-Native Pattern
// Reason: Server-side orchestration; not IDE-native
// Reconsideration: If IDE integrations are prioritized
// ============================================================================

/// <summary>
/// Workspace context.
/// </summary>
public class WorkspaceContext
{
    /// <summary>Workspace path.</summary>
    public required string WorkspacePath { get; init; }

    /// <summary>Open files.</summary>
    public IReadOnlyList<string> OpenFiles { get; init; } = Array.Empty<string>();

    /// <summary>Active file.</summary>
    public string? ActiveFile { get; init; }

    /// <summary>Cursor position.</summary>
    public (int Line, int Column)? CursorPosition { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for workspace-native multi-agent orchestration.
/// Implements the "Workspace-Native Multi-Agent Orchestration" pattern.
///
/// This is a low-priority pattern because cognitive-mesh uses
/// server-side orchestration, not IDE-native.
/// </summary>
public interface IWorkspaceOrchestrationPort
{
    /// <summary>Gets workspace context.</summary>
    Task<WorkspaceContext> GetContextAsync(CancellationToken cancellationToken = default);

    /// <summary>Orchestrates agents in workspace.</summary>
    Task<string> OrchestrateAsync(WorkspaceContext context, string task, CancellationToken cancellationToken = default);

    /// <summary>Syncs workspace state.</summary>
    Task SyncStateAsync(WorkspaceContext context, CancellationToken cancellationToken = default);
}
