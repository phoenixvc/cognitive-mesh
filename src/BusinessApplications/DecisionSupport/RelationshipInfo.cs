namespace CognitiveMesh.BusinessApplications.DecisionSupport;

/// <summary>
/// Represents a causal relationship between two entities in decision support analysis.
/// </summary>
public class RelationshipInfo
{
    /// <summary>Gets or sets the name of the cause entity.</summary>
    public string? CauseEntity { get; set; }

    /// <summary>Gets or sets the name of the effect entity.</summary>
    public string? EffectEntity { get; set; }

    /// <summary>Gets or sets the type of relationship.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the strength of the relationship.</summary>
    public double Strength { get; set; }

    /// <summary>Gets or sets the evidence supporting the relationship.</summary>
    public string Evidence { get; set; } = string.Empty;
}
