using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class UncertaintyQuantifier
{
    private readonly ILogger<UncertaintyQuantifier> _logger;

    public UncertaintyQuantifier(ILogger<UncertaintyQuantifier> logger)
    {
        _logger = logger;
    }

    public async Task<UncertaintyResult> QuantifyUncertaintyAsync(string prediction, List<string> factors)
    {
        try
        {
            _logger.LogInformation($"Starting uncertainty quantification for prediction: {prediction}");

            // Simulate uncertainty quantification logic
            await Task.Delay(1000);

            var result = new UncertaintyResult
            {
                Prediction = prediction,
                Factors = factors,
                UncertaintyScore = CalculateUncertaintyScore(factors)
            };

            _logger.LogInformation($"Successfully quantified uncertainty for prediction: {prediction}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to quantify uncertainty for prediction: {prediction}. Error: {ex.Message}");
            throw;
        }
    }

    private double CalculateUncertaintyScore(List<string> factors)
    {
        // Simulate uncertainty score calculation
        return factors.Count * 0.1;
    }
}

public class UncertaintyResult
{
    public string Prediction { get; set; }
    public List<string> Factors { get; set; }
    public double UncertaintyScore { get; set; }
}
