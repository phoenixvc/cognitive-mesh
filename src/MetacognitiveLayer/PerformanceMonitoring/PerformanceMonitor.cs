namespace MetacognitiveLayer.PerformanceMonitoring
{
    /// <summary>
    /// Tracks performance metrics across the mesh and triggers alerts on anomalies.
    /// </summary>
    public class PerformanceMonitor : IDisposable
    {
        private readonly IMetricsStore _metricsStore;
        private readonly Dictionary<string, MetricAggregator> _aggregators;
        private readonly Dictionary<string, MetricThreshold> _thresholds;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceMonitor"/> class.
        /// </summary>
        /// <param name="metricsStore">The metrics store for persisting metric data.</param>
        public PerformanceMonitor(IMetricsStore metricsStore)
        {
            _metricsStore = metricsStore ?? throw new ArgumentNullException(nameof(metricsStore));
            _aggregators = new Dictionary<string, MetricAggregator>();
            _thresholds = new Dictionary<string, MetricThreshold>();
        }

        /// <summary>
        /// Registers a threshold for a metric. When the metric violates this threshold,
        /// <see cref="CheckThresholdsAsync"/> will report a violation.
        /// </summary>
        /// <param name="metricName">The name of the metric to monitor.</param>
        /// <param name="threshold">The threshold configuration.</param>
        public void RegisterThreshold(string metricName, MetricThreshold threshold)
        {
            if (string.IsNullOrWhiteSpace(metricName))
                throw new ArgumentException("Metric name cannot be null or whitespace.", nameof(metricName));
            if (threshold == null)
                throw new ArgumentNullException(nameof(threshold));

            _thresholds[metricName] = threshold;
        }

        /// <summary>
        /// Removes a previously registered threshold.
        /// </summary>
        /// <param name="metricName">The name of the metric whose threshold should be removed.</param>
        /// <returns>True if the threshold was removed; false if it was not found.</returns>
        public bool RemoveThreshold(string metricName)
        {
            if (string.IsNullOrWhiteSpace(metricName))
                return false;

            return _thresholds.Remove(metricName);
        }

        /// <summary>
        /// Records a metric value with the given name and optional tags.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="value">The value to record.</param>
        /// <param name="tags">Optional tags to associate with the metric.</param>
        public void RecordMetric(string name, double value, IDictionary<string, string>? tags = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Metric name cannot be null or whitespace.", nameof(name));
                
            var timestamp = DateTime.UtcNow;
            var metric = new Metric
            {
                Name = name,
                Value = value,
                Timestamp = timestamp,
                Tags = tags ?? new Dictionary<string, string>()
            };

            // Store the raw metric
            _metricsStore.StoreMetric(metric);

            // Update aggregator for this metric
            if (!_aggregators.TryGetValue(name, out var aggregator))
            {
                aggregator = new MetricAggregator(name);
                _aggregators[name] = aggregator;
            }
            aggregator.AddValue(value, timestamp);
        }

        /// <summary>
        /// Queries metrics that match the given criteria.
        /// </summary>
        /// <param name="name">The name of the metric to query.</param>
        /// <param name="since">The start time for the query.</param>
        /// <param name="until">The end time for the query. If null, uses current time.</param>
        /// <param name="tags">Optional tags to filter by.</param>
        /// <returns>A collection of matching metrics.</returns>
        public async Task<IEnumerable<Metric>> QueryMetricsAsync(
            string name, 
            DateTime since, 
            DateTime? until = null,
            IDictionary<string, string>? tags = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Metric name cannot be null or whitespace.", nameof(name));
                
            if (since > (until ?? DateTime.UtcNow))
                throw new ArgumentException("Start time must be before end time.", nameof(since));

            try
            {
                return await _metricsStore.QueryMetricsAsync(name, since, until, tags);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to query metrics for '{name}'", ex);
            }
        }

        /// <summary>
        /// Gets aggregated statistics for a metric over a time window.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="window">The time window to aggregate over.</param>
        /// <returns>Aggregated statistics, or null if no data is available.</returns>
        public MetricStatistics GetAggregatedStats(string name, TimeSpan window)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Metric name cannot be null or whitespace.", nameof(name));
                
            if (window <= TimeSpan.Zero)
                throw new ArgumentException("Window must be a positive time span.", nameof(window));

            if (_aggregators.TryGetValue(name, out var aggregator))
            {
                return aggregator.GetStats(window);
            }

            return null!;
        }

        /// <summary>
        /// Checks if any metrics have exceeded their defined thresholds.
        /// Compares each registered threshold against the current aggregated metric values
        /// and returns any violations found.
        /// </summary>
        /// <returns>A collection of threshold violations, if any.</returns>
        public Task<IEnumerable<ThresholdViolation>> CheckThresholdsAsync()
        {
            if (_thresholds.Count == 0)
            {
                return Task.FromResult<IEnumerable<ThresholdViolation>>(Array.Empty<ThresholdViolation>());
            }

            var violations = new List<ThresholdViolation>();
            var now = DateTime.UtcNow;

            foreach (var (metricName, threshold) in _thresholds)
            {
                if (!_aggregators.TryGetValue(metricName, out var aggregator))
                {
                    continue;
                }

                var window = threshold.EvaluationWindow ?? TimeSpan.FromMinutes(5);
                var stats = aggregator.GetStats(window);
                if (stats == null || stats.Count == 0)
                {
                    continue;
                }

                // Determine which aggregated value to evaluate based on the threshold's aggregation mode
                double currentValue = threshold.AggregationMode switch
                {
                    ThresholdAggregation.Average => stats.Average,
                    ThresholdAggregation.Min => stats.Min,
                    ThresholdAggregation.Max => stats.Max,
                    ThresholdAggregation.Sum => stats.Sum,
                    _ => stats.Average
                };

                bool violated = threshold.Condition switch
                {
                    ThresholdCondition.GreaterThan => currentValue > threshold.Value,
                    ThresholdCondition.GreaterThanOrEqual => currentValue >= threshold.Value,
                    ThresholdCondition.LessThan => currentValue < threshold.Value,
                    ThresholdCondition.LessThanOrEqual => currentValue <= threshold.Value,
                    ThresholdCondition.Equal => Math.Abs(currentValue - threshold.Value) < 1e-9,
                    ThresholdCondition.NotEqual => Math.Abs(currentValue - threshold.Value) >= 1e-9,
                    _ => false
                };

                if (violated)
                {
                    violations.Add(new ThresholdViolation
                    {
                        MetricName = metricName,
                        Value = currentValue,
                        Threshold = threshold.Value,
                        Condition = threshold.Condition.ToString(),
                        Timestamp = now
                    });
                }
            }

            return Task.FromResult<IEnumerable<ThresholdViolation>>(violations);
        }

        /// <summary>
        /// Releases managed and unmanaged resources used by the <see cref="PerformanceMonitor"/>.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Flush any pending metrics
                    foreach (var aggregator in _aggregators.Values)
                    {
                        aggregator.Flush();
                    }
                    
                    // Dispose the metrics store if needed
                    if (_metricsStore is IDisposable disposableStore)
                    {
                        disposableStore.Dispose();
                    }
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
    }

    /// <summary>
    /// Represents a single metric data point.
    /// </summary>
    public class Metric
    {
        /// <summary>Gets or sets the name of the metric.</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>Gets or sets the recorded value.</summary>
        public double Value { get; set; }
        /// <summary>Gets or sets the timestamp of the measurement.</summary>
        public DateTime Timestamp { get; set; }
        /// <summary>Gets or sets the tags associated with this metric.</summary>
        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Represents aggregated statistics for a metric.
    /// </summary>
    public class MetricStatistics
    {
        /// <summary>Gets or sets the metric name.</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>Gets or sets the minimum value in the window.</summary>
        public double Min { get; set; }
        /// <summary>Gets or sets the maximum value in the window.</summary>
        public double Max { get; set; }
        /// <summary>Gets or sets the average value in the window.</summary>
        public double Average { get; set; }
        /// <summary>Gets or sets the sum of all values in the window.</summary>
        public double Sum { get; set; }
        /// <summary>Gets or sets the count of measurements in the window.</summary>
        public int Count { get; set; }
        /// <summary>Gets or sets the start of the aggregation window.</summary>
        public DateTime WindowStart { get; set; }
        /// <summary>Gets or sets the end of the aggregation window.</summary>
        public DateTime WindowEnd { get; set; }
    }

    /// <summary>
    /// Represents a threshold violation for a metric.
    /// </summary>
    public class ThresholdViolation
    {
        /// <summary>Gets or sets the name of the violated metric.</summary>
        public string MetricName { get; set; } = string.Empty;
        /// <summary>Gets or sets the actual value that violated the threshold.</summary>
        public double Value { get; set; }
        /// <summary>Gets or sets the threshold value.</summary>
        public double Threshold { get; set; }
        /// <summary>Gets or sets the condition that was violated (e.g., "greater than", "less than").</summary>
        public string Condition { get; set; } = string.Empty;
        /// <summary>Gets or sets the timestamp of the violation.</summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Defines a threshold configuration for a metric that determines
    /// when a <see cref="ThresholdViolation"/> should be raised.
    /// </summary>
    public class MetricThreshold
    {
        /// <summary>Gets or sets the threshold value to compare against.</summary>
        public double Value { get; set; }

        /// <summary>Gets or sets the comparison condition for this threshold.</summary>
        public ThresholdCondition Condition { get; set; } = ThresholdCondition.GreaterThan;

        /// <summary>Gets or sets the aggregation mode used to derive the current value from the metric window.</summary>
        public ThresholdAggregation AggregationMode { get; set; } = ThresholdAggregation.Average;

        /// <summary>Gets or sets the time window over which to evaluate the metric. Defaults to 5 minutes if null.</summary>
        public TimeSpan? EvaluationWindow { get; set; }
    }

    /// <summary>
    /// Specifies the comparison condition for a metric threshold check.
    /// </summary>
    public enum ThresholdCondition
    {
        /// <summary>Metric value exceeds the threshold.</summary>
        GreaterThan,
        /// <summary>Metric value is at or above the threshold.</summary>
        GreaterThanOrEqual,
        /// <summary>Metric value is below the threshold.</summary>
        LessThan,
        /// <summary>Metric value is at or below the threshold.</summary>
        LessThanOrEqual,
        /// <summary>Metric value equals the threshold.</summary>
        Equal,
        /// <summary>Metric value does not equal the threshold.</summary>
        NotEqual
    }

    /// <summary>
    /// Specifies how metric values are aggregated before comparison against a threshold.
    /// </summary>
    public enum ThresholdAggregation
    {
        /// <summary>Use the average of metric values in the window.</summary>
        Average,
        /// <summary>Use the minimum value in the window.</summary>
        Min,
        /// <summary>Use the maximum value in the window.</summary>
        Max,
        /// <summary>Use the sum of values in the window.</summary>
        Sum
    }

    /// <summary>
    /// Defines the contract for a persistent metric storage backend.
    /// </summary>
    public interface IMetricsStore
    {
        /// <summary>
        /// Stores a single metric data point.
        /// </summary>
        /// <param name="metric">The metric to store.</param>
        void StoreMetric(Metric metric);

        /// <summary>
        /// Queries metrics matching the specified criteria.
        /// </summary>
        /// <param name="name">The metric name to query.</param>
        /// <param name="since">The start of the query window.</param>
        /// <param name="until">The end of the query window. If null, uses current time.</param>
        /// <param name="tags">Optional tags to filter by.</param>
        /// <returns>A collection of matching metrics.</returns>
        Task<IEnumerable<Metric>> QueryMetricsAsync(
            string name,
            DateTime since,
            DateTime? until = null,
            IDictionary<string, string>? tags = null);
    }

    /// <summary>
    /// Aggregates metric values over time windows.
    /// </summary>
    internal class MetricAggregator
    {
        private readonly string _name;
        private readonly Queue<(double Value, DateTime Timestamp)> _values;
        private double _sum;
        private double _min = double.MaxValue;
        private double _max = double.MinValue;
        private int _count;

        public MetricAggregator(string name)
        {
            _name = name;
            _values = new Queue<(double, DateTime)>();
        }

        public void AddValue(double value, DateTime timestamp)
        {
            _values.Enqueue((value, timestamp));
            _sum += value;
            _count++;
            
            if (value < _min) _min = value;
            if (value > _max) _max = value;
            
            // Remove old values outside our window
            var cutoff = timestamp - TimeSpan.FromMinutes(5); // Default 5-minute window
            while (_values.Count > 0 && _values.Peek().Timestamp < cutoff)
            {
                var (oldValue, _) = _values.Dequeue();
                _sum -= oldValue;
                _count--;
                
                // If we removed the min or max, we need to rescan
                if (oldValue <= _min || oldValue >= _max)
                {
                    RescanMinMax();
                }
            }
        }

        public MetricStatistics GetStats(TimeSpan window)
        {
            var now = DateTime.UtcNow;
            var cutoff = now - window;
            
            // Ensure we have data in the window
            if (_values.Count == 0 || _values.Peek().Timestamp > now)
                return null!;
                
            return new MetricStatistics
            {
                Name = _name,
                Min = _min,
                Max = _max,
                Average = _count > 0 ? _sum / _count : 0,
                Sum = _sum,
                Count = _count,
                WindowStart = cutoff,
                WindowEnd = now
            };
        }

        public void Flush()
        {
            // Save any pending metrics to persistent storage
            // This would be called during disposal or periodically
        }

        private void RescanMinMax()
        {
            if (_values.Count == 0)
            {
                _min = double.MaxValue;
                _max = double.MinValue;
                return;
            }
            
            _min = double.MaxValue;
            _max = double.MinValue;
            
            foreach (var (value, _) in _values)
            {
                if (value < _min) _min = value;
                if (value > _max) _max = value;
            }
        }
    }
}
