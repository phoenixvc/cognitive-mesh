using AzureOpenAIClient = Azure.AI.OpenAI.OpenAIClient;
using CognitiveMesh.ReasoningLayer.LLMReasoning.Abstractions;
using System.Text.Json;

namespace CognitiveMesh.ReasoningLayer.LLMReasoning.Implementations
{
    /// <summary>
    /// Implementation of ILLMClient using Azure OpenAI
    /// </summary>
    public class OpenAIClient : ILLMClient, IDisposable
    {
        private readonly string _modelName;
        private readonly string _deploymentName;
        private readonly string _apiKey;
        private readonly string _endpoint;
        private readonly ILogger<OpenAIClient>? _logger;
        private AzureOpenAIClient? _client;
        private bool _disposed = false;
        private readonly int _maxTokens;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <inheritdoc/>
        public string ModelName => _modelName;

        /// <inheritdoc/>
        public int MaxTokens => _maxTokens;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAIClient"/> class.
        /// </summary>
        /// <param name="apiKey">The Azure OpenAI API key.</param>
        /// <param name="endpoint">The Azure OpenAI endpoint.</param>
        /// <param name="deploymentName">The deployment name to use.</param>
        /// <param name="modelName">The name of the model to use.</param>
        /// <param name="maxTokens">The maximum number of tokens the model can handle.</param>
        /// <param name="logger">Optional logger instance.</param>
        public OpenAIClient(
            string apiKey,
            string endpoint,
            string deploymentName,
            string modelName = "gpt-4",
            int maxTokens = 8192,
            ILogger<OpenAIClient>? logger = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be null or whitespace.", nameof(apiKey));
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Endpoint cannot be null or whitespace.", nameof(endpoint));
            if (string.IsNullOrWhiteSpace(deploymentName))
                throw new ArgumentException("Deployment name cannot be null or whitespace.", nameof(deploymentName));
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be null or whitespace.", nameof(modelName));

            _apiKey = apiKey;
            _endpoint = endpoint.TrimEnd('/');
            _deploymentName = deploymentName;
            _modelName = modelName;
            _maxTokens = maxTokens > 0 ? maxTokens : 8192;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_client != null)
                return;

