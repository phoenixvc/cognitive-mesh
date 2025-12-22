using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CognitiveMesh.AgencyLayer.DecisionExecution;
using CognitiveMesh.AgencyLayer.DecisionExecution.Adapters;
using CognitiveMesh.AgencyLayer.DecisionExecution.Events;
using CognitiveMesh.ReasoningLayer.StructuredReasoning.Models;
using CognitiveMesh.ReasoningLayer.StructuredReasoning.Ports;
using CognitiveMesh.Shared.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.AgencyLayer.DecisionExecution
{
    public class DecisionExecutorTests
    {
        private readonly Mock<ILogger<DecisionExecutor>> _loggerMock;
        private readonly Mock<IKnowledgeGraphManager> _knowledgeGraphMock;
        private readonly Mock<IDecisionReasoningEngine> _reasoningEngineMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly DecisionExecutor _executor;

        public DecisionExecutorTests()
        {
            _loggerMock = new Mock<ILogger<DecisionExecutor>>();
            _knowledgeGraphMock = new Mock<IKnowledgeGraphManager>();
            _reasoningEngineMock = new Mock<IDecisionReasoningEngine>();
            _mediatorMock = new Mock<IMediator>();

            _executor = new DecisionExecutor(
                _loggerMock.Object,
                _knowledgeGraphMock.Object,
                _reasoningEngineMock.Object,
                _mediatorMock.Object
            );
        }

        [Fact]
        public async Task ExecuteDecisionAsync_ShouldUseEngineAndPublishNotification()
        {
            // Arrange
            var request = new DecisionRequest { RequestId = "req-1" };
            var expectedResult = new DecisionResult
            {
                RequestId = "req-1",
                Status = DecisionStatus.Completed
            };

            _reasoningEngineMock.Setup(x => x.DetermineOutcomeAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _executor.ExecuteDecisionAsync(request);

            // Assert
            Assert.Same(expectedResult, result);
            _reasoningEngineMock.Verify(x => x.DetermineOutcomeAsync(request, It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(x => x.Publish(It.Is<DecisionMadeNotification>(n => n.DecisionResult == expectedResult), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    public class ConclAIveReasoningAdapterTests
    {
        private readonly Mock<IConclAIveOrchestratorPort> _orchestratorMock;
        private readonly Mock<ILogger<ConclAIveReasoningAdapter>> _loggerMock;
        private readonly ConclAIveReasoningAdapter _adapter;

        public ConclAIveReasoningAdapterTests()
        {
            _orchestratorMock = new Mock<IConclAIveOrchestratorPort>();
            _loggerMock = new Mock<ILogger<ConclAIveReasoningAdapter>>();
            _adapter = new ConclAIveReasoningAdapter(_orchestratorMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task DetermineOutcomeAsync_ShouldCallOrchestratorAndMapResult()
        {
            // Arrange
            var request = new DecisionRequest
            {
                RequestId = "req-1",
                Parameters = new Dictionary<string, object> { { "query", "Should I deploy?" } }
            };

            var reasoningOutput = new ReasoningOutput
            {
                ConfidenceScore = 0.8,
                Conclusion = "Yes",
                Summary = "Because tests passed",
                ReasoningSteps = new List<ReasoningStep>()
            };

            _orchestratorMock.Setup(x => x.ReasonAsync(It.IsAny<string>(), It.IsAny<ReasoningRecipeType?>(), It.IsAny<Dictionary<string, string>?>()))
                .ReturnsAsync(reasoningOutput);

            // Act
            var result = await _adapter.DetermineOutcomeAsync(request);

            // Assert
            Assert.Equal(DecisionStatus.Completed, result.Status);
            Assert.Equal(DecisionOutcome.Success, result.Outcome);
            Assert.Equal("Yes", result.Results["Conclusion"]);
            _orchestratorMock.Verify(x => x.ReasonAsync(It.Is<string>(s => s.Contains("Should I deploy")), null, It.IsAny<Dictionary<string, string>>()), Times.Once);
        }
    }
}
