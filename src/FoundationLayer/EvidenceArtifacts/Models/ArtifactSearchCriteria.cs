namespace CognitiveMesh.FoundationLayer.EvidenceArtifacts.Models;

/// <summary>
/// Search criteria for querying evidence artifacts.
/// All criteria properties are optional; when set, they are combined with AND logic.
/// </summary>
public class ArtifactSearchCriteria
{
    /// <summary>
    /// Filter by the type of source that produced the artifact.
    /// </summary>
    public string? SourceType { get; set; }

    /// <summary>
    /// Filter by correlation identifier to find related artifacts.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Filter by tags; artifacts must contain all specified tags.
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Filter to include only artifacts submitted after this date.
    /// </summary>
    public DateTimeOffset? SubmittedAfter { get; set; }

    /// <summary>
    /// The maximum number of results to return. Defaults to 50.
    /// </summary>
    public int MaxResults { get; set; } = 50;
}
