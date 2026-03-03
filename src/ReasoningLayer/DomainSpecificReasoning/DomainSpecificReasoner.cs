namespace CognitiveMesh.ReasoningLayer.DomainSpecificReasoning;

/// <summary>
/// Port interface for retrieving domain-specific knowledge from external sources
/// such as knowledge bases, vector databases, or specialized APIs.
/// </summary>
public interface IDomainKnowledgePort
{
    /// <summary>
    /// Retrieves domain-specific knowledge relevant to the given domain and key phrases.
    /// </summary>
    /// <param name="domain">The knowledge domain to search within (e.g., "finance", "healthcare", "legal").</param>
    /// <param name="keyPhrases">Comma-separated key phrases extracted from the input to guide retrieval.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A string containing the retrieved domain knowledge context.</returns>
    Task<string> RetrieveKnowledgeAsync(string domain, string keyPhrases, CancellationToken cancellationToken = default);
}

/// <summary>
/// Applies domain-specific reasoning by extracting key phrases from input, retrieving
/// relevant domain knowledge via the <see cref="IDomainKnowledgePort"/>, and synthesizing
/// an expert response using LLM-based reasoning.
/// </summary>
public class DomainSpecificReasoner
{
    private readonly TextAnalyticsClient _textAnalyticsClient;
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<DomainSpecificReasoner> _logger;
    private readonly IDomainKnowledgePort _domainKnowledgePort;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainSpecificReasoner"/> class.
    /// </summary>
    /// <param name="textAnalyticsClient">The Azure Text Analytics client for key phrase extraction.</param>
    /// <param name="openAIClient">The Azure OpenAI client for LLM-based reasoning.</param>
    /// <param name="logger">The logger instance for structured logging.</param>
    /// <param name="domainKnowledgePort">The port for retrieving domain-specific knowledge.</param>
    public DomainSpecificReasoner(
        TextAnalyticsClient textAnalyticsClient,
        OpenAIClient openAIClient,
        ILogger<DomainSpecificReasoner> logger,
        IDomainKnowledgePort domainKnowledgePort)
    {
        _textAnalyticsClient = textAnalyticsClient ?? throw new ArgumentNullException(nameof(textAnalyticsClient));
        _openAIClient = openAIClient ?? throw new ArgumentNullException(nameof(openAIClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _domainKnowledgePort = domainKnowledgePort ?? throw new ArgumentNullException(nameof(domainKnowledgePort));
    }

    /// <summary>
    /// Applies domain-specific specialized knowledge to answer a question or process input
    /// within a particular domain context.
    /// </summary>
    /// <param name="domain">The knowledge domain (e.g., "finance", "healthcare", "legal").</param>
    /// <param name="input">The input text or question to process with domain expertise.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The domain-expert response synthesized from retrieved knowledge and LLM reasoning.</returns>
    public async Task<string> ApplySpecializedKnowledgeAsync(string domain, string input, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Applying specialized knowledge for domain: {Domain}", domain);

            var keyPhrases = await ExtractKeyPhrasesAsync(input, cancellationToken);
            _logger.LogDebug("Extracted key phrases for domain {Domain}: {KeyPhrases}", domain, keyPhrases);

            var domainKnowledge = await _domainKnowledgePort.RetrieveKnowledgeAsync(domain, keyPhrases, cancellationToken);

            if (string.IsNullOrWhiteSpace(domainKnowledge))
            {
                _logger.LogWarning("No domain knowledge retrieved for domain: {Domain} with key phrases: {KeyPhrases}. Proceeding with general knowledge.", domain, keyPhrases);
                domainKnowledge = "No specific domain knowledge was found. Use general expertise to answer.";
            }

            var systemPrompt = $"You are an expert in {domain}. Use the following domain-specific knowledge to answer the question.";
            var chatCompletionOptions = new ChatCompletionsOptions
            {
                DeploymentName = "text-davinci-003",
                Temperature = 0.3f,
                MaxTokens = 800,
                Messages =
                {
                    new ChatRequestSystemMessage(systemPrompt),
                    new ChatRequestUserMessage($"Domain Knowledge:\n{domainKnowledge}\n\nQuestion: {input}")
                }
            };

            var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions, cancellationToken);

            _logger.LogInformation("Successfully applied specialized knowledge for domain: {Domain}", domain);
            return response.Value.Choices[0].Message.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying specialized knowledge for domain: {Domain} with input: {Input}", domain, input);
            throw;
        }
    }

    private async Task<string> ExtractKeyPhrasesAsync(string input, CancellationToken cancellationToken = default)
    {
        var response = await _textAnalyticsClient.ExtractKeyPhrasesAsync(input, cancellationToken: cancellationToken);
        return string.Join(", ", response.Value);
    }
}