namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - UX Pattern
// Reason: No scaffolding generation needed for current use cases
// Reconsideration: If project scaffolding becomes a requirement
// ============================================================================

/// <summary>
/// Scaffolding template.
/// </summary>
public class ScaffoldingTemplate
{
    /// <summary>Template identifier.</summary>
    public required string TemplateId { get; init; }

    /// <summary>Template name.</summary>
    public required string Name { get; init; }

    /// <summary>Template type.</summary>
    public required string Type { get; init; }

    /// <summary>Files to generate.</summary>
    public IReadOnlyList<string> Files { get; init; } = Array.Empty<string>();
}

/// <summary>
/// [LOW PRIORITY] Port for agent-assisted scaffolding.
/// Implements the "Agent-Assisted Scaffolding" pattern.
///
/// This is a low-priority pattern because scaffolding generation
/// is not needed for current use cases.
/// </summary>
public interface IScaffoldingPort
{
    /// <summary>Generates scaffolding.</summary>
    Task<IReadOnlyList<(string Path, string Content)>> GenerateAsync(string templateId, Dictionary<string, string> parameters, CancellationToken cancellationToken = default);

    /// <summary>Lists templates.</summary>
    Task<IReadOnlyList<ScaffoldingTemplate>> ListTemplatesAsync(CancellationToken cancellationToken = default);

    /// <summary>Previews scaffolding.</summary>
    Task<IReadOnlyList<string>> PreviewAsync(string templateId, Dictionary<string, string> parameters, CancellationToken cancellationToken = default);
}
