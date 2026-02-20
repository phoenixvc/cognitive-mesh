namespace CognitiveMesh.BusinessApplications.NISTCompliance.Models;

/// <summary>
/// Represents a request to review submitted NIST compliance evidence.
/// </summary>
public class NISTReviewRequest
{
    /// <summary>
    /// The unique identifier of the evidence to review.
    /// </summary>
    public Guid EvidenceId { get; set; }

    /// <summary>
    /// The identifier of the reviewer performing the review.
    /// </summary>
    public string ReviewerId { get; set; } = string.Empty;

    /// <summary>
    /// The review decision (e.g., "Approved", "Rejected").
    /// </summary>
    public string Decision { get; set; } = string.Empty;

    /// <summary>
    /// Optional notes from the reviewer explaining the decision.
    /// </summary>
    public string? Notes { get; set; }
}
