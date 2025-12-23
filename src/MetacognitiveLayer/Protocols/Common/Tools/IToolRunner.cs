namespace MetacognitiveLayer.Protocols.Common.Tools
{
    /// <summary>
    /// Context information for tool execution.
    /// </summary>
    public class ToolContext
    {
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public Dictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Interface for tool execution systems that run tools in various environments.
    /// </summary>
    public interface IToolRunner
    {
        /// <summary>
        /// Executes a tool with the provided inputs and context.
        /// </summary>
        /// <param name="toolId">The identifier of the tool to execute</param>
        /// <param name="input">The input parameters for the tool</param>
        /// <param name="context">The execution context</param>
        /// <returns>The result of the tool execution</returns>
        Task<object> Execute(string toolId, Dictionary<string, object> input, ToolContext context);

        /// <summary>
        /// Gets the available tools managed by this runner.
        /// </summary>
        Task<Dictionary<string, object>> GetAvailableToolsAsync();

        /// <summary>
        /// Validates if a tool exists and can be executed.
        /// </summary>
        Task<bool> ValidateToolAsync(string toolId);
    }
}