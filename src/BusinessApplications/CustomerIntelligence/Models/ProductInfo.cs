using System.Collections.Generic;

namespace CognitiveMesh.BusinessApplications.CustomerIntelligence.Models;

/// <summary>
/// Represents product information for customer intelligence analysis.
/// </summary>
public class ProductInfo
{
    /// <summary>Unique product identifier.</summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>Product name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Product description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Product category.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Product specifications.</summary>
    public Dictionary<string, string> Specifications { get; set; } = new();

    /// <summary>Common issues reported for this product.</summary>
    public List<string> CommonIssues { get; set; } = new();
}
