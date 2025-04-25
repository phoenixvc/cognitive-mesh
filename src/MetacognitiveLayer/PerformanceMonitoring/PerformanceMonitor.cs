using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class PerformanceMonitor
{
    private readonly ILogger<PerformanceMonitor> _logger;

    public PerformanceMonitor(ILogger<PerformanceMonitor> logger)
    {
        _logger = logger;
    }

    public async Task<PerformanceMetrics> MonitorPerformanceAsync(string systemComponent, List<string> metrics)
    {
        try
        {
            _logger.LogInformation($"Starting performance monitoring for component: {systemComponent}");

            // Simulate performance monitoring logic
            await Task.Delay(1000);

            var result = new PerformanceMetrics
            {
                Component = systemComponent,
                Metrics = metrics,
                PerformanceScore = CalculatePerformanceScore(metrics)
            };

            _logger.LogInformation($"Successfully monitored performance for component: {systemComponent}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to monitor performance for component: {systemComponent}. Error: {ex.Message}");
            throw;
        }
    }

    private double CalculatePerformanceScore(List<string> metrics)
    {
        // Simulate performance score calculation
        return metrics.Count * 0.2;
    }
}

public class PerformanceMetrics
{
    public string Component { get; set; }
    public List<string> Metrics { get; set; }
    public double PerformanceScore { get; set; }
}
