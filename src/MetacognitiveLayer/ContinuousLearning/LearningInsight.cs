namespace MetacognitiveLayer.ContinuousLearning;

/// <summary>
/// Represents an insight generated from continuous learning analysis.
/// </summary>
public class LearningInsight
{
    /// <summary>Gets or sets the insight identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the insight type (e.g. PerformanceTrend, FeedbackInsight, ImprovementOpportunity).</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the insight title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the insight description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the severity (High, Medium, Low).</summary>
    public string Severity { get; set; } = string.Empty;
}
