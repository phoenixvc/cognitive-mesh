using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Azure.AI.OpenAI;
using Azure.Messaging.EventGrid;

public class PerformanceMonitor
{
    private readonly ILogger<PerformanceMonitor> _logger;
    private readonly OpenAIClient _openAIClient;
    private readonly EventGridPublisherClient _eventGridClient;

    public PerformanceMonitor(ILogger<PerformanceMonitor> logger, OpenAIClient openAIClient, EventGridPublisherClient eventGridClient)
    {
        _logger = logger;
        _openAIClient = openAIClient;
        _eventGridClient = eventGridClient;
    }

    public async Task<PerformanceMetrics> MonitorPerformanceAsync(string systemComponent, List<string> metrics)
    {
        try
        {
            _logger.LogInformation($"Starting performance monitoring for component: {systemComponent}");

            // Simulate performance monitoring logic
            await Task.Delay(1000);

            var result = new PerformanceMetrics
            {
                Component = systemComponent,
                Metrics = metrics,
                PerformanceScore = CalculatePerformanceScore(metrics)
            };

            _logger.LogInformation($"Successfully monitored performance for component: {systemComponent}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to monitor performance for component: {systemComponent}. Error: {ex.Message}");
            throw;
        }
    }

    public async Task<string> GeneratePerformanceReportAsync(string systemComponent, List<string> metrics)
    {
        try
        {
            var performanceMetrics = await MonitorPerformanceAsync(systemComponent, metrics);

            var systemPrompt = "You are a performance reporting system. Generate a detailed performance report based on the provided metrics.";
            var userPrompt = $"Component: {systemComponent}\nMetrics: {string.Join(", ", metrics)}\nPerformance Score: {performanceMetrics.PerformanceScore}";

            var chatCompletionOptions = new ChatCompletionsOptions
            {
                DeploymentName = "your-deployment-name",
                Temperature = 0.3f,
                MaxTokens = 800,
                Messages =
                {
                    new ChatRequestSystemMessage(systemPrompt),
                    new ChatRequestUserMessage(userPrompt)
                }
            };

            var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
            var report = response.Value.Choices[0].Message.Content;

            _logger.LogInformation($"Generated performance report for component: {systemComponent}");
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to generate performance report for component: {systemComponent}. Error: {ex.Message}");
            throw;
        }
    }

    private double CalculatePerformanceScore(List<string> metrics)
    {
        // Simulate performance score calculation
        return metrics.Count * 0.2;
    }
}

public class PerformanceMetrics
{
    public string Component { get; set; }
    public List<string> Metrics { get; set; }
    public double PerformanceScore { get; set; }
}
