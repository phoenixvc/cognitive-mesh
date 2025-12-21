using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Extensions.Logging;
using CognitiveMesh.MetacognitiveLayer.UncertaintyQuantification;
using CognitiveMesh.Shared.Interfaces;

namespace CognitiveMesh.MetacognitiveLayer.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="UncertaintyQuantifier"/>.
    /// </summary>
    [TestClass]
    public class UncertaintyQuantifierTests
    {
        private Mock<ILLMClient> _mockLLMClient = null!;
        private Mock<ILogger<UncertaintyQuantifier>> _mockLogger = null!;
        private UncertaintyQuantifier _uncertaintyQuantifier = null!;

        /// <summary>
        /// Sets up the test environment.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _mockLLMClient = new Mock<ILLMClient>();
            _mockLogger = new Mock<ILogger<UncertaintyQuantifier>>();
            _uncertaintyQuantifier = new UncertaintyQuantifier(_mockLogger.Object, _mockLLMClient.Object);
        }

        /// <summary>
        /// Verifies that CalculateConfidenceScoreAsync returns the score provided by the LLM.
        /// </summary>
        [TestMethod]
        public async Task CalculateConfidenceScoreAsync_ReturnsScoreFromLLM()
        {
            // Arrange
            string data = "The sky is blue.";
            string expectedScoreString = "0.95";
            double expectedScore = 0.95;

            _mockLLMClient.Setup(p => p.GenerateCompletionAsync(
                It.IsAny<string>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedScoreString);

            // Act
            double result = await _uncertaintyQuantifier.CalculateConfidenceScoreAsync(data);

            // Assert
            Assert.AreEqual(expectedScore, result, 0.001);
            _mockLLMClient.Verify(p => p.GenerateCompletionAsync(
                It.Is<string>(s => s.Contains(data)),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that CalculateConfidenceScoreAsync clamps the score between 0.0 and 1.0.
        /// </summary>
        [TestMethod]
        public async Task CalculateConfidenceScoreAsync_ClampsScoreToRange()
        {
            // Arrange
            string data = "Super confident!";
            _mockLLMClient.Setup(p => p.GenerateCompletionAsync(
                It.IsAny<string>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync("1.5"); // LLM returns > 1.0

            // Act
            double result = await _uncertaintyQuantifier.CalculateConfidenceScoreAsync(data);

            // Assert
            Assert.AreEqual(1.0, result, 0.001);

             _mockLLMClient.Setup(p => p.GenerateCompletionAsync(
                It.IsAny<string>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync("-0.5"); // LLM returns < 0.0

            result = await _uncertaintyQuantifier.CalculateConfidenceScoreAsync(data);
            Assert.AreEqual(0.0, result, 0.001);
        }

        /// <summary>
        /// Verifies that CalculateConfidenceScoreAsync uses a heuristic fallback when the LLM response cannot be parsed.
        /// </summary>
        [TestMethod]
        public async Task CalculateConfidenceScoreAsync_UsesHeuristic_WhenLLMFailsToParse()
        {
            // Arrange
            string data = "I am maybe going to the store."; // Contains "maybe" -> -0.1 penalty
            _mockLLMClient.Setup(p => p.GenerateCompletionAsync(
                It.IsAny<string>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync("I am not sure about the score"); // Not a number

            // Act
            double result = await _uncertaintyQuantifier.CalculateConfidenceScoreAsync(data);

            // Assert
            // Heuristic base 1.0 - 0.1 (maybe) = 0.9
            Assert.AreEqual(0.9, result, 0.001);
        }

        /// <summary>
        /// Verifies that CalculateConfidenceScoreAsync uses a heuristic fallback when the LLM call throws an exception.
        /// </summary>
        [TestMethod]
        public async Task CalculateConfidenceScoreAsync_UsesHeuristic_WhenLLMThrows()
        {
            // Arrange
            string data = "I might be wrong."; // Contains "might" -> -0.1
            _mockLLMClient.Setup(p => p.GenerateCompletionAsync(
                It.IsAny<string>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("LLM Down"));

            // Act
            double result = await _uncertaintyQuantifier.CalculateConfidenceScoreAsync(data);

            // Assert
            // Heuristic base 1.0 - 0.1 (might) = 0.9
            Assert.AreEqual(0.9, result, 0.001);
        }

        /// <summary>
        /// Verifies that CalculateConfidenceScoreAsync returns a default score when the LLM client is null.
        /// </summary>
        [TestMethod]
        public async Task CalculateConfidenceScoreAsync_ReturnsDefault_WhenLLMClientIsNull()
        {
            // Arrange
            var quantifier = new UncertaintyQuantifier(_mockLogger.Object, null);
            string data = "Some data";

            // Act
            double result = await quantifier.CalculateConfidenceScoreAsync(data);

            // Assert
            Assert.AreEqual(1.0, result);
        }
    }
}
