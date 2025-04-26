using Azure.AI.OpenAI;
using System.Text.Json;

public class ArgenticAgentComponent
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _completionDeployment;
    private readonly Dictionary<string, ToolDefinition> _availableTools;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ArgenticAgentComponent> _logger;
    private readonly ToolIntegrator _toolIntegrator;

    public ArgenticAgentComponent(
        string openAIEndpoint,
        string openAIApiKey,
        string completionDeployment,
        Dictionary<string, ToolDefinition> availableTools,
        ToolIntegrator toolIntegrator,
        ILogger<ArgenticAgentComponent> logger)
    {
        _openAIClient = new OpenAIClient(new Uri(openAIEndpoint), new AzureKeyCredential(openAIApiKey));
        _completionDeployment = completionDeployment;
        _availableTools = availableTools;
        _httpClient = new HttpClient();
        _toolIntegrator = toolIntegrator;
        _logger = logger;
    }

    public async Task<PlanningResult> CreatePlanAsync(string task, Dictionary<string, string> context)
    {
        try
        {
            var strategicPlan = await GenerateStrategicPlanAsync(task, context);
            var steps = await GenerateStepsAsync(task, strategicPlan, context);
            var toolCalls = new List<ToolCallPlan>();

            foreach (var step in steps)
            {
                var toolCall = await PlanToolCallAsync(step, _availableTools.Values.ToList());
                toolCalls.Add(toolCall);
            }

            return new PlanningResult
            {
                Task = task,
                StrategicPlan = strategicPlan,
                Steps = steps,
                ToolCalls = toolCalls
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating plan for task: {Task}", task);
            throw;
        }
    }

    private async Task<string> GenerateStrategicPlanAsync(string task, Dictionary<string, string> context)
    {
        var contextText = new StringBuilder();
        foreach (var kvp in context)
        {
            contextText.AppendLine($"{kvp.Key}: {kvp.Value}");
        }

        var systemPrompt = "You are a strategic planning system that creates high-level plans for accomplishing tasks. " +
                          "Focus on overall approach, key milestones, and critical considerations. " +
                          "Your plan should be concise but comprehensive.";

        var userPrompt = $"Task: {task}\n\nContext:\n{contextText}\n\nCreate a strategic plan for accomplishing this task.";

        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.3f,
            MaxTokens = 800,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            }
        };

        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);

        return response.Value.Choices[0].Message.Content;
    }

    private async Task<List<string>> GenerateStepsAsync(string task, string strategicPlan, Dictionary<string, string> context)
    {
        var contextText = new StringBuilder();
        foreach (var kvp in context)
        {
            contextText.AppendLine($"{kvp.Key}: {kvp.Value}");
        }

        var systemPrompt = "You are a tactical planning system that breaks down strategic plans into concrete steps. " +
                          "Each step should be specific, actionable, and contribute to the overall plan. " +
                          "Provide 3-7 sequential steps.";

        var userPrompt = $"Task: {task}\n\nStrategic Plan: {strategicPlan}\n\nContext:\n{contextText}\n\n" +
                        "Break down this strategic plan into specific, sequential steps.";

        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.3f,
            MaxTokens = 800,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            }
        };

        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);

        var stepsText = response.Value.Choices[0].Message.Content;
        return ParseSteps(stepsText);
    }

    private List<string> ParseSteps(string stepsText)
    {
        var steps = new List<string>();
        var lines = stepsText.Split('\n');
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("1.") ||
                trimmedLine.StartsWith("2.") ||
                trimmedLine.StartsWith("3.") ||
                trimmedLine.StartsWith("4.") ||
                trimmedLine.StartsWith("5.") ||
                trimmedLine.StartsWith("6.") ||
                trimmedLine.StartsWith("7.") ||
                trimmedLine.StartsWith("-"))
            {
                var step = trimmedLine.Substring(trimmedLine.IndexOf(' ') + 1).Trim();
                steps.Add(step);
            }
        }

        return steps;
    }

    private async Task<ToolCallPlan> PlanToolCallAsync(string step, List<ToolDefinition> availableTools)
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

    public async Task<ExecutionResult> ExecutePlanAsync(PlanningResult plan)
    {
        var results = new List<StepResult>();
        var allStepsSuccessful = true;

        try
        {
            for (int i = 0; i < plan.Steps.Count; i++)
            {
                var step = plan.Steps[i];
                var toolCall = plan.ToolCalls[i];

                try
                {
                    var toolResult = await ExecuteToolCallAsync(toolCall);

                    results.Add(new StepResult
                    {
                        Step = step,
                        ToolCall = toolCall,
                        Result = toolResult,
                        Success = true
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new StepResult
                    {
                        Step = step,
                        ToolCall = toolCall,
                        Result = $"Error: {ex.Message}",
                        Success = false
                    });

                    allStepsSuccessful = false;
                }
            }

            var summary = await GenerateSummaryAsync(plan, results);
            var reflection = await GenerateReflectionAsync(plan, results);

            return new ExecutionResult
            {
                Plan = plan,
                StepResults = results,
                Success = allStepsSuccessful,
                Summary = summary,
                Reflection = reflection
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing plan for task: {Task}", plan.Task);
            throw;
        }
    }

    private async Task<string> ExecuteToolCallAsync(ToolCallPlan toolCall)
    {
        if (!_availableTools.TryGetValue(toolCall.ToolName, out var toolDefinition))
        {
            throw new Exception($"Tool '{toolCall.ToolName}' not found");
        }

        return await _toolIntegrator.ExecuteToolAsync(toolCall.ToolName, toolCall.Parameters);
    }

    private async Task<string> CallToolApiAsync(string endpoint, Dictionary<string, object> parameters)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(parameters),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string> GenerateSummaryAsync(PlanningResult plan, List<StepResult> results)
    {
        var resultsText = new StringBuilder();
        foreach (var result in results)
        {
            resultsText.AppendLine($"Step: {result.Step}");
            resultsText.AppendLine($"Tool: {result.ToolCall.ToolName}");
            resultsText.AppendLine($"Success: {result.Success}");
            resultsText.AppendLine($"Result: {result.Result}");
            resultsText.AppendLine();
        }

        var systemPrompt = "You are a summarization system that creates concise summaries of task execution results. " +
                          "Focus on key outcomes, achievements, and any important information discovered.";

        var userPrompt = $"Task: {plan.Task}\n\nExecution Results:\n{resultsText}\n\n" +
                        "Create a concise summary of the execution results, focusing on key outcomes and findings.";

        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.3f,
            MaxTokens = 500,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            }
        };

        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);

        return response.Value.Choices[0].Message.Content;
    }

    private async Task<string> GenerateReflectionAsync(PlanningResult plan, List<StepResult> results)
    {
        var resultsText = new StringBuilder();
        foreach (var result in results)
        {
            resultsText.AppendLine($"Step: {result.Step}");
            resultsText.AppendLine($"Tool: {result.ToolCall.ToolName}");
            resultsText.AppendLine($"Success: {result.Success}");
            resultsText.AppendLine($"Result: {result.Result}");
            resultsText.AppendLine();
        }

        var systemPrompt = "You are a reflective system that analyzes task execution to identify lessons learned. " +
                          "Focus on what went well, what could be improved, and insights for future tasks.";

        var userPrompt = $"Task: {plan.Task}\n\nStrategic Plan: {plan.StrategicPlan}\n\nExecution Results:\n{resultsText}\n\n" +
                        "Reflect on this execution. What went well? What could be improved? What lessons can be learned for future tasks?";

        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.4f,
            MaxTokens = 800,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            }
        };

        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);

        return response.Value.Choices[0].Message.Content;
    }
}

public class ToolDefinition
{
    public string Name { get; set; }
    public string Description { get; set; }
    public object ParametersSchema { get; set; }
    public string Endpoint { get; set; }
}

public class PlanningResult
{
    public string Task { get; set; }
    public string StrategicPlan { get; set; }
    public List<string> Steps { get; set; } = new List<string>();
    public List<ToolCallPlan> ToolCalls { get; set; } = new List<ToolCallPlan>();
}

public class ToolCallPlan
{
    public string Step { get; set; }
    public string ToolName { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
}

public class StepResult
{
    public string Step { get; set; }
    public ToolCallPlan ToolCall { get; set; }
    public string Result { get; set; }
    public bool Success { get; set; }
}

public class ExecutionResult
{
    public PlanningResult Plan { get; set; }
    public List<StepResult> StepResults { get; set; } = new List<StepResult>();
    public bool Success { get; set; }
    public string Summary { get; set; }
    public string Reflection { get; set; }
}
