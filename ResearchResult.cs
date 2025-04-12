public class ResearchResult
{
    public string Topic { get; set; }
    public string ResearchPlan { get; set; }
    public string Findings { get; set; }
    public string Insights { get; set; }
    public List<ResearchSource> Sources { get; set; } = new List<ResearchSource>();
}
