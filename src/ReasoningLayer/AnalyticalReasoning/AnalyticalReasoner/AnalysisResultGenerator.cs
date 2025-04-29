using System.Threading.Tasks;
using Azure.AI.OpenAI;

public class AnalysisResultGenerator
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _completionDeployment;

    public AnalysisResultGenerator(OpenAIClient openAIClient, string completionDeployment)
    {
        _openAIClient = openAIClient;
        _completionDeployment = completionDeployment;
    }

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
