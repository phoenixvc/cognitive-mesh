using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class WebScrapingTool : BaseTool
{
    public WebScrapingTool(ILogger<WebScrapingTool> logger) : base(logger)
    {
    }

    public override async Task<string> ExecuteAsync(Dictionary<string, object> parameters)
    {
        if (!parameters.TryGetValue("url", out var urlObj) || urlObj is not string url)
        {
            _logger.LogError("Missing or invalid 'url' parameter");
            throw new Exception("Missing or invalid 'url' parameter");
        }

        try
        {
            var webScrapingEndpoint = "https://api.webscraping.com/scrape";
            var content = new StringContent(JsonSerializer.Serialize(new { url }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(webScrapingEndpoint, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Web scraping executed successfully for URL: {Url}", url);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing web scraping for URL: {Url}", url);
            throw;
        }
    }
}
