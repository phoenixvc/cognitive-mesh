using System.Text.Json;

namespace MetacognitiveLayer.ReasoningTransparency.Strategies
{
    /// <summary>
    /// Generates transparency reports in JSON format.
    /// </summary>
    public class JsonReportFormatStrategy : IReportFormatStrategy
    {
        /// <inheritdoc/>
        public string Format => "json";

        /// <inheritdoc/>
        public string GenerateReport(ReasoningTrace trace, Dictionary<string, object> aggregations)
        {
            var reportData = new
            {
                TraceId = trace.Id,
                TraceName = trace.Name,
                GeneratedAt = DateTime.UtcNow,
                Summary = aggregations,
                Steps = trace.Steps
            };

            return JsonSerializer.Serialize(reportData, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}
