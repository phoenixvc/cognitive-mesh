public class ProductInfo
{
    public string ProductId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public Dictionary<string, string> Specifications { get; set; } = new Dictionary<string, string>();
    public List<string> CommonIssues { get; set; } = new List<string>();
}
