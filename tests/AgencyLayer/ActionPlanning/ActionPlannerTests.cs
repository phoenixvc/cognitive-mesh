using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using CognitiveMesh.AgencyLayer.ActionPlanning;
using CognitiveMesh.Shared.Interfaces;

namespace CognitiveMesh.AgencyLayer.ActionPlanning.Tests
{
    public class ActionPlannerTests
    {
        private readonly Mock<ILogger<ActionPlanner>> _mockLogger;
        private readonly Mock<IKnowledgeGraphManager> _mockKG;
        private readonly Mock<ILLMClient> _mockLLM;
        private readonly ActionPlanner _planner;

        public ActionPlannerTests()
        {
            _mockLogger = new Mock<ILogger<ActionPlanner>>();
            _mockKG = new Mock<IKnowledgeGraphManager>();
            _mockLLM = new Mock<ILLMClient>();
            _planner = new ActionPlanner(_mockLogger.Object, _mockKG.Object, _mockLLM.Object);
        }

        [Fact]
        public async Task GeneratePlanAsync_ShouldStorePlanInKG()
        {
            // Arrange
            string goal = "Test Goal";

            _mockKG.Setup(x => x.AddNodeAsync(It.IsAny<string>(), It.IsAny<ActionPlan>(), "ActionPlan", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _planner.GeneratePlanAsync(goal);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            _mockKG.Verify(x => x.AddNodeAsync(It.IsAny<string>(), It.IsAny<ActionPlan>(), "ActionPlan", It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecutePlanAsync_ShouldExecuteAndStoreResult()
        {
            // Arrange
            string planId = "plan-123";
            var plan = new ActionPlan
            {
                Id = planId,
                Description = "Do something",
                Status = ActionPlanStatus.Pending
            };

            // Mock GetNodeAsync to return the plan
            _mockKG.Setup(x => x.GetNodeAsync<ActionPlan>(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);

            // Mock UpdateNodeAsync
            _mockKG.Setup(x => x.UpdateNodeAsync(planId, It.IsAny<ActionPlan>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Mock LLM completion
            string expectedResult = "Done successfully";
            _mockLLM.Setup(x => x.GenerateCompletionAsync(plan.Description, It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultPlan = await _planner.ExecutePlanAsync(planId);

            // Assert
            Assert.Equal(ActionPlanStatus.Completed, resultPlan.Status);
            Assert.Equal(expectedResult, resultPlan.Result);
            Assert.NotNull(resultPlan.CompletedAt);

            // Verify interactions
            // 1. GetNode
            _mockKG.Verify(x => x.GetNodeAsync<ActionPlan>(planId, It.IsAny<CancellationToken>()), Times.Once);

            // 2. Update status to InProgress
            // Note: Since we are passing the same object reference, verifying exact sequence/state might be tricky with simple Verifies if we don't capture arguments, but we can verify UpdateNodeAsync was called twice.
            _mockKG.Verify(x => x.UpdateNodeAsync(planId, It.IsAny<ActionPlan>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

            // 3. LLM call
            _mockLLM.Verify(x => x.GenerateCompletionAsync(plan.Description, It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecutePlanAsync_ShouldThrowIfPlanNotFound()
        {
            // Arrange
            string planId = "non-existent";
            _mockKG.Setup(x => x.GetNodeAsync<ActionPlan>(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ActionPlan?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _planner.ExecutePlanAsync(planId));
        }

        [Fact]
        public async Task ExecutePlanAsync_ShouldHandleLLMError()
        {
            // Arrange
            string planId = "plan-error";
            var plan = new ActionPlan
            {
                Id = planId,
                Description = "Do something risky",
                Status = ActionPlanStatus.Pending
            };

            _mockKG.Setup(x => x.GetNodeAsync<ActionPlan>(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);

            _mockLLM.Setup(x => x.GenerateCompletionAsync(It.IsAny<string>(), It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("LLM Failed"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _planner.ExecutePlanAsync(planId));
            Assert.Equal("LLM Failed", ex.Message);

            // Verify plan status was updated to Failed
            Assert.Equal(ActionPlanStatus.Failed, plan.Status);
            Assert.Equal("LLM Failed", plan.Error);

            // Verify UpdateNodeAsync was called (once for InProgress, once for Failed)
            _mockKG.Verify(x => x.UpdateNodeAsync(planId, It.IsAny<ActionPlan>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }
}
