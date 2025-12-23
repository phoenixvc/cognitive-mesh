using AgencyLayer.ToolIntegration.Models;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.ToolIntegration
{
    /// <summary>
    /// A generic tool whose behavior is defined by a ToolDefinition object.
    /// This class replaces the need for numerous, nearly-identical tool classes.
    /// </summary>
    public class ConfigurableTool : BaseTool
    {
        private readonly ToolDefinition _definition;

        public override string Name => _definition.Name;
        public override string Description => _definition.Description;

        /// <summary>
        /// Initializes a new instance of the ConfigurableTool class.
        /// </summary>
        /// <param name="definition">The configuration object that defines the tool's behavior.</param>
        /// <param name="logger">The logger instance for this tool.</param>
        public ConfigurableTool(ToolDefinition definition, ILogger<ConfigurableTool> logger) : base(logger)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        /// <summary>
        /// Executes the tool's logic based on its configuration.
        /// </summary>
        /// <param name="parameters">A dictionary of parameters for the tool.</param>
        /// <returns>A string representing the mock result of the tool's execution.</returns>
        public override async Task<string> ExecuteAsync(Dictionary<string, object> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (!parameters.TryGetValue(_definition.ParameterKey, out var parameterValue) || parameterValue == null)
                throw new ArgumentException($"Missing or invalid '{_definition.ParameterKey}' parameter for tool '{Name}'.");

            _logger.LogInformation(_definition.LogMessage);

            // Simulate some processing time based on the definition
            if (_definition.DelayMs > 0)
            {
                await Task.Delay(_definition.DelayMs);
            }

            // Return a mock result formatted according to the definition
            return string.Format(_definition.ResultTemplate, parameterValue);
        }
    }
}
