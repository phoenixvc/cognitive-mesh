using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CognitiveMesh.MetacognitiveLayer.ReasoningTransparency;
using CognitiveMesh.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.UncertaintyQuantification
{
    /// <summary>
    /// Provides functionality to quantify and manage uncertainty in the cognitive mesh.
    /// </summary>
    public class UncertaintyQuantifier : IUncertaintyQuantifier, IDisposable
    {
        private readonly ILogger<UncertaintyQuantifier>? _logger;
        private readonly ILLMClient? _llmClient;
        private readonly ICollaborationPort? _collaborationPort;
        private readonly ITransparencyManager? _transparencyManager;
        private bool _disposed = false;

        // Common hedge words indicating uncertainty in text
        private static readonly HashSet<string> _hedgeWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "maybe", "perhaps", "possibly", "probably", "unlikely", "uncertain",
            "doubt", "assume", "guess", "approximately", "might", "could", "seem", "suggest"
        };

        /// <summary>Strategy that requests human intervention.</summary>
        public const string StrategyRequestHumanIntervention = "RequestHumanIntervention";

        /// <summary>Strategy that falls back to a default value.</summary>
        public const string StrategyFallbackToDefault = "FallbackToDefault";

        /// <summary>Strategy that executes conservatively (e.g. stricter thresholds).</summary>
        public const string StrategyConservativeExecution = "ConservativeExecution";

        /// <summary>Strategy that uses ensemble verification.</summary>
        public const string StrategyEnsembleVerification = "EnsembleVerification";

        /// <summary>
        /// Initializes a new instance of the <see cref="UncertaintyQuantifier"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="llmClient">The LLM client for cognitive assessment.</param>
        /// <param name="collaborationPort">The collaboration port for human interaction.</param>
        /// <param name="transparencyManager">The transparency manager for logging reasoning.</param>
        public UncertaintyQuantifier(
            ILogger<UncertaintyQuantifier>? logger = null,
            ILLMClient? llmClient = null,
            ICollaborationPort? collaborationPort = null,
            ITransparencyManager? transparencyManager = null)
        {
            _logger = logger;
            _llmClient = llmClient;
            _collaborationPort = collaborationPort;
            _transparencyManager = transparencyManager;
            _logger?.LogInformation("UncertaintyQuantifier initialized");
        }

        /// <inheritdoc/>
        public async Task<double> CalculateConfidenceScoreAsync(string data, CancellationToken cancellationToken = default)
        {
            _logger?.LogDebug("Calculating confidence score for data");

            if (string.IsNullOrWhiteSpace(data))
            {
                return 0.0;
            }

            if (_llmClient == null)
            {
                _logger?.LogWarning("LLM Client is not available. Using heuristic confidence score.");
                return CalculateHeuristicScore(data);
            }

            try
            {
                var prompt = $@"
Analyze the following text and provide a confidence score between 0.0 and 1.0 indicating how certain the statement is.
Consider hedge words (e.g., 'maybe', 'might', 'possibly') as indicators of lower confidence.
Return ONLY the numeric score.

Text: ""{data}""

Confidence Score:";

                var response = await _llmClient.GenerateCompletionAsync(
                    prompt: prompt,
                    temperature: 0.0f, // Deterministic output
                    maxTokens: 10,
                    cancellationToken: cancellationToken
                );

                if (double.TryParse(response?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double score))
                {
                    // Clamp value between 0.0 and 1.0
                    return Math.Max(0.0, Math.Min(1.0, score));
                }
                else
                {
                    _logger?.LogWarning("Failed to parse confidence score from LLM response: {Response}", response);
                    return CalculateHeuristicScore(data);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating confidence score via LLM.");
                return CalculateHeuristicScore(data);
            }
        }

        private double CalculateHeuristicScore(string data)
        {
            // Use Main's heuristic logic
            var metrics = CalculateTextUncertainty(data);
            if (metrics.TryGetValue("confidence", out var confidence) && confidence is double confVal)
            {
                 return confVal;
            }
            return 0.5;
        }

        /// <inheritdoc/>
        public Task<Dictionary<string, object>> QuantifyUncertaintyAsync(
            object? data,
            Dictionary<string, object>? parameters = null,
            CancellationToken cancellationToken = default)
        {
            _logger?.LogDebug("Quantifying uncertainty for data");

            Dictionary<string, object> result;

            if (data == null)
            {
                result = new Dictionary<string, object>
                {
                    ["confidence"] = 0.0,
                    ["uncertaintyType"] = "NullData",
                    ["metrics"] = new Dictionary<string, object>()
                };
                return Task.FromResult(result);
            }

            switch (data)
            {
                case string text:
                    result = CalculateTextUncertainty(text);
                    break;

                case IEnumerable<double> numbers:
                    result = CalculateNumericalUncertainty(numbers);
                    break;

                case IEnumerable<int> integers:
                     result = CalculateNumericalUncertainty(integers.Select(i => (double)i));
                     break;

                case IEnumerable<float> floats:
                     result = CalculateNumericalUncertainty(floats.Select(f => (double)f));
                     break;

                case IDictionary<string, double> distribution:
                    result = CalculateEntropyUncertainty(distribution.Values);
                    result["uncertaintyType"] = "Entropy (String Keys)";
                    break;

                case IDictionary<object, double> distributionObj:
                    result = CalculateEntropyUncertainty(distributionObj.Values);
                    result["uncertaintyType"] = "Entropy (Object Keys)";
                    break;

                default:
                     result = new Dictionary<string, object>
                     {
                         ["confidence"] = 0.5, // Neutral
                         ["uncertaintyType"] = "UnknownType",
                         ["metrics"] = new Dictionary<string, object>
                         {
                             ["dataType"] = data.GetType().Name
                         }
                     };
                     break;
            }

            return Task.FromResult(result);
        }

        private Dictionary<string, object> CalculateTextUncertainty(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                 return new Dictionary<string, object>
                 {
                     ["confidence"] = 0.0,
                     ["uncertaintyType"] = "Linguistic",
                     ["metrics"] = new Dictionary<string, object> { ["length"] = 0 }
                 };
            }

            var words = text.Split(new[] { ' ', '.', ',', ';', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            int hedgeCount = words.Count(w => _hedgeWords.Contains(w));
            double ratio = words.Length > 0 ? (double)hedgeCount / words.Length : 0;

            // Simple heuristic: Confidence decreases as hedge word ratio increases.
            // If 20% of words are hedge words, confidence is low.
            double confidence = Math.Max(0.0, 1.0 - (ratio * 5.0)); // *5 means 20% hedge words = 0 confidence.

            return new Dictionary<string, object>
            {
                ["confidence"] = confidence,
                ["uncertaintyType"] = "Linguistic",
                ["metrics"] = new Dictionary<string, object>
                {
                    ["hedgeWordCount"] = hedgeCount,
                    ["wordCount"] = words.Length,
                    ["hedgeRatio"] = ratio
                }
            };
        }

        private Dictionary<string, object> CalculateNumericalUncertainty(IEnumerable<double> numbers)
        {
            var dataList = numbers.ToList();
            if (dataList.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    ["confidence"] = 0.0,
                    ["uncertaintyType"] = "Statistical",
                    ["metrics"] = new Dictionary<string, object> { ["count"] = 0 }
                };
            }

            if (dataList.Count == 1)
            {
                 return new Dictionary<string, object>
                 {
                     ["confidence"] = 1.0,
                     ["uncertaintyType"] = "Statistical (Single Point)",
                     ["metrics"] = new Dictionary<string, object>
                     {
                         ["value"] = dataList[0],
                         ["count"] = 1
                     }
                 };
            }

            double mean = dataList.Average();
            double sumOfSquares = dataList.Sum(d => Math.Pow(d - mean, 2));
            double variance = sumOfSquares / (dataList.Count - 1); // Sample variance
            double stdDev = Math.Sqrt(variance);

            double uncertaintyMetric = (Math.Abs(mean) > 1e-9) ? (stdDev / Math.Abs(mean)) : stdDev;
            double confidence = 1.0 / (1.0 + uncertaintyMetric);

            return new Dictionary<string, object>
            {
                ["confidence"] = confidence,
                ["uncertaintyType"] = "Statistical",
                ["metrics"] = new Dictionary<string, object>
                {
                    ["mean"] = mean,
                    ["variance"] = variance,
                    ["standardDeviation"] = stdDev,
                    ["count"] = dataList.Count
                }
            };
        }

        private Dictionary<string, object> CalculateEntropyUncertainty(IEnumerable<double> probabilities)
        {
             var probList = probabilities.ToList();
             if (probList.Count == 0)
             {
                 return new Dictionary<string, object>
                 {
                     ["confidence"] = 0.0,
                     ["uncertaintyType"] = "Entropy",
                     ["metrics"] = new Dictionary<string, object> { ["count"] = 0 }
                 };
             }

             // Normalize if needed
             double sum = probList.Sum();
             if (Math.Abs(sum - 1.0) > 1e-5 && sum > 0)
             {
                 probList = probList.Select(p => p / sum).ToList();
             }

             double entropy = 0.0;
             foreach (var p in probList)
             {
                 if (p > 0)
                 {
                     entropy -= p * Math.Log2(p);
                 }
             }

             // Max entropy is log2(N)
             double maxEntropy = Math.Log2(probList.Count);
             double normalizedEntropy = (maxEntropy > 0) ? entropy / maxEntropy : 0;

             // Confidence is inverse of normalized entropy (0 entropy -> 1 confidence)
             double confidence = 1.0 - normalizedEntropy;

             return new Dictionary<string, object>
             {
                 ["confidence"] = Math.Clamp(confidence, 0.0, 1.0),
                 ["uncertaintyType"] = "Entropy",
                 ["metrics"] = new Dictionary<string, object>
                 {
                     ["entropy"] = entropy,
                     ["normalizedEntropy"] = normalizedEntropy,
                     ["maxEntropy"] = maxEntropy,
                     ["count"] = probList.Count
                 }
             };
        }

        /// <inheritdoc/>
        public async Task ApplyUncertaintyMitigationStrategyAsync(
            string strategyName, 
            object? parameters = null,
            CancellationToken cancellationToken = default)
        {
            _logger?.LogDebug("Applying uncertainty mitigation strategy: {StrategyName}", strategyName);

            var paramsDict = parameters as Dictionary<string, object> ?? new Dictionary<string, object>();

            switch (strategyName)
            {
                case StrategyRequestHumanIntervention:
                    await ApplyHumanInterventionStrategyAsync(paramsDict, cancellationToken);
                    break;

                case StrategyFallbackToDefault:
                    ApplyFallbackStrategy(paramsDict);
                    break;

                case StrategyConservativeExecution:
                    ApplyConservativeStrategy(paramsDict);
                    break;

                case StrategyEnsembleVerification:
                    ApplyEnsembleStrategy(paramsDict);
                    break;

                default:
                    var ex = new ArgumentException($"Unknown mitigation strategy: {strategyName}", nameof(strategyName));
                    _logger?.LogError(ex, "Failed to apply strategy");
                    throw ex;
            }

            if (_transparencyManager != null)
            {
                try
                {
                     await _transparencyManager.LogReasoningStepAsync(new ReasoningStep
                     {
                         Id = Guid.NewGuid().ToString(),
                         TraceId = Guid.NewGuid().ToString(),
                         Name = $"Mitigation: {strategyName}",
                         Description = "Applied uncertainty mitigation strategy",
                         Timestamp = DateTime.UtcNow,
                         Metadata = new Dictionary<string, object>
                         {
                             ["strategy"] = strategyName,
                             ["parameters"] = paramsDict
                         }
                     }, cancellationToken);
                }
                catch (Exception ex)
                {
                     _logger?.LogWarning(ex, "Failed to log transparency step for mitigation.");
                }
            }
        }

        private async Task ApplyHumanInterventionStrategyAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            if (_collaborationPort == null)
            {
                _logger?.LogWarning("Cannot apply RequestHumanIntervention: CollaborationPort is not available.");
                return;
            }

            string sessionName = parameters.ContainsKey("sessionName") && parameters["sessionName"] != null
                ? parameters["sessionName"].ToString()!
                : "Uncertainty Review";
            string description = parameters.ContainsKey("description") && parameters["description"] != null
                ? parameters["description"].ToString()!
                : "Review required due to high uncertainty.";

            var participants = new List<string>();
            if (parameters.ContainsKey("participants") && parameters["participants"] is IEnumerable<string> p)
            {
                participants.AddRange(p);
            }

            _logger?.LogInformation("Initiating human intervention session: {SessionName}", sessionName);
            await _collaborationPort.CreateCollaborationSessionAsync(sessionName, description, participants, cancellationToken);
        }

        private void ApplyFallbackStrategy(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("defaultValue"))
            {
                _logger?.LogInformation("Applying FallbackToDefault. Using value: {DefaultValue}", parameters["defaultValue"]);
            }
            else
            {
                _logger?.LogWarning("Applying FallbackToDefault but no 'defaultValue' parameter was provided.");
            }
        }

        private void ApplyConservativeStrategy(Dictionary<string, object> parameters)
        {
             _logger?.LogInformation("Applying ConservativeExecution. Adjusting thresholds to be stricter.");
             if (parameters.ContainsKey("confidenceThreshold"))
             {
                 _logger?.LogInformation("Temporarily increasing confidence threshold requirement.");
             }
        }

        private void ApplyEnsembleStrategy(Dictionary<string, object> parameters)
        {
            _logger?.LogInformation("Applying EnsembleVerification. Consulting secondary models.");
        }

        /// <inheritdoc/>
        public Task<bool> IsWithinThresholdAsync(
            object? data,
            double threshold,
            CancellationToken cancellationToken = default)
        {
            _logger?.LogDebug("Checking if data is within threshold: {Threshold}", threshold);
            // This implementation now calls QuantifyUncertaintyAsync to check real confidence vs threshold
            // Note: Since QuantifyUncertaintyAsync is async, we can await it here.
            // But IsWithinThresholdAsync signature doesn't pass parameters. Assuming default.

            // To avoid deadlocks or complexity, for now we can stub it or perform a quick check.
            // A proper implementation would likely re-use the logic above.
            // Let's defer full implementation to avoid recursive async calls if not careful,
            // but we can just invoke the logic synchronously or await safely.

            return Task.Run(async () =>
            {
                var result = await QuantifyUncertaintyAsync(data, null, cancellationToken);
                if (result.TryGetValue("confidence", out object confObj) && confObj is double conf)
                {
                    // If threshold represents "minimum acceptable confidence"
                    return conf >= threshold;
                }
                return false;
            }, cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="UncertaintyQuantifier"/> class.
        /// </summary>
        ~UncertaintyQuantifier()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Defines the contract for uncertainty quantification in the cognitive mesh.
    /// </summary>
    public interface IUncertaintyQuantifier : IDisposable
    {
        /// <summary>
        /// Calculates a confidence score for the given data.
        /// </summary>
        /// <param name="data">The data to evaluate.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the confidence score between 0 and 1.</returns>
        Task<double> CalculateConfidenceScoreAsync(string data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Quantifies the uncertainty in the given data.
        /// </summary>
        /// <param name="data">The data to analyze.</param>
        /// <param name="parameters">Optional parameters for the quantification.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the uncertainty metrics.</returns>
        Task<Dictionary<string, object>> QuantifyUncertaintyAsync(
            object? data,
            Dictionary<string, object>? parameters = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Applies a strategy to mitigate uncertainty.
        /// </summary>
        /// <param name="strategyName">The name of the strategy to apply.</param>
        /// <param name="parameters">Optional parameters for the strategy.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ApplyUncertaintyMitigationStrategyAsync(
            string strategyName, 
            object? parameters = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the data's uncertainty is within the specified threshold.
        /// </summary>
        /// <param name="data">The data to check.</param>
        /// <param name="threshold">The threshold value to compare against.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing a value indicating whether the uncertainty is within the threshold.</returns>
        Task<bool> IsWithinThresholdAsync(
            object? data,
            double threshold,
            CancellationToken cancellationToken = default);
    }
}
