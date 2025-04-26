using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class RecommendationSystemTool : BaseTool
{
    public RecommendationSystemTool(ILogger<RecommendationSystemTool> logger) : base(logger)
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
            var recommendationSystemEndpoint = "https://api.recommendationsystem.com/recommend";
            var content = new StringContent(JsonSerializer.Serialize(new { data }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(recommendationSystemEndpoint, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Recommendation system executed successfully for data: {Data}", data);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing recommendation system for data: {Data}", data);
            throw;
        }
    }
}
