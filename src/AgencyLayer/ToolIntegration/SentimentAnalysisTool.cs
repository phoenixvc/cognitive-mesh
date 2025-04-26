using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class SentimentAnalysisTool : BaseTool
{
    private readonly HttpClient _httpClient;

    public SentimentAnalysisTool(ILogger<SentimentAnalysisTool> logger, HttpClient httpClient) : base(logger)
    {
        _httpClient = httpClient;
    }

    public override async Task<string> ExecuteAsync(Dictionary<string, object> parameters)
    {
        if (!parameters.TryGetValue("text", out var textObj) || textObj is not string text)
        {
            _logger.LogError("Missing or invalid 'text' parameter");
            throw new Exception("Missing or invalid 'text' parameter");
        }

        try
        {
            var sentimentAnalysisEndpoint = "https://api.sentimentanalysis.com/analyze";
            var content = new StringContent(JsonSerializer.Serialize(new { text }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(sentimentAnalysisEndpoint, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Sentiment analysis executed successfully for text: {Text}", text);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sentiment analysis for text: {Text}", text);
            throw;
        }
    }
}
