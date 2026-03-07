namespace FoundationLayer.Security.Ports;

/// <summary>
/// The three components of the Lethal Trifecta threat model.
/// When all three are present simultaneously, the system is at critical risk.
/// </summary>
public enum TrifectaComponent
{
    /// <summary>
    /// Agent has ability to read sensitive data (secrets, PII, credentials).
    /// </summary>
    SensitiveDataAccess,

    /// <summary>
    /// Agent has ability to communicate externally (network, email, webhooks).
    /// </summary>
    ExternalCommunication,

    /// <summary>
    /// Agent has ability to execute arbitrary code or commands.
    /// </summary>
    CodeExecution
}

/// <summary>
/// Risk level based on trifecta analysis.
/// </summary>
public enum TrifectaRiskLevel
{
    /// <summary>None of the trifecta components present.</summary>
    None,
    /// <summary>One component present.</summary>
    Low,
    /// <summary>Two components present.</summary>
    Elevated,
    /// <summary>All three components present - critical risk.</summary>
    Critical
}

/// <summary>
/// Request to analyze an agent's threat profile.
/// </summary>
public class ThreatAnalysisRequest
{
    /// <summary>The agent to analyze.</summary>
    public required string AgentId { get; init; }

    /// <summary>Specific tools to analyze (null = all granted tools).</summary>
    public IReadOnlyList<string>? ToolIds { get; init; }

    /// <summary>Include inherited permissions from authority scopes.</summary>
    public bool IncludeInheritedPermissions { get; init; } = true;

    /// <summary>Analyze hypothetical permissions (for planning).</summary>
    public IReadOnlyList<string>? HypotheticalPermissions { get; init; }
}

/// <summary>
/// Evidence of a trifecta component being present.
/// </summary>
public class TrifectaEvidence
{
    /// <summary>The component this evidence relates to.</summary>
    public required TrifectaComponent Component { get; init; }

    /// <summary>Whether this component is present.</summary>
    public required bool IsPresent { get; init; }

    /// <summary>Specific capabilities that contribute to this component.</summary>
    public IReadOnlyList<string> Capabilities { get; init; } = Array.Empty<string>();

    /// <summary>Tools that provide this capability.</summary>
    public IReadOnlyList<string> ContributingTools { get; init; } = Array.Empty<string>();

    /// <summary>Permissions that enable this capability.</summary>
    public IReadOnlyList<string> EnablingPermissions { get; init; } = Array.Empty<string>();

    /// <summary>Mitigations that reduce the risk.</summary>
    public IReadOnlyList<string> ActiveMitigations { get; init; } = Array.Empty<string>();

    /// <summary>Residual risk after mitigations (0.0 - 1.0).</summary>
    public double ResidualRisk { get; init; }
}

/// <summary>
/// Result of a threat analysis.
/// </summary>
public class ThreatAnalysisResult
{
    /// <summary>The agent analyzed.</summary>
    public required string AgentId { get; init; }

    /// <summary>Overall risk level.</summary>
    public required TrifectaRiskLevel RiskLevel { get; init; }

    /// <summary>Evidence for each trifecta component.</summary>
    public IReadOnlyDictionary<TrifectaComponent, TrifectaEvidence> Evidence { get; init; }
        = new Dictionary<TrifectaComponent, TrifectaEvidence>();

    /// <summary>Count of trifecta components present.</summary>
    public int ComponentsPresent { get; init; }

    /// <summary>Overall risk score (0.0 - 1.0).</summary>
    public double RiskScore { get; init; }

    /// <summary>Recommended mitigations to reduce risk.</summary>
    public IReadOnlyList<ThreatMitigation> RecommendedMitigations { get; init; } = Array.Empty<ThreatMitigation>();

    /// <summary>When the analysis was performed.</summary>
    public DateTimeOffset AnalyzedAt { get; init; }
}

/// <summary>
/// A recommended mitigation action.
/// </summary>
public class ThreatMitigation
{
    /// <summary>Unique identifier for this mitigation.</summary>
    public string MitigationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Which component this mitigates.</summary>
    public required TrifectaComponent TargetComponent { get; init; }

    /// <summary>Description of the mitigation.</summary>
    public required string Description { get; init; }

    /// <summary>Type of mitigation (RemovePermission, AddControl, SplitCapability).</summary>
    public required string MitigationType { get; init; }

    /// <summary>Impact on functionality (0.0 = none, 1.0 = severe).</summary>
    public double FunctionalityImpact { get; init; }

    /// <summary>Risk reduction if applied (0.0 - 1.0).</summary>
    public double RiskReduction { get; init; }

    /// <summary>Whether this mitigation is automatically enforceable.</summary>
    public bool IsAutomatable { get; init; }
}

/// <summary>
/// Alert when a critical threat condition is detected.
/// </summary>
public class ThreatAlert
{
    public required string AlertId { get; init; }
    public required string AgentId { get; init; }
    public required TrifectaRiskLevel RiskLevel { get; init; }
    public required string Description { get; init; }
    public IReadOnlyList<TrifectaComponent> ComponentsInvolved { get; init; } = Array.Empty<TrifectaComponent>();
    public DateTimeOffset DetectedAt { get; init; }
    public bool IsAcknowledged { get; init; }
    public string? AcknowledgedBy { get; init; }
}

/// <summary>
/// Port for the Lethal Trifecta threat model.
/// Implements formal security analysis for agent capabilities.
/// </summary>
/// <remarks>
/// The Lethal Trifecta identifies the three capabilities that, when combined,
/// create maximum risk: data access + external communication + code execution.
/// This port analyzes agent configurations and raises alerts when approaching
/// or entering the danger zone.
/// </remarks>
public interface IThreatModelPort
{
    /// <summary>
    /// Analyzes an agent's threat profile.
    /// </summary>
    /// <param name="request">The analysis request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The threat analysis result.</returns>
    Task<ThreatAnalysisResult> AnalyzeAgentAsync(
        ThreatAnalysisRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a proposed permission change won't create a trifecta.
    /// </summary>
    /// <param name="agentId">The agent to check.</param>
    /// <param name="proposedPermissions">Permissions to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The threat analysis after proposed changes.</returns>
    Task<ThreatAnalysisResult> ValidatePermissionChangeAsync(
        string agentId,
        IEnumerable<string> proposedPermissions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a mitigation to reduce an agent's threat level.
    /// </summary>
    /// <param name="agentId">The agent to modify.</param>
    /// <param name="mitigationId">The mitigation to apply.</param>
    /// <param name="appliedBy">Who applied the mitigation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ApplyMitigationAsync(
        string agentId,
        string mitigationId,
        string appliedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active threat alerts.
    /// </summary>
    /// <param name="riskLevel">Minimum risk level to include.</param>
    /// <param name="includeAcknowledged">Whether to include acknowledged alerts.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active alerts.</returns>
    Task<IReadOnlyList<ThreatAlert>> GetActiveAlertsAsync(
        TrifectaRiskLevel? riskLevel = null,
        bool includeAcknowledged = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledges a threat alert.
    /// </summary>
    /// <param name="alertId">The alert to acknowledge.</param>
    /// <param name="acknowledgedBy">Who acknowledged.</param>
    /// <param name="justification">Why the alert is acceptable.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AcknowledgeAlertAsync(
        string alertId,
        string acknowledgedBy,
        string justification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the system-wide threat summary.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary of all agent threat levels.</returns>
    Task<IReadOnlyDictionary<TrifectaRiskLevel, int>> GetThreatSummaryAsync(
        CancellationToken cancellationToken = default);
}
