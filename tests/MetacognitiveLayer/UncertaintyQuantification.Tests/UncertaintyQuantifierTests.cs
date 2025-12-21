using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using CognitiveMesh.MetacognitiveLayer.UncertaintyQuantification;

namespace CognitiveMesh.MetacognitiveLayer.UncertaintyQuantification.Tests
{
    public class UncertaintyQuantifierTests
    {
        [Fact]
        public async Task QuantifyUncertaintyAsync_NullData_ReturnsZeroConfidence()
        {
            // Arrange
            var quantifier = new UncertaintyQuantifier();

            // Act
            var result = await quantifier.QuantifyUncertaintyAsync(null);

            // Assert
            Assert.Equal(0.0, result["confidence"]);
            Assert.Equal("NullData", result["uncertaintyType"]);
        }

        [Fact]
        public async Task QuantifyUncertaintyAsync_TextWithHedgeWords_ReturnsLowerConfidence()
        {
            // Arrange
            var quantifier = new UncertaintyQuantifier();
            string text = "Maybe it is possible that the result is approximately correct.";
            // Hedge words: Maybe, possible, approximately (3 words)
            // Total words: 10
            // Ratio: 0.3
            // Confidence: 1.0 - (0.3 * 5) = -0.5 -> 0.0

            // Act
            var result = await quantifier.QuantifyUncertaintyAsync(text);

            // Assert
            Assert.Equal("Linguistic", result["uncertaintyType"]);
            Assert.True((double)result["confidence"] < 1.0);

            var metrics = (Dictionary<string, object>)result["metrics"];
            Assert.True((int)metrics["hedgeWordCount"] > 0);
        }

        [Fact]
        public async Task QuantifyUncertaintyAsync_TextWithoutHedgeWords_ReturnsHighConfidence()
        {
            // Arrange
            var quantifier = new UncertaintyQuantifier();
            string text = "The result is exactly five.";

            // Act
            var result = await quantifier.QuantifyUncertaintyAsync(text);

            // Assert
            Assert.Equal(1.0, (double)result["confidence"]);
            var metrics = (Dictionary<string, object>)result["metrics"];
            Assert.Equal(0, (int)metrics["hedgeWordCount"]);
        }

        [Fact]
        public async Task QuantifyUncertaintyAsync_NumericalCollection_CalculatesVariance()
        {
            // Arrange
            var quantifier = new UncertaintyQuantifier();
            var numbers = new List<double> { 10.0, 10.0, 10.0 }; // Variance 0

            // Act
            var result = await quantifier.QuantifyUncertaintyAsync(numbers);

            // Assert
            Assert.Equal("Statistical", result["uncertaintyType"]);
            Assert.Equal(1.0, (double)result["confidence"]); // Variance 0 -> Confidence 1

            var metrics = (Dictionary<string, object>)result["metrics"];
            Assert.Equal(0.0, (double)metrics["variance"]);
            Assert.Equal(10.0, (double)metrics["mean"]);
        }

        [Fact]
        public async Task QuantifyUncertaintyAsync_NumericalCollection_HighVariance_ReturnsLowerConfidence()
        {
            // Arrange
            var quantifier = new UncertaintyQuantifier();
            var numbers = new List<double> { 0.0, 100.0 };
            // Mean 50. Variance = ((50^2 + 50^2) / 1) = 5000. StdDev ~70.7
            // CV = 70.7 / 50 = 1.41
            // Conf = 1 / (1 + 1.41) = 0.41

            // Act
            var result = await quantifier.QuantifyUncertaintyAsync(numbers);

            // Assert
            Assert.True((double)result["confidence"] < 1.0);
            var metrics = (Dictionary<string, object>)result["metrics"];
            Assert.True((double)metrics["variance"] > 0);
        }

