namespace CognitiveMesh.ReasoningLayer.EthicalReasoning;

/// <summary>
/// Performs ethical reasoning analysis by evaluating actions and decisions
/// against ethical frameworks using LLM-powered assessment.
/// </summary>
public class EthicalReasoner
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _completionDeployment;
    private readonly ILogger<EthicalReasoner> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EthicalReasoner"/> class.
    /// </summary>
    public EthicalReasoner(string openAIEndpoint, string openAIApiKey, string completionDeployment, ILogger<EthicalReasoner> logger)
    {
        _openAIClient = new OpenAIClient(new Uri(openAIEndpoint), new AzureKeyCredential(openAIApiKey));
        _completionDeployment = completionDeployment;
        _logger = logger;
    }

    /// <summary>
    /// Analyzes the ethical implications of the given input scenario.
    /// </summary>
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