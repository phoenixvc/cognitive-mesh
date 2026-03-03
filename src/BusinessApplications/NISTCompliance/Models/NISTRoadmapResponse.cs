namespace CognitiveMesh.BusinessApplications.NISTCompliance.Models;

/// <summary>
/// Represents the NIST AI RMF improvement roadmap for an organization,
/// identifying gaps and recommending prioritized actions.
/// </summary>
public class NISTRoadmapResponse
{
    /// <summary>
    /// The identifier of the organization this roadmap belongs to.
    /// </summary>
    public string OrganizationId { get; set; } = string.Empty;

    /// <summary>
    /// The identified compliance gaps with recommended actions.
    /// </summary>
    public List<NISTGapItem> Gaps { get; set; } = new();

    /// <summary>
    /// The timestamp when this roadmap was generated.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; set; }
}
