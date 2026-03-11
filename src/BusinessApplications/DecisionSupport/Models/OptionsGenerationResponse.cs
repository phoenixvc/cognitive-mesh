namespace CognitiveMesh.BusinessApplications.DecisionSupport.Models;

/// <summary>
/// Response payload containing generated options and their analysis.
/// </summary>
public class OptionsGenerationResponse
{
    /// <summary>
    /// Gets or sets the original situation description.
    /// </summary>
    public string Situation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the analyzed options.
    /// </summary>
    public List<OptionAnalysis> Options { get; set; } = new();

    /// <summary>
    /// Gets or sets the comparison of all generated options.
    /// </summary>
    public string Comparison { get; set; } = string.Empty;
}

/// <summary>
/// Represents the analysis of a single decision option.
/// </summary>
public class OptionAnalysis
{
    /// <summary>
    /// Gets or sets the option description.
    /// </summary>
    public string Option { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the multi-perspective analysis of the option.
    /// </summary>
    public string Analysis { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pros and cons evaluation.
    /// </summary>
    public string ProsAndCons { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the potential outcomes prediction.
    /// </summary>
    public string PotentialOutcomes { get; set; } = string.Empty;
}
