namespace CognitiveMesh.BusinessApplications.DecisionSupport;

public class RelationshipInfo
{
    public string CauseEntity { get; set; }
    public string EffectEntity { get; set; }
    public string Type { get; set; }
    public double Strength { get; set; }
    public string Evidence { get; set; }
}
