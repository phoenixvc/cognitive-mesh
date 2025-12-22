using System;
using System.Threading;
using System.Threading.Tasks;
using CognitiveMesh.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.ReasoningLayer.LLMReasoning
{
    /// <summary>
    /// Factory class for creating ILLMClient instances
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
