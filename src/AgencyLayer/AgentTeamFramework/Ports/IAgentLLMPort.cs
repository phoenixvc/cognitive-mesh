namespace AgencyLayer.AgentTeamFramework.Ports;

/// <summary>
/// Port for LLM interactions required by the agent team framework.
/// Implementations bridge to the actual LLM client (e.g., Azure OpenAI, Anthropic).
/// This decouples the team framework from specific LLM client libraries.
/// </summary>
public interface IAgentLLMPort
{
    /// <summary>
    /// Sends a system prompt and user message to an LLM and returns the response.
    /// </summary>
    /// <param name="systemPrompt">The agent's system prompt/persona.</param>
    /// <param name="userMessage">The task description and context.</param>
    /// <param name="temperature">Sampling temperature (0.0–1.0).</param>
    /// <param name="maxTokens">Maximum tokens for the response, or <c>null</c> for model default.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The LLM completion result.</returns>
    Task<LLMCompletionResult> CompleteAsync(
        string systemPrompt,
        string userMessage,
        double temperature = 0.7,
        int? maxTokens = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result from an LLM completion request.
/// </summary>
public sealed class LLMCompletionResult
{
    /// <summary>
    /// The generated text from the LLM.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Number of tokens used (prompt + completion), if available.
    /// </summary>
    public int TokensUsed { get; init; }
}
