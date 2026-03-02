using Microsoft.Extensions.Logging;

namespace AgencyLayer.ToolIntegration
{
    /// <summary>
    /// Abstract base class for all tools in the tool integration framework.
    /// </summary>
    public abstract class BaseTool
    {
        /// <summary>The logger instance for this tool.</summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTool"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        protected BaseTool(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Gets the name of the tool.</summary>
        public abstract string Name { get; }
        /// <summary>Gets the description of the tool.</summary>
        public abstract string Description { get; }
        /// <summary>
        /// Executes the tool with the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters for tool execution.</param>
        /// <returns>A string containing the tool execution result.</returns>
        public abstract Task<string> ExecuteAsync(Dictionary<string, object> parameters);
    }
}
