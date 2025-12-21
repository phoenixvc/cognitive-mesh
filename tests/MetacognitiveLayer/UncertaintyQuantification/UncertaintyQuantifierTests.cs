using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using CognitiveMesh.MetacognitiveLayer.UncertaintyQuantification;
using CognitiveMesh.AgencyLayer.HumanCollaboration;
using CognitiveMesh.MetacognitiveLayer.ReasoningTransparency;

namespace CognitiveMesh.MetacognitiveLayer.UncertaintyQuantification.Tests
{
    /// <summary>
    /// Tests for the <see cref="UncertaintyQuantifier"/> class.
    /// </summary>
    public class UncertaintyQuantifierTests
    {
        private readonly Mock<ILogger<UncertaintyQuantifier>> _mockLogger;
        private readonly Mock<ICollaborationManager> _mockCollaborationManager;
        private readonly Mock<ITransparencyManager> _mockTransparencyManager;
        private readonly UncertaintyQuantifier _quantifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="UncertaintyQuantifierTests"/> class.
        /// </summary>
        public UncertaintyQuantifierTests()
        {
            _mockLogger = new Mock<ILogger<UncertaintyQuantifier>>();
            _mockCollaborationManager = new Mock<ICollaborationManager>();
            _mockTransparencyManager = new Mock<ITransparencyManager>();

            _quantifier = new UncertaintyQuantifier(
                _mockLogger.Object,
                _mockCollaborationManager.Object,
                _mockTransparencyManager.Object);
        }

        /// <summary>
        /// Verifies that RequestHumanIntervention strategy calls the collaboration manager.
        /// </summary>
        [Fact]
        public async Task ApplyUncertaintyMitigationStrategyAsync_RequestHumanIntervention_CallsCollaborationManager()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                { "sessionName", "Test Session" },
                { "description", "Test Description" }
            };

            // Act
            await _quantifier.ApplyUncertaintyMitigationStrategyAsync(
                UncertaintyQuantifier.StrategyRequestHumanIntervention,
                parameters);

            // Assert
            _mockCollaborationManager.Verify(m => m.CreateSessionAsync(
                It.Is<string>(s => s == "Test Session"),
                It.Is<string>(s => s == "Test Description"),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()), Times.Once);

            _mockTransparencyManager.Verify(m => m.LogReasoningStepAsync(
                It.IsAny<ReasoningStep>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that missing dependencies are handled gracefully.
        /// </summary>
        [Fact]
        public async Task ApplyUncertaintyMitigationStrategyAsync_RequestHumanIntervention_HandlesMissingDependency()
        {
            // Arrange
            var quantifierNoCollab = new UncertaintyQuantifier(_mockLogger.Object, null, _mockTransparencyManager.Object);

            // Act
            await quantifierNoCollab.ApplyUncertaintyMitigationStrategyAsync(
                UncertaintyQuantifier.StrategyRequestHumanIntervention,
                new Dictionary<string, object>());

            // Assert
            // Should not throw, just log warning
             // Cannot easily verify extension method LogWarning on Mock<ILogger> without setup,
             // but absence of exception is the key test here.
        }

        /// <summary>
        /// Verifies that FallbackToDefault strategy logs the action.
        /// </summary>
        [Fact]
        public async Task ApplyUncertaintyMitigationStrategyAsync_FallbackToDefault_LogsAction()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                { "defaultValue", 42 }
            };

            // Act
            await _quantifier.ApplyUncertaintyMitigationStrategyAsync(
                UncertaintyQuantifier.StrategyFallbackToDefault,
                parameters);

            // Assert
            // Verify transparency log called
            _mockTransparencyManager.Verify(m => m.LogReasoningStepAsync(
                It.Is<ReasoningStep>(s => s.Metadata["strategy"].ToString() == UncertaintyQuantifier.StrategyFallbackToDefault),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that ConservativeExecution strategy logs the action.
        /// </summary>
        [Fact]
        public async Task ApplyUncertaintyMitigationStrategyAsync_ConservativeExecution_LogsAction()
        {
            // Act
            await _quantifier.ApplyUncertaintyMitigationStrategyAsync(
                UncertaintyQuantifier.StrategyConservativeExecution);

            // Assert
            _mockTransparencyManager.Verify(m => m.LogReasoningStepAsync(
                It.Is<ReasoningStep>(s => s.Metadata["strategy"].ToString() == UncertaintyQuantifier.StrategyConservativeExecution),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that an unknown strategy throws an exception.
        /// </summary>
        [Fact]
        public async Task ApplyUncertaintyMitigationStrategyAsync_UnknownStrategy_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _quantifier.ApplyUncertaintyMitigationStrategyAsync("UnknownStrategy"));
        }
    }
}
