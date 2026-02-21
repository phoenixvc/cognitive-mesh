using CognitiveMesh.ReasoningLayer.LLMReasoning.Abstractions;

namespace CognitiveMesh.ReasoningLayer.LLMReasoning
{
    /// <summary>
    /// Factory class for creating ILLMClient instances.
    /// Supports multi-provider routing: Azure OpenAI, OpenAI, MiniMax, DeepSeek,
    /// Google, Anthropic, xAI, Meta (via Ollama), and any OpenAI-compatible endpoint.
    /// </summary>
    public static class LLMClientFactory
    {
        /// <summary>
        /// Creates a new instance of ILLMClient using Azure OpenAI
        /// </summary>
        /// <param name="apiKey">The Azure OpenAI API key</param>
        /// <param name="endpoint">The Azure OpenAI endpoint</param>
        /// <param name="deploymentName">The deployment name to use</param>
        /// <param name="modelName">Optional model name (default: "gpt-4")</param>
        /// <param name="maxTokens">Optional maximum tokens (default: 8192)</param>
        /// <param name="logger">Optional logger instance</param>
        /// <returns>An initialized ILLMClient instance</returns>
        public static async Task<ILLMClient> CreateAzureOpenAIClientAsync(
            string apiKey,
            string endpoint,
            string deploymentName,
            string modelName = "gpt-4",
            int maxTokens = 8192,
            ILogger? logger = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key is required", nameof(apiKey));
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Endpoint is required", nameof(endpoint));
            if (string.IsNullOrWhiteSpace(deploymentName))
                throw new ArgumentException("Deployment name is required", nameof(deploymentName));

            var client = new Implementations.OpenAIClient(
                apiKey,
                endpoint,
                deploymentName,
                modelName,
                maxTokens,
                logger as ILogger<Implementations.OpenAIClient>);

            await client.InitializeAsync();
            return client;
        }

        /// <summary>
        /// Creates a MiniMax client for MiniMax-M1 or MiniMax-M2.5 models.
        /// </summary>
        /// <param name="apiKey">MiniMax API key.</param>
        /// <param name="modelName">Model name (default: "MiniMax-M2.5").</param>
        /// <param name="maxTokens">Maximum output tokens (default: 32000).</param>
        /// <param name="endpoint">API endpoint (default: https://api.minimax.chat/v1).</param>
        /// <param name="logger">Optional logger instance.</param>
        /// <returns>An initialized ILLMClient instance.</returns>
        public static async Task<ILLMClient> CreateMiniMaxClientAsync(
            string apiKey,
            string modelName = "MiniMax-M2.5",
            int maxTokens = 32_000,
            string endpoint = "https://api.minimax.chat/v1",
            ILogger? logger = null)
        {
            var client = new Implementations.MiniMaxClient(
                apiKey,
                modelName,
                maxTokens,
                endpoint,
                logger as ILogger<Implementations.MiniMaxClient>);

            await client.InitializeAsync();
            return client;
        }

        /// <summary>
        /// Creates a client for any OpenAI-compatible API endpoint (OpenAI, DeepSeek,
        /// Grok/xAI, Llama via Ollama, Anthropic via proxy, etc.).
        /// </summary>
        /// <param name="apiKey">API key (empty for local models).</param>
        /// <param name="endpoint">The API base URL.</param>
        /// <param name="modelName">The model identifier.</param>
        /// <param name="maxTokens">Maximum output tokens.</param>
        /// <param name="logger">Optional logger instance.</param>
        /// <returns>An initialized ILLMClient instance.</returns>
        public static async Task<ILLMClient> CreateOpenAICompatibleClientAsync(
            string apiKey,
            string endpoint,
            string modelName,
            int maxTokens = 16_384,
            ILogger? logger = null)
        {
            var client = new Implementations.OpenAICompatibleClient(
                apiKey,
                endpoint,
                modelName,
                maxTokens,
                logger as ILogger<Implementations.OpenAICompatibleClient>);

            await client.InitializeAsync();
            return client;
        }

