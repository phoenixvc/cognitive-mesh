using AgencyLayer.CognitiveSandwich.Engines;
using AgencyLayer.CognitiveSandwich.Models;
using AgencyLayer.CognitiveSandwich.Ports;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.AgencyLayer.CognitiveSandwich;

/// <summary>
/// Unit tests for <see cref="CognitiveSandwichEngine"/>, covering process creation,
/// phase transitions, step-back operations, cognitive debt monitoring, and audit trails.
/// </summary>
public class CognitiveSandwichEngineTests
{
    private readonly Mock<IPhaseConditionPort> _conditionPortMock;
    private readonly Mock<ICognitiveDebtPort> _cognitiveDebtPortMock;
    private readonly Mock<IAuditLoggingAdapter> _auditLoggingAdapterMock;
    private readonly Mock<ILogger<CognitiveSandwichEngine>> _loggerMock;
    private readonly CognitiveSandwichEngine _sut;

    public CognitiveSandwichEngineTests()
    {
        _conditionPortMock = new Mock<IPhaseConditionPort>();
        _cognitiveDebtPortMock = new Mock<ICognitiveDebtPort>();
        _auditLoggingAdapterMock = new Mock<IAuditLoggingAdapter>();
        _loggerMock = new Mock<ILogger<CognitiveSandwichEngine>>();

        // Defaults: conditions pass, no cognitive debt breach
        _conditionPortMock
            .Setup(x => x.CheckPreconditionsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConditionCheckResult { AllMet = true, Results = [] });

        _conditionPortMock
            .Setup(x => x.CheckPostconditionsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PhaseOutput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConditionCheckResult { AllMet = true, Results = [] });

        _cognitiveDebtPortMock
            .Setup(x => x.IsThresholdBreachedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _cognitiveDebtPortMock
            .Setup(x => x.AssessDebtAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CognitiveDebtAssessment { DebtScore = 10.0, IsBreached = false, Recommendations = [] });

        _auditLoggingAdapterMock
            .Setup(x => x.GetAuditEntriesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PhaseAuditEntry>());

        _sut = new CognitiveSandwichEngine(
            _conditionPortMock.Object,
            _cognitiveDebtPortMock.Object,
            _auditLoggingAdapterMock.Object,
            _loggerMock.Object);
    }

    // -----------------------------------------------------------------------
    // Constructor null-guard tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullConditionPort_ThrowsArgumentNullException()
    {
        var act = () => new CognitiveSandwichEngine(
            null!,
            _cognitiveDebtPortMock.Object,
            _auditLoggingAdapterMock.Object,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("conditionPort");
    }

    [Fact]
    public void Constructor_NullCognitiveDebtPort_ThrowsArgumentNullException()
    {
        var act = () => new CognitiveSandwichEngine(
            _conditionPortMock.Object,
            null!,
            _auditLoggingAdapterMock.Object,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("cognitiveDebtPort");
    }

    [Fact]
    public void Constructor_NullAuditLoggingAdapter_ThrowsArgumentNullException()
    {
        var act = () => new CognitiveSandwichEngine(
            _conditionPortMock.Object,
            _cognitiveDebtPortMock.Object,
            null!,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("auditLoggingAdapter");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new CognitiveSandwichEngine(
            _conditionPortMock.Object,
            _cognitiveDebtPortMock.Object,
            _auditLoggingAdapterMock.Object,
            null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // -----------------------------------------------------------------------
    // CreateProcessAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateProcessAsync_ValidConfig_ReturnsProcessWithCorrectProperties()
    {
        var config = CreateValidConfig();

        var process = await _sut.CreateProcessAsync(config);

        process.Should().NotBeNull();
        process.ProcessId.Should().NotBeNullOrEmpty();
        process.TenantId.Should().Be(config.TenantId);
        process.Name.Should().Be(config.Name);
        process.Phases.Should().HaveCount(3);
        process.State.Should().Be(SandwichProcessState.Created);
        process.CurrentPhaseIndex.Should().Be(0);
        process.MaxStepBacks.Should().Be(config.MaxStepBacks);
        process.StepBackCount.Should().Be(0);
        process.CognitiveDebtThreshold.Should().Be(config.CognitiveDebtThreshold);
    }

    [Fact]
    public async Task CreateProcessAsync_ValidConfig_LogsAuditEntry()
    {
        var config = CreateValidConfig();

        await _sut.CreateProcessAsync(config);

        _auditLoggingAdapterMock.Verify(
            x => x.LogAuditEntryAsync(
                It.Is<PhaseAuditEntry>(e => e.EventType == PhaseAuditEventType.ProcessCreated),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateProcessAsync_TooFewPhases_ThrowsArgumentException()
    {
        var config = new SandwichProcessConfig
        {
            TenantId = "tenant-1",
            Name = "Invalid Process",
            Phases = new List<PhaseDefinition>
            {
                new() { Name = "Phase 1", Order = 0 },
                new() { Name = "Phase 2", Order = 1 }
            }
        };

        var act = () => _sut.CreateProcessAsync(config);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*between 3 and 7 phases*");
    }

    [Fact]
    public async Task CreateProcessAsync_TooManyPhases_ThrowsArgumentException()
    {
        var phases = Enumerable.Range(0, 8)
            .Select(i => new PhaseDefinition { Name = $"Phase {i}", Order = i })
            .ToList();

        var config = new SandwichProcessConfig
        {
            TenantId = "tenant-1",
            Name = "Too Many Phases",
            Phases = phases
        };

        var act = () => _sut.CreateProcessAsync(config);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*between 3 and 7 phases*");
    }

    [Fact]
    public async Task CreateProcessAsync_EmptyTenantId_ThrowsArgumentException()
    {
        var config = CreateValidConfig();
        config.TenantId = "";

        var act = () => _sut.CreateProcessAsync(config);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*TenantId*");
    }

    [Fact]
    public async Task CreateProcessAsync_EmptyName_ThrowsArgumentException()
    {
        var config = CreateValidConfig();
        config.Name = "";

        var act = () => _sut.CreateProcessAsync(config);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*name*");
    }

    // -----------------------------------------------------------------------
    // GetProcessAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetProcessAsync_ExistingProcess_ReturnsProcess()
    {
        var config = CreateValidConfig();
        var created = await _sut.CreateProcessAsync(config);

        var retrieved = await _sut.GetProcessAsync(created.ProcessId);

        retrieved.Should().BeSameAs(created);
    }

    [Fact]
    public async Task GetProcessAsync_NonExistentProcess_ThrowsInvalidOperationException()
    {
        var act = () => _sut.GetProcessAsync("non-existent-id");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // -----------------------------------------------------------------------
    // TransitionToNextPhaseAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task TransitionToNextPhaseAsync_PreconditionsMet_TransitionsSuccessfully()
    {
        var config = CreateValidConfig();
        var process = await _sut.CreateProcessAsync(config);
        var context = CreateTransitionContext();

        var result = await _sut.TransitionToNextPhaseAsync(process.ProcessId, context);

        result.Success.Should().BeTrue();
        result.PhaseId.Should().Be(process.Phases[0].PhaseId);
        result.NextPhaseId.Should().Be(process.Phases[1].PhaseId);
        result.ValidationErrors.Should().BeEmpty();
        result.AuditEntry.Should().NotBeNull();

        var updatedProcess = await _sut.GetProcessAsync(process.ProcessId);
        updatedProcess.CurrentPhaseIndex.Should().Be(1);
        updatedProcess.State.Should().Be(SandwichProcessState.InProgress);
    }

    [Fact]
    public async Task TransitionToNextPhaseAsync_PostconditionsNotMet_BlocksTransition()
    {
        var config = CreateValidConfig();
        var process = await _sut.CreateProcessAsync(config);

        _conditionPortMock
            .Setup(x => x.CheckPostconditionsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PhaseOutput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConditionCheckResult
            {
                AllMet = false,
                Results = new List<ConditionEvaluation>
                {
                    new() { ConditionId = "cond-1", Met = false, Reason = "Quality score too low" }
                }
            });

        var context = CreateTransitionContext();
        context.PhaseOutput = new PhaseOutput { PhaseId = process.Phases[0].PhaseId };

        var result = await _sut.TransitionToNextPhaseAsync(process.ProcessId, context);

        result.Success.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Contains("Quality score too low"));

        var updatedProcess = await _sut.GetProcessAsync(process.ProcessId);
        updatedProcess.CurrentPhaseIndex.Should().Be(0, "phase index should not advance when postconditions fail");
    }

    [Fact]
    public async Task TransitionToNextPhaseAsync_PreconditionsNotMet_BlocksTransition()
    {
        var config = CreateValidConfig();
        var process = await _sut.CreateProcessAsync(config);

        _conditionPortMock
            .Setup(x => x.CheckPreconditionsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConditionCheckResult
            {
                AllMet = false,
                Results = new List<ConditionEvaluation>
                {
                    new() { ConditionId = "precond-1", Met = false, Reason = "Required data missing" }
                }
            });

        var context = CreateTransitionContext();

        var result = await _sut.TransitionToNextPhaseAsync(process.ProcessId, context);

        result.Success.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Contains("Required data missing"));

        var updatedProcess = await _sut.GetProcessAsync(process.ProcessId);
        updatedProcess.CurrentPhaseIndex.Should().Be(0, "phase index should not advance when preconditions fail");
    }

    [Fact]
    public async Task TransitionToNextPhaseAsync_CognitiveDebtBreached_BlocksTransition()
    {
        var config = CreateValidConfig();
        var process = await _sut.CreateProcessAsync(config);

        _cognitiveDebtPortMock
            .Setup(x => x.IsThresholdBreachedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _cognitiveDebtPortMock
            .Setup(x => x.AssessDebtAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CognitiveDebtAssessment
            {
                DebtScore = 85.0,
                IsBreached = true,
                Recommendations = new List<string> { "Add human review checkpoint" }
            });

        var context = CreateTransitionContext();

        var result = await _sut.TransitionToNextPhaseAsync(process.ProcessId, context);

        result.Success.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Contains("Cognitive debt threshold breached"));

        _auditLoggingAdapterMock.Verify(
            x => x.LogAuditEntryAsync(
                It.Is<PhaseAuditEntry>(e => e.EventType == PhaseAuditEventType.CognitiveDebtBreached),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TransitionToNextPhaseAsync_LastPhase_CompletesProcess()
    {
        var config = CreateValidConfig();
        var process = await _sut.CreateProcessAsync(config);

        // Transition through all phases
        for (int i = 0; i < config.Phases.Count - 1; i++)
        {
            var context = CreateTransitionContext();
            var result = await _sut.TransitionToNextPhaseAsync(process.ProcessId, context);
            result.Success.Should().BeTrue($"transition {i} should succeed");
        }

        // One more transition from the last phase
        var finalContext = CreateTransitionContext();
        var finalResult = await _sut.TransitionToNextPhaseAsync(process.ProcessId, finalContext);

        finalResult.Success.Should().BeTrue();
        finalResult.NextPhaseId.Should().BeNull("no more phases after the last one");

        var completedProcess = await _sut.GetProcessAsync(process.ProcessId);
        completedProcess.State.Should().Be(SandwichProcessState.Completed);

        _auditLoggingAdapterMock.Verify(
            x => x.LogAuditEntryAsync(
                It.Is<PhaseAuditEntry>(e => e.EventType == PhaseAuditEventType.ProcessCompleted),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TransitionToNextPhaseAsync_CompletedProcess_ReturnsBlocked()
    {
        var config = CreateValidConfig();
        var process = await _sut.CreateProcessAsync(config);

        // Complete all phases
        for (int i = 0; i < config.Phases.Count; i++)
        {
            await _sut.TransitionToNextPhaseAsync(process.ProcessId, CreateTransitionContext());
        }

        // Try to transition again
        var result = await _sut.TransitionToNextPhaseAsync(process.ProcessId, CreateTransitionContext());

        result.Success.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Process is already completed.");
    }

    [Fact]
    public async Task TransitionToNextPhaseAsync_PhaseRequiresHumanValidation_SetsAwaitingHumanReview()
    {
        var config = CreateValidConfig();
        // Make the second phase require human validation
        var mutablePhases = config.Phases.ToList();
        mutablePhases[1] = new PhaseDefinition
        {
            Name = "Human Review Phase",
            Description = "Requires human review",
            Order = 1,
            RequiresHumanValidation = true
        };
        config.Phases = mutablePhases;

        var process = await _sut.CreateProcessAsync(config);
        var context = CreateTransitionContext();

        var result = await _sut.TransitionToNextPhaseAsync(process.ProcessId, context);

        result.Success.Should().BeTrue();

        var updatedProcess = await _sut.GetProcessAsync(process.ProcessId);
        updatedProcess.State.Should().Be(SandwichProcessState.AwaitingHumanReview);
        updatedProcess.Phases[1].Status.Should().Be(PhaseStatus.AwaitingReview);

        _auditLoggingAdapterMock.Verify(
            x => x.LogAuditEntryAsync(
                It.Is<PhaseAuditEntry>(e => e.EventType == PhaseAuditEventType.HumanValidationRequested),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // StepBackAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task StepBackAsync_ValidStepBack_RollsBackPhases()
    {
        var config = CreateValidConfig();
        var process = await _sut.CreateProcessAsync(config);

        // Advance to phase 2 (index 1)
        await _sut.TransitionToNextPhaseAsync(process.ProcessId, CreateTransitionContext());

        var targetPhaseId = process.Phases[0].PhaseId;
        var reason = new StepBackReason
        {
            Reason = "Need to revisit initial analysis",
            InitiatedBy = "user-1"
        };

        var result = await _sut.StepBackAsync(process.ProcessId, targetPhaseId, reason);

        result.Success.Should().BeTrue();

        var updatedProcess = await _sut.GetProcessAsync(process.ProcessId);
        updatedProcess.CurrentPhaseIndex.Should().Be(0);
        updatedProcess.StepBackCount.Should().Be(1);
        updatedProcess.State.Should().Be(SandwichProcessState.SteppedBack);
        updatedProcess.Phases[0].Status.Should().Be(PhaseStatus.InProgress);
        updatedProcess.Phases[1].Status.Should().Be(PhaseStatus.RolledBack);

        _auditLoggingAdapterMock.Verify(
            x => x.LogAuditEntryAsync(
                It.Is<PhaseAuditEntry>(e => e.EventType == PhaseAuditEventType.StepBackPerformed),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StepBackAsync_ExceedsMaxStepBacks_BlocksStepBack()
    {
        var config = CreateValidConfig();
        config.MaxStepBacks = 1;
        var process = await _sut.CreateProcessAsync(config);

        // Advance, step back, advance again
        await _sut.TransitionToNextPhaseAsync(process.ProcessId, CreateTransitionContext());
        var firstStepBack = await _sut.StepBackAsync(
            process.ProcessId,
            process.Phases[0].PhaseId,
            new StepBackReason { Reason = "First step-back", InitiatedBy = "user-1" });
        firstStepBack.Success.Should().BeTrue();

        // Advance again
        await _sut.TransitionToNextPhaseAsync(process.ProcessId, CreateTransitionContext());

        // Try a second step-back (should be blocked since MaxStepBacks = 1)
        var secondStepBack = await _sut.StepBackAsync(
            process.ProcessId,
            process.Phases[0].PhaseId,
            new StepBackReason { Reason = "Second step-back", InitiatedBy = "user-1" });

        secondStepBack.Success.Should().BeFalse();
        secondStepBack.ValidationErrors.Should().Contain(e => e.Contains("Maximum step-back count"));
    }

    [Fact]
    public async Task StepBackAsync_TargetPhaseNotFound_ThrowsInvalidOperationException()
    {
        var config = CreateValidConfig();
        var process = await _sut.CreateProcessAsync(config);
        await _sut.TransitionToNextPhaseAsync(process.ProcessId, CreateTransitionContext());

        var act = () => _sut.StepBackAsync(
            process.ProcessId,
            "non-existent-phase-id",
            new StepBackReason { Reason = "Invalid target", InitiatedBy = "user-1" });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task StepBackAsync_TargetPhaseNotBefore_ThrowsInvalidOperationException()
    {
        var config = CreateValidConfig();
        var process = await _sut.CreateProcessAsync(config);

        // Try to step back to current phase (index 0) from index 0
        var act = () => _sut.StepBackAsync(
            process.ProcessId,
            process.Phases[0].PhaseId,
            new StepBackReason { Reason = "Can't step back to self", InitiatedBy = "user-1" });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*must be before the current phase*");
    }

    // -----------------------------------------------------------------------
    // GetAuditTrailAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAuditTrailAsync_ExistingProcess_ReturnsAuditEntries()
    {
        var config = CreateValidConfig();
        var process = await _sut.CreateProcessAsync(config);

        var expectedEntries = new List<PhaseAuditEntry>
        {
            new()
            {
                ProcessId = process.ProcessId,
                PhaseId = process.Phases[0].PhaseId,
                EventType = PhaseAuditEventType.ProcessCreated,
                UserId = "system",
                Details = "Process created"
            }
        };

        _auditLoggingAdapterMock
            .Setup(x => x.GetAuditEntriesAsync(process.ProcessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEntries);

        var auditTrail = await _sut.GetAuditTrailAsync(process.ProcessId);

        auditTrail.Should().HaveCount(1);
        auditTrail[0].EventType.Should().Be(PhaseAuditEventType.ProcessCreated);
    }

    [Fact]
    public async Task GetAuditTrailAsync_NonExistentProcess_ThrowsInvalidOperationException()
    {
        var act = () => _sut.GetAuditTrailAsync("non-existent-id");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task TransitionToNextPhaseAsync_RecordsMultipleAuditEntries()
    {
        var config = CreateValidConfig();
        var process = await _sut.CreateProcessAsync(config);

        await _sut.TransitionToNextPhaseAsync(process.ProcessId, CreateTransitionContext());

        // Should have: ProcessCreated + PhaseTransitionStarted + PhaseTransitionCompleted
        _auditLoggingAdapterMock.Verify(
            x => x.LogAuditEntryAsync(It.IsAny<PhaseAuditEntry>(), It.IsAny<CancellationToken>()),
            Times.AtLeast(3));
    }

    // -----------------------------------------------------------------------
    // Integration scenario: full lifecycle
    // -----------------------------------------------------------------------

    [Fact]
    public async Task FullLifecycle_CreateTransitionComplete_AllStatesTransitionCorrectly()
    {
        var config = CreateValidConfig();
        var process = await _sut.CreateProcessAsync(config);
        process.State.Should().Be(SandwichProcessState.Created);

        // Transition through all 3 phases
        var result1 = await _sut.TransitionToNextPhaseAsync(process.ProcessId, CreateTransitionContext());
        result1.Success.Should().BeTrue();
        process.State.Should().Be(SandwichProcessState.InProgress);

        var result2 = await _sut.TransitionToNextPhaseAsync(process.ProcessId, CreateTransitionContext());
        result2.Success.Should().BeTrue();
        process.State.Should().Be(SandwichProcessState.InProgress);

        var result3 = await _sut.TransitionToNextPhaseAsync(process.ProcessId, CreateTransitionContext());
        result3.Success.Should().BeTrue();
        process.State.Should().Be(SandwichProcessState.Completed);

        process.Phases[0].Status.Should().Be(PhaseStatus.Completed);
        process.Phases[1].Status.Should().Be(PhaseStatus.Completed);
        process.Phases[2].Status.Should().Be(PhaseStatus.Completed);
    }

    // -----------------------------------------------------------------------
    // Helper methods
    // -----------------------------------------------------------------------

    private static SandwichProcessConfig CreateValidConfig()
    {
        return new SandwichProcessConfig
        {
            TenantId = "tenant-1",
            Name = "Test Cognitive Sandwich",
            MaxStepBacks = 3,
            CognitiveDebtThreshold = 70.0,
            Phases = new List<PhaseDefinition>
            {
                new()
                {
                    Name = "Research & Analysis",
                    Description = "Gather and analyze relevant data",
                    Order = 0
                },
                new()
                {
                    Name = "Synthesis & Design",
                    Description = "Synthesize findings into actionable design",
                    Order = 1
                },
                new()
                {
                    Name = "Review & Validation",
                    Description = "Review and validate the final output",
                    Order = 2
                }
            }
        };
    }

    private static PhaseTransitionContext CreateTransitionContext()
    {
        return new PhaseTransitionContext
        {
            UserId = "user-1",
            TransitionReason = "Phase work completed"
        };
    }
}
