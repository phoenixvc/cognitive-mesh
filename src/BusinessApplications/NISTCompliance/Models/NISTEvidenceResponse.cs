namespace CognitiveMesh.BusinessApplications.NISTCompliance.Models;

/// <summary>
/// Represents the response returned after successfully submitting NIST compliance evidence.
/// </summary>
public class NISTEvidenceResponse
{
    /// <summary>
    /// The unique identifier assigned to the submitted evidence.
    /// </summary>
    public Guid EvidenceId { get; set; }

    /// <summary>
    /// The identifier of the NIST statement this evidence was submitted for.
    /// </summary>
    public string StatementId { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when the evidence was submitted.
    /// </summary>
    public DateTimeOffset SubmittedAt { get; set; }

    /// <summary>
    /// The current review status of the evidence (e.g., "Pending", "Approved", "Rejected").
    /// </summary>
    public string ReviewStatus { get; set; } = string.Empty;

    /// <summary>
    /// A human-readable message describing the submission result.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
