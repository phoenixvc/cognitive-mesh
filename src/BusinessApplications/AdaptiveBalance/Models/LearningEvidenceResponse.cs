namespace CognitiveMesh.BusinessApplications.AdaptiveBalance.Models;

/// <summary>
/// Represents the response after submitting learning evidence.
/// </summary>
public class LearningEvidenceResponse
{
    /// <summary>
    /// The unique identifier assigned to this learning event.
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// The timestamp when the learning evidence was recorded.
    /// </summary>
    public DateTimeOffset RecordedAt { get; set; }

    /// <summary>
    /// A human-readable message describing the submission result.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
