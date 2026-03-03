using CognitiveMesh.BusinessApplications.Common.Models;
using CognitiveMesh.BusinessApplications.ConvenerServices.Ports;
using CognitiveMesh.BusinessApplications.ConvenerServices.Ports.Models;
using CognitiveMesh.BusinessApplications.ValueGeneration.Controllers;
using CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FoundationLayer.AuditLogging;

namespace CognitiveMesh.Tests.BusinessApplications.ValueGeneration;

/// <summary>
/// Unit tests for <see cref="ValueGenerationController"/>, covering value
/// diagnostics, organizational blindness detection, employability checks,
/// consent gating, manual review triggering, and audit logging.
/// </summary>
public class ValueGenerationControllerTests
{
    private readonly Mock<IValueDiagnosticPort> _valueDiagnosticPortMock;
    private readonly Mock<IOrgBlindnessDetectionPort> _orgBlindnessPortMock;
    private readonly Mock<IEmployabilityPort> _employabilityPortMock;
    private readonly Mock<IConsentPort> _consentPortMock;
    private readonly Mock<IManualAdjudicationPort> _manualAdjudicationPortMock;
    private readonly Mock<IAuditLoggingAdapter> _auditLoggerMock;
    private readonly Mock<ILogger<ValueGenerationController>> _loggerMock;
    private readonly ValueGenerationController _sut;

