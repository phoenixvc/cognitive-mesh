namespace CognitiveMesh.BusinessApplications.NISTCompliance.Models;

/// <summary>
/// Represents the complete NIST AI RMF compliance checklist for an organization,
/// grouped by pillar with completion status.
/// </summary>
public class NISTChecklistResponse
{
    /// <summary>
    /// The identifier of the organization this checklist belongs to.
    /// </summary>
    public string OrganizationId { get; set; } = string.Empty;

    /// <summary>
    /// The NIST AI RMF pillars and their associated statements.
    /// </summary>
    public List<NISTChecklistPillar> Pillars { get; set; } = new();

    /// <summary>
    /// The total number of statements across all pillars.
    /// </summary>
    public int TotalStatements { get; set; }

    /// <summary>
    /// The number of statements that have been completed.
    /// </summary>
    public int CompletedStatements { get; set; }
}
