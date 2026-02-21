namespace AgencyLayer.ToolIntegration.Models
{
    /// <summary>
    /// Defines the configuration for a generic, mock tool.
    /// This allows creating multiple tool behaviors from a single class
    /// instead of creating many near-identical tool classes.
    /// </summary>
    public class ToolDefinition
    {
        /// <summary>
        /// The display name of the tool.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// A short description of the tool's purpose.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// The key for the primary input parameter expected in the ExecuteAsync dictionary.
        /// e.g., "data", "text".
        /// </summary>
        public required string ParameterKey { get; set; }

        /// <summary>
        /// The informational message to log upon execution.
        /// </summary>
        public required string LogMessage { get; set; }

        /// <summary>
        /// The simulated processing delay in milliseconds.
        /// </summary>
        public int DelayMs { get; set; } = 100;

        /// <summary>
        /// A template for the mock result string.
        /// The parameter value can be inserted using string formatting.
        /// e.g., "Classification results for: {0}"
        /// </summary>
        public required string ResultTemplate { get; set; }
    }
}
