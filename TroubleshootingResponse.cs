public class TroubleshootingResponse
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string Diagnosis { get; set; }
    public string SolutionSteps { get; set; }
    public string RootCauseAnalysis { get; set; }
    public List<KnowledgeReference> RelatedKnowledgeBase { get; set; } = new List<KnowledgeReference>();
}
