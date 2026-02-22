namespace CognitiveMesh.BusinessApplications.NISTCompliance.Models;

/// <summary>
/// Represents the audit log response for NIST AI RMF compliance activities.
/// </summary>
public class NISTAuditLogResponse
{
    /// <summary>
    /// The identifier of the organization this audit log belongs to.
    /// </summary>
    public string OrganizationId { get; set; } = string.Empty;

    /// <summary>
    /// The audit log entries.
    /// </summary>
    public List<NISTAuditEntry> Entries { get; set; } = new();

    /// <summary>
    /// The total count of audit entries available for this organization.
    /// </summary>
    public int TotalCount { get; set; }
}
