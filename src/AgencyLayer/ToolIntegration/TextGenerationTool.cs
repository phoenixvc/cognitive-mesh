using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Azure.AI.OpenAI;

public class TextGenerationTool : BaseTool
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _completionDeployment;

    public TextGenerationTool(OpenAIClient openAIClient, string completionDeployment, ILogger<TextGenerationTool> logger) 
        : base(logger)
    {
        _openAIClient = openAIClient;
        _completionDeployment = completionDeployment;
    }

    public override async Task<string> ExecuteAsync(Dictionary<string, object> parameters)
    {
        if (!parameters.TryGetValue("prompt", out var promptObj) || promptObj is not string prompt)
        {
            _logger.LogError("Missing or invalid 'prompt' parameter");
            throw new Exception("Missing or invalid 'prompt' parameter");
        }

        try
        {
            var chatCompletionOptions = new ChatCompletionsOptions
            {
                DeploymentName = _completionDeployment,
                Temperature = 0.7f,
                MaxTokens = 500,
                Messages =
                {
                    new ChatRequestSystemMessage("You are a helpful assistant."),
                    new ChatRequestUserMessage(prompt)
                }
            };

            var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);

            _logger.LogInformation("Text generation executed successfully for prompt: {Prompt}", prompt);
            return response.Value.Choices[0].Message.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing text generation for prompt: {Prompt}", prompt);
            throw;
        }
    }
}
