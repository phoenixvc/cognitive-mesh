using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class DataCleaningTool : BaseTool
{
    public DataCleaningTool(ILogger<DataCleaningTool> logger) : base(logger)
    {
    }

    public override async Task<string> ExecuteAsync(Dictionary<string, object> parameters)
    {
        if (!parameters.TryGetValue("data", out var dataObj) || dataObj is not string data)
        {
            _logger.LogError("Missing or invalid 'data' parameter");
            throw new Exception("Missing or invalid 'data' parameter");
        }

        try
        {
            var cleanedData = CleanData(data);
            _logger.LogInformation("Data cleaning executed successfully for data: {Data}", data);
            return cleanedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing data cleaning for data: {Data}", data);
            throw;
        }
    }

    private string CleanData(string data)
    {
        // Simulate data cleaning logic
        return data.Trim().Replace("  ", " ");
    }
}
