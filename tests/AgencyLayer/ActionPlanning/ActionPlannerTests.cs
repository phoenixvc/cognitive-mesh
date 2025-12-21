using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CognitiveMesh.AgencyLayer.ActionPlanning;
using CognitiveMesh.Shared.Interfaces;

namespace CognitiveMesh.Tests.AgencyLayer.ActionPlanning
{
    public class ActionPlannerTests
    {
        private readonly Mock<ILogger<ActionPlanner>> _mockLogger;
        private readonly Mock<IKnowledgeGraphManager> _mockKnowledgeGraphManager;
        private readonly Mock<ILLMClient> _mockLlmClient;
        private readonly Mock<ISemanticSearchManager> _mockSemanticSearchManager;
        private readonly ActionPlanner _planner;

        public ActionPlannerTests()
        {
            _mockLogger = new Mock<ILogger<ActionPlanner>>();
            _mockKnowledgeGraphManager = new Mock<IKnowledgeGraphManager>();
            _mockLlmClient = new Mock<ILLMClient>();
            _mockSemanticSearchManager = new Mock<ISemanticSearchManager>();

            _planner = new ActionPlanner(
                _mockLogger.Object,
                _mockKnowledgeGraphManager.Object,
                _mockLlmClient.Object,
                _mockSemanticSearchManager.Object
            );
        }

        [Fact]
        public async Task GeneratePlanAsync_ShouldUseComponentsAndReturnPlans()
        {
            // Arrange
            var goal = "Analyze market trends";
            var constraints = new[] { "Quickly", "Low cost" };

            // Mock Semantic Search
            _mockSemanticSearchManager
                .Setup(m => m.SearchAsync("skills-index", goal))
                .ReturnsAsync("Relevant Skill: MarketAnalysisTool");

            // Mock Knowledge Graph
            var mockPolicyNode = new Dictionary<string, object> { ["n"] = "Policy: ComplianceCheck" };
            _mockKnowledgeGraphManager
                .Setup(m => m.QueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { mockPolicyNode });

            // Mock LLM Response
            var expectedPlan = new[]
            {
                new { Name = "Step 1", Description = "Gather data", Priority = 1 },
                new { Name = "Step 2", Description = "Analyze data", Priority = 2 }
            };
            var jsonResponse = JsonSerializer.Serialize(expectedPlan);

            _mockLlmClient
                .Setup(m => m.GenerateChatCompletionAsync(
                    It.IsAny<IEnumerable<ChatMessage>>(),
                    It.IsAny<float>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(jsonResponse);

            // Act
            var result = await _planner.GeneratePlanAsync(goal, constraints);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Step 1", result.First().Name);

            // Verify interactions
            _mockSemanticSearchManager.Verify(m => m.SearchAsync("skills-index", goal), Times.Once);
            _mockKnowledgeGraphManager.Verify(m => m.QueryAsync(It.Is<string>(s => s.Contains("MATCH")), It.IsAny<CancellationToken>()), Times.Once);
            _mockLlmClient.Verify(m => m.GenerateChatCompletionAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GeneratePlanAsync_ShouldHandleInvalidJsonGracefully()
        {
            // Arrange
            var goal = "Do something";
            _mockSemanticSearchManager.Setup(m => m.SearchAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("");
            _mockKnowledgeGraphManager.Setup(m => m.QueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(Enumerable.Empty<Dictionary<string, object>>());

            // Return invalid JSON
            _mockLlmClient.Setup(m => m.GenerateChatCompletionAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Not a JSON string");

            // Act
            var result = await _planner.GeneratePlanAsync(goal);

            // Assert
            Assert.Single(result);
            Assert.Equal(ActionPlanStatus.Failed, result.First().Status);
            Assert.Contains("Plan Generation Failed", result.First().Name);
        }
    }
}
