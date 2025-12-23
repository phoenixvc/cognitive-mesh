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

        /// <summary>
        /// Verifies that text with hedge words results in lower confidence.
        /// </summary>
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

        /// <summary>
        /// Verifies that text without hedge words results in high confidence.
        /// </summary>
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

        /// <summary>
        /// Verifies that variance is calculated for numerical collections.
        /// </summary>
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

        /// <summary>
        /// Verifies that high variance results in lower confidence.
        /// </summary>
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

        /// <summary>
        /// Verifies that entropy is calculated for distributions.
        /// </summary>
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

        /// <summary>
        /// Verifies that skewed distributions have higher confidence.
        /// </summary>
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

        /// <summary>
        /// Verifies that text logic is used when LLM is not available.
        /// </summary>
        [Fact]
        public async Task CalculateConfidenceScoreAsync_UsesTextLogic_WhenLLMNotAvailable()
        {
             // Arrange
            var quantifier = new UncertaintyQuantifier(); // No LLM
            string text = "Maybe yes.";

            // Act
            double score = await quantifier.CalculateConfidenceScoreAsync(text);

            // Assert
            Assert.True(score < 1.0);
        }

        /// <summary>
        /// Verifies IsWithinThresholdAsync checks confidence correctly.
        /// </summary>
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

        /// <summary>
        /// Verifies IsWithinThresholdAsync fails if confidence is below threshold.
        /// </summary>
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

        /// <summary>
        /// Verifies fallback strategy execution.
        /// </summary>
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

        /// <summary>
        /// Verifies conservative execution strategy.
        /// </summary>
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

        /// <summary>
        /// Verifies ensemble verification strategy.
        /// </summary>
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

        /// <summary>
        /// Verifies that an unknown strategy throws an ArgumentException.
        /// </summary>
        [Fact]
        public async Task ApplyUncertaintyMitigationStrategyAsync_UnknownStrategy_ThrowsArgumentException()
        {
            // Arrange
            var quantifier = new UncertaintyQuantifier();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                quantifier.ApplyUncertaintyMitigationStrategyAsync("UnknownStrategy"));
        }

        // --- LLM Tests ---

        /// <summary>
        /// Verifies that CalculateConfidenceScoreAsync returns the score from the LLM.
        /// </summary>
        [Fact]
        public async Task CalculateConfidenceScoreAsync_ReturnsScoreFromLLM()
        {
            // Arrange
            var mockLLMClient = new Mock<ILLMClient>();
            string data = "The sky is blue.";
            string expectedScoreString = "0.95";
            double expectedScore = 0.95;

            mockLLMClient.Setup(p => p.GenerateCompletionAsync(
                It.IsAny<string>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedScoreString);

            var quantifier = new UncertaintyQuantifier(llmClient: mockLLMClient.Object);

            // Act
            double result = await quantifier.CalculateConfidenceScoreAsync(data);

            // Assert
            Assert.Equal(expectedScore, result, 0.001);
            mockLLMClient.Verify(p => p.GenerateCompletionAsync(
                It.Is<string>(s => s.Contains(data)),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that CalculateConfidenceScoreAsync clamps the score to the valid range.
        /// </summary>
        [Fact]
        public async Task CalculateConfidenceScoreAsync_ClampsScoreToRange()
        {
            // Arrange
            var mockLLMClient = new Mock<ILLMClient>();
            string data = "Super confident!";
            mockLLMClient.Setup(p => p.GenerateCompletionAsync(
                It.IsAny<string>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync("1.5"); // LLM returns > 1.0

            var quantifier = new UncertaintyQuantifier(llmClient: mockLLMClient.Object);

            // Act
            double result = await quantifier.CalculateConfidenceScoreAsync(data);

            // Assert
            Assert.Equal(1.0, result, 0.001);

             mockLLMClient.Setup(p => p.GenerateCompletionAsync(
                It.IsAny<string>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync("-0.5"); // LLM returns < 0.0

            result = await quantifier.CalculateConfidenceScoreAsync(data);
            Assert.Equal(0.0, result, 0.001);
        }

        /// <summary>
        /// Verifies that CalculateConfidenceScoreAsync uses heuristic fallback when LLM response parsing fails.
        /// </summary>
        [Fact]
        public async Task CalculateConfidenceScoreAsync_UsesHeuristic_WhenLLMFailsToParse()
        {
            // Arrange
            var mockLLMClient = new Mock<ILLMClient>();
            string data = "I am maybe going to the store."; // 7 words, 1 hedge word ("maybe"). Ratio = 1/7.
            // Heuristic: 1.0 - (ratio * 5.0) = 1.0 - (1/7 * 5) = 1.0 - 0.714 = 0.286...

            mockLLMClient.Setup(p => p.GenerateCompletionAsync(
                It.IsAny<string>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync("I am not sure about the score"); // Not a number

            var quantifier = new UncertaintyQuantifier(llmClient: mockLLMClient.Object);

            // Act
            double result = await quantifier.CalculateConfidenceScoreAsync(data);

            // Assert
            Assert.Equal(0.2857, result, 0.001);
        }

        /// <summary>
        /// Verifies that CalculateConfidenceScoreAsync uses heuristic fallback when LLM throws an exception.
        /// </summary>
        [Fact]
        public async Task CalculateConfidenceScoreAsync_UsesHeuristic_WhenLLMThrows()
        {
            // Arrange
            var mockLLMClient = new Mock<ILLMClient>();
            string data = "I might be wrong."; // 4 words, 1 hedge word ("might"). Ratio = 0.25.
            // Heuristic: 1.0 - (0.25 * 5.0) = 1.0 - 1.25 = -0.25 -> clamped to 0.0

            mockLLMClient.Setup(p => p.GenerateCompletionAsync(
                It.IsAny<string>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("LLM Down"));

            var quantifier = new UncertaintyQuantifier(llmClient: mockLLMClient.Object);

            // Act
            double result = await quantifier.CalculateConfidenceScoreAsync(data);

            // Assert
            Assert.Equal(0.0, result, 0.001);
        }
    }
}
