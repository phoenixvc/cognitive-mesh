using System.Text.Json.Serialization;

namespace CognitiveMesh.FoundationLayer.PolicyStore.Models;

/// <summary>
/// Represents a remediation policy that defines how incidents of a given category
/// and severity should be handled by the self-healing subsystem.
/// </summary>
public sealed class RemediationPolicy
{
    /// <summary>Gets or sets the unique identifier of the policy.</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Gets or sets the version number of the policy.</summary>
    public int Version { get; set; } = 1;

    /// <summary>Gets or sets a value indicating whether the policy is currently active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the incident category this policy applies to (e.g., infrastructure, application).</summary>
    public string IncidentCategory { get; set; } = string.Empty;

    /// <summary>Gets or sets the severity level this policy applies to (e.g., low, medium, high, critical).</summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>Gets or sets the set of remediation actions permitted by this policy.</summary>
    public RemediationAction AllowedActions { get; set; }

    /// <summary>Gets or sets the ranking weights used to prioritise remediation actions.</summary>
    public Dictionary<string, double> RankingWeights { get; set; } = new();

    /// <summary>Gets or sets the maximum number of retries before escalation.</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Gets or sets the number of repeated failures required to trigger escalation.</summary>
    public int RepeatedFailureEscalationThreshold { get; set; } = 5;

    /// <summary>Gets or sets a value indicating whether a human must approve remediation actions.</summary>
    public bool HumanInLoopRequired { get; set; }

    /// <summary>Gets or sets the roles permitted to approve remediation actions when human-in-the-loop is required.</summary>
    public List<string> ApproverRoles { get; set; } = [];

    /// <summary>Gets or sets the timestamp when the policy was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the timestamp when the policy was last updated.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Gets or sets the identity of the user or system that created the policy.</summary>
    public string CreatedBy { get; set; } = string.Empty;
}
