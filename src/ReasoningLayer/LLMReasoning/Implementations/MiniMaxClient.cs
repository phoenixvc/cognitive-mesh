using CognitiveMesh.ReasoningLayer.LLMReasoning.Abstractions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CognitiveMesh.ReasoningLayer.LLMReasoning.Implementations
{
    /// <summary>
    /// Implementation of <see cref="ILLMClient"/> for MiniMax M2.5 and M1 models.
    /// Uses the MiniMax Chat Completions API which is OpenAI-compatible.
    /// </summary>
    public class MiniMaxClient : ILLMClient, IDisposable
    {
        private readonly string _modelName;
        private readonly string _apiKey;
        private readonly string _endpoint;
        private readonly ILogger<MiniMaxClient>? _logger;
        private readonly int _maxTokens;
        private HttpClient? _httpClient;
        private bool _disposed;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <inheritdoc/>
        public string ModelName => _modelName;

        /// <inheritdoc/>
        public int MaxTokens => _maxTokens;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniMaxClient"/> class.
        /// </summary>
        /// <param name="apiKey">The MiniMax API key.</param>
        /// <param name="modelName">The model name (e.g., "MiniMax-M2.5", "MiniMax-M1").</param>
        /// <param name="maxTokens">Maximum output tokens.</param>
        /// <param name="endpoint">API endpoint (default: https://api.minimax.chat/v1).</param>
        /// <param name="logger">Optional logger instance.</param>
        public MiniMaxClient(
            string apiKey,
            string modelName = "MiniMax-M2.5",
            int maxTokens = 32_000,
            string endpoint = "https://api.minimax.chat/v1",
            ILogger<MiniMaxClient>? logger = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be null or whitespace.", nameof(apiKey));

            _apiKey = apiKey;
            _modelName = string.IsNullOrWhiteSpace(modelName) ? "MiniMax-M2.5" : modelName;
            _maxTokens = maxTokens > 0 ? maxTokens : 32_000;
            _endpoint = endpoint?.TrimEnd('/') ?? "https://api.minimax.chat/v1";
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            };
        }

        /// <inheritdoc/>
        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_httpClient != null) return Task.CompletedTask;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_endpoint),
                Timeout = TimeSpan.FromSeconds(120)
            };
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            _logger?.LogInformation("MiniMax client initialized for model {ModelName} at {Endpoint}",
                _modelName, _endpoint);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<string> GenerateCompletionAsync(
            string prompt,
            float temperature = 0.7f,
            int maxTokens = 1000,
            CancellationToken cancellationToken = default)
        {
            var messages = new List<CognitiveMesh.Shared.Interfaces.ChatMessage>
            {
                new("user", prompt)
            };
            return await GenerateChatCompletionAsync(messages, temperature, maxTokens, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<string> GenerateChatCompletionAsync(
            IEnumerable<CognitiveMesh.Shared.Interfaces.ChatMessage> messages,
            float temperature = 0.7f,
            int maxTokens = 1000,
            CancellationToken cancellationToken = default)
        {
            if (messages == null || !messages.Any())
                throw new ArgumentException("Messages cannot be null or empty.", nameof(messages));

            await EnsureInitializedAsync(cancellationToken);

            var requestBody = new
            {
                model = _modelName,
                messages = messages.Select(m => new { role = m.Role.ToLower(), content = m.Content }),
                temperature = Math.Clamp(temperature, 0f, 2f),
                max_tokens = Math.Min(maxTokens, _maxTokens),
                stream = false
            };

            var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                _logger?.LogDebug("MiniMax chat completion request: {MessageCount} messages, model={Model}",
                    messages.Count(), _modelName);

                var response = await _httpClient!.PostAsync("/v1/chat/completions", content, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger?.LogError("MiniMax API error {StatusCode}: {Body}",
                        (int)response.StatusCode, responseBody);
                    throw new InvalidOperationException(
                        $"MiniMax API request failed with status {(int)response.StatusCode}: {responseBody}");
                }

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                var choiceContent = root
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString()?.Trim() ?? string.Empty;

                _logger?.LogDebug("MiniMax generated {Length} character response", choiceContent.Length);
                return choiceContent;
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError(ex, "MiniMax HTTP request failed");
                throw new InvalidOperationException("Failed to communicate with MiniMax API", ex);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger?.LogError(ex, "MiniMax request timed out");
                throw new InvalidOperationException("MiniMax API request timed out", ex);
            }
        }

        /// <inheritdoc/>
        public Task<float[]> GetEmbeddingsAsync(string text, CancellationToken cancellationToken = default)
        {
            // MiniMax M2.5 does not natively provide embeddings.
            // In a polyglot setup, embeddings should be routed to a dedicated embedding model.
            throw new NotSupportedException(
                $"Model {_modelName} does not support embeddings. " +
                "Configure a dedicated embedding model (e.g., text-embedding-3-large) for the Embeddings use case.");
        }

        /// <inheritdoc/>
        public Task<float[][]> GetBatchEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException(
                $"Model {_modelName} does not support embeddings. " +
                "Configure a dedicated embedding model for the Embeddings use case.");
        }

        /// <summary>
        /// Generates a completion with extended parameters (ILLMClient from ReasoningLayer).
        /// </summary>
        public async Task<string> GenerateCompletionAsync(
            string prompt,
            int maxTokens = 1000,
            float temperature = 0.7f,
            IEnumerable<string>? stopSequences = null,
            CancellationToken cancellationToken = default)
        {
            // stopSequences are passed in the request body if the API supports them
            return await GenerateCompletionAsync(prompt, temperature, maxTokens, cancellationToken);
        }

        /// <summary>
        /// Generates an embedding via the completion endpoint (forwarding to ReasoningLayer interface).
        /// </summary>
        public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
            => GetEmbeddingsAsync(text, cancellationToken);

        /// <summary>
        /// Generates multiple completions by calling the API multiple times.
        /// </summary>
        public async Task<IEnumerable<string>> GenerateMultipleCompletionsAsync(
            string prompt,
            int numCompletions = 3,
            int maxTokens = 500,
            float temperature = 0.8f,
            CancellationToken cancellationToken = default)
        {
            var results = new List<string>(numCompletions);
            for (int i = 0; i < numCompletions; i++)
            {
                var completion = await GenerateCompletionAsync(prompt, temperature, maxTokens, cancellationToken);
                results.Add(completion);
            }
            return results;
        }

        /// <summary>
        /// Approximate token count.
        /// </summary>
        public int GetTokenCount(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            return (int)Math.Ceiling(text.Length / 4.0);
        }

        /// <summary>
        /// Creates a chat session backed by MiniMax.
        /// </summary>
        public IChatSession StartChat(string? systemMessage = null)
        {
            return new MiniMaxChatSession(this, systemMessage);
        }

        private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
        {
            if (_httpClient == null)
                await InitializeAsync(cancellationToken);
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                }
                _disposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private sealed class MiniMaxChatSession : IChatSession
        {
            private readonly MiniMaxClient _owner;
            private readonly List<CognitiveMesh.Shared.Interfaces.ChatMessage> _history = new();

            public MiniMaxChatSession(MiniMaxClient owner, string? systemMessage)
            {
                _owner = owner;
                if (systemMessage != null)
                    _history.Add(new CognitiveMesh.Shared.Interfaces.ChatMessage("system", systemMessage));
            }

            public IReadOnlyList<CognitiveMesh.Shared.Interfaces.ChatMessage> History => _history.AsReadOnly();

            public async Task<string> SendMessageAsync(string message, CancellationToken cancellationToken = default)
            {
                _history.Add(new CognitiveMesh.Shared.Interfaces.ChatMessage("user", message));
                var response = await _owner.GenerateChatCompletionAsync(_history, cancellationToken: cancellationToken);
                _history.Add(new CognitiveMesh.Shared.Interfaces.ChatMessage("assistant", response));
                return response;
            }

            public void Dispose() { }
        }
    }
}