            try
            {
                _logger?.LogInformation("Initializing Azure OpenAI client for model {ModelName}", _modelName);

                var credential = new AzureKeyCredential(_apiKey);
                var options = new OpenAIClientOptions();
                _client = new AzureOpenAIClient(new Uri(_endpoint), credential, options);

                _logger?.LogInformation("Successfully initialized Azure OpenAI client");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize Azure OpenAI client");
                throw new InvalidOperationException("Failed to initialize Azure OpenAI client", ex);
            }

            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<string> GenerateCompletionAsync(
            string prompt,
            int maxTokens = 1000,
            float temperature = 0.7f,
            IEnumerable<string>? stopSequences = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be null or whitespace.", nameof(prompt));
            if (temperature < 0 || temperature > 2)
                throw new ArgumentOutOfRangeException(nameof(temperature), "Temperature must be between 0 and 2.");
            if (maxTokens <= 0 || maxTokens > _maxTokens)
                throw new ArgumentOutOfRangeException(nameof(maxTokens), $"Max tokens must be between 1 and {_maxTokens}.");

            await EnsureInitializedAsync(cancellationToken);

            try
            {
                _logger?.LogDebug("Generating completion with {Length} characters", prompt?.Length ?? 0);

                var options = new CompletionsOptions
                {
                    DeploymentName = _deploymentName,
                    Prompts = { prompt },
                    MaxTokens = maxTokens,
                    Temperature = temperature,
                    NucleusSamplingFactor = 0.95f,
                    FrequencyPenalty = 0,
                    PresencePenalty = 0,
                    GenerationSampleCount = 1
                };

                if (stopSequences != null)
                {
                    foreach (var seq in stopSequences)
                        options.StopSequences.Add(seq);
                }

                var response = await _client!.GetCompletionsAsync(options, cancellationToken);
                var completion = response.Value.Choices[0].Text?.Trim() ?? string.Empty;

                _logger?.LogDebug("Successfully generated completion with {Length} characters", completion.Length);
                return completion;
            }
            catch (RequestFailedException ex)
            {
                _logger?.LogError(ex, "Azure OpenAI API request failed with status {Status}", ex.Status);
                throw new InvalidOperationException("Failed to generate completion", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to generate completion");
                throw new InvalidOperationException("Failed to generate completion", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string> GenerateChatCompletionAsync(
            IEnumerable<ChatMessage> messages,
            float temperature = 0.7f,
            int maxTokens = 1000,
            CancellationToken cancellationToken = default)
        {
            if (messages == null || !messages.Any())
                throw new ArgumentException("Messages cannot be null or empty.", nameof(messages));
            if (temperature < 0 || temperature > 2)
                throw new ArgumentOutOfRangeException(nameof(temperature), "Temperature must be between 0 and 2.");
            if (maxTokens <= 0 || maxTokens > _maxTokens)
                throw new ArgumentOutOfRangeException(nameof(maxTokens), $"Max tokens must be between 1 and {_maxTokens}.");

            await EnsureInitializedAsync(cancellationToken);

            try
            {
                _logger?.LogDebug("Generating chat completion with {Count} messages", messages.Count());
                
                var chatMessages = messages.Select(m =>
                {
                    ChatRequestMessage msg = m.Role.ToLower() switch
                    {
                        "system" => new ChatRequestSystemMessage(m.Content),
                        "assistant" => new ChatRequestAssistantMessage(m.Content),
                        _ => new ChatRequestUserMessage(m.Content)
                    };
                    return msg;
                }).ToList();

                var options = new ChatCompletionsOptions(_deploymentName, chatMessages)
                {
                    MaxTokens = maxTokens,
                    Temperature = temperature,
                    NucleusSamplingFactor = 0.95f,
                    FrequencyPenalty = 0,
                    PresencePenalty = 0,
                };

                var response = await _client!.GetChatCompletionsAsync(options, cancellationToken);
                var completion = response.Value.Choices[0].Message?.Content?.Trim() ?? string.Empty;

                _logger?.LogDebug("Successfully generated chat completion with {Length} characters", completion.Length);
                return completion;
            }
            catch (RequestFailedException ex)
            {
                _logger?.LogError(ex, "Azure OpenAI API request failed with status {Status}", ex.Status);
                throw new InvalidOperationException("Failed to generate chat completion", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to generate chat completion");
                throw new InvalidOperationException("Failed to generate chat completion", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<float[]> GetEmbeddingsAsync(
            string text,
            CancellationToken cancellationToken = default)
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
            if (texts.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Texts cannot contain null or whitespace entries.", nameof(texts));

            await EnsureInitializedAsync(cancellationToken);

            try
            {
                _logger?.LogDebug("Getting embeddings for {Count} texts", texts.Count());
                
                var options = new EmbeddingsOptions(_deploymentName, texts);
                var response = await _client!.GetEmbeddingsAsync(options, cancellationToken);

                var data = response.Value?.Data;
                var embeddings = data != null
                    ? data.Select(d => d.Embedding.ToArray()).ToArray()
                    : [];
                
                _logger?.LogDebug("Successfully retrieved {Count} embeddings", embeddings.Length);
                return embeddings;
            }
            catch (RequestFailedException ex)
            {
                _logger?.LogError(ex, "Azure OpenAI API request failed with status {Status}", ex.Status);
                throw new InvalidOperationException("Failed to get embeddings", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get embeddings");
                throw new InvalidOperationException("Failed to get embeddings", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        {
            return await GetEmbeddingsAsync(text, cancellationToken);
        }

        /// <inheritdoc/>
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
                var completion = await GenerateCompletionAsync(prompt, maxTokens, temperature, cancellationToken: cancellationToken);
                results.Add(completion);
            }
            return results;
        }

        /// <inheritdoc/>
        public int GetTokenCount(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;
            // Approximate token count: ~4 characters per token for English text
            return (int)Math.Ceiling(text.Length / 4.0);
        }

        /// <inheritdoc/>
        public IChatSession StartChat(string? systemMessage = null)
        {
            return new AzureChatSession(this, systemMessage);
        }

        private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
        {
            if (_client == null)
            {
                await InitializeAsync(cancellationToken);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_client is IDisposable disposable) disposable.Dispose();
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

        private sealed class AzureChatSession : IChatSession
        {
            private readonly OpenAIClient _owner;
            private readonly List<ChatMessage> _history = new();

            public AzureChatSession(OpenAIClient owner, string? systemMessage)
            {
                _owner = owner;
                if (systemMessage != null)
                    _history.Add(new ChatMessage("system", systemMessage));
            }

            public IReadOnlyList<ChatMessage> History => _history.AsReadOnly();

            public async Task<string> SendMessageAsync(string message, CancellationToken cancellationToken = default)
            {
                _history.Add(new ChatMessage("user", message));
                var response = await _owner.GenerateChatCompletionAsync(_history, cancellationToken: cancellationToken);
                _history.Add(new ChatMessage("assistant", response));
                return response;
            }

            public void Dispose()
            {
                // No unmanaged resources to release
            }
        }
    }
}
