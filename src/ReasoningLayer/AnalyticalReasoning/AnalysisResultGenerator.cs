using CognitiveMesh.ReasoningLayer.AnalyticalReasoning.Models;

namespace CognitiveMesh.ReasoningLayer.AnalyticalReasoning;

/// <summary>
/// Generates analytical results by invoking an OpenAI completion deployment
/// with structured prompts for data-driven analysis.
/// </summary>
public class AnalysisResultGenerator
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _completionDeployment;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisResultGenerator"/> class.
    /// </summary>
    /// <param name="openAIClient">The OpenAI client for generating completions.</param>
    /// <param name="completionDeployment">The deployment name to use for completions.</param>
    public AnalysisResultGenerator(OpenAIClient openAIClient, string completionDeployment)
    {
        _openAIClient = openAIClient;
        _completionDeployment = completionDeployment;
    }

    /// <summary>
    /// Generates an analytical result for the specified data query.
    /// </summary>
    public async Task<AnalyticalResult> GenerateAnalysisResultAsync(string dataQuery)
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
