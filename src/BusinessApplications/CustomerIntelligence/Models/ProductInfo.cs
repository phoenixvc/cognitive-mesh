using System.Collections.Generic;

namespace CognitiveMesh.BusinessApplications.CustomerIntelligence.Models;

public class ProductInfo
{
    public string ProductId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public Dictionary<string, string> Specifications { get; set; } = new();
    public List<string> CommonIssues { get; set; } = new();
}
