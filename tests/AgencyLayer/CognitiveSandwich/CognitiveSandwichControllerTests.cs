using AgencyLayer.CognitiveSandwich.Controllers;
using AgencyLayer.CognitiveSandwich.Models;
using AgencyLayer.CognitiveSandwich.Ports;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.AgencyLayer.CognitiveSandwich;

/// <summary>
/// Unit tests for <see cref="CognitiveSandwichController"/>, covering all six REST endpoints,
/// constructor null guards, and error handling scenarios.
/// </summary>
public class CognitiveSandwichControllerTests
{
    private readonly Mock<IPhaseManagerPort> _phaseManagerMock;
    private readonly Mock<ICognitiveDebtPort> _cognitiveDebtPortMock;
    private readonly Mock<ILogger<CognitiveSandwichController>> _loggerMock;
    private readonly CognitiveSandwichController _sut;

    public CognitiveSandwichControllerTests()
    {
        _phaseManagerMock = new Mock<IPhaseManagerPort>();
        _cognitiveDebtPortMock = new Mock<ICognitiveDebtPort>();
        _loggerMock = new Mock<ILogger<CognitiveSandwichController>>();

        _sut = new CognitiveSandwichController(
            _phaseManagerMock.Object,
            _cognitiveDebtPortMock.Object,
            _loggerMock.Object);
    }

