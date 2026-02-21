namespace MetacognitiveLayer.Protocols.LLM
{
    /// <summary>
    /// Options for LLM completion requests.
    /// </summary>
    public class LLMOptions
    {
        /// <summary>
        /// Maximum number of tokens to generate
        /// </summary>
        public int MaxTokens { get; set; } = 1000;

        /// <summary>
        /// Temperature for generation (0.0 to 1.0)
        /// </summary>
        public float Temperature { get; set; } = 0.7f;

        /// <summary>
        /// Model to use for completion
        /// </summary>
        public string Model { get; set; } = "default";

        /// <summary>
        /// Whether to include citations in the response
        /// </summary>
        public bool IncludeCitations { get; set; } = false;

        /// <summary>
        /// System message for chat-based models
        /// </summary>
        public string SystemMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interface for Large Language Model providers.
    /// </summary>
    public interface ILLMProvider
    {
        /// <summary>
        /// Completes a prompt using the LLM provider.
        /// </summary>
        /// <param name="prompt">The prompt to complete</param>
        /// <param name="options">Options for the completion</param>
        /// <returns>The completed text</returns>
        Task<string> CompletePromptAsync(string prompt, LLMOptions? options = null);

        /// <summary>
        /// Embeds text into a vector representation.
        /// </summary>
        /// <param name="text">The text to embed</param>
        /// <returns>The embedding vector as a string</returns>
        Task<string> CreateEmbeddingAsync(string text);
    }
}