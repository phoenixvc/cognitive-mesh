public class SituationAnalysisResponse
{
    public string SituationId { get; set; }
    public string MultiPerspectiveAnalysis { get; set; }
    public string KeyFactors { get; set; }
    public string CausalReasoning { get; set; }
    public List<EntityInfo> Entities { get; set; } = new List<EntityInfo>();
    public List<RelationshipInfo> Relationships { get; set; } = new List<RelationshipInfo>();
}
