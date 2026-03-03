using CognitiveMesh.BusinessApplications.ImpactMetrics.Controllers;
using CognitiveMesh.BusinessApplications.ImpactMetrics.Models;
using CognitiveMesh.BusinessApplications.ImpactMetrics.Ports;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.BusinessApplications.ImpactMetrics;

/// <summary>
/// Unit tests for <see cref="ImpactMetricsController"/> covering all eight
/// endpoints, constructor null guards, and error handling scenarios.
/// </summary>
public class ImpactMetricsControllerTests
{
    private readonly Mock<IPsychologicalSafetyPort> _safetyPortMock;
    private readonly Mock<IMissionAlignmentPort> _alignmentPortMock;
    private readonly Mock<IAdoptionTelemetryPort> _telemetryPortMock;
    private readonly Mock<IImpactAssessmentPort> _assessmentPortMock;
    private readonly Mock<ILogger<ImpactMetricsController>> _loggerMock;
    private readonly ImpactMetricsController _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImpactMetricsControllerTests"/> class.
    /// </summary>
    public ImpactMetricsControllerTests()
    {
        _safetyPortMock = new Mock<IPsychologicalSafetyPort>();
        _alignmentPortMock = new Mock<IMissionAlignmentPort>();
        _telemetryPortMock = new Mock<IAdoptionTelemetryPort>();
        _assessmentPortMock = new Mock<IImpactAssessmentPort>();
        _loggerMock = new Mock<ILogger<ImpactMetricsController>>();

        _sut = new ImpactMetricsController(
            _safetyPortMock.Object,
            _alignmentPortMock.Object,
            _telemetryPortMock.Object,
            _assessmentPortMock.Object,
            _loggerMock.Object);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // -----------------------------------------------------------------------
    // Constructor null-guard tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullSafetyPort_ThrowsArgumentNullException()
    {
        var act = () => new ImpactMetricsController(
            null!, _alignmentPortMock.Object, _telemetryPortMock.Object,
            _assessmentPortMock.Object, _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("safetyPort");
    }

    [Fact]
    public void Constructor_NullAlignmentPort_ThrowsArgumentNullException()
    {
        var act = () => new ImpactMetricsController(
            _safetyPortMock.Object, null!, _telemetryPortMock.Object,
            _assessmentPortMock.Object, _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("alignmentPort");
    }

    [Fact]
    public void Constructor_NullTelemetryPort_ThrowsArgumentNullException()
    {
        var act = () => new ImpactMetricsController(
            _safetyPortMock.Object, _alignmentPortMock.Object, null!,
            _assessmentPortMock.Object, _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("telemetryPort");
    }

    [Fact]
    public void Constructor_NullAssessmentPort_ThrowsArgumentNullException()
    {
        var act = () => new ImpactMetricsController(
            _safetyPortMock.Object, _alignmentPortMock.Object, _telemetryPortMock.Object,
            null!, _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("assessmentPort");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new ImpactMetricsController(
            _safetyPortMock.Object, _alignmentPortMock.Object, _telemetryPortMock.Object,
            _assessmentPortMock.Object, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // -----------------------------------------------------------------------
    // CalculateSafetyScoreAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CalculateSafetyScoreAsync_ValidRequest_Returns200WithScore()
    {
        // Arrange
        var expectedScore = new PsychologicalSafetyScore(
            ScoreId: "score-1", TeamId: "team-1", TenantId: "tenant-1",
            OverallScore: 85.0, Dimensions: new Dictionary<SafetyDimension, double>(),
            SurveyResponseCount: 6, BehavioralSignalCount: 3,
            CalculatedAt: DateTimeOffset.UtcNow, ConfidenceLevel: ConfidenceLevel.Medium);

        _safetyPortMock
            .Setup(p => p.CalculateSafetyScoreAsync(
                "team-1", "tenant-1", It.IsAny<Dictionary<SafetyDimension, double>>(), default))
            .ReturnsAsync(expectedScore);

        var request = new CalculateSafetyScoreRequest
        {
            TenantId = "tenant-1",
            SurveyScores = new Dictionary<SafetyDimension, double>
            {
                { SafetyDimension.TrustInAI, 90 }
            }
        };

        // Act
        var result = await _sut.CalculateSafetyScoreAsync("team-1", request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(expectedScore);
    }

    [Fact]
    public async Task CalculateSafetyScoreAsync_EmptyTeamId_Returns400()
    {
        var request = new CalculateSafetyScoreRequest { TenantId = "tenant-1" };

        var result = await _sut.CalculateSafetyScoreAsync("", request);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task CalculateSafetyScoreAsync_PortThrows_Returns500()
    {
        _safetyPortMock
            .Setup(p => p.CalculateSafetyScoreAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<SafetyDimension, double>>(), default))
            .ThrowsAsync(new InvalidOperationException("Error"));

        var request = new CalculateSafetyScoreRequest
        {
            TenantId = "tenant-1",
            SurveyScores = new Dictionary<SafetyDimension, double>()
        };

        var result = await _sut.CalculateSafetyScoreAsync("team-1", request);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // -----------------------------------------------------------------------
    // GetHistoricalScoresAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetHistoricalScoresAsync_ValidParams_Returns200()
    {
        var scores = new List<PsychologicalSafetyScore>
        {
            new("s-1", "team-1", "tenant-1", 80, new Dictionary<SafetyDimension, double>(),
                6, 3, DateTimeOffset.UtcNow, ConfidenceLevel.Medium)
        };

        _safetyPortMock
            .Setup(p => p.GetHistoricalScoresAsync("team-1", "tenant-1", default))
            .ReturnsAsync(scores.AsReadOnly());

        var result = await _sut.GetHistoricalScoresAsync("team-1", "tenant-1");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetHistoricalScoresAsync_EmptyTenantId_Returns400()
    {
        var result = await _sut.GetHistoricalScoresAsync("team-1", "");

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    // -----------------------------------------------------------------------
    // AssessAlignmentAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AssessAlignmentAsync_ValidRequest_Returns200()
    {
        var alignment = new MissionAlignment(
            AlignmentId: "a-1", DecisionId: "dec-1", MissionStatementHash: "ABCD",
            AlignmentScore: 0.85, ValueMatches: new List<string> { "innovation" },
            Conflicts: new List<string>(), AssessedAt: DateTimeOffset.UtcNow);

        _alignmentPortMock
            .Setup(p => p.AssessAlignmentAsync("dec-1", "context", "mission", default))
            .ReturnsAsync(alignment);

        var request = new AssessAlignmentRequest
        {
            DecisionId = "dec-1",
            DecisionContext = "context",
            MissionStatement = "mission"
        };

        var result = await _sut.AssessAlignmentAsync(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(alignment);
    }

    [Fact]
    public async Task AssessAlignmentAsync_InvalidModelState_Returns400()
    {
        _sut.ModelState.AddModelError("DecisionId", "Required");

        var request = new AssessAlignmentRequest
        {
            DecisionId = "",
            DecisionContext = "context",
            MissionStatement = "mission"
        };

        var result = await _sut.AssessAlignmentAsync(request);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task AssessAlignmentAsync_PortThrows_Returns500()
    {
        _alignmentPortMock
            .Setup(p => p.AssessAlignmentAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ThrowsAsync(new Exception("Service error"));

        var request = new AssessAlignmentRequest
        {
            DecisionId = "dec-err",
            DecisionContext = "context",
            MissionStatement = "mission"
        };

        var result = await _sut.AssessAlignmentAsync(request);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // -----------------------------------------------------------------------
    // RecordTelemetryAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RecordTelemetryAsync_ValidRequest_Returns200()
    {
        _telemetryPortMock
            .Setup(p => p.RecordActionAsync(It.IsAny<AdoptionTelemetry>(), default))
            .Returns(Task.CompletedTask);

        var request = new RecordTelemetryRequest
        {
            UserId = "user-1",
            TenantId = "tenant-1",
            ToolId = "tool-1",
            Action = AdoptionAction.FeatureUse,
            DurationMs = 500,
            Context = "test"
        };

        var result = await _sut.RecordTelemetryAsync(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task RecordTelemetryAsync_InvalidModelState_Returns400()
    {
        _sut.ModelState.AddModelError("UserId", "Required");

        var request = new RecordTelemetryRequest();

        var result = await _sut.RecordTelemetryAsync(request);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    // -----------------------------------------------------------------------
    // GetUsageSummaryAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetUsageSummaryAsync_ValidTenantId_Returns200()
    {
        var events = new List<AdoptionTelemetry>
        {
            new("t-1", "u-1", "tenant-1", "tool-1", AdoptionAction.Login,
                DateTimeOffset.UtcNow, null, null)
        };

        _telemetryPortMock
            .Setup(p => p.GetUsageSummaryAsync("tenant-1", default))
            .ReturnsAsync(events.AsReadOnly());

        var result = await _sut.GetUsageSummaryAsync("tenant-1");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetUsageSummaryAsync_EmptyTenantId_Returns400()
    {
        var result = await _sut.GetUsageSummaryAsync("");

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    // -----------------------------------------------------------------------
    // GetResistancePatternsAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetResistancePatternsAsync_ValidTenantId_Returns200()
    {
        var indicators = new List<ResistanceIndicator>
        {
            new(ResistanceType.Override, 0.7, 5, DateTimeOffset.UtcNow, "High override rate")
        };

        _telemetryPortMock
            .Setup(p => p.DetectResistancePatternsAsync("tenant-1", default))
            .ReturnsAsync(indicators.AsReadOnly());

        var result = await _sut.GetResistancePatternsAsync("tenant-1");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetResistancePatternsAsync_EmptyTenantId_Returns400()
    {
        var result = await _sut.GetResistancePatternsAsync("");

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    // -----------------------------------------------------------------------
    // GenerateAssessmentAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GenerateAssessmentAsync_ValidRequest_Returns200()
    {
        var assessment = new ImpactAssessment(
            AssessmentId: "a-1", TenantId: "tenant-1",
            PeriodStart: DateTimeOffset.UtcNow.AddDays(-7), PeriodEnd: DateTimeOffset.UtcNow,
            ProductivityDelta: 0.1, QualityDelta: 0.2, TimeToDecisionDelta: -0.15,
            UserSatisfactionScore: 75, AdoptionRate: 0.6,
            ResistanceIndicators: new List<ResistanceIndicator>());

        _assessmentPortMock
            .Setup(p => p.GenerateAssessmentAsync(
                "tenant-1", It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default))
            .ReturnsAsync(assessment);

        var request = new GenerateAssessmentRequest
        {
            PeriodStart = DateTimeOffset.UtcNow.AddDays(-7),
            PeriodEnd = DateTimeOffset.UtcNow
        };

        var result = await _sut.GenerateAssessmentAsync("tenant-1", request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(assessment);
    }

    [Fact]
    public async Task GenerateAssessmentAsync_EmptyTenantId_Returns400()
    {
        var request = new GenerateAssessmentRequest
        {
            PeriodStart = DateTimeOffset.UtcNow.AddDays(-7),
            PeriodEnd = DateTimeOffset.UtcNow
        };

        var result = await _sut.GenerateAssessmentAsync("", request);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task GenerateAssessmentAsync_PortThrows_Returns500()
    {
        _assessmentPortMock
            .Setup(p => p.GenerateAssessmentAsync(
                It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default))
            .ThrowsAsync(new Exception("Error"));

        var request = new GenerateAssessmentRequest
        {
            PeriodStart = DateTimeOffset.UtcNow.AddDays(-7),
            PeriodEnd = DateTimeOffset.UtcNow
        };

        var result = await _sut.GenerateAssessmentAsync("tenant-err", request);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // -----------------------------------------------------------------------
    // GenerateReportAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GenerateReportAsync_ValidTenantId_Returns200()
    {
        var report = new ImpactReport(
            ReportId: "r-1", TenantId: "tenant-1",
            PeriodStart: DateTimeOffset.UtcNow.AddDays(-30), PeriodEnd: DateTimeOffset.UtcNow,
            SafetyScore: 82, AlignmentScore: 0.75, AdoptionRate: 0.65,
            OverallImpactScore: 78.5,
            Recommendations: new List<string> { "Continue monitoring" },
            GeneratedAt: DateTimeOffset.UtcNow);

        _assessmentPortMock
            .Setup(p => p.GenerateReportAsync(
                "tenant-1", It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default))
            .ReturnsAsync(report);

        var result = await _sut.GenerateReportAsync("tenant-1", null, null);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(report);
    }

    [Fact]
    public async Task GenerateReportAsync_EmptyTenantId_Returns400()
    {
        var result = await _sut.GenerateReportAsync("", null, null);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task GenerateReportAsync_PortThrows_Returns500()
    {
        _assessmentPortMock
            .Setup(p => p.GenerateReportAsync(
                It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default))
            .ThrowsAsync(new Exception("Report error"));

        var result = await _sut.GenerateReportAsync("tenant-err", null, null);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
