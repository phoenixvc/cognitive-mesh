using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class DataVisualizationTool : BaseTool
{
    private readonly HttpClient _httpClient;

    public DataVisualizationTool(ILogger<DataVisualizationTool> logger, HttpClient httpClient) : base(logger)
    {
        _httpClient = httpClient;
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
            var dataVisualizationEndpoint = "https://api.datavisualization.com/visualize";
            var content = new StringContent(JsonSerializer.Serialize(new { data }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(dataVisualizationEndpoint, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Data visualization executed successfully for data: {Data}", data);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing data visualization for data: {Data}", data);
            throw;
        }
    }

    public async Task<string> GenerateVisualizationAsync(string data)
    {
        try
        {
            var dataVisualizationEndpoint = "https://api.datavisualization.com/visualize";
            var content = new StringContent(JsonSerializer.Serialize(new { data }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(dataVisualizationEndpoint, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Visualization generated successfully for data: {Data}", data);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating visualization for data: {Data}", data);
            throw;
        }
    }

    public async Task<byte[]> GenerateVisualizationReportAsync(string data, string format)
    {
        try
        {
            var dataVisualizationEndpoint = "https://api.datavisualization.com/visualize/report";
            var content = new StringContent(JsonSerializer.Serialize(new { data, format }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(dataVisualizationEndpoint, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsByteArrayAsync();
            _logger.LogInformation("Visualization report generated successfully for data: {Data} in format: {Format}", data, format);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating visualization report for data: {Data} in format: {Format}", data, format);
            throw;
        }
    }
}
