using Azure.AI.OpenAI;
using CognitiveMesh.ReasoningLayer.AnalyticalReasoning.Models;
using OpenAI.Chat;

namespace CognitiveMesh.ReasoningLayer.AnalyticalReasoning;

/// <summary>
/// Generates analytical results by invoking an OpenAI completion deployment
/// with structured prompts for data-driven analysis.
/// </summary>
public class AnalysisResultGenerator
{
    private readonly ChatClient _chatClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisResultGenerator"/> class.
    /// </summary>
    /// <param name="openAIClient">The Azure OpenAI client for generating completions.</param>
    /// <param name="completionDeployment">The deployment name to use for completions.</param>
    public AnalysisResultGenerator(AzureOpenAIClient openAIClient, string completionDeployment)
    {
        _chatClient = openAIClient.GetChatClient(completionDeployment);
    }

    /// <summary>
    /// Generates an analytical result for the specified data query.
    /// </summary>
    public async Task<AnalyticalResult> GenerateAnalysisResultAsync(string dataQuery)
    {
        var systemPrompt = "You are an analytical system that performs data-driven analysis based on the provided query. " +
                           "Generate a detailed analysis report.";

        var completion = await _chatClient.CompleteChatAsync(
            [
                new SystemChatMessage(systemPrompt),
                new UserChatMessage($"Data Query: {dataQuery}")
            ],
            new ChatCompletionOptions
            {
                Temperature = 0.3f,
                MaxOutputTokenCount = 800
            });

        return new AnalyticalResult
        {
            Query = dataQuery,
            AnalysisReport = completion.Value.Content[0].Text
        };
    }
}
