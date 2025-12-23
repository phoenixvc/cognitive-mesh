namespace FoundationLayer.LLM.Abstractions
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
        /// Creates a new chat session.
        /// </summary>
        /// <param name="systemPrompt">Optional system prompt to initialize the chat.</param>
        /// <returns>A new chat session.</returns>
        IChatSession CreateChatSession(string systemPrompt = null);
    }

    /// <summary>
    /// Represents a chat session with the model.
    /// </summary>
    public interface IChatSession : IDisposable
    {
        /// <summary>
        /// Adds a user message to the chat.
        /// </summary>
        /// <param name="message">The user's message.</param>
        void AddUserMessage(string message);

        /// <summary>
        /// Gets a response from the model.
        /// </summary>
        /// <returns>The model's response.</returns>
        Task<string> GetResponseAsync();
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
