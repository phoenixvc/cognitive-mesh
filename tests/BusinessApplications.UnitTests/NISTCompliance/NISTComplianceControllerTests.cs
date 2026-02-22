using CognitiveMesh.BusinessApplications.NISTCompliance.Controllers;
using CognitiveMesh.BusinessApplications.NISTCompliance.Models;
using CognitiveMesh.BusinessApplications.NISTCompliance.Ports;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.BusinessApplications.NISTCompliance;

/// <summary>
/// Unit tests for <see cref="NISTComplianceController"/> covering all six
/// endpoints, constructor null guards, validation, and delegation.
/// </summary>
public class NISTComplianceControllerTests
{
    private readonly Mock<ILogger<NISTComplianceController>> _loggerMock;
    private readonly Mock<INISTComplianceServicePort> _servicePortMock;
    private readonly NISTComplianceController _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="NISTComplianceControllerTests"/> class.
    /// </summary>
    public NISTComplianceControllerTests()
    {
        _loggerMock = new Mock<ILogger<NISTComplianceController>>();
        _servicePortMock = new Mock<INISTComplianceServicePort>();

        _sut = new NISTComplianceController(
            _loggerMock.Object,
            _servicePortMock.Object);
    }

    // -----------------------------------------------------------------------
    // Constructor null-guard tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new NISTComplianceController(null!, _servicePortMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_NullServicePort_ThrowsArgumentNullException()
    {
        var act = () => new NISTComplianceController(_loggerMock.Object, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("servicePort");
    }

    // -----------------------------------------------------------------------
    // SubmitEvidenceAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SubmitEvidenceAsync_ValidRequest_ReturnsDelegatedResponse()
    {
        // Arrange
        var request = new NISTEvidenceRequest
        {
            StatementId = "GOV-1",
            ArtifactType = "Document",
            Content = "Policy document content",
            SubmittedBy = "user-1"
        };

        var expected = new NISTEvidenceResponse
        {
            EvidenceId = Guid.NewGuid(),
            StatementId = "GOV-1",
            SubmittedAt = DateTimeOffset.UtcNow,
            ReviewStatus = "Pending",
            Message = "Evidence submitted successfully."
        };

        _servicePortMock
            .Setup(s => s.SubmitEvidenceAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.SubmitEvidenceAsync(request, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        _servicePortMock.Verify(s => s.SubmitEvidenceAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitEvidenceAsync_NullRequest_ThrowsArgumentNullException()
    {
        var act = () => _sut.SubmitEvidenceAsync(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SubmitEvidenceAsync_MissingStatementId_ThrowsArgumentException()
    {
        var request = new NISTEvidenceRequest
        {
            StatementId = "",
            ArtifactType = "Document",
            Content = "Content",
            SubmittedBy = "user-1"
        };

        var act = () => _sut.SubmitEvidenceAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*StatementId*");
    }

    [Fact]
    public async Task SubmitEvidenceAsync_MissingArtifactType_ThrowsArgumentException()
    {
        var request = new NISTEvidenceRequest
        {
            StatementId = "GOV-1",
            ArtifactType = "",
            Content = "Content",
            SubmittedBy = "user-1"
        };

        var act = () => _sut.SubmitEvidenceAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*ArtifactType*");
    }

    [Fact]
    public async Task SubmitEvidenceAsync_MissingContent_ThrowsArgumentException()
    {
        var request = new NISTEvidenceRequest
        {
            StatementId = "GOV-1",
            ArtifactType = "Document",
            Content = "  ",
            SubmittedBy = "user-1"
        };

        var act = () => _sut.SubmitEvidenceAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Content*");
    }

    [Fact]
    public async Task SubmitEvidenceAsync_MissingSubmittedBy_ThrowsArgumentException()
    {
        var request = new NISTEvidenceRequest
        {
            StatementId = "GOV-1",
            ArtifactType = "Document",
            Content = "Content",
            SubmittedBy = ""
        };

        var act = () => _sut.SubmitEvidenceAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*SubmittedBy*");
    }

    // -----------------------------------------------------------------------
    // GetChecklistAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetChecklistAsync_ValidOrganizationId_ReturnsDelegatedResponse()
    {
        // Arrange
        var expected = new NISTChecklistResponse
        {
            OrganizationId = "org-1",
            TotalStatements = 12,
            CompletedStatements = 3,
            Pillars = new List<NISTChecklistPillar>()
        };

        _servicePortMock
            .Setup(s => s.GetChecklistAsync("org-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetChecklistAsync("org-1", CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        _servicePortMock.Verify(s => s.GetChecklistAsync("org-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetChecklistAsync_NullOrganizationId_ThrowsArgumentException()
    {
        var act = () => _sut.GetChecklistAsync(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetChecklistAsync_EmptyOrganizationId_ThrowsArgumentException()
    {
        var act = () => _sut.GetChecklistAsync("", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // -----------------------------------------------------------------------
    // GetScoreAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetScoreAsync_ValidOrganizationId_ReturnsDelegatedResponse()
    {
        // Arrange
        var expected = new NISTScoreResponse
        {
            OrganizationId = "org-1",
            OverallScore = 3.5,
            PillarScores = new List<NISTChecklistPillarScore>(),
            AssessedAt = DateTimeOffset.UtcNow
        };

        _servicePortMock
            .Setup(s => s.GetScoreAsync("org-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetScoreAsync("org-1", CancellationToken.None);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetScoreAsync_EmptyOrganizationId_ThrowsArgumentException()
    {
        var act = () => _sut.GetScoreAsync("  ", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // -----------------------------------------------------------------------
    // SubmitReviewAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SubmitReviewAsync_ValidRequest_ReturnsDelegatedResponse()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        var request = new NISTReviewRequest
        {
            EvidenceId = evidenceId,
            ReviewerId = "reviewer-1",
            Decision = "Approved",
            Notes = "Looks good."
        };

        var expected = new NISTReviewResponse
        {
            EvidenceId = evidenceId,
            NewStatus = "Approved",
            ReviewedAt = DateTimeOffset.UtcNow,
            Message = "Review completed."
        };

        _servicePortMock
            .Setup(s => s.SubmitReviewAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.SubmitReviewAsync(request, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task SubmitReviewAsync_NullRequest_ThrowsArgumentNullException()
    {
        var act = () => _sut.SubmitReviewAsync(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SubmitReviewAsync_EmptyEvidenceId_ThrowsArgumentException()
    {
        var request = new NISTReviewRequest
        {
            EvidenceId = Guid.Empty,
            ReviewerId = "reviewer-1",
            Decision = "Approved"
        };

        var act = () => _sut.SubmitReviewAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*EvidenceId*");
    }

    [Fact]
    public async Task SubmitReviewAsync_MissingReviewerId_ThrowsArgumentException()
    {
        var request = new NISTReviewRequest
        {
            EvidenceId = Guid.NewGuid(),
            ReviewerId = "",
            Decision = "Approved"
        };

        var act = () => _sut.SubmitReviewAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*ReviewerId*");
    }

    [Fact]
    public async Task SubmitReviewAsync_MissingDecision_ThrowsArgumentException()
    {
        var request = new NISTReviewRequest
        {
            EvidenceId = Guid.NewGuid(),
            ReviewerId = "reviewer-1",
            Decision = ""
        };

        var act = () => _sut.SubmitReviewAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Decision*");
    }

    // -----------------------------------------------------------------------
    // GetRoadmapAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetRoadmapAsync_ValidOrganizationId_ReturnsDelegatedResponse()
    {
        // Arrange
        var expected = new NISTRoadmapResponse
        {
            OrganizationId = "org-1",
            Gaps = new List<NISTGapItem>(),
            GeneratedAt = DateTimeOffset.UtcNow
        };

        _servicePortMock
            .Setup(s => s.GetRoadmapAsync("org-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetRoadmapAsync("org-1", CancellationToken.None);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetRoadmapAsync_EmptyOrganizationId_ThrowsArgumentException()
    {
        var act = () => _sut.GetRoadmapAsync("", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // -----------------------------------------------------------------------
    // GetAuditLogAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAuditLogAsync_ValidParams_ReturnsDelegatedResponse()
    {
        // Arrange
        var expected = new NISTAuditLogResponse
        {
            OrganizationId = "org-1",
            Entries = new List<NISTAuditEntry>(),
            TotalCount = 0
        };

        _servicePortMock
            .Setup(s => s.GetAuditLogAsync("org-1", 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetAuditLogAsync("org-1", 50, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetAuditLogAsync_EmptyOrganizationId_ThrowsArgumentException()
    {
        var act = () => _sut.GetAuditLogAsync("", 50, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetAuditLogAsync_ZeroMaxResults_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _sut.GetAuditLogAsync("org-1", 0, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>().WithParameterName("maxResults");
    }

    [Fact]
    public async Task GetAuditLogAsync_NegativeMaxResults_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _sut.GetAuditLogAsync("org-1", -1, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>().WithParameterName("maxResults");
    }
}
