namespace CognitiveMesh.BusinessApplications.DecisionSupport.Models;

/// <summary>
/// Response payload containing the results of a situation analysis.
/// </summary>
public class SituationAnalysisResponse
{
    /// <summary>
    /// Gets or sets the unique identifier for this analysis.
    /// </summary>
    public string SituationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the multi-perspective analysis result.
    /// </summary>
    public string MultiPerspectiveAnalysis { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key factors identified in the situation.
    /// </summary>
    public string KeyFactors { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the causal reasoning output.
    /// </summary>
    public string CausalReasoning { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entities identified in the causal analysis.
    /// </summary>
    public List<EntityInfo> Entities { get; set; } = new();

    /// <summary>
    /// Gets or sets the relationships identified in the causal analysis.
    /// </summary>
    public List<RelationshipInfo> Relationships { get; set; } = new();
}

/// <summary>
/// Represents an entity identified during causal analysis, for API response use.
/// </summary>
public class EntityInfo
{
    /// <summary>
    /// Gets or sets the name of the entity.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the entity.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the entity.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
