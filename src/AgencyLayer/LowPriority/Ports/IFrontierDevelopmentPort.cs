namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - Workspace-Native Pattern
// Reason: Using stable models intentionally for production reliability
// Reconsideration: If frontier model capabilities become critical
// ============================================================================

/// <summary>
/// Frontier model.
/// </summary>
public class FrontierModel
{
    /// <summary>Model identifier.</summary>
    public required string ModelId { get; init; }

    /// <summary>Model name.</summary>
    public required string Name { get; init; }

    /// <summary>Release date.</summary>
    public DateTimeOffset ReleaseDate { get; init; }

    /// <summary>Capabilities.</summary>
    public IReadOnlyList<string> Capabilities { get; init; } = Array.Empty<string>();

    /// <summary>Is experimental.</summary>
    public bool IsExperimental { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for frontier-focused development.
/// Implements the "Frontier-Focused Development" pattern.
///
/// This is a low-priority pattern because stable models are used
/// intentionally for production reliability.
/// </summary>
public interface IFrontierDevelopmentPort
{
    /// <summary>Gets frontier models.</summary>
    Task<IReadOnlyList<FrontierModel>> GetFrontierModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>Evaluates frontier model.</summary>
    Task<Dictionary<string, double>> EvaluateModelAsync(string modelId, CancellationToken cancellationToken = default);

    /// <summary>Opts into frontier model.</summary>
    Task OptInAsync(string modelId, CancellationToken cancellationToken = default);

    /// <summary>Reports frontier model issue.</summary>
    Task ReportIssueAsync(string modelId, string issue, CancellationToken cancellationToken = default);
}
