using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using CognitiveMesh.Shared.Interfaces;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<OpenAIClient> _logger;
        private AzureOpenAIClient _client;
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
            ILogger<OpenAIClient> logger = null)
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
        }

        /// <inheritdoc/>
        public async Task<string> GenerateCompletionAsync(
            string prompt,
            float temperature = 0.7f,
            int maxTokens = 1000,
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

                var response = await _client.GetCompletionsAsync(options, cancellationToken);
                var completion = response.Value.Choices[0].Text.Trim();
                
                _logger?.LogDebug("Successfully generated completion with {Length} characters", completion?.Length ?? 0);
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
                
                var chatMessages = messages.Select(m => new ChatRequestMessage(
                    m.Role.ToLower() switch
                    {
                        "system" => ChatRole.System,
                        "assistant" => ChatRole.Assistant,
                        _ => ChatRole.User
                    },
                    m.Content
                )).ToList();

                var options = new ChatCompletionsOptions
                {
                    DeploymentName = _deploymentName,
                    Messages = { chatMessages },
                    MaxTokens = maxTokens,
                    Temperature = temperature,
                    NucleusSamplingFactor = 0.95f,
                    FrequencyPenalty = 0,
                    PresencePenalty = 0,
                };

                var response = await _client.GetChatCompletionsAsync(options, cancellationToken);
                var completion = response.Value.Choices[0].Message.Content.Trim();
                
                _logger?.LogDebug("Successfully generated chat completion with {Length} characters", completion?.Length ?? 0);
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
            return results.FirstOrDefault();
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
                var response = await _client.GetEmbeddingsAsync(options, cancellationToken);
                
                var embeddings = response.Value.Data
                    .Select(d => d.Embedding.ToArray())
                    .ToArray();
                
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
                    _client?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
