using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.AI.OpenAI;

public class ToolCallPlanner
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _completionDeployment;

    public ToolCallPlanner(OpenAIClient openAIClient, string completionDeployment)
    {
        _openAIClient = openAIClient;
        _completionDeployment = completionDeployment;
    }

    public async Task<ToolCallPlan> PlanToolCallAsync(string step, List<ToolDefinition> availableTools)
    {
        var toolsText = new StringBuilder();
        foreach (var tool in availableTools)
        {
            toolsText.AppendLine($"Tool: {tool.Name}");
            toolsText.AppendLine($"Description: {tool.Description}");
            toolsText.AppendLine($"Parameters: {JsonSerializer.Serialize(tool.ParametersSchema)}");
            toolsText.AppendLine();
        }

        var systemPrompt = "You are a tool selection system that identifies the most appropriate tool for a given step. " +
                          "Select a single tool and specify the parameters to use.";

        var userPrompt = $"Step: {step}\n\nAvailable Tools:\n{toolsText}\n\n" +
                        "Select the most appropriate tool for this step and specify the parameters to use. " +
                        "Respond in JSON format with 'tool' and 'parameters' fields.";

        var functionDefinitions = availableTools.Select(tool => new FunctionDefinition
        {
            Name = tool.Name,
            Description = tool.Description,
            Parameters = BinaryData.FromString(JsonSerializer.Serialize(tool.ParametersSchema))
        }).ToList();

        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.1f,
            Functions = functionDefinitions,
            FunctionCall = FunctionDefinition.Auto,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            }
        };

        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
        var message = response.Value.Choices[0].Message;

        if (message.FunctionCall != null)
        {
            var toolName = message.FunctionCall.Name;
            var parametersJson = message.FunctionCall.Arguments;
            var parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(parametersJson);

            return new ToolCallPlan
            {
                Step = step,
                ToolName = toolName,
                Parameters = parameters
            };
        }

        return new ToolCallPlan
        {
            Step = step,
            ToolName = "NoToolSelected",
            Parameters = new Dictionary<string, object>()
        };
    }
}
