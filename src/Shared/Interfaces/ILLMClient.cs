using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CognitiveMesh.Shared.Interfaces
{
    /// <summary>
    /// Defines the interface for LLM (Large Language Model) client operations
    /// </summary>
    public interface ILLMClient : IDisposable
    {
        /// <summary>
        /// Gets the name of the LLM model being used
        /// </summary>
        string ModelName { get; }

        /// <summary>
        /// Gets the maximum number of tokens the model can handle
        /// </summary>
        int MaxTokens { get; }

        /// <summary>
        /// Initializes the LLM client
        /// </summary>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a completion based on the provided prompt
        /// </summary>
        Task<string> GenerateCompletionAsync(
            string prompt, 
            float temperature = 0.7f, 
            int maxTokens = 1000,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a chat completion based on the provided messages
        /// </summary>
        Task<string> GenerateChatCompletionAsync(
            IEnumerable<ChatMessage> messages, 
            float temperature = 0.7f, 
            int maxTokens = 1000,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets embeddings for the provided text
        /// </summary>
        Task<float[]> GetEmbeddingsAsync(
            string text, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets embeddings for multiple texts in a single batch
        /// </summary>
        Task<float[][]> GetBatchEmbeddingsAsync(
            IEnumerable<string> texts, 
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents a message in a chat conversation
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// The role of the message sender (e.g., "user", "assistant", "system")
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// The content of the message
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Creates a new chat message
        /// </summary>
        public ChatMessage(string role, string content)
        {
            Role = role ?? throw new ArgumentNullException(nameof(role));
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }
    }
}
