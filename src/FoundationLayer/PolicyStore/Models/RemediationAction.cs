using System.Text.Json.Serialization;

namespace CognitiveMesh.FoundationLayer.PolicyStore.Models;

/// <summary>
/// Defines the set of remediation actions that can be applied to an incident.
/// This enum supports bitwise combinations of its member values.
/// </summary>
[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RemediationAction
{
    /// <summary>No remediation action.</summary>
    None = 0,

    /// <summary>Retry the failed operation.</summary>
    Retry = 1,

    /// <summary>Roll back the changes that caused the incident.</summary>
    Rollback = 2,

    /// <summary>Reassign the task to a different agent or handler.</summary>
    Reassign = 4,

    /// <summary>Restart the affected service or process.</summary>
    Restart = 8,

    /// <summary>Escalate the incident to a human operator or higher-level handler.</summary>
    Escalate = 16
}
