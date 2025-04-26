using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public abstract class BaseTool
{
    protected readonly ILogger<BaseTool> _logger;

    protected BaseTool(ILogger<BaseTool> logger)
    {
        _logger = logger;
    }

    public abstract Task<string> ExecuteAsync(Dictionary<string, object> parameters);
}
