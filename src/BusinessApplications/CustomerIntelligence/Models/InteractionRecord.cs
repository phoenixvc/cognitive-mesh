using System;
using System.Collections.Generic;

namespace CognitiveMesh.BusinessApplications.CustomerIntelligence.Models
{
public class InteractionRecord
{
    public string Id { get; set; }
    public string QueryId { get; set; }
    public string Type { get; set; }
    public string Query { get; set; }
    public string Response { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public Dictionary<string, double> EvaluationScores { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

} // namespace CognitiveMesh.BusinessApplications.CustomerIntelligence.Models