    // -----------------------------------------------------------------------
    // Constructor null-guard tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullPhaseManager_ThrowsArgumentNullException()
    {
        var act = () => new CognitiveSandwichController(
            null!,
            _cognitiveDebtPortMock.Object,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("phaseManager");
    }

    [Fact]
    public void Constructor_NullCognitiveDebtPort_ThrowsArgumentNullException()
    {
        var act = () => new CognitiveSandwichController(
            _phaseManagerMock.Object,
            null!,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("cognitiveDebtPort");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new CognitiveSandwichController(
            _phaseManagerMock.Object,
            _cognitiveDebtPortMock.Object,
            null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // -----------------------------------------------------------------------
    // CreateSandwichProcess tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateSandwichProcess_ValidConfig_ReturnsCreated()
    {
        var config = CreateValidConfig();
        var process = CreateSampleProcess();

        _phaseManagerMock
            .Setup(x => x.CreateProcessAsync(config, It.IsAny<CancellationToken>()))
            .ReturnsAsync(process);

        var result = await _sut.CreateSandwichProcess(config, CancellationToken.None);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdResult.Value.Should().Be(process);
        createdResult.ActionName.Should().Be(nameof(CognitiveSandwichController.GetSandwichProcess));
    }

    [Fact]
    public async Task CreateSandwichProcess_InvalidConfig_ReturnsBadRequest()
    {
        var config = new SandwichProcessConfig { TenantId = "", Name = "" };

        _phaseManagerMock
            .Setup(x => x.CreateProcessAsync(config, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("TenantId is required."));

        var result = await _sut.CreateSandwichProcess(config, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task CreateSandwichProcess_UnexpectedError_ReturnsInternalServerError()
    {
        var config = CreateValidConfig();

        _phaseManagerMock
            .Setup(x => x.CreateProcessAsync(config, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected failure"));

        var result = await _sut.CreateSandwichProcess(config, CancellationToken.None);

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // -----------------------------------------------------------------------
    // GetSandwichProcess tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetSandwichProcess_ExistingProcess_ReturnsOk()
    {
        var process = CreateSampleProcess();

        _phaseManagerMock
            .Setup(x => x.GetProcessAsync(process.ProcessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(process);

        var result = await _sut.GetSandwichProcess(process.ProcessId, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(process);
    }

    [Fact]
    public async Task GetSandwichProcess_NonExistentProcess_ReturnsNotFound()
    {
        _phaseManagerMock
            .Setup(x => x.GetProcessAsync("missing-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Process 'missing-id' not found."));

        var result = await _sut.GetSandwichProcess("missing-id", CancellationToken.None);

        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetSandwichProcess_UnexpectedError_ReturnsInternalServerError()
    {
        _phaseManagerMock
            .Setup(x => x.GetProcessAsync("id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database failure"));

        var result = await _sut.GetSandwichProcess("id", CancellationToken.None);

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // -----------------------------------------------------------------------
    // AdvancePhase tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AdvancePhase_ValidTransition_ReturnsOkWithResult()
    {
        var processId = "proc-1";
        var context = new PhaseTransitionContext
        {
            UserId = "user-1",
            TransitionReason = "Work complete"
        };
        var phaseResult = new PhaseResult
        {
            Success = true,
            PhaseId = "phase-1",
            NextPhaseId = "phase-2",
            ValidationErrors = []
        };

        _phaseManagerMock
            .Setup(x => x.TransitionToNextPhaseAsync(processId, context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(phaseResult);

        var result = await _sut.AdvancePhase(processId, context, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(phaseResult);
    }

    [Fact]
    public async Task AdvancePhase_ProcessNotFound_ReturnsNotFound()
    {
        var context = new PhaseTransitionContext
        {
            UserId = "user-1",
            TransitionReason = "Advance"
        };

        _phaseManagerMock
            .Setup(x => x.TransitionToNextPhaseAsync("missing-id", context, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Process 'missing-id' not found."));

        var result = await _sut.AdvancePhase("missing-id", context, CancellationToken.None);

        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task AdvancePhase_InvalidArguments_ReturnsBadRequest()
    {
        var context = new PhaseTransitionContext();

        _phaseManagerMock
            .Setup(x => x.TransitionToNextPhaseAsync("", context, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("processId is required"));

        var result = await _sut.AdvancePhase("", context, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task AdvancePhase_UnexpectedError_ReturnsInternalServerError()
    {
        var context = new PhaseTransitionContext
        {
            UserId = "user-1",
            TransitionReason = "Advance"
        };

        _phaseManagerMock
            .Setup(x => x.TransitionToNextPhaseAsync("id", context, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected failure"));

        var result = await _sut.AdvancePhase("id", context, CancellationToken.None);

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // -----------------------------------------------------------------------
    // StepBack tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task StepBack_ValidRequest_ReturnsOkWithResult()
    {
        var processId = "proc-1";
        var request = new StepBackRequest
        {
            TargetPhaseId = "phase-0",
            Reason = "Need to revisit",
            InitiatedBy = "user-1"
        };
        var phaseResult = new PhaseResult
        {
            Success = true,
            PhaseId = "phase-1",
            NextPhaseId = "phase-0",
            ValidationErrors = []
        };

        _phaseManagerMock
            .Setup(x => x.StepBackAsync(
                processId,
                request.TargetPhaseId,
                It.Is<StepBackReason>(r => r.Reason == request.Reason && r.InitiatedBy == request.InitiatedBy),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(phaseResult);

        var result = await _sut.StepBack(processId, request, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(phaseResult);
    }

    [Fact]
    public async Task StepBack_MissingTargetPhaseId_ReturnsBadRequest()
    {
        var request = new StepBackRequest
        {
            TargetPhaseId = "",
            Reason = "Test",
            InitiatedBy = "user-1"
        };

        var result = await _sut.StepBack("proc-1", request, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task StepBack_ProcessNotFound_ReturnsNotFound()
    {
        var request = new StepBackRequest
        {
            TargetPhaseId = "phase-0",
            Reason = "Test",
            InitiatedBy = "user-1"
        };

        _phaseManagerMock
            .Setup(x => x.StepBackAsync(
                "missing-id",
                request.TargetPhaseId,
                It.IsAny<StepBackReason>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Process 'missing-id' not found."));

        var result = await _sut.StepBack("missing-id", request, CancellationToken.None);

        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task StepBack_TargetPhaseNotBefore_ReturnsBadRequest()
    {
        var request = new StepBackRequest
        {
            TargetPhaseId = "phase-2",
            Reason = "Test",
            InitiatedBy = "user-1"
        };

        _phaseManagerMock
            .Setup(x => x.StepBackAsync(
                "proc-1",
                request.TargetPhaseId,
                It.IsAny<StepBackReason>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Step-back target phase must be before the current phase."));

        var result = await _sut.StepBack("proc-1", request, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task StepBack_UnexpectedError_ReturnsInternalServerError()
    {
        var request = new StepBackRequest
        {
            TargetPhaseId = "phase-0",
            Reason = "Test",
            InitiatedBy = "user-1"
        };

        _phaseManagerMock
            .Setup(x => x.StepBackAsync(
                "proc-1",
                request.TargetPhaseId,
                It.IsAny<StepBackReason>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected failure"));

        var result = await _sut.StepBack("proc-1", request, CancellationToken.None);

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // -----------------------------------------------------------------------
    // GetAuditTrail tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAuditTrail_ExistingProcess_ReturnsOkWithEntries()
    {
        var processId = "proc-1";
        var entries = new List<PhaseAuditEntry>
        {
            new()
            {
                ProcessId = processId,
                PhaseId = "phase-0",
                EventType = PhaseAuditEventType.ProcessCreated,
                UserId = "system",
                Details = "Process created"
            }
        };

        _phaseManagerMock
            .Setup(x => x.GetAuditTrailAsync(processId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var result = await _sut.GetAuditTrail(processId, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        var returnedEntries = okResult.Value.Should().BeAssignableTo<IReadOnlyList<PhaseAuditEntry>>().Subject;
        returnedEntries.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAuditTrail_ProcessNotFound_ReturnsNotFound()
    {
        _phaseManagerMock
            .Setup(x => x.GetAuditTrailAsync("missing-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Process 'missing-id' not found."));

        var result = await _sut.GetAuditTrail("missing-id", CancellationToken.None);

        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetAuditTrail_UnexpectedError_ReturnsInternalServerError()
    {
        _phaseManagerMock
            .Setup(x => x.GetAuditTrailAsync("id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database failure"));

        var result = await _sut.GetAuditTrail("id", CancellationToken.None);

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // -----------------------------------------------------------------------
    // GetCognitiveDebtAssessment tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetCognitiveDebtAssessment_ExistingProcess_ReturnsOk()
    {
        var process = CreateSampleProcess();
        var assessment = new CognitiveDebtAssessment
        {
            ProcessId = process.ProcessId,
            PhaseId = process.Phases[0].PhaseId,
            DebtScore = 25.0,
            IsBreached = false,
            Recommendations = []
        };

        _phaseManagerMock
            .Setup(x => x.GetProcessAsync(process.ProcessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(process);

        _cognitiveDebtPortMock
            .Setup(x => x.AssessDebtAsync(process.ProcessId, process.Phases[0].PhaseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assessment);

        var result = await _sut.GetCognitiveDebtAssessment(process.ProcessId, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(assessment);
    }

    [Fact]
    public async Task GetCognitiveDebtAssessment_ProcessNotFound_ReturnsNotFound()
    {
        _phaseManagerMock
            .Setup(x => x.GetProcessAsync("missing-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Process 'missing-id' not found."));

        var result = await _sut.GetCognitiveDebtAssessment("missing-id", CancellationToken.None);

        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetCognitiveDebtAssessment_UnexpectedError_ReturnsInternalServerError()
    {
        _phaseManagerMock
            .Setup(x => x.GetProcessAsync("id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database failure"));

        var result = await _sut.GetCognitiveDebtAssessment("id", CancellationToken.None);

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
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
                new() { Name = "Research", Description = "Research phase", Order = 0 },
                new() { Name = "Synthesis", Description = "Synthesis phase", Order = 1 },
                new() { Name = "Review", Description = "Review phase", Order = 2 }
            }
        };
    }

    private static SandwichProcess CreateSampleProcess()
    {
        return new SandwichProcess
        {
            ProcessId = "proc-1",
            TenantId = "tenant-1",
            Name = "Test Process",
            CurrentPhaseIndex = 0,
            Phases = new List<Phase>
            {
                new() { PhaseId = "phase-0", Name = "Research", Order = 0, Status = PhaseStatus.Pending },
                new() { PhaseId = "phase-1", Name = "Synthesis", Order = 1, Status = PhaseStatus.Pending },
                new() { PhaseId = "phase-2", Name = "Review", Order = 2, Status = PhaseStatus.Pending }
            },
            State = SandwichProcessState.Created,
            MaxStepBacks = 3,
            StepBackCount = 0,
            CognitiveDebtThreshold = 70.0
        };
    }
}
