using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class ClusteringTool : BaseTool
{
    public ClusteringTool(ILogger<ClusteringTool> logger) : base(logger)
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
            var clusteringEndpoint = "https://api.clustering.com/cluster";
            var content = new StringContent(JsonSerializer.Serialize(new { data }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(clusteringEndpoint, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Clustering executed successfully for data: {Data}", data);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing clustering for data: {Data}", data);
            throw;
        }
    }
}
