using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.MetacognitiveLayer.UncertaintyQuantification
{
    /// <summary>
    /// Provides functionality to quantify and manage uncertainty in the cognitive mesh.
    /// </summary>
    public class UncertaintyQuantifier : IUncertaintyQuantifier, IDisposable
    {
        private readonly ILogger<UncertaintyQuantifier> _logger;
        private bool _disposed = false;

        // Common hedge words indicating uncertainty in text
        private static readonly HashSet<string> _hedgeWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "maybe", "perhaps", "possibly", "probably", "unlikely", "uncertain",
            "doubt", "assume", "guess", "approximately", "might", "could", "seem", "suggest"
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="UncertaintyQuantifier"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public UncertaintyQuantifier(ILogger<UncertaintyQuantifier> logger = null)
        {
            _logger = logger;
            _logger?.LogInformation("UncertaintyQuantifier initialized");
        }

        /// <inheritdoc/>
        public Task<double> CalculateConfidenceScoreAsync(string data, CancellationToken cancellationToken = default)
        {
            _logger?.LogDebug("Calculating confidence score for data");

            if (string.IsNullOrWhiteSpace(data))
            {
                return Task.FromResult(0.0);
            }

            var metrics = CalculateTextUncertainty(data);
            if (metrics.TryGetValue("confidence", out var confidence) && confidence is double confVal)
            {
                 return Task.FromResult(confVal);
            }

            return Task.FromResult(0.5); // Default neutral confidence
        }

        /// <inheritdoc/>
        public Task<Dictionary<string, object>> QuantifyUncertaintyAsync(
            object data, 
            Dictionary<string, object> parameters = null,
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

                // Handle generic dictionary if possible (IDictionary is non-generic, but values are object)
                // Or check for other Enumerable types that might be distributions

                default:
                     // Fallback check for generic IDictionary<TKey, double> using reflection or dynamic if needed,
                     // but for now, let's treat generic unknown iterables as potential collections of numbers if they are strictly numbers?
                     // No, that's dangerous.

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
                 // Single value: Cannot calculate variance. Treat as a point estimate with no internal uncertainty info.
                 return new Dictionary<string, object>
                 {
                     ["confidence"] = 1.0, // Or unknown? Assuming 1.0 if explicit single value provided without context.
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

            // Heuristic for confidence based on Coefficient of Variation (CV = StdDev / |Mean|)
            // If Mean is 0, use StdDev directly.
            double uncertaintyMetric = (Math.Abs(mean) > 1e-9) ? (stdDev / Math.Abs(mean)) : stdDev;

            // Map uncertainty (0 -> infinity) to confidence (1 -> 0)
            // Using 1 / (1 + x)
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
        public Task ApplyUncertaintyMitigationStrategyAsync(
            string strategyName, 
            object parameters = null,
            CancellationToken cancellationToken = default)
        {
            _logger?.LogDebug("Applying uncertainty mitigation strategy: {StrategyName}", strategyName);
            // TODO: Implement actual mitigation strategy application
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<bool> IsWithinThresholdAsync(
            object data, 
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
            object data, 
            Dictionary<string, object> parameters = null,
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
            object parameters = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the data's uncertainty is within the specified threshold.
        /// </summary>
        /// <param name="data">The data to check.</param>
        /// <param name="threshold">The threshold value to compare against.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing a value indicating whether the uncertainty is within the threshold.</returns>
        Task<bool> IsWithinThresholdAsync(
            object data, 
            double threshold,
            CancellationToken cancellationToken = default);
    }
}
