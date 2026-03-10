using System.Collections.Generic;

namespace CognitiveMesh.BusinessApplications.ResearchAnalysis;

/// <summary>
/// Request model for initiating a research analysis on a topic.
/// </summary>
public class ResearchRequest
{
    /// <summary>The research topic.</summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>Specific areas to focus the research on.</summary>
    public List<string> FocusAreas { get; set; } = new List<string>();

    /// <summary>Research depth level: 1 = Basic, 2 = Standard, 3 = Deep.</summary>
    public int Depth { get; set; } = 2;
}
