namespace MetacognitiveLayer.ReasoningTransparency.Strategies
{
    /// <summary>
    /// Strategy interface for formatting transparency reports.
    /// </summary>
    public interface IReportFormatStrategy
    {
        /// <summary>
        /// Gets the format identifier (e.g., "json", "markdown").
        /// </summary>
        string Format { get; }

        /// <summary>
        /// Generates a report for the given trace and aggregations.
        /// </summary>
        /// <param name="trace">The reasoning trace to report on.</param>
        /// <param name="aggregations">Calculated aggregations and statistics.</param>
        /// <returns>The formatted report as a string.</returns>
        string GenerateReport(ReasoningTrace trace, Dictionary<string, object> aggregations);
    }
}
