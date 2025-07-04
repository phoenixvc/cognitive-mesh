using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveMesh.MetacognitiveLayer.PerformanceMonitoring
{
    /// <summary>
    /// Tracks performance metrics across the mesh and triggers alerts on anomalies.
    /// </summary>
    public class PerformanceMonitor : IDisposable
    {
        private readonly IMetricsStore _metricsStore;
        private readonly Dictionary<string, MetricAggregator> _aggregators;
        private bool _disposed = false;

        public PerformanceMonitor(IMetricsStore metricsStore)
        {
            _metricsStore = metricsStore ?? throw new ArgumentNullException(nameof(metricsStore));
            _aggregators = new Dictionary<string, MetricAggregator>();
        }

        /// <summary>
        /// Records a metric value with the given name and optional tags.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="value">The value to record.</param>
        /// <param name="tags">Optional tags to associate with the metric.</param>
        public void RecordMetric(string name, double value, IDictionary<string, string> tags = null)
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
            IDictionary<string, string> tags = null)
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
            
            return null;
        }

        /// <summary>
        /// Checks if any metrics have exceeded their defined thresholds.
        /// </summary>
        /// <returns>A collection of threshold violations, if any.</returns>
        public async Task<IEnumerable<ThresholdViolation>> CheckThresholdsAsync()
        {
            // TODO: Implement threshold checking logic
            // This would check configured thresholds against current metric values
            // and return any violations
            return Array.Empty<ThresholdViolation>();
        }

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
        public string Name { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
        public IDictionary<string, string> Tags { get; set; }
    }

    /// <summary>
    /// Represents aggregated statistics for a metric.
    /// </summary>
    public class MetricStatistics
    {
        public string Name { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double Average { get; set; }
        public double Sum { get; set; }
        public int Count { get; set; }
        public DateTime WindowStart { get; set; }
        public DateTime WindowEnd { get; set; }
    }

    /// <summary>
    /// Represents a threshold violation for a metric.
    /// </summary>
    public class ThresholdViolation
    {
        public string MetricName { get; set; }
        public double Value { get; set; }
        public double Threshold { get; set; }
        public string Condition { get; set; } // e.g., "greater than", "less than"
        public DateTime Timestamp { get; set; }
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
                return null;
                
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
