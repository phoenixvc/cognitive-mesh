using System;
using System.Collections.Generic;

namespace CognitiveMesh.BusinessApplications.CustomerIntelligence.Models
{
/// <summary>
/// Represents an interaction record for customer intelligence tracking.
/// </summary>
public class InteractionRecord
{
    /// <summary>Unique record identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Associated query identifier.</summary>
    public string QueryId { get; set; } = string.Empty;

    /// <summary>Interaction type.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>The query content.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>The response content.</summary>
    public string Response { get; set; } = string.Empty;

    /// <summary>Time taken to process.</summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>Evaluation scores for the interaction.</summary>
    public Dictionary<string, double> EvaluationScores { get; set; } = new();

    /// <summary>When the interaction occurred.</summary>
    public DateTimeOffset Timestamp { get; set; }
}

} // namespace CognitiveMesh.BusinessApplications.CustomerIntelligence.Models
