using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.SelfEvaluation
{
    /// <summary>
    /// Provides self-evaluation capabilities for the cognitive mesh.
    /// Analyzes performance metrics, learning progress, insights, and behavioral validity.
    /// </summary>
    public class SelfEvaluator : ISelfEvaluator, IDisposable
    {
        private readonly ILogger<SelfEvaluator>? _logger;
        private bool _disposed = false;

        /// <summary>
        /// Well-known metric keys used in performance evaluation.
        /// </summary>
        private static class MetricKeys
        {
            public const string Latency = "latency";
            public const string Throughput = "throughput";
            public const string ErrorRate = "errorRate";
            public const string SuccessRate = "successRate";
            public const string MemoryUsage = "memoryUsage";
            public const string CpuUsage = "cpuUsage";
            public const string Accuracy = "accuracy";
            public const string CompletionRate = "completionRate";
            public const string ConfidenceScore = "confidenceScore";
            public const string IterationCount = "iterationCount";
            public const string TotalIterations = "totalIterations";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfEvaluator"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public SelfEvaluator(ILogger<SelfEvaluator>? logger = null)
        {
            _logger = logger;
            _logger?.LogInformation("SelfEvaluator initialized");
        }

        /// <inheritdoc/>
        public Task<Dictionary<string, object>> EvaluatePerformanceAsync(
            string componentName,
            Dictionary<string, object> metrics,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger?.LogDebug("Evaluating performance for component: {ComponentName}", componentName);

            if (string.IsNullOrWhiteSpace(componentName))
            {
                throw new ArgumentException("Component name cannot be null or whitespace.", nameof(componentName));
            }

            if (metrics == null || metrics.Count == 0)
            {
                _logger?.LogWarning("No metrics provided for component {ComponentName}; returning baseline evaluation", componentName);
                return Task.FromResult(new Dictionary<string, object>
                {
                    ["score"] = 0.0,
                    ["assessment"] = "no_data",
                    ["recommendations"] = new[] { "Provide performance metrics for meaningful evaluation." }
                });
            }

            var recommendations = new List<string>();
            var dimensionScores = new List<double>();

            // Evaluate latency (lower is better, target < 200ms)
            if (TryGetDouble(metrics, MetricKeys.Latency, out var latency))
            {
                var latencyScore = latency <= 0 ? 1.0 : Math.Max(0.0, 1.0 - (latency / 1000.0));
                dimensionScores.Add(latencyScore);
                if (latencyScore < 0.5)
                {
                    recommendations.Add($"High latency detected ({latency:F0}ms). Consider caching, reducing payload, or optimizing processing pipeline.");
                }
            }

            // Evaluate error rate (lower is better, target 0%)
            if (TryGetDouble(metrics, MetricKeys.ErrorRate, out var errorRate))
            {
                var errorScore = Math.Max(0.0, 1.0 - errorRate);
                dimensionScores.Add(errorScore);
                if (errorRate > 0.05)
                {
                    recommendations.Add($"Error rate is {errorRate:P1}. Investigate error sources and consider adding retry logic or circuit breakers.");
                }
            }

            // Evaluate success rate (higher is better)
            if (TryGetDouble(metrics, MetricKeys.SuccessRate, out var successRate))
            {
                dimensionScores.Add(Math.Clamp(successRate, 0.0, 1.0));
                if (successRate < 0.95)
                {
                    recommendations.Add($"Success rate is {successRate:P1}. Review failure cases and improve handling.");
                }
            }

            // Evaluate throughput (normalized against a baseline; higher is better)
            if (TryGetDouble(metrics, MetricKeys.Throughput, out var throughput))
            {
                var throughputScore = throughput <= 0 ? 0.0 : Math.Min(1.0, throughput / 100.0);
                dimensionScores.Add(throughputScore);
                if (throughputScore < 0.5)
                {
                    recommendations.Add($"Throughput is low ({throughput:F1} ops/s). Consider parallelism or resource scaling.");
                }
            }

            // Evaluate memory usage (lower percentage is better)
            if (TryGetDouble(metrics, MetricKeys.MemoryUsage, out var memoryUsage))
            {
                var memoryScore = Math.Max(0.0, 1.0 - memoryUsage);
                dimensionScores.Add(memoryScore);
                if (memoryUsage > 0.8)
                {
                    recommendations.Add($"Memory usage is {memoryUsage:P0}. Investigate memory leaks or increase available memory.");
                }
            }

            // Evaluate CPU usage (lower percentage is better)
            if (TryGetDouble(metrics, MetricKeys.CpuUsage, out var cpuUsage))
            {
                var cpuScore = Math.Max(0.0, 1.0 - cpuUsage);
                dimensionScores.Add(cpuScore);
                if (cpuUsage > 0.85)
                {
                    recommendations.Add($"CPU usage is {cpuUsage:P0}. Profile hotspots and optimize computation-heavy paths.");
                }
            }

            // Evaluate accuracy (higher is better)
            if (TryGetDouble(metrics, MetricKeys.Accuracy, out var accuracy))
            {
                dimensionScores.Add(Math.Clamp(accuracy, 0.0, 1.0));
                if (accuracy < 0.9)
                {
                    recommendations.Add($"Accuracy is {accuracy:P1}. Review model or logic for systematic errors.");
                }
            }

            // Calculate composite score
            double compositeScore = dimensionScores.Count > 0
                ? dimensionScores.Average()
                : 0.5; // Neutral score when no recognized metrics

            var assessment = compositeScore switch
            {
                >= 0.9 => "optimal",
                >= 0.75 => "good",
                >= 0.5 => "acceptable",
                >= 0.25 => "degraded",
                _ => "critical"
            };

            if (recommendations.Count == 0 && compositeScore >= 0.9)
            {
                recommendations.Add("Performance is within optimal parameters. Continue monitoring.");
            }

            _logger?.LogInformation(
                "Performance evaluation for {ComponentName}: score={Score:F2}, assessment={Assessment}, metricsEvaluated={Count}",
                componentName, compositeScore, assessment, dimensionScores.Count);

            return Task.FromResult(new Dictionary<string, object>
            {
                ["score"] = compositeScore,
                ["assessment"] = assessment,
                ["recommendations"] = recommendations.ToArray(),
                ["evaluatedMetrics"] = dimensionScores.Count,
                ["componentName"] = componentName
            });
        }

        /// <inheritdoc/>
        public Task<Dictionary<string, object>> AssessLearningProgressAsync(
            string learningTaskId,
            Dictionary<string, object> metrics,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger?.LogDebug("Assessing learning progress for task: {TaskId}", learningTaskId);

            if (string.IsNullOrWhiteSpace(learningTaskId))
            {
                throw new ArgumentException("Learning task ID cannot be null or whitespace.", nameof(learningTaskId));
            }

            if (metrics == null || metrics.Count == 0)
            {
                _logger?.LogWarning("No metrics provided for learning task {TaskId}; returning baseline assessment", learningTaskId);
                return Task.FromResult(new Dictionary<string, object>
                {
                    ["progress"] = 0.0,
                    ["confidence"] = 0.0,
                    ["nextSteps"] = new[] { "Provide learning metrics to enable progress assessment." }
                });
            }

            var nextSteps = new List<string>();

            // Calculate progress from completion rate or iteration-based progress
            double progress = 0.0;
            if (TryGetDouble(metrics, MetricKeys.CompletionRate, out var completionRate))
            {
                progress = Math.Clamp(completionRate, 0.0, 1.0);
            }
            else if (TryGetDouble(metrics, MetricKeys.IterationCount, out var iterationCount)
                     && TryGetDouble(metrics, MetricKeys.TotalIterations, out var totalIterations)
                     && totalIterations > 0)
            {
                progress = Math.Clamp(iterationCount / totalIterations, 0.0, 1.0);
            }

            // Calculate confidence from accuracy and success rate
            var confidenceFactors = new List<double>();
            if (TryGetDouble(metrics, MetricKeys.Accuracy, out var accuracy))
            {
                confidenceFactors.Add(Math.Clamp(accuracy, 0.0, 1.0));
            }
            if (TryGetDouble(metrics, MetricKeys.SuccessRate, out var successRate))
            {
                confidenceFactors.Add(Math.Clamp(successRate, 0.0, 1.0));
            }
            if (TryGetDouble(metrics, MetricKeys.ConfidenceScore, out var confidenceScore))
            {
                confidenceFactors.Add(Math.Clamp(confidenceScore, 0.0, 1.0));
            }

            double confidence = confidenceFactors.Count > 0
                ? confidenceFactors.Average()
                : progress * 0.5; // Heuristic: 50% of progress as confidence when no explicit signals

            // Generate actionable next steps based on the assessment
            if (progress < 0.25)
            {
                nextSteps.Add("Learning task is in early stages. Focus on gathering diverse training examples.");
            }
            else if (progress < 0.5)
            {
                nextSteps.Add("Learning task is progressing. Begin evaluating model quality on a validation set.");
            }
            else if (progress < 0.75)
            {
                nextSteps.Add("Learning task is past halfway. Focus on edge cases and error analysis.");
            }
            else if (progress < 1.0)
            {
                nextSteps.Add("Learning task is near completion. Run comprehensive evaluation and prepare for deployment.");
            }
            else
            {
                nextSteps.Add("Learning task is complete. Monitor deployed performance and schedule retraining if drift is detected.");
            }

            if (confidence < 0.5)
            {
                nextSteps.Add("Confidence is low. Consider increasing training data volume or improving data quality.");
            }

            if (TryGetDouble(metrics, MetricKeys.ErrorRate, out var errorRate) && errorRate > 0.1)
            {
                nextSteps.Add($"Error rate is {errorRate:P1}. Investigate common failure patterns and add targeted training examples.");
            }

            _logger?.LogInformation(
                "Learning progress assessment for {TaskId}: progress={Progress:F2}, confidence={Confidence:F2}",
                learningTaskId, progress, confidence);

            return Task.FromResult(new Dictionary<string, object>
            {
                ["progress"] = progress,
                ["confidence"] = confidence,
                ["nextSteps"] = nextSteps.ToArray(),
                ["learningTaskId"] = learningTaskId
            });
        }

        /// <inheritdoc/>
        public Task<Dictionary<string, object>> GenerateInsightsAsync(
            string context,
            Dictionary<string, object> data,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger?.LogDebug("Generating insights for context: {Context}", context);

            if (string.IsNullOrWhiteSpace(context))
            {
                throw new ArgumentException("Context cannot be null or whitespace.", nameof(context));
            }

            if (data == null || data.Count == 0)
            {
                _logger?.LogWarning("No data provided for insight generation in context {Context}", context);
                return Task.FromResult(new Dictionary<string, object>
                {
                    ["keyInsights"] = new[] { "Insufficient data to generate insights." },
                    ["patterns"] = Array.Empty<object>(),
                    ["recommendations"] = new[] { "Provide data points for meaningful insight generation." }
                });
            }

            var keyInsights = new List<string>();
            var patterns = new List<object>();
            var recommendations = new List<string>();

            // Analyze numeric data to detect patterns
            var numericEntries = new Dictionary<string, double>();
            foreach (var kvp in data)
            {
                if (TryConvertToDouble(kvp.Value, out var numValue))
                {
                    numericEntries[kvp.Key] = numValue;
                }
            }

            if (numericEntries.Count > 0)
            {
                var average = numericEntries.Values.Average();
                var min = numericEntries.MinBy(kv => kv.Value);
                var max = numericEntries.MaxBy(kv => kv.Value);

                keyInsights.Add($"Across {numericEntries.Count} numeric metrics, average value is {average:F2}.");

                if (numericEntries.Count > 1)
                {
                    keyInsights.Add($"Highest metric: '{max.Key}' ({max.Value:F2}). Lowest metric: '{min.Key}' ({min.Value:F2}).");

                    // Detect spread / variance
                    var range = max.Value - min.Value;
                    if (range > average * 0.5 && average > 0)
                    {
                        patterns.Add(new Dictionary<string, object>
                        {
                            ["type"] = "high_variance",
                            ["description"] = $"High spread detected: range {range:F2} relative to mean {average:F2}.",
                            ["affectedMetrics"] = new[] { min.Key, max.Key }
                        });
                        recommendations.Add($"Investigate the variance between '{min.Key}' and '{max.Key}' to find root causes.");
                    }

                    // Detect outliers using simple z-score-like approach
                    if (numericEntries.Count >= 3)
                    {
                        var mean = numericEntries.Values.Average();
                        var stdDev = Math.Sqrt(numericEntries.Values.Select(v => Math.Pow(v - mean, 2)).Average());
                        if (stdDev > 0)
                        {
                            var outliers = numericEntries
                                .Where(kv => Math.Abs(kv.Value - mean) > 2 * stdDev)
                                .Select(kv => kv.Key)
                                .ToList();

                            if (outliers.Count > 0)
                            {
                                patterns.Add(new Dictionary<string, object>
                                {
                                    ["type"] = "outlier",
                                    ["description"] = $"Outlier(s) detected beyond 2 standard deviations from mean.",
                                    ["affectedMetrics"] = outliers.ToArray()
                                });
                                recommendations.Add($"Review outlier metric(s): {string.Join(", ", outliers)}.");
                            }
                        }
                    }
                }
            }

            // Analyze non-numeric data for categorical patterns
            var nonNumericEntries = data
                .Where(kvp => !numericEntries.ContainsKey(kvp.Key) && kvp.Value != null)
                .ToList();

            if (nonNumericEntries.Count > 0)
            {
                keyInsights.Add($"Data contains {nonNumericEntries.Count} non-numeric entries in context '{context}'.");
            }

            if (keyInsights.Count == 0)
            {
                keyInsights.Add($"Data analyzed for context '{context}' with {data.Count} entries. No significant patterns found.");
            }

            if (recommendations.Count == 0)
            {
                recommendations.Add("Continue collecting data to enable trend detection over time.");
            }

            _logger?.LogInformation(
                "Generated {InsightCount} insights and {PatternCount} patterns for context {Context}",
                keyInsights.Count, patterns.Count, context);

            return Task.FromResult(new Dictionary<string, object>
            {
                ["keyInsights"] = keyInsights.ToArray(),
                ["patterns"] = patterns.ToArray(),
                ["recommendations"] = recommendations.ToArray(),
                ["context"] = context,
                ["dataPointsAnalyzed"] = data.Count
            });
        }

        /// <inheritdoc/>
        public Task<bool> ValidateBehaviorAsync(
            string behaviorName,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger?.LogDebug("Validating behavior: {BehaviorName}", behaviorName);

            if (string.IsNullOrWhiteSpace(behaviorName))
            {
                throw new ArgumentException("Behavior name cannot be null or whitespace.", nameof(behaviorName));
            }

            if (parameters == null)
            {
                _logger?.LogWarning("No parameters provided for behavior validation of {BehaviorName}", behaviorName);
                return Task.FromResult(false);
            }

            // Validate that required parameter keys exist and have non-null values
            bool hasRequiredKeys = parameters.Count > 0;
            bool allValuesPopulated = parameters.Values.All(v => v != null);

            // Check for known risky patterns in parameter values
            bool hasSafeValues = true;
            foreach (var kvp in parameters)
            {
                if (kvp.Value is string strValue)
                {
                    // Reject empty strings for string-typed parameters
                    if (string.IsNullOrWhiteSpace(strValue))
                    {
                        _logger?.LogWarning(
                            "Behavior '{BehaviorName}' has empty string parameter '{Key}'",
                            behaviorName, kvp.Key);
                        hasSafeValues = false;
                    }
                }

                if (kvp.Value is double numValue)
                {
                    // Reject NaN or Infinity
                    if (double.IsNaN(numValue) || double.IsInfinity(numValue))
                    {
                        _logger?.LogWarning(
                            "Behavior '{BehaviorName}' has invalid numeric parameter '{Key}': {Value}",
                            behaviorName, kvp.Key, numValue);
                        hasSafeValues = false;
                    }
                }
            }

            var isValid = hasRequiredKeys && allValuesPopulated && hasSafeValues;

            _logger?.LogInformation(
                "Behavior validation for '{BehaviorName}': valid={IsValid}, paramCount={ParamCount}",
                behaviorName, isValid, parameters.Count);

            return Task.FromResult(isValid);
        }

        /// <summary>
        /// Attempts to extract a double value from a dictionary by key.
        /// </summary>
        private static bool TryGetDouble(Dictionary<string, object> dict, string key, out double value)
        {
            value = 0.0;
            if (!dict.TryGetValue(key, out var raw))
                return false;
            return TryConvertToDouble(raw, out value);
        }

        /// <summary>
        /// Attempts to convert an object to a double value.
        /// </summary>
        private static bool TryConvertToDouble(object? obj, out double value)
        {
            value = 0.0;
            if (obj == null) return false;

            if (obj is double d) { value = d; return true; }
            if (obj is float f) { value = f; return true; }
            if (obj is int i) { value = i; return true; }
            if (obj is long l) { value = l; return true; }
            if (obj is decimal dec) { value = (double)dec; return true; }

            return obj is string s && double.TryParse(s, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out value);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="SelfEvaluator"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
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
        /// Finalizer for <see cref="SelfEvaluator"/>.
        /// </summary>
        ~SelfEvaluator()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Defines the contract for self-evaluation in the cognitive mesh.
    /// </summary>
    public interface ISelfEvaluator : IDisposable
    {
        /// <summary>
        /// Evaluates the performance of a component.
        /// </summary>
        /// <param name="componentName">The name of the component to evaluate.</param>
        /// <param name="metrics">The metrics to use for evaluation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the evaluation results.</returns>
        Task<Dictionary<string, object>> EvaluatePerformanceAsync(
            string componentName,
            Dictionary<string, object> metrics,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Assesses the progress of a learning task.
        /// </summary>
        /// <param name="learningTaskId">The ID of the learning task.</param>
        /// <param name="metrics">The metrics to use for assessment.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the assessment results.</returns>
        Task<Dictionary<string, object>> AssessLearningProgressAsync(
            string learningTaskId,
            Dictionary<string, object> metrics,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates insights based on the provided data and context.
        /// </summary>
        /// <param name="context">The context for insight generation.</param>
        /// <param name="data">The data to analyze.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the generated insights.</returns>
        Task<Dictionary<string, object>> GenerateInsightsAsync(
            string context,
            Dictionary<string, object> data,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates a behavior against defined criteria.
        /// </summary>
        /// <param name="behaviorName">The name of the behavior to validate.</param>
        /// <param name="parameters">The parameters for the behavior.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing a value indicating whether the behavior is valid.</returns>
        Task<bool> ValidateBehaviorAsync(
            string behaviorName,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default);
    }
}
