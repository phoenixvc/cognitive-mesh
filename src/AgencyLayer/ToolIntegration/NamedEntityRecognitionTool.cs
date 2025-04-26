using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class NamedEntityRecognitionTool : BaseTool
{
    public NamedEntityRecognitionTool(ILogger<NamedEntityRecognitionTool> logger) : base(logger)
    {
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
            var namedEntityRecognitionEndpoint = "https://api.namedentityrecognition.com/analyze";
            var content = new StringContent(JsonSerializer.Serialize(new { text }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(namedEntityRecognitionEndpoint, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Named entity recognition executed successfully for text: {Text}", text);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing named entity recognition for text: {Text}", text);
            throw;
        }
    }
}
