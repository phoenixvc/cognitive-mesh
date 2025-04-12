public class CustomerInquiryResponse
{
    public string InquiryId { get; set; }
    public string Category { get; set; }
    public string ResponseOptions { get; set; }
    public bool RequiresEscalation { get; set; }
    public string NextSteps { get; set; }
    public List<KnowledgeReference> RelevantKnowledge { get; set; } = new List<KnowledgeReference>();
}
