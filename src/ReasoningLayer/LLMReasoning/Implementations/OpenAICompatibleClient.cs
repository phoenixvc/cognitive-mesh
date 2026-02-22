using CognitiveMesh.ReasoningLayer.LLMReasoning.Abstractions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CognitiveMesh.ReasoningLayer.LLMReasoning.Implementations
{
    /// <summary>
    /// Generic OpenAI-compatible client that works with any provider exposing the
    /// /v1/chat/completions endpoint (OpenAI, DeepSeek, Grok, Llama via Ollama, etc.).
    /// </summary>
    public class OpenAICompatibleClient : ILLMClient, IDisposable
    {
        private readonly string _modelName;
        private readonly string _apiKey;
        private readonly string _endpoint;
        private readonly ILogger<OpenAICompatibleClient>? _logger;
        private readonly int _maxTokens;
        private HttpClient? _httpClient;
        private bool _disposed;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <inheritdoc/>
        public string ModelName => _modelName;

        /// <inheritdoc/>
        public int MaxTokens => _maxTokens;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAICompatibleClient"/> class.
        /// </summary>
        /// <param name="apiKey">The provider API key (empty for local models).</param>
        /// <param name="endpoint">The API base URL (e.g., https://api.openai.com/v1).</param>
        /// <param name="modelName">The model identifier.</param>
        /// <param name="maxTokens">Maximum output tokens.</param>
        /// <param name="logger">Optional logger instance.</param>
        public OpenAICompatibleClient(
            string apiKey,
            string endpoint,
            string modelName,
            int maxTokens = 16_384,
            ILogger<OpenAICompatibleClient>? logger = null)
        {
            _apiKey = apiKey ?? string.Empty;
            _endpoint = endpoint?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(endpoint));
            _modelName = modelName ?? throw new ArgumentNullException(nameof(modelName));
            _maxTokens = maxTokens > 0 ? maxTokens : 16_384;
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

            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _apiKey);
            }
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            _logger?.LogInformation("OpenAI-compatible client initialized for model {ModelName} at {Endpoint}",
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
                _logger?.LogDebug("Chat completion request to {Endpoint}: {MessageCount} messages, model={Model}",
                    _endpoint, messages.Count(), _modelName);

                var response = await _httpClient!.PostAsync("/v1/chat/completions", content, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger?.LogError("API error {StatusCode}: {Body}",
                        (int)response.StatusCode, responseBody);
                    throw new InvalidOperationException(
                        $"API request failed with status {(int)response.StatusCode}: {responseBody}");
                }

                using var doc = JsonDocument.Parse(responseBody);
                var choiceContent = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString()?.Trim() ?? string.Empty;

                _logger?.LogDebug("Generated {Length} character response from {Model}",
                    choiceContent.Length, _modelName);
                return choiceContent;
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError(ex, "HTTP request to {Endpoint} failed", _endpoint);
                throw new InvalidOperationException($"Failed to communicate with {_endpoint}", ex);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger?.LogError(ex, "Request to {Endpoint} timed out", _endpoint);
                throw new InvalidOperationException($"Request to {_endpoint} timed out", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<float[]> GetEmbeddingsAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be null or whitespace.", nameof(text));

            var results = await GetBatchEmbeddingsAsync(new[] { text }, cancellationToken);
            return results.FirstOrDefault() ?? [];
        }

        /// <inheritdoc/>
        public async Task<float[][]> GetBatchEmbeddingsAsync(
            IEnumerable<string> texts,
            CancellationToken cancellationToken = default)
        {
            if (texts == null || !texts.Any())
                throw new ArgumentException("Texts cannot be null or empty.", nameof(texts));

            await EnsureInitializedAsync(cancellationToken);

            var requestBody = new
            {
                model = _modelName,
                input = texts.ToArray()
            };

            var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient!.PostAsync("/v1/embeddings", content, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        $"Embeddings request failed with status {(int)response.StatusCode}: {responseBody}");
                }

                using var doc = JsonDocument.Parse(responseBody);
                var data = doc.RootElement.GetProperty("data");
                var embeddings = new List<float[]>();

                foreach (var item in data.EnumerateArray())
                {
                    var embedding = item.GetProperty("embedding")
                        .EnumerateArray()
                        .Select(e => e.GetSingle())
                        .ToArray();
                    embeddings.Add(embedding);
                }

                return embeddings.ToArray();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to get embeddings from {_endpoint}", ex);
            }
        }

        /// <summary>Generates a completion with extended parameters.</summary>
        public async Task<string> GenerateCompletionAsync(
            string prompt,
            int maxTokens = 1000,
            float temperature = 0.7f,
            IEnumerable<string>? stopSequences = null,
            CancellationToken cancellationToken = default)
        {
            return await GenerateCompletionAsync(prompt, temperature, maxTokens, cancellationToken);
        }

        /// <summary>Generates an embedding.</summary>
        public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
            => GetEmbeddingsAsync(text, cancellationToken);

        /// <summary>Generates multiple completions.</summary>
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
                results.Add(await GenerateCompletionAsync(prompt, temperature, maxTokens, cancellationToken));
            }
            return results;
        }

        /// <summary>Approximate token count.</summary>
        public int GetTokenCount(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            return (int)Math.Ceiling(text.Length / 4.0);
        }

        /// <summary>Creates a chat session.</summary>
        public IChatSession StartChat(string? systemMessage = null)
        {
            return new CompatibleChatSession(this, systemMessage);
        }

        private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
        {
            if (_httpClient == null) await InitializeAsync(cancellationToken);
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing) _httpClient?.Dispose();
                _disposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private sealed class CompatibleChatSession : IChatSession
        {
            private readonly OpenAICompatibleClient _owner;
            private readonly List<ChatMessage> _history = new();

            public CompatibleChatSession(OpenAICompatibleClient owner, string? systemMessage)
            {
                _owner = owner;
                if (systemMessage != null)
                    _history.Add(new ChatMessage("system", systemMessage));
            }

            public IReadOnlyList<ChatMessage> History => _history.AsReadOnly();

            public async Task<string> SendMessageAsync(string message, CancellationToken cancellationToken = default)
            {
                _history.Add(new ChatMessage("user", message));
                var sharedMessages = _history.Select(m => new CognitiveMesh.Shared.Interfaces.ChatMessage(m.Role, m.Content));
                var response = await _owner.GenerateChatCompletionAsync(sharedMessages, cancellationToken: cancellationToken);
                _history.Add(new ChatMessage("assistant", response));
                return response;
            }

            public void Dispose() { }
        }
    }
}
