using System.Text;

namespace MetacognitiveLayer.ReasoningTransparency.Strategies
{
    /// <summary>
    /// Generates transparency reports in Markdown format.
    /// </summary>
    public class MarkdownReportFormatStrategy : IReportFormatStrategy
    {
        /// <inheritdoc/>
        public string Format => "markdown";

        /// <inheritdoc/>
        public string GenerateReport(ReasoningTrace trace, Dictionary<string, object> aggregations)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"# Transparency Report: {trace.Name}");
            sb.AppendLine();
            sb.AppendLine($"**Trace ID:** {trace.Id}");
            sb.AppendLine($"**Generated At:** {DateTime.UtcNow}");
            sb.AppendLine($"**Description:** {trace.Description}");
            sb.AppendLine();

            sb.AppendLine("## Summary");
            foreach (var kvp in aggregations)
            {
                sb.AppendLine($"- **{FormatKey(kvp.Key)}**: {kvp.Value}");
            }
            sb.AppendLine();

            sb.AppendLine("## Reasoning Steps");

            if (trace.Steps != null && trace.Steps.Any())
            {
                foreach (var step in trace.Steps.OrderBy(s => s.Timestamp))
                {
                    sb.AppendLine($"### Step: {step.Name}");
                    sb.AppendLine($"- **ID**: {step.Id}");
                    sb.AppendLine($"- **Timestamp**: {step.Timestamp}");
                    sb.AppendLine($"- **Confidence**: {step.Confidence:P}");
                    sb.AppendLine($"- **Description**: {step.Description}");

                    if (step.Inputs != null && step.Inputs.Any())
                    {
                        sb.AppendLine("- **Inputs**:");
                        foreach (var input in step.Inputs)
                        {
                            sb.AppendLine($"  - {input.Key}: {input.Value}");
                        }
                    }

                    if (step.Outputs != null && step.Outputs.Any())
                    {
                        sb.AppendLine("- **Outputs**:");
                        foreach (var output in step.Outputs)
                        {
                            sb.AppendLine($"  - {output.Key}: {output.Value}");
                        }
                    }

                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("*No steps recorded.*");
            }

            return sb.ToString();
        }

        private string FormatKey(string key)
        {
            // Simple helper to format keys like "AverageConfidence" to "Average Confidence"
            return System.Text.RegularExpressions.Regex.Replace(key, "([a-z])([A-Z])", "$1 $2");
        }
    }
}