    public ValueGenerationControllerTests()
    {
        _valueDiagnosticPortMock = new Mock<IValueDiagnosticPort>();
        _orgBlindnessPortMock = new Mock<IOrgBlindnessDetectionPort>();
        _employabilityPortMock = new Mock<IEmployabilityPort>();
        _consentPortMock = new Mock<IConsentPort>();
        _manualAdjudicationPortMock = new Mock<IManualAdjudicationPort>();
        _auditLoggerMock = new Mock<IAuditLoggingAdapter>();
        _loggerMock = new Mock<ILogger<ValueGenerationController>>();

        _sut = new ValueGenerationController(
            _valueDiagnosticPortMock.Object,
            _orgBlindnessPortMock.Object,
            _employabilityPortMock.Object,
            _consentPortMock.Object,
            _manualAdjudicationPortMock.Object,
            _auditLoggerMock.Object,
            _loggerMock.Object);

        // Provide a default HttpContext so User.Identity works
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // -----------------------------------------------------------------------
    // Constructor null-guard tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullValueDiagnosticPort_ThrowsArgumentNullException()
    {
        var act = () => new ValueGenerationController(
            null!,
            _orgBlindnessPortMock.Object,
            _employabilityPortMock.Object,
            _consentPortMock.Object,
            _manualAdjudicationPortMock.Object,
            _auditLoggerMock.Object,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("valueDiagnosticPort");
    }

    [Fact]
    public void Constructor_NullOrgBlindnessPort_ThrowsArgumentNullException()
    {
        var act = () => new ValueGenerationController(
            _valueDiagnosticPortMock.Object,
            null!,
            _employabilityPortMock.Object,
            _consentPortMock.Object,
            _manualAdjudicationPortMock.Object,
            _auditLoggerMock.Object,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("orgBlindnessPort");
    }

    [Fact]
    public void Constructor_NullEmployabilityPort_ThrowsArgumentNullException()
    {
        var act = () => new ValueGenerationController(
            _valueDiagnosticPortMock.Object,
            _orgBlindnessPortMock.Object,
            null!,
            _consentPortMock.Object,
            _manualAdjudicationPortMock.Object,
            _auditLoggerMock.Object,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("employabilityPort");
    }

    [Fact]
    public void Constructor_NullConsentPort_ThrowsArgumentNullException()
    {
        var act = () => new ValueGenerationController(
            _valueDiagnosticPortMock.Object,
            _orgBlindnessPortMock.Object,
            _employabilityPortMock.Object,
            null!,
            _manualAdjudicationPortMock.Object,
            _auditLoggerMock.Object,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("consentPort");
    }

    [Fact]
    public void Constructor_NullAuditLogger_ThrowsArgumentNullException()
    {
        var act = () => new ValueGenerationController(
            _valueDiagnosticPortMock.Object,
            _orgBlindnessPortMock.Object,
            _employabilityPortMock.Object,
            _consentPortMock.Object,
            _manualAdjudicationPortMock.Object,
            null!,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("auditLogger");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new ValueGenerationController(
            _valueDiagnosticPortMock.Object,
            _orgBlindnessPortMock.Object,
            _employabilityPortMock.Object,
            _consentPortMock.Object,
            _manualAdjudicationPortMock.Object,
            _auditLoggerMock.Object,
            null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // -----------------------------------------------------------------------
    // RunValueDiagnosticAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RunValueDiagnosticAsync_ValidTeamRequest_Returns200WithResult()
    {
        // Arrange
        var request = new ValueDiagnosticApiRequest
        {
            TargetId = "team-42",
            TargetType = "Team",
            TenantId = "tenant-1"
        };

        var expectedResponse = new ValueDiagnosticResponse
        {
            TargetId = "team-42",
            ValueScore = 120.0,
            ValueProfile = "Connector",
            Strengths = new List<string> { "High Impact Delivery" },
            DevelopmentOpportunities = new List<string> { "Increase cross-team collaboration" },
            ModelVersion = "ValueDiagnostic-v1.0",
            CorrelationId = "test-correlation"
        };

        _valueDiagnosticPortMock
            .Setup(p => p.RunValueDiagnosticAsync(It.IsAny<ValueDiagnosticRequest>()))
            .ReturnsAsync(expectedResponse);

        _auditLoggerMock
            .Setup(a => a.LogEventAsync(It.IsAny<AuditEvent>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.RunValueDiagnosticAsync(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task RunValueDiagnosticAsync_UserTargetWithConsent_Returns200()
    {
        // Arrange
        var request = new ValueDiagnosticApiRequest
        {
            TargetId = "user-1",
            TargetType = "User",
            TenantId = "tenant-1"
        };

        _consentPortMock
            .Setup(c => c.ValidateConsentAsync(It.IsAny<ValidateConsentRequest>()))
            .ReturnsAsync(new ValidateConsentResponse { HasConsent = true, ConsentRecordId = "consent-1" });

        var expectedResponse = new ValueDiagnosticResponse
        {
            TargetId = "user-1",
            ValueScore = 200.0,
            ValueProfile = "Innovator",
            ModelVersion = "ValueDiagnostic-v1.0"
        };

        _valueDiagnosticPortMock
            .Setup(p => p.RunValueDiagnosticAsync(It.IsAny<ValueDiagnosticRequest>()))
            .ReturnsAsync(expectedResponse);

        _auditLoggerMock
            .Setup(a => a.LogEventAsync(It.IsAny<AuditEvent>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.RunValueDiagnosticAsync(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task RunValueDiagnosticAsync_UserTargetWithoutConsent_Returns403()
    {
        // Arrange
        var request = new ValueDiagnosticApiRequest
        {
            TargetId = "user-1",
            TargetType = "User",
            TenantId = "tenant-1"
        };

        _consentPortMock
            .Setup(c => c.ValidateConsentAsync(It.IsAny<ValidateConsentRequest>()))
            .ReturnsAsync(new ValidateConsentResponse { HasConsent = false });

        _auditLoggerMock
            .Setup(a => a.LogEventAsync(It.IsAny<AuditEvent>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.RunValueDiagnosticAsync(request);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task RunValueDiagnosticAsync_InvalidModelState_Returns400()
    {
        // Arrange
        var request = new ValueDiagnosticApiRequest
        {
            TargetId = "user-1",
            TargetType = "User",
            TenantId = "tenant-1"
        };

        _sut.ModelState.AddModelError("TargetId", "Required");

        // Act
        var result = await _sut.RunValueDiagnosticAsync(request);

        // Assert
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task RunValueDiagnosticAsync_LowScore_TriggersReview()
    {
        // Arrange
        var request = new ValueDiagnosticApiRequest
        {
            TargetId = "team-low",
            TargetType = "Team",
            TenantId = "tenant-1"
        };

        var response = new ValueDiagnosticResponse
        {
            TargetId = "team-low",
            ValueScore = 30.0,
            ValueProfile = "Contributor",
            ModelVersion = "ValueDiagnostic-v1.0"
        };

        _valueDiagnosticPortMock
            .Setup(p => p.RunValueDiagnosticAsync(It.IsAny<ValueDiagnosticRequest>()))
            .ReturnsAsync(response);

        _auditLoggerMock
            .Setup(a => a.LogEventAsync(It.IsAny<AuditEvent>()))
            .ReturnsAsync(true);

        _manualAdjudicationPortMock
            .Setup(m => m.SubmitForReviewAsync(It.IsAny<ManualReviewRequest>()))
            .ReturnsAsync(new ManualReviewResponse { ReviewId = "rev-1" });

        // Act
        var result = await _sut.RunValueDiagnosticAsync(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _manualAdjudicationPortMock.Verify(
            m => m.SubmitForReviewAsync(It.Is<ManualReviewRequest>(
                r => r.ReviewType == ReviewTypes.ValueDiagnosticAlert)),
            Times.Once);
    }

    [Fact]
    public async Task RunValueDiagnosticAsync_PortThrows_Returns500()
    {
        // Arrange
        var request = new ValueDiagnosticApiRequest
        {
            TargetId = "team-err",
            TargetType = "Team",
            TenantId = "tenant-1"
        };

        _valueDiagnosticPortMock
            .Setup(p => p.RunValueDiagnosticAsync(It.IsAny<ValueDiagnosticRequest>()))
            .ThrowsAsync(new InvalidOperationException("Data not found"));

        // Act
        var result = await _sut.RunValueDiagnosticAsync(request);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task RunValueDiagnosticAsync_Success_LogsAuditEvent()
    {
        // Arrange
        var request = new ValueDiagnosticApiRequest
        {
            TargetId = "team-audit",
            TargetType = "Team",
            TenantId = "tenant-1"
        };

        _valueDiagnosticPortMock
            .Setup(p => p.RunValueDiagnosticAsync(It.IsAny<ValueDiagnosticRequest>()))
            .ReturnsAsync(new ValueDiagnosticResponse
            {
                TargetId = "team-audit",
                ValueScore = 100.0,
                ValueProfile = "Connector",
                ModelVersion = "ValueDiagnostic-v1.0"
            });

        _auditLoggerMock
            .Setup(a => a.LogEventAsync(It.IsAny<AuditEvent>()))
            .ReturnsAsync(true);

        // Act
        await _sut.RunValueDiagnosticAsync(request);

        // Assert
        _auditLoggerMock.Verify(
            a => a.LogEventAsync(It.Is<AuditEvent>(
                e => e.EventType == "ValueDiagnostic.Completed")),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // DetectOrganizationalBlindnessAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DetectOrganizationalBlindnessAsync_ValidRequest_Returns200()
    {
        // Arrange
        var request = new OrgBlindnessDetectionApiRequest
        {
            OrganizationId = "org-1",
            TenantId = "tenant-1",
            DepartmentFilters = new[] { "Engineering" }
        };

        var expectedResponse = new OrgBlindnessDetectionResponse
        {
            OrganizationId = "org-1",
            BlindnessRiskScore = 0.25,
            IdentifiedBlindSpots = new List<string> { "No significant value blindness detected." },
            ModelVersion = "OrgBlindness-v1.1"
        };

        _orgBlindnessPortMock
            .Setup(p => p.DetectOrganizationalBlindnessAsync(It.IsAny<OrgBlindnessDetectionRequest>()))
            .ReturnsAsync(expectedResponse);

        _auditLoggerMock
            .Setup(a => a.LogEventAsync(It.IsAny<AuditEvent>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DetectOrganizationalBlindnessAsync(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task DetectOrganizationalBlindnessAsync_InvalidModelState_Returns400()
    {
        // Arrange
        var request = new OrgBlindnessDetectionApiRequest
        {
            OrganizationId = "org-1",
            TenantId = "tenant-1"
        };

        _sut.ModelState.AddModelError("OrganizationId", "Required");

        // Act
        var result = await _sut.DetectOrganizationalBlindnessAsync(request);

        // Assert
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task DetectOrganizationalBlindnessAsync_HighRisk_TriggersReview()
    {
        // Arrange
        var request = new OrgBlindnessDetectionApiRequest
        {
            OrganizationId = "org-risky",
            TenantId = "tenant-1"
        };

        var response = new OrgBlindnessDetectionResponse
        {
            OrganizationId = "org-risky",
            BlindnessRiskScore = 0.85,
            IdentifiedBlindSpots = new List<string> { "Overvaluing 'Marketing' compared to its impact." },
            ModelVersion = "OrgBlindness-v1.1"
        };

        _orgBlindnessPortMock
            .Setup(p => p.DetectOrganizationalBlindnessAsync(It.IsAny<OrgBlindnessDetectionRequest>()))
            .ReturnsAsync(response);

        _auditLoggerMock
            .Setup(a => a.LogEventAsync(It.IsAny<AuditEvent>()))
            .ReturnsAsync(true);

        _manualAdjudicationPortMock
            .Setup(m => m.SubmitForReviewAsync(It.IsAny<ManualReviewRequest>()))
            .ReturnsAsync(new ManualReviewResponse { ReviewId = "rev-org-1" });

        // Act
        var result = await _sut.DetectOrganizationalBlindnessAsync(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _manualAdjudicationPortMock.Verify(
            m => m.SubmitForReviewAsync(It.Is<ManualReviewRequest>(
                r => r.ReviewType == ReviewTypes.OrgBlindnessAlert)),
            Times.Once);
    }

    [Fact]
    public async Task DetectOrganizationalBlindnessAsync_PortThrows_Returns500()
    {
        // Arrange
        var request = new OrgBlindnessDetectionApiRequest
        {
            OrganizationId = "org-err",
            TenantId = "tenant-1"
        };

        _orgBlindnessPortMock
            .Setup(p => p.DetectOrganizationalBlindnessAsync(It.IsAny<OrgBlindnessDetectionRequest>()))
            .ThrowsAsync(new InvalidOperationException("No data"));

        // Act
        var result = await _sut.DetectOrganizationalBlindnessAsync(request);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task DetectOrganizationalBlindnessAsync_Success_LogsAuditEvent()
    {
        // Arrange
        var request = new OrgBlindnessDetectionApiRequest
        {
            OrganizationId = "org-audit",
            TenantId = "tenant-1"
        };

        _orgBlindnessPortMock
            .Setup(p => p.DetectOrganizationalBlindnessAsync(It.IsAny<OrgBlindnessDetectionRequest>()))
            .ReturnsAsync(new OrgBlindnessDetectionResponse
            {
                OrganizationId = "org-audit",
                BlindnessRiskScore = 0.1,
                ModelVersion = "OrgBlindness-v1.1"
            });

        _auditLoggerMock
            .Setup(a => a.LogEventAsync(It.IsAny<AuditEvent>()))
            .ReturnsAsync(true);

        // Act
        await _sut.DetectOrganizationalBlindnessAsync(request);

        // Assert
        _auditLoggerMock.Verify(
            a => a.LogEventAsync(It.Is<AuditEvent>(
                e => e.EventType == "OrgBlindness.Completed")),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // CheckEmployabilityAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CheckEmployabilityAsync_ValidRequestLowRisk_Returns200()
    {
        // Arrange
        var request = new EmployabilityCheckApiRequest
        {
            UserId = "user-safe",
            TenantId = "tenant-1"
        };

        _consentPortMock
            .Setup(c => c.ValidateConsentAsync(It.IsAny<ValidateConsentRequest>()))
            .ReturnsAsync(new ValidateConsentResponse { HasConsent = true, ConsentRecordId = "consent-emp-1" });

        var response = new EmployabilityCheckResponse
        {
            UserId = "user-safe",
            EmployabilityRiskScore = 0.15,
            RiskLevel = "Low",
            RiskFactors = new List<string>(),
            RecommendedActions = new List<string> { "Continue developing your existing skills." },
            ModelVersion = "EmployabilityPredictor-v1.0"
        };

        _employabilityPortMock
            .Setup(p => p.CheckEmployabilityAsync(It.IsAny<EmployabilityCheckRequest>()))
            .ReturnsAsync(response);

        _auditLoggerMock
            .Setup(a => a.LogEventAsync(It.IsAny<AuditEvent>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.CheckEmployabilityAsync(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task CheckEmployabilityAsync_MissingConsent_Returns403()
    {
        // Arrange
        var request = new EmployabilityCheckApiRequest
        {
            UserId = "user-no-consent",
            TenantId = "tenant-1"
        };

        _consentPortMock
            .Setup(c => c.ValidateConsentAsync(It.IsAny<ValidateConsentRequest>()))
            .ReturnsAsync(new ValidateConsentResponse { HasConsent = false });

        _auditLoggerMock
            .Setup(a => a.LogEventAsync(It.IsAny<AuditEvent>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.CheckEmployabilityAsync(request);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task CheckEmployabilityAsync_HighRisk_Returns202WithReviewId()
    {
        // Arrange
        var request = new EmployabilityCheckApiRequest
        {
            UserId = "user-risky",
            TenantId = "tenant-1"
        };

        _consentPortMock
            .Setup(c => c.ValidateConsentAsync(It.IsAny<ValidateConsentRequest>()))
            .ReturnsAsync(new ValidateConsentResponse { HasConsent = true, ConsentRecordId = "consent-2" });

        var response = new EmployabilityCheckResponse
        {
            UserId = "user-risky",
            EmployabilityRiskScore = 0.85,
            RiskLevel = "High",
            RiskFactors = new List<string> { "Skill gap identified." },
            RecommendedActions = new List<string> { "Explore training." },
            ModelVersion = "EmployabilityPredictor-v1.0"
        };

        _employabilityPortMock
            .Setup(p => p.CheckEmployabilityAsync(It.IsAny<EmployabilityCheckRequest>()))
            .ReturnsAsync(response);

        _auditLoggerMock
            .Setup(a => a.LogEventAsync(It.IsAny<AuditEvent>()))
            .ReturnsAsync(true);

        _manualAdjudicationPortMock
            .Setup(m => m.SubmitForReviewAsync(It.IsAny<ManualReviewRequest>()))
            .ReturnsAsync(new ManualReviewResponse
            {
                ReviewId = "rev-emp-1",
                Status = ReviewStatus.Pending,
                EstimatedCompletionTime = DateTimeOffset.UtcNow.AddHours(24)
            });

        // Act
        var result = await _sut.CheckEmployabilityAsync(request);

        // Assert
        var acceptedResult = result.Result.Should().BeOfType<AcceptedResult>().Subject;
        acceptedResult.StatusCode.Should().Be(StatusCodes.Status202Accepted);
    }

    [Fact]
    public async Task CheckEmployabilityAsync_InvalidModelState_Returns400()
    {
        // Arrange
        var request = new EmployabilityCheckApiRequest
        {
            UserId = "user-1",
            TenantId = "tenant-1"
        };

        _sut.ModelState.AddModelError("UserId", "Required");

        // Act
        var result = await _sut.CheckEmployabilityAsync(request);

        // Assert
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task CheckEmployabilityAsync_MissingConsent_LogsConsentAudit()
    {
        // Arrange
        var request = new EmployabilityCheckApiRequest
        {
            UserId = "user-consent-audit",
            TenantId = "tenant-1"
        };

        _consentPortMock
            .Setup(c => c.ValidateConsentAsync(It.IsAny<ValidateConsentRequest>()))
            .ReturnsAsync(new ValidateConsentResponse { HasConsent = false });

        _auditLoggerMock
            .Setup(a => a.LogEventAsync(It.IsAny<AuditEvent>()))
            .ReturnsAsync(true);

        // Act
        await _sut.CheckEmployabilityAsync(request);

        // Assert
        _auditLoggerMock.Verify(
            a => a.LogEventAsync(It.Is<AuditEvent>(
                e => e.EventType == "Employability.ConsentMissing")),
            Times.Once);
    }

    [Fact]
    public async Task CheckEmployabilityAsync_LowRisk_LogsCompletedAudit()
    {
        // Arrange
        var request = new EmployabilityCheckApiRequest
        {
            UserId = "user-audit-ok",
            TenantId = "tenant-1"
        };

        _consentPortMock
            .Setup(c => c.ValidateConsentAsync(It.IsAny<ValidateConsentRequest>()))
            .ReturnsAsync(new ValidateConsentResponse { HasConsent = true, ConsentRecordId = "c-3" });

        _employabilityPortMock
            .Setup(p => p.CheckEmployabilityAsync(It.IsAny<EmployabilityCheckRequest>()))
            .ReturnsAsync(new EmployabilityCheckResponse
            {
                UserId = "user-audit-ok",
                EmployabilityRiskScore = 0.1,
                RiskLevel = "Low",
                ModelVersion = "EmployabilityPredictor-v1.0"
            });

        _auditLoggerMock
            .Setup(a => a.LogEventAsync(It.IsAny<AuditEvent>()))
            .ReturnsAsync(true);

        // Act
        await _sut.CheckEmployabilityAsync(request);

        // Assert
        _auditLoggerMock.Verify(
            a => a.LogEventAsync(It.Is<AuditEvent>(
                e => e.EventType == "Employability.Completed")),
            Times.Once);
    }

    [Fact]
    public async Task CheckEmployabilityAsync_PortThrows_Returns500()
    {
        // Arrange
        var request = new EmployabilityCheckApiRequest
        {
            UserId = "user-err",
            TenantId = "tenant-1"
        };

        _consentPortMock
            .Setup(c => c.ValidateConsentAsync(It.IsAny<ValidateConsentRequest>()))
            .ReturnsAsync(new ValidateConsentResponse { HasConsent = true, ConsentRecordId = "c-4" });

        _employabilityPortMock
            .Setup(p => p.CheckEmployabilityAsync(It.IsAny<EmployabilityCheckRequest>()))
            .ThrowsAsync(new InvalidOperationException("No data"));

        // Act
        var result = await _sut.CheckEmployabilityAsync(request);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // -----------------------------------------------------------------------
    // GetEmployabilityReviewStatusAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetEmployabilityReviewStatusAsync_ValidReviewId_Returns200()
    {
        // Arrange
        var reviewRecord = new ReviewRecord
        {
            ReviewId = "rev-100",
            TenantId = "tenant-1",
            Status = ReviewStatus.Pending,
            ReviewType = ReviewTypes.EmployabilityHighRisk
        };

        _manualAdjudicationPortMock
            .Setup(m => m.GetReviewStatusAsync("rev-100", "tenant-1"))
            .ReturnsAsync(reviewRecord);

        // Act
        var result = await _sut.GetEmployabilityReviewStatusAsync("rev-100", "tenant-1");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(reviewRecord);
    }

    [Fact]
    public async Task GetEmployabilityReviewStatusAsync_ReviewNotFound_Returns404()
    {
        // Arrange
        _manualAdjudicationPortMock
            .Setup(m => m.GetReviewStatusAsync("rev-missing", "tenant-1"))
            .ReturnsAsync((ReviewRecord?)null);

        // Act
        var result = await _sut.GetEmployabilityReviewStatusAsync("rev-missing", "tenant-1");

        // Assert
        var notFound = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFound.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetEmployabilityReviewStatusAsync_EmptyReviewId_Returns400()
    {
        // Act
        var result = await _sut.GetEmployabilityReviewStatusAsync("", "tenant-1");

        // Assert
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task GetEmployabilityReviewStatusAsync_EmptyTenantId_Returns400()
    {
        // Act
        var result = await _sut.GetEmployabilityReviewStatusAsync("rev-1", "");

        // Assert
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task GetEmployabilityReviewStatusAsync_PortThrows_Returns500()
    {
        // Arrange
        _manualAdjudicationPortMock
            .Setup(m => m.GetReviewStatusAsync("rev-err", "tenant-1"))
            .ThrowsAsync(new Exception("Service unavailable"));

        // Act
        var result = await _sut.GetEmployabilityReviewStatusAsync("rev-err", "tenant-1");

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