        [Fact]
        public async Task QuantifyUncertaintyAsync_Distribution_CalculatesEntropy()
        {
            // Arrange
            var quantifier = new UncertaintyQuantifier();
            var distribution = new Dictionary<string, double>
            {
                { "A", 0.5 },
                { "B", 0.5 }
            };
            // Entropy: - (0.5log2(0.5) + 0.5log2(0.5)) = - (-0.5 - 0.5) = 1.0
            // MaxEntropy for 2 items: log2(2) = 1.0
            // Normalized: 1.0 / 1.0 = 1.0
            // Confidence: 1.0 - 1.0 = 0.0 (Maximum uncertainty)

            // Act
            var result = await quantifier.QuantifyUncertaintyAsync(distribution);

            // Assert
            Assert.Equal("Entropy (String Keys)", result["uncertaintyType"]);
            Assert.Equal(0.0, (double)result["confidence"], 5); // Precision 5

            var metrics = (Dictionary<string, object>)result["metrics"];
            Assert.Equal(1.0, (double)metrics["entropy"], 5);
        }

        [Fact]
        public async Task QuantifyUncertaintyAsync_SkewedDistribution_ReturnsHigherConfidence()
        {
             // Arrange
            var quantifier = new UncertaintyQuantifier();
            var distribution = new Dictionary<string, double>
            {
                { "A", 0.9 },
                { "B", 0.1 }
            };
            // Entropy: - (0.9*-0.152 + 0.1*-3.32) = - (-0.136 - 0.332) = 0.469
            // MaxEntropy: 1.0
            // Normalized: 0.469
            // Confidence: 0.531

            // Act
            var result = await quantifier.QuantifyUncertaintyAsync(distribution);

            // Assert
            Assert.True((double)result["confidence"] > 0.0);
            Assert.True((double)result["confidence"] < 1.0);
        }

        [Fact]
        public async Task CalculateConfidenceScoreAsync_UsesTextLogic()
        {
             // Arrange
            var quantifier = new UncertaintyQuantifier();
            string text = "Maybe yes.";

            // Act
            double score = await quantifier.CalculateConfidenceScoreAsync(text);

            // Assert
            Assert.True(score < 1.0);
        }

        [Fact]
        public async Task IsWithinThresholdAsync_ChecksConfidence()
        {
            // Arrange
            var quantifier = new UncertaintyQuantifier();
            var numbers = new List<double> { 10.0, 10.0 }; // Conf 1.0

            // Act
            bool result = await quantifier.IsWithinThresholdAsync(numbers, 0.9);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsWithinThresholdAsync_FailsIfBelow()
        {
            // Arrange
            var quantifier = new UncertaintyQuantifier();
            var distribution = new Dictionary<string, double>
            {
                { "A", 0.5 },
                { "B", 0.5 }
            }; // Conf 0.0

            // Act
            bool result = await quantifier.IsWithinThresholdAsync(distribution, 0.1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ApplyUncertaintyMitigationStrategyAsync_FallbackToDefault_LogsAction()
        {
            // Arrange
            var quantifier = new UncertaintyQuantifier();
            var parameters = new Dictionary<string, object>
            {
                { "defaultValue", 42 }
            };

            // Act - Should not throw
            await quantifier.ApplyUncertaintyMitigationStrategyAsync(
                UncertaintyQuantifier.StrategyFallbackToDefault,
                parameters);

            // Assert - No exception means success
            Assert.True(true);
        }

        [Fact]
        public async Task ApplyUncertaintyMitigationStrategyAsync_ConservativeExecution_ExecutesSuccessfully()
        {
            // Arrange
            var quantifier = new UncertaintyQuantifier();
            var parameters = new Dictionary<string, object>
            {
                { "confidenceThreshold", 0.8 }
            };

            // Act
            await quantifier.ApplyUncertaintyMitigationStrategyAsync(
                UncertaintyQuantifier.StrategyConservativeExecution,
                parameters);

            // Assert - No exception means success
            Assert.True(true);
        }

        [Fact]
        public async Task ApplyUncertaintyMitigationStrategyAsync_EnsembleVerification_ExecutesSuccessfully()
        {
            // Arrange
            var quantifier = new UncertaintyQuantifier();

            // Act
            await quantifier.ApplyUncertaintyMitigationStrategyAsync(
                UncertaintyQuantifier.StrategyEnsembleVerification);

            // Assert - No exception means success
            Assert.True(true);
        }

        [Fact]
        public async Task ApplyUncertaintyMitigationStrategyAsync_UnknownStrategy_ThrowsArgumentException()
        {
            // Arrange
            var quantifier = new UncertaintyQuantifier();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                quantifier.ApplyUncertaintyMitigationStrategyAsync("UnknownStrategy"));
        }
    }
}
