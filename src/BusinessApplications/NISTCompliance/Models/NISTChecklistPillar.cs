namespace CognitiveMesh.BusinessApplications.NISTCompliance.Models;

/// <summary>
/// Represents a single NIST AI RMF pillar within the compliance checklist,
/// containing its associated statements.
/// </summary>
public class NISTChecklistPillar
{
    /// <summary>
    /// The unique identifier of the pillar (e.g., "GOVERN", "MAP", "MEASURE", "MANAGE").
    /// </summary>
    public string PillarId { get; set; } = string.Empty;

    /// <summary>
    /// The human-readable name of the pillar.
    /// </summary>
    public string PillarName { get; set; } = string.Empty;

    /// <summary>
    /// The statements within this pillar.
    /// </summary>
    public List<NISTChecklistStatement> Statements { get; set; } = new();
}
