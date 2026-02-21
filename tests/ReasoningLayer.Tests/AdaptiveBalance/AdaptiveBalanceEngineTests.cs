using CognitiveMesh.ReasoningLayer.AdaptiveBalance.Engines;
using CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.ReasoningLayer.Tests.AdaptiveBalance;

/// <summary>
/// Tests for <see cref="AdaptiveBalanceEngine"/>.
/// </summary>
public class AdaptiveBalanceEngineTests
{
    private readonly Mock<ILogger<AdaptiveBalanceEngine>> _loggerMock;
    private readonly AdaptiveBalanceEngine _engine;

    public AdaptiveBalanceEngineTests()
    {
        _loggerMock = new Mock<ILogger<AdaptiveBalanceEngine>>();
        _engine = new AdaptiveBalanceEngine(_loggerMock.Object);
    }

    // ─── Constructor null guard tests ─────────────────────────────────

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AdaptiveBalanceEngine(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ─── GetBalance tests ─────────────────────────────────────────────

    [Fact]
    public async Task GetBalanceAsync_EmptyContext_ReturnsDefaultPositions()
    {
        // Arrange
        var context = new Dictionary<string, string>();

        // Act
        var result = await _engine.GetBalanceAsync(context, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Dimensions.Should().HaveCount(5);
        result.Dimensions.Should().OnlyContain(d => d.Value == AdaptiveBalanceEngine.DefaultPosition);
        result.OverallConfidence.Should().Be(0.3);
    }

    [Fact]
    public async Task GetBalanceAsync_NullContext_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _engine.GetBalanceAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetBalanceAsync_WithThreatLevel_AdjustsRiskDimension()
    {
        // Arrange
        var context = new Dictionary<string, string> { ["threat_level"] = "high" };

        // Act
        var result = await _engine.GetBalanceAsync(context, CancellationToken.None);

        // Assert
        var riskDimension = result.Dimensions.First(d => d.Dimension == SpectrumDimension.Risk);
        riskDimension.Value.Should().Be(0.2);
        riskDimension.Rationale.Should().Contain("threat level");
    }

    [Fact]
    public async Task GetBalanceAsync_WithRevenueTarget_AdjustsProfitDimension()
    {
        // Arrange
        var context = new Dictionary<string, string> { ["revenue_target"] = "500000" };

        // Act
        var result = await _engine.GetBalanceAsync(context, CancellationToken.None);

        // Assert
        var profitDimension = result.Dimensions.First(d => d.Dimension == SpectrumDimension.Profit);
        profitDimension.Value.Should().BeGreaterThan(AdaptiveBalanceEngine.DefaultPosition);
        profitDimension.Rationale.Should().Contain("revenue target");
    }

    [Fact]
    public async Task GetBalanceAsync_WithCustomerSentiment_AdjustsAgreeableness()
    {
        // Arrange
        var context = new Dictionary<string, string> { ["customer_sentiment"] = "negative" };

        // Act
        var result = await _engine.GetBalanceAsync(context, CancellationToken.None);

        // Assert
        var agreeablenessDimension = result.Dimensions.First(d => d.Dimension == SpectrumDimension.Agreeableness);
        agreeablenessDimension.Value.Should().Be(0.3);
    }

    [Fact]
    public async Task GetBalanceAsync_WithComplianceMode_AdjustsIdentityGrounding()
    {
        // Arrange
        var context = new Dictionary<string, string> { ["compliance_mode"] = "strict" };

        // Act
        var result = await _engine.GetBalanceAsync(context, CancellationToken.None);

        // Assert
        var identityDimension = result.Dimensions.First(d => d.Dimension == SpectrumDimension.IdentityGrounding);
        identityDimension.Value.Should().Be(0.9);
    }

    [Fact]
    public async Task GetBalanceAsync_WithDataVolume_AdjustsLearningRate()
    {
        // Arrange
        var context = new Dictionary<string, string> { ["data_volume"] = "high" };

        // Act
        var result = await _engine.GetBalanceAsync(context, CancellationToken.None);

        // Assert
        var learningDimension = result.Dimensions.First(d => d.Dimension == SpectrumDimension.LearningRate);
        learningDimension.Value.Should().Be(0.8);
    }

    [Fact]
    public async Task GetBalanceAsync_MultipleContextKeys_HigherConfidence()
    {
        // Arrange
        var context = new Dictionary<string, string>
        {
            ["threat_level"] = "low",
            ["revenue_target"] = "100000",
            ["customer_sentiment"] = "positive"
        };

        // Act
        var result = await _engine.GetBalanceAsync(context, CancellationToken.None);

        // Assert
        result.OverallConfidence.Should().Be(0.8);
    }

    [Fact]
    public async Task GetBalanceAsync_AllContextKeys_MaximumConfidence()
    {
        // Arrange
        var context = new Dictionary<string, string>
        {
            ["threat_level"] = "low",
            ["revenue_target"] = "100000",
            ["customer_sentiment"] = "positive",
            ["compliance_mode"] = "standard",
            ["data_volume"] = "medium"
        };

        // Act
        var result = await _engine.GetBalanceAsync(context, CancellationToken.None);

        // Assert
        result.OverallConfidence.Should().Be(0.95);
    }

    [Fact]
    public async Task GetBalanceAsync_DimensionsHaveValidBounds()
    {
        // Arrange
        var context = new Dictionary<string, string> { ["threat_level"] = "critical" };

        // Act
        var result = await _engine.GetBalanceAsync(context, CancellationToken.None);

        // Assert
        result.Dimensions.Should().OnlyContain(d =>
            d.LowerBound >= 0.0 &&
            d.UpperBound <= 1.0 &&
            d.LowerBound <= d.Value &&
            d.Value <= d.UpperBound);
    }

    [Fact]
    public async Task GetBalanceAsync_GeneratesUniqueRecommendationId()
    {
        // Arrange
        var context = new Dictionary<string, string>();

        // Act
        var result1 = await _engine.GetBalanceAsync(context, CancellationToken.None);
        var result2 = await _engine.GetBalanceAsync(context, CancellationToken.None);

        // Assert
        result1.RecommendationId.Should().NotBe(result2.RecommendationId);
    }

    // ─── ApplyOverride tests ──────────────────────────────────────────

    [Fact]
    public async Task ApplyOverrideAsync_ValidOverride_ReturnsUpdatedRecommendation()
    {
        // Arrange
        var balanceOverride = new BalanceOverride(
            OverrideId: Guid.NewGuid(),
            Dimension: SpectrumDimension.Risk,
            OriginalValue: 0.5,
            NewValue: 0.8,
            Rationale: "Manual risk adjustment",
            OverriddenBy: "admin-user",
            OverriddenAt: DateTimeOffset.UtcNow);

        // Act
        var result = await _engine.ApplyOverrideAsync(balanceOverride, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var riskDimension = result.Dimensions.First(d => d.Dimension == SpectrumDimension.Risk);
        riskDimension.Value.Should().Be(0.8);
        riskDimension.Rationale.Should().Contain("Manual override");
    }

    [Fact]
    public async Task ApplyOverrideAsync_ValueAboveOne_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var balanceOverride = new BalanceOverride(
            OverrideId: Guid.NewGuid(),
            Dimension: SpectrumDimension.Profit,
            OriginalValue: 0.5,
            NewValue: 1.5,
            Rationale: "Invalid override",
            OverriddenBy: "admin-user",
            OverriddenAt: DateTimeOffset.UtcNow);

        // Act
        var act = () => _engine.ApplyOverrideAsync(balanceOverride, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task ApplyOverrideAsync_ValueBelowZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var balanceOverride = new BalanceOverride(
            OverrideId: Guid.NewGuid(),
            Dimension: SpectrumDimension.Profit,
            OriginalValue: 0.5,
            NewValue: -0.1,
            Rationale: "Invalid override",
            OverriddenBy: "admin-user",
            OverriddenAt: DateTimeOffset.UtcNow);

        // Act
        var act = () => _engine.ApplyOverrideAsync(balanceOverride, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task ApplyOverrideAsync_NullOverride_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _engine.ApplyOverrideAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ApplyOverrideAsync_BoundaryValueZero_Succeeds()
    {
        // Arrange
        var balanceOverride = new BalanceOverride(
            OverrideId: Guid.NewGuid(),
            Dimension: SpectrumDimension.Agreeableness,
            OriginalValue: 0.5,
            NewValue: 0.0,
            Rationale: "Set to minimum",
            OverriddenBy: "admin",
            OverriddenAt: DateTimeOffset.UtcNow);

        // Act
        var result = await _engine.ApplyOverrideAsync(balanceOverride, CancellationToken.None);

        // Assert
        var dimension = result.Dimensions.First(d => d.Dimension == SpectrumDimension.Agreeableness);
        dimension.Value.Should().Be(0.0);
    }

    [Fact]
    public async Task ApplyOverrideAsync_BoundaryValueOne_Succeeds()
    {
        // Arrange
        var balanceOverride = new BalanceOverride(
            OverrideId: Guid.NewGuid(),
            Dimension: SpectrumDimension.LearningRate,
            OriginalValue: 0.5,
            NewValue: 1.0,
            Rationale: "Set to maximum",
            OverriddenBy: "admin",
            OverriddenAt: DateTimeOffset.UtcNow);

        // Act
        var result = await _engine.ApplyOverrideAsync(balanceOverride, CancellationToken.None);

        // Assert
        var dimension = result.Dimensions.First(d => d.Dimension == SpectrumDimension.LearningRate);
        dimension.Value.Should().Be(1.0);
    }

    // ─── GetSpectrumHistory tests ─────────────────────────────────────

    [Fact]
    public async Task GetSpectrumHistoryAsync_NoHistory_ReturnsEmptyList()
    {
        // Act
        var result = await _engine.GetSpectrumHistoryAsync(
            SpectrumDimension.Profit, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSpectrumHistoryAsync_AfterGetBalance_ReturnsPositions()
    {
        // Arrange
        var context = new Dictionary<string, string> { ["threat_level"] = "high" };
        await _engine.GetBalanceAsync(context, CancellationToken.None);

        // Act
        var result = await _engine.GetSpectrumHistoryAsync(
            SpectrumDimension.Risk, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Dimension.Should().Be(SpectrumDimension.Risk);
        result[0].Value.Should().Be(0.2);
    }

    // ─── Milestone Workflow: CreateWorkflow tests ─────────────────────

    [Fact]
    public async Task CreateWorkflowAsync_ValidPhases_ReturnsNotStartedWorkflow()
    {
        // Arrange
        var phases = CreateTestPhases(3);

        // Act
        var result = await _engine.CreateWorkflowAsync(phases, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(WorkflowStatus.NotStarted);
        result.Phases.Should().HaveCount(3);
        result.CurrentPhaseIndex.Should().Be(0);
        result.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task CreateWorkflowAsync_EmptyPhases_ThrowsArgumentException()
    {
        // Act
        var act = () => _engine.CreateWorkflowAsync([], CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*At least one phase*");
    }

    [Fact]
    public async Task CreateWorkflowAsync_NullPhases_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _engine.CreateWorkflowAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ─── Milestone Workflow: AdvancePhase tests ───────────────────────

    [Fact]
    public async Task AdvancePhaseAsync_FirstAdvance_SetsStatusToInProgress()
    {
        // Arrange
        var phases = CreateTestPhases(3);
        var workflow = await _engine.CreateWorkflowAsync(phases, CancellationToken.None);

        // Act
        var result = await _engine.AdvancePhaseAsync(workflow.WorkflowId, CancellationToken.None);

        // Assert
        result.Status.Should().Be(WorkflowStatus.InProgress);
        result.CurrentPhaseIndex.Should().Be(1);
    }

    [Fact]
    public async Task AdvancePhaseAsync_LastPhase_SetsStatusToCompleted()
    {
        // Arrange
        var phases = CreateTestPhases(2);
        var workflow = await _engine.CreateWorkflowAsync(phases, CancellationToken.None);

        // Advance to last phase
        await _engine.AdvancePhaseAsync(workflow.WorkflowId, CancellationToken.None);

        // Act - advance past last phase
        var result = await _engine.AdvancePhaseAsync(workflow.WorkflowId, CancellationToken.None);

        // Assert
        result.Status.Should().Be(WorkflowStatus.Completed);
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task AdvancePhaseAsync_CompletedWorkflow_ThrowsInvalidOperationException()
    {
        // Arrange
        var phases = CreateTestPhases(1);
        var workflow = await _engine.CreateWorkflowAsync(phases, CancellationToken.None);
        await _engine.AdvancePhaseAsync(workflow.WorkflowId, CancellationToken.None);

        // Act
        var act = () => _engine.AdvancePhaseAsync(workflow.WorkflowId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already completed*");
    }

    [Fact]
    public async Task AdvancePhaseAsync_NonExistentWorkflow_ThrowsInvalidOperationException()
    {
        // Act
        var act = () => _engine.AdvancePhaseAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // ─── Milestone Workflow: RollbackPhase tests ──────────────────────

    [Fact]
    public async Task RollbackPhaseAsync_WithRollbackTarget_RollsBackToTarget()
    {
        // Arrange
        var phases = new List<MilestonePhase>
        {
            new("phase-1", "Phase 1", [], [], false, null),
            new("phase-2", "Phase 2", [], [], false, "phase-1"),
            new("phase-3", "Phase 3", [], [], false, "phase-1")
        };
        var workflow = await _engine.CreateWorkflowAsync(phases, CancellationToken.None);
        await _engine.AdvancePhaseAsync(workflow.WorkflowId, CancellationToken.None); // -> phase-2
        await _engine.AdvancePhaseAsync(workflow.WorkflowId, CancellationToken.None); // -> phase-3

        // Act
        var result = await _engine.RollbackPhaseAsync(workflow.WorkflowId, CancellationToken.None);

        // Assert
        result.Status.Should().Be(WorkflowStatus.RolledBack);
        result.CurrentPhaseIndex.Should().Be(0); // rolled back to phase-1
    }

    [Fact]
    public async Task RollbackPhaseAsync_NoRollbackTarget_ThrowsInvalidOperationException()
    {
        // Arrange
        var phases = new List<MilestonePhase>
        {
            new("phase-1", "Phase 1", [], [], false, null),
            new("phase-2", "Phase 2", [], [], false, null)
        };
        var workflow = await _engine.CreateWorkflowAsync(phases, CancellationToken.None);
        await _engine.AdvancePhaseAsync(workflow.WorkflowId, CancellationToken.None);

        // Act
        var act = () => _engine.RollbackPhaseAsync(workflow.WorkflowId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*does not support rollback*");
    }

    [Fact]
    public async Task RollbackPhaseAsync_NotStartedWorkflow_ThrowsInvalidOperationException()
    {
        // Arrange
        var phases = CreateTestPhases(2);
        var workflow = await _engine.CreateWorkflowAsync(phases, CancellationToken.None);

        // Act
        var act = () => _engine.RollbackPhaseAsync(workflow.WorkflowId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*has not started*");
    }

    // ─── GetWorkflow tests ────────────────────────────────────────────

    [Fact]
    public async Task GetWorkflowAsync_ExistingWorkflow_ReturnsWorkflow()
    {
        // Arrange
        var phases = CreateTestPhases(2);
        var workflow = await _engine.CreateWorkflowAsync(phases, CancellationToken.None);

        // Act
        var result = await _engine.GetWorkflowAsync(workflow.WorkflowId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.WorkflowId.Should().Be(workflow.WorkflowId);
    }

    [Fact]
    public async Task GetWorkflowAsync_NonExistentWorkflow_ReturnsNull()
    {
        // Act
        var result = await _engine.GetWorkflowAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    // ─── Helpers ──────────────────────────────────────────────────────

    private static List<MilestonePhase> CreateTestPhases(int count)
    {
        var phases = new List<MilestonePhase>();
        for (var i = 0; i < count; i++)
        {
            phases.Add(new MilestonePhase(
                PhaseId: $"phase-{i + 1}",
                Name: $"Phase {i + 1}",
                PreConditions: [$"pre-condition-{i + 1}"],
                PostConditions: [$"post-condition-{i + 1}"],
                FeedbackEnabled: i % 2 == 0,
                RollbackToPhaseId: i > 0 ? $"phase-{i}" : null));
        }
        return phases;
    }
}
