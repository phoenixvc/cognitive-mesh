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
    public class ConclAIveOrchestratorTests
    {
        private readonly Mock<ILogger<ConclAIveOrchestrator>> _loggerMock;
        private readonly Mock<IDebateReasoningPort> _debateReasoningMock;
        private readonly Mock<ISequentialReasoningPort> _sequentialReasoningMock;
        private readonly Mock<IStrategicSimulationPort> _strategicSimulationMock;
        private readonly Mock<ILLMClient> _llmClientMock;
        private readonly ConclAIveOrchestrator _orchestrator;

        public ConclAIveOrchestratorTests()
        {
            _loggerMock = new Mock<ILogger<ConclAIveOrchestrator>>();
            _debateReasoningMock = new Mock<IDebateReasoningPort>();
            _sequentialReasoningMock = new Mock<ISequentialReasoningPort>();
            _strategicSimulationMock = new Mock<IStrategicSimulationPort>();
            _llmClientMock = new Mock<ILLMClient>();

            _orchestrator = new ConclAIveOrchestrator(
                _loggerMock.Object,
                _debateReasoningMock.Object,
                _sequentialReasoningMock.Object,
                _strategicSimulationMock.Object,
                _llmClientMock.Object
            );
        }

        [Fact]
        public async Task ReasonAsync_WithExplicitRecipe_ShouldUseSpecifiedRecipe()
        {
            // Arrange
            var query = "Test query";
            var expectedOutput = new ReasoningOutput
            {
                RecipeType = ReasoningRecipeType.DebateAndVote,
                Conclusion = "Test conclusion"
            };

            _llmClientMock
                .Setup(x => x.GenerateCompletionAsync(It.IsAny<string>()))
                .ReturnsAsync("Perspective 1\nPerspective 2");

            _debateReasoningMock
                .Setup(x => x.ExecuteDebateAsync(It.IsAny<DebateRequest>()))
                .ReturnsAsync(expectedOutput);

            // Act
            var result = await _orchestrator.ReasonAsync(
                query,
                ReasoningRecipeType.DebateAndVote
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ReasoningRecipeType.DebateAndVote, result.RecipeType);
            _debateReasoningMock.Verify(x => x.ExecuteDebateAsync(It.IsAny<DebateRequest>()), Times.Once);
        }

        [Fact]
        public async Task ReasonAsync_WithSequentialRecipe_ShouldUseSequentialReasoning()
        {
            // Arrange
            var query = "Complex multi-step problem";
            var expectedOutput = new ReasoningOutput
            {
                RecipeType = ReasoningRecipeType.Sequential,
                Conclusion = "Sequential conclusion"
            };

            _sequentialReasoningMock
                .Setup(x => x.ExecuteSequentialReasoningAsync(It.IsAny<SequentialReasoningRequest>()))
                .ReturnsAsync(expectedOutput);

            // Act
            var result = await _orchestrator.ReasonAsync(
                query,
                ReasoningRecipeType.Sequential
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ReasoningRecipeType.Sequential, result.RecipeType);
            _sequentialReasoningMock.Verify(
                x => x.ExecuteSequentialReasoningAsync(It.IsAny<SequentialReasoningRequest>()),
                Times.Once
            );
        }

        [Fact]
        public async Task ReasonAsync_WithStrategicSimulation_ShouldUseStrategicSimulation()
        {
            // Arrange
            var query = "What are the possible outcomes?";
            var expectedOutput = new ReasoningOutput
            {
                RecipeType = ReasoningRecipeType.StrategicSimulation,
                Conclusion = "Strategic conclusion"
            };

            _strategicSimulationMock
                .Setup(x => x.ExecuteStrategicSimulationAsync(It.IsAny<StrategicSimulationRequest>()))
                .ReturnsAsync(expectedOutput);

            // Act
            var result = await _orchestrator.ReasonAsync(
                query,
                ReasoningRecipeType.StrategicSimulation
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ReasoningRecipeType.StrategicSimulation, result.RecipeType);
            _strategicSimulationMock.Verify(
                x => x.ExecuteStrategicSimulationAsync(It.IsAny<StrategicSimulationRequest>()),
                Times.Once
            );
        }

        [Fact]
        public async Task ReasonAsync_WithAutoSelection_ShouldSelectAppropriateRecipe()
        {
            // Arrange
            var query = "Test query for auto-selection";
            var expectedOutput = new ReasoningOutput
            {
                RecipeType = ReasoningRecipeType.Sequential,
                Conclusion = "Auto-selected conclusion"
            };

            _llmClientMock
                .Setup(x => x.GenerateCompletionAsync(It.IsAny<string>()))
                .ReturnsAsync("SEQUENTIAL");

            _sequentialReasoningMock
                .Setup(x => x.ExecuteSequentialReasoningAsync(It.IsAny<SequentialReasoningRequest>()))
                .ReturnsAsync(expectedOutput);

            // Act
            var result = await _orchestrator.ReasonAsync(query);

            // Assert
            Assert.NotNull(result);
            _llmClientMock.Verify(x => x.GenerateCompletionAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ReasonAsync_ShouldIncludeContext()
        {
            // Arrange
            var query = "Test query";
            var context = new Dictionary<string, string>
            {
                { "domain", "Healthcare" },
                { "priority", "High" }
            };

            var expectedOutput = new ReasoningOutput
            {
                RecipeType = ReasoningRecipeType.Sequential,
                Conclusion = "Test"
            };

            _sequentialReasoningMock
                .Setup(x => x.ExecuteSequentialReasoningAsync(It.IsAny<SequentialReasoningRequest>()))
                .ReturnsAsync(expectedOutput);

            // Act
            var result = await _orchestrator.ReasonAsync(
                query,
                ReasoningRecipeType.Sequential,
                context
            );

            // Assert
            Assert.NotNull(result);
            _sequentialReasoningMock.Verify(
                x => x.ExecuteSequentialReasoningAsync(
                    It.Is<SequentialReasoningRequest>(r => r.Context.ContainsKey("domain"))
                ),
                Times.Once
            );
        }
    }
}
