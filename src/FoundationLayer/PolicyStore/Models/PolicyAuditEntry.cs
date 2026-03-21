using System.Text.Json.Serialization;

namespace CognitiveMesh.FoundationLayer.PolicyStore.Models;

/// <summary>
/// Represents an audit log entry that records a change made to a remediation policy.
/// </summary>
public sealed class PolicyAuditEntry
{
    /// <summary>Gets or sets the unique identifier of the audit entry.</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Gets or sets the identifier of the policy that was changed.</summary>
    public Guid PolicyId { get; set; }

    /// <summary>Gets or sets the type of operation performed (e.g., Create, Update, Delete).</summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>Gets or sets the version of the policy after the operation.</summary>
    public int Version { get; set; }

    /// <summary>Gets or sets the identity of the actor who performed the operation.</summary>
    public string Actor { get; set; } = string.Empty;

    /// <summary>Gets or sets the timestamp when the operation occurred.</summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets or sets optional details describing the operation.</summary>
    public string? Details { get; set; }

    /// <summary>Gets or sets the incident category of the policy that was changed.</summary>
    public string IncidentCategory { get; set; } = string.Empty;
}
