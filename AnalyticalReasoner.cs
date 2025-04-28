using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Azure.AI.OpenAI;

public class AnalyticalReasoner
{
    private readonly ILogger<AnalyticalReasoner> _logger;
    private readonly OpenAIClient _openAIClient;
    private readonly string _completionDeployment;

    public AnalyticalReasoner(ILogger<AnalyticalReasoner> logger, OpenAIClient openAIClient, string completionDeployment)
    {
        _logger = logger;
        _openAIClient = openAIClient;
        _completionDeployment = completionDeployment;
    }

    public async Task<AnalyticalResult> PerformDataDrivenAnalysisAsync(string dataQuery)
    {
        try
        {
            _logger.LogInformation($"Starting data-driven analysis for query: {dataQuery}");

            // Simulate data-driven analysis logic
            var analysisResult = await GenerateAnalysisResultAsync(dataQuery);

            _logger.LogInformation($"Successfully performed data-driven analysis for query: {dataQuery}");
            return analysisResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to perform data-driven analysis for query: {dataQuery}");
            throw;
        }
    }

    private async Task<AnalyticalResult> GenerateAnalysisResultAsync(string dataQuery)
    {
        var systemPrompt = "You are an analytical system that performs data-driven analysis based on the provided query. " +
                           "Generate a detailed analysis report.";

        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.3f,
            MaxTokens = 800,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage($"Data Query: {dataQuery}")
            }
        };

        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);

        return new AnalyticalResult
        {
            Query = dataQuery,
            AnalysisReport = response.Value.Choices[0].Message.Content
        };
    }
}

public class AnalyticalResult
{
    public string Query { get; set; }
    public string AnalysisReport { get; set; }
}
