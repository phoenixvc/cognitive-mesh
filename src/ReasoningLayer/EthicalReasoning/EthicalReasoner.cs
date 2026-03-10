using OpenAI.Chat;

namespace CognitiveMesh.ReasoningLayer.EthicalReasoning;

/// <summary>
/// Performs ethical reasoning analysis by evaluating actions and decisions
/// against ethical frameworks using LLM-powered assessment.
/// </summary>
public class EthicalReasoner
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<EthicalReasoner> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EthicalReasoner"/> class.
    /// </summary>
    public EthicalReasoner(string openAIEndpoint, string openAIApiKey, string completionDeployment, ILogger<EthicalReasoner> logger)
    {
        var aoaiClient = new AzureOpenAIClient(new Uri(openAIEndpoint), new AzureKeyCredential(openAIApiKey));
        _chatClient = aoaiClient.GetChatClient(completionDeployment);
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

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(input)
            };

            var options = new ChatCompletionOptions
            {
                Temperature = 0.3f,
                MaxOutputTokenCount = 800
            };

            var completion = await _chatClient.CompleteChatAsync(messages, options);
            return completion.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error considering ethical implications with input: {Input}", input);
            throw;
        }
    }
}
