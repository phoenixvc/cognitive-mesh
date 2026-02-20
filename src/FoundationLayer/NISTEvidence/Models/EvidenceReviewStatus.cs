namespace CognitiveMesh.FoundationLayer.NISTEvidence.Models;

/// <summary>
/// Represents the review status of a NIST evidence record.
/// </summary>
public enum EvidenceReviewStatus
{
    /// <summary>
    /// The evidence record is pending review.
    /// </summary>
    Pending,

    /// <summary>
    /// The evidence record is currently under review.
    /// </summary>
    InReview,

    /// <summary>
    /// The evidence record has been approved.
    /// </summary>
    Approved,

    /// <summary>
    /// The evidence record has been rejected.
    /// </summary>
    Rejected,

    /// <summary>
    /// The evidence record needs revision before it can be approved.
    /// </summary>
    NeedsRevision
}
