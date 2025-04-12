public class ResearchRequest
{
    public string Topic { get; set; }
    public List<string> FocusAreas { get; set; } = new List<string>();
    public int Depth { get; set; } = 2; // 1=Basic, 2=Standard, 3=Deep
}
