using Microsoft.Extensions.Logging;

namespace AgencyLayer.ToolIntegration
{
    public abstract class BaseTool
    {
        protected readonly ILogger _logger;

        protected BaseTool(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract Task<string> ExecuteAsync(Dictionary<string, object> parameters);
    }
}