        /// <summary>
        /// Creates an ILLMClient by resolving the provider from the model key
        /// in the <see cref="LLMModelRegistry"/>. Supports all registered providers.
        /// </summary>
        /// <param name="modelKey">The model key from the registry (e.g., "claude-sonnet-4", "minimax-m2.5").</param>
        /// <param name="apiKey">The provider API key.</param>
        /// <param name="endpoint">Optional custom endpoint override.</param>
        /// <param name="deploymentName">Azure OpenAI deployment name (only for azure-openai provider).</param>
        /// <param name="logger">Optional logger instance.</param>
        /// <returns>An initialized ILLMClient instance.</returns>
        public static async Task<ILLMClient> CreateForModelAsync(
            string modelKey,
            string apiKey,
            string? endpoint = null,
            string? deploymentName = null,
            ILogger? logger = null)
        {
            var model = LLMModelRegistry.GetAllModels()
                .FirstOrDefault(m => m.Key.Equals(modelKey, StringComparison.OrdinalIgnoreCase));

            if (model == null)
                throw new ArgumentException($"Unknown model key '{modelKey}'. Check LLMModelRegistry.", nameof(modelKey));

            var resolvedEndpoint = endpoint ?? model.DefaultEndpoint;

            return model.Provider.ToLower() switch
            {
                "azure-openai" => await CreateAzureOpenAIClientAsync(
                    apiKey,
                    resolvedEndpoint,
                    deploymentName ?? modelKey,
                    modelKey,
                    model.MaxOutputTokens,
                    logger),

                "minimax" => await CreateMiniMaxClientAsync(
                    apiKey,
                    model.DisplayName,
                    model.MaxOutputTokens,
                    resolvedEndpoint,
                    logger),

                // All other providers use the OpenAI-compatible client
                // (openai, google, deepseek, meta, xai, anthropic-via-proxy)
                _ => await CreateOpenAICompatibleClientAsync(
                    apiKey,
                    resolvedEndpoint,
                    modelKey,
                    model.MaxOutputTokens,
                    logger)
            };
        }

        /// <summary>
        /// Creates a new instance of ILLMClient using Azure OpenAI from configuration
        /// </summary>
        /// <param name="config">Configuration dictionary containing required parameters</param>
        /// <param name="logger">Optional logger instance</param>
        /// <returns>An initialized ILLMClient instance</returns>
        public static async Task<ILLMClient> CreateFromConfigAsync(
            IReadOnlyDictionary<string, string> config,
            ILogger? logger = null)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            // Check for new multi-provider config format
            if (config.TryGetValue("ModelKey", out var modelKey) && !string.IsNullOrWhiteSpace(modelKey))
            {
                if (!config.TryGetValue("ApiKey", out var key) || string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("ApiKey is required in configuration");

                config.TryGetValue("Endpoint", out var ep);
                config.TryGetValue("DeploymentName", out var dn);

                return await CreateForModelAsync(modelKey, key, ep, dn, logger);
            }

            // Legacy Azure OpenAI format
            if (!config.TryGetValue("ApiKey", out var apiKey) || string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("ApiKey is required in configuration");

            if (!config.TryGetValue("Endpoint", out var endpoint) || string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Endpoint is required in configuration");

            if (!config.TryGetValue("DeploymentName", out var deploymentName) || string.IsNullOrWhiteSpace(deploymentName))
                throw new ArgumentException("DeploymentName is required in configuration");

            config.TryGetValue("ModelName", out var modelName);
            config.TryGetValue("MaxTokens", out var maxTokensStr);

            int maxTokens = 8192;
            if (!string.IsNullOrWhiteSpace(maxTokensStr) && !int.TryParse(maxTokensStr, out maxTokens))
            {
                logger?.LogWarning("Invalid MaxTokens value '{MaxTokens}', using default {DefaultMaxTokens}",
                    maxTokensStr, maxTokens);
                maxTokens = 8192;
            }

            return await CreateAzureOpenAIClientAsync(
                apiKey,
                endpoint,
                deploymentName,
                modelName ?? "gpt-4",
                maxTokens,
                logger);
        }
    }
}
