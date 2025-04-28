using Azure.AI.OpenAI;
using System.Text.Json;

public class SelfEvaluator
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _completionDeployment;

    public SelfEvaluator(string openAIEndpoint, string openAIApiKey, string completionDeployment)
    {
        _openAIClient = new OpenAIClient(new Uri(openAIEndpoint), new AzureKeyCredential(openAIApiKey));
        _completionDeployment = completionDeployment;
    }

    public async Task<SystemPerformanceEvaluation> EvaluateSystemPerformanceAsync(string systemComponent, List<string> metrics)
    {
        try
        {
            var systemPrompt = "You are a system performance evaluator. Analyze the provided metrics to evaluate the performance of the specified system component.";
            var userPrompt = $"Component: {systemComponent}\nMetrics: {string.Join(", ", metrics)}";

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
            var evaluation = response.Value.Choices[0].Message.Content;

            return new SystemPerformanceEvaluation
            {
                Component = systemComponent,
                Metrics = metrics,
                Evaluation = evaluation
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to evaluate system performance for component: {systemComponent}. Error: {ex.Message}");
        }
    }

    public async Task<string> GenerateSelfEvaluationReportAsync(string systemComponent, List<string> metrics)
    {
        try
        {
            var evaluation = await EvaluateSystemPerformanceAsync(systemComponent, metrics);

            var systemPrompt = "You are a self-evaluation report generator. Generate a detailed self-evaluation report based on the provided performance evaluation.";
            var userPrompt = $"Component: {systemComponent}\nMetrics: {string.Join(", ", metrics)}\nEvaluation: {evaluation.Evaluation}";

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
            var report = response.Value.Choices[0].Message.Content;

            return report;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to generate self-evaluation report for component: {systemComponent}. Error: {ex.Message}");
        }
    }
}

public class SystemPerformanceEvaluation
{
    public string Component { get; set; }
    public List<string> Metrics { get; set; }
    public string Evaluation { get; set; }
}
