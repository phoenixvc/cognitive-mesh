namespace CognitiveMesh.BusinessApplications.NISTCompliance.Models;

/// <summary>
/// Represents the response returned after reviewing NIST compliance evidence.
/// </summary>
public class NISTReviewResponse
{
    /// <summary>
    /// The unique identifier of the evidence that was reviewed.
    /// </summary>
    public Guid EvidenceId { get; set; }

    /// <summary>
    /// The new review status of the evidence after the review.
    /// </summary>
    public string NewStatus { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when the review was completed.
    /// </summary>
    public DateTimeOffset ReviewedAt { get; set; }

    /// <summary>
    /// A human-readable message describing the review result.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
