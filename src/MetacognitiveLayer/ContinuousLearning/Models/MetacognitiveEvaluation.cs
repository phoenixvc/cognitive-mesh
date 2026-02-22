namespace MetacognitiveLayer.ContinuousLearning.Models;

/// <summary>
/// Represents the result of a metacognitive evaluation across multiple dimensions.
/// This is a pure serializable DTO; no constructor injection or <see cref="Microsoft.Extensions.Logging.ILogger{T}"/> is required.
/// </summary>
public class MetacognitiveEvaluation
{
    /// <summary>Gets or sets the query identifier.</summary>
    public string QueryId { get; set; } = string.Empty;

    /// <summary>Gets or sets the factual accuracy evaluation.</summary>
    public EvaluationDimension FactualAccuracy { get; set; } = new();

    /// <summary>Gets or sets the reasoning quality evaluation.</summary>
    public EvaluationDimension ReasoningQuality { get; set; } = new();

    /// <summary>Gets or sets the relevance evaluation.</summary>
    public EvaluationDimension Relevance { get; set; } = new();

    /// <summary>Gets or sets the completeness evaluation.</summary>
    public EvaluationDimension Completeness { get; set; } = new();

    /// <summary>Gets or sets the improvement suggestions.</summary>
    public List<string> ImprovementSuggestions { get; set; } = new();

    /// <summary>Gets or sets the evaluation time.</summary>
    public TimeSpan EvaluationTime { get; set; }
}

/// <summary>
/// Represents a single dimension of evaluation with a score and explanation.
/// This is a pure serializable DTO; no constructor injection or <see cref="Microsoft.Extensions.Logging.ILogger{T}"/> is required.
/// </summary>
public class EvaluationDimension
{
    /// <summary>Gets or sets the dimension name.</summary>
    public string Dimension { get; set; } = string.Empty;

    /// <summary>Gets or sets the score.</summary>
    public double Score { get; set; }

    /// <summary>Gets or sets the explanation.</summary>
    public string Explanation { get; set; } = string.Empty;
}
