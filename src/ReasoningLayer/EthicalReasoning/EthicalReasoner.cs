namespace CognitiveMesh.ReasoningLayer.EthicalReasoning;

public class EthicalReasoner
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _completionDeployment;
    private readonly ILogger<EthicalReasoner> _logger;

    public EthicalReasoner(string openAIEndpoint, string openAIApiKey, string completionDeployment, ILogger<EthicalReasoner> logger)
    {
        _openAIClient = new OpenAIClient(new Uri(openAIEndpoint), new AzureKeyCredential(openAIApiKey));
        _completionDeployment = completionDeployment;
        _logger = logger;
    }

    public async Task<string> ConsiderEthicalImplicationsAsync(string input)
    {
        try
        {
            var systemPrompt = "You are an ethical reasoner. Consider the ethical implications of the following scenario and provide a detailed analysis.";

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
            _logger.LogError(ex, "Error considering ethical implications with input: {Input}", input);
            throw;
        }
    }
}