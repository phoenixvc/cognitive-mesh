using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveMesh.ReasoningLayer.LLMReasoning.Abstractions
{
    /// <summary>
    /// Defines the contract for interacting with a Language Model (LLM) service.
    /// </summary>
    public interface ILLMClient : IDisposable
    {
        /// <summary>
        /// Gets the name of the model being used.
        /// </summary>
        string ModelName { get; }

        /// <summary>
        /// Generates a completion based on the provided prompt.
        /// </summary>
        /// <param name="prompt">The prompt to send to the model.</param>
        /// <param name="maxTokens">The maximum number of tokens to generate.</param>
        /// <param name="temperature">The sampling temperature to use.</param>
        /// <param name="stopSequences">Optional sequences where the model will stop generating.</param>
        /// <returns>The generated text completion.</returns>
        Task<string> GenerateCompletionAsync(
            string prompt,
            int maxTokens = 1000,
            float temperature = 0.7f,
            IEnumerable<string> stopSequences = null);

        /// <summary>
        /// Generates an embedding vector for the given text.
        /// </summary>
        /// <param name="text">The text to generate an embedding for.</param>
        /// <returns>An array of floats representing the embedding vector.</returns>
        Task<float[]> GenerateEmbeddingAsync(string text);

        /// <summary>
        /// Generates multiple completions for the same prompt.
        /// </summary>
        /// <param name="prompt">The prompt to send to the model.</param>
        /// <param name="numCompletions">The number of completions to generate.</param>
        /// <param name="maxTokens">The maximum number of tokens to generate per completion.</param>
        /// <param name="temperature">The sampling temperature to use.</param>
        /// <returns>A list of generated completions.</returns>
        Task<IEnumerable<string>> GenerateMultipleCompletionsAsync(
            string prompt,
            int numCompletions = 3,
            int maxTokens = 500,
            float temperature = 0.8f);

        /// <summary>
        /// Gets the token count for the given text.
        /// </summary>
        /// <param name="text">The text to count tokens for.</param>
        /// <returns>The number of tokens in the text.</returns>
        int GetTokenCount(string text);

        /// <summary>
        /// Starts a chat session with the model.
        /// </summary>
        /// <param name="systemMessage">Optional system message to set the behavior of the assistant.</param>
        /// <returns>A chat session that can be used to exchange messages with the model.</returns>
        IChatSession StartChat(string systemMessage = null);
    }

    /// <summary>
    /// Represents a chat session with the model.
    /// </summary>
    public interface IChatSession : IDisposable
    {
        /// <summary>
        /// Sends a message to the model and gets a response.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>The model's response.</returns>
        Task<string> SendMessageAsync(string message);

        /// <summary>
        /// Gets the conversation history.
        /// </summary>
        IReadOnlyList<ChatMessage> History { get; }
    }

    /// <summary>
    /// Represents a message in a chat conversation.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Gets the role of the message sender (e.g., "user", "assistant", "system").
        /// </summary>
        public string Role { get; }

        /// <summary>
        /// Gets the content of the message.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Gets the timestamp when the message was created.
        /// </summary>
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatMessage"/> class.
        /// </summary>
        /// <param name="role">The role of the message sender.</param>
        /// <param name="content">The content of the message.</param>
        public ChatMessage(string role, string content)
        {
            Role = role ?? throw new ArgumentNullException(nameof(role));
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }
    }
}
