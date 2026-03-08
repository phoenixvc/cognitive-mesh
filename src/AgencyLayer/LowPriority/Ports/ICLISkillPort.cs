namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - CLI-Specific Pattern
// Reason: Architecture is web API-centric; no CLI tooling planned
// Reconsideration: If cognitive-mesh adds CLI tooling
// ============================================================================

/// <summary>
/// A CLI skill definition.
/// </summary>
public class CLISkill
{
    /// <summary>Skill identifier.</summary>
    public required string SkillId { get; init; }

    /// <summary>Command name.</summary>
    public required string Command { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Arguments.</summary>
    public IReadOnlyList<CLIArgument> Arguments { get; init; } = Array.Empty<CLIArgument>();

    /// <summary>Examples.</summary>
    public IReadOnlyList<string> Examples { get; init; } = Array.Empty<string>();
}

/// <summary>
/// CLI argument.
/// </summary>
public class CLIArgument
{
    /// <summary>Name.</summary>
    public required string Name { get; init; }

    /// <summary>Type.</summary>
    public required string Type { get; init; }

    /// <summary>Required.</summary>
    public bool Required { get; init; }

    /// <summary>Description.</summary>
    public string? Description { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for CLI-first skill design.
/// Implements the "CLI-First Skill Design" pattern.
///
/// This is a low-priority pattern because cognitive-mesh uses a web API-centric
/// architecture. Consider implementing if CLI tooling is added.
/// </summary>
public interface ICLISkillPort
{
    /// <summary>Registers a CLI skill.</summary>
    Task RegisterSkillAsync(CLISkill skill, CancellationToken cancellationToken = default);

    /// <summary>Executes a CLI skill.</summary>
    Task<string> ExecuteSkillAsync(string command, IEnumerable<string> args, CancellationToken cancellationToken = default);

    /// <summary>Lists available skills.</summary>
    Task<IReadOnlyList<CLISkill>> ListSkillsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets skill help.</summary>
    Task<string> GetHelpAsync(string command, CancellationToken cancellationToken = default);
}
