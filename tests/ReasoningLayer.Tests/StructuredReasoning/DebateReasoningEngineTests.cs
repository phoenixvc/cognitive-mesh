using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using CognitiveMesh.ReasoningLayer.StructuredReasoning.Models;
using CognitiveMesh.ReasoningLayer.StructuredReasoning.Ports;
using CognitiveMesh.ReasoningLayer.StructuredReasoning.Engines;
using CognitiveMesh.ReasoningLayer.LLMReasoning.Abstractions;

namespace CognitiveMesh.ReasoningLayer.Tests.StructuredReasoning
{
    public class DebateReasoningEngineTests
    {
        private readonly Mock<ILogger<DebateReasoningEngine>> _loggerMock;
        private readonly Mock<ILLMClient> _llmClientMock;
        private readonly DebateReasoningEngine _engine;

        public DebateReasoningEngineTests()
        {
            _loggerMock = new Mock<ILogger<DebateReasoningEngine>>();
            _llmClientMock = new Mock<ILLMClient>();
            _engine = new DebateReasoningEngine(_loggerMock.Object, _llmClientMock.Object);
        }

        [Fact]
        public async Task ExecuteDebateAsync_ShouldGenerateArgumentsFromMultiplePerspectives()
        {
            // Arrange
            var request = new DebateRequest
            {
                Question = "Should companies prioritize profit or sustainability?",
                Perspectives = new List<string> { "Business Perspective", "Environmental Perspective" },
                Context = new Dictionary<string, string> { { "industry", "Technology" } }
            };

            _llmClientMock
                .Setup(x => x.GenerateCompletionAsync(It.IsAny<string>()))
                .ReturnsAsync("Position: Companies should prioritize profit\nSupporting Points:\n- Shareholder value\n- Economic growth");

            // Act
            var result = await _engine.ExecuteDebateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ReasoningRecipeType.DebateAndVote, result.RecipeType);
            Assert.NotEmpty(result.ReasoningTrace);
            Assert.True(result.ReasoningTrace.Count >= 2); // At least 2 perspectives
        }

        [Fact]
        public async Task ExecuteDebateAsync_ShouldIncludeSynthesisStep()
        {
            // Arrange
            var request = new DebateRequest
            {
                Question = "Test question",
                Perspectives = new List<string> { "Perspective A", "Perspective B" },
                Context = new Dictionary<string, string>()
            };

            _llmClientMock
                .Setup(x => x.GenerateCompletionAsync(It.IsAny<string>()))
                .ReturnsAsync("Position: Test position\nSupporting Points:\n- Point 1\nConfidence: 75");

            // Act
            var result = await _engine.ExecuteDebateAsync(request);

            // Assert
            Assert.NotNull(result.Conclusion);
            Assert.True(result.Confidence > 0 && result.Confidence <= 1.0);
            Assert.Contains(result.ReasoningTrace, step => step.StepName.Contains("Synthesis"));
        }

        [Fact]
        public async Task ExecuteDebateAsync_ShouldIncludeCrossExamination()
        {
            // Arrange
            var request = new DebateRequest
            {
                Question = "Test question",
                Perspectives = new List<string> { "Perspective A", "Perspective B" },
                Context = new Dictionary<string, string>()
            };

            _llmClientMock
                .Setup(x => x.GenerateCompletionAsync(It.IsAny<string>()))
                .ReturnsAsync("Position: Test\nSupporting Points:\n- Point 1");

            // Act
            var result = await _engine.ExecuteDebateAsync(request);

            // Assert
            Assert.Contains(result.ReasoningTrace, step => 
                step.Metadata.ContainsKey("type") && step.Metadata["type"] == "cross_examination");
        }
    }
}
