using System;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;

public class UncertaintyQuantifier
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _completionDeployment;
    private readonly ILogger<UncertaintyQuantifier> _logger;

    public UncertaintyQuantifier(
        string openAIEndpoint,
        string openAIApiKey,
        string completionDeployment,
        ILogger<UncertaintyQuantifier> logger)
    {
        _openAIClient = new OpenAIClient(new Uri(openAIEndpoint), new AzureKeyCredential(openAIApiKey));
        _completionDeployment = completionDeployment;
        _logger = logger;
    }

    public async Task<string> QuantifyUncertaintyAsync(string input)
    {
        try
        {
            var systemPrompt = "You are an AI that quantifies uncertainty in the given input. " +
                               "Provide a detailed analysis of the uncertainties present in the input.";

            var chatCompletionOptions = new ChatCompletionsOptions
            {
                DeploymentName = _completionDeployment,
                Temperature = 0.3f,
                MaxTokens = 800,
                Messages =
                {
                    new ChatRequestSystemMessage(systemPrompt),
                    new ChatRequestUserMessage(input)
                }
            };

            var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
            return response.Value.Choices[0].Message.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error quantifying uncertainty with input: {Input}", input);
            throw;
        }
    }

    public async Task<string> GenerateUncertaintyReportAsync(string input)
    {
        try
        {
            var systemPrompt = "You are an AI that generates a report on the uncertainties in the given input. " +
                               "Provide a comprehensive report detailing the uncertainties and their potential impacts.";

            var chatCompletionOptions = new ChatCompletionsOptions
            {
                DeploymentName = _completionDeployment,
                Temperature = 0.3f,
                MaxTokens = 800,
                Messages =
                {
                    new ChatRequestSystemMessage(systemPrompt),
                    new ChatRequestUserMessage(input)
                }
            };

            var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
            return response.Value.Choices[0].Message.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating uncertainty report with input: {Input}", input);
            throw;
        }
    }
}
