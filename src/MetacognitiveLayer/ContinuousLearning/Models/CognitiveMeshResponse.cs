namespace MetacognitiveLayer.ContinuousLearning.Models;

/// <summary>
/// Represents a response from the cognitive mesh pipeline.
/// </summary>
public class CognitiveMeshResponse
{
    /// <summary>Gets or sets the query identifier.</summary>
    public string QueryId { get; set; } = string.Empty;

    /// <summary>Gets or sets the original query.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>Gets or sets the generated response.</summary>
    public string Response { get; set; } = string.Empty;

    /// <summary>Gets or sets the processing time.</summary>
    public TimeSpan ProcessingTime { get; set; }
}
