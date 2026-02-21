using CognitiveMesh.ReasoningLayer.ValueGeneration.Engines;
using CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.ReasoningLayer.ValueGeneration;

/// <summary>
/// Unit tests for <see cref="OrganizationalValueBlindnessEngine"/>, covering
/// blindspot detection, risk scoring, and data gap handling.
/// </summary>
public class OrganizationalValueBlindnessEngineTests
{
    private readonly Mock<ILogger<OrganizationalValueBlindnessEngine>> _loggerMock;
    private readonly Mock<IOrganizationalDataRepository> _repoMock;
    private readonly OrganizationalValueBlindnessEngine _sut;

    public OrganizationalValueBlindnessEngineTests()
    {
        _loggerMock = new Mock<ILogger<OrganizationalValueBlindnessEngine>>();
        _repoMock = new Mock<IOrganizationalDataRepository>();

        _sut = new OrganizationalValueBlindnessEngine(_loggerMock.Object, _repoMock.Object);
    }

    // -----------------------------------------------------------------------
    // Constructor null-guard tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new OrganizationalValueBlindnessEngine(null!, _repoMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        var act = () => new OrganizationalValueBlindnessEngine(_loggerMock.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("orgDataRepository");
    }

    // -----------------------------------------------------------------------
    // Blind spot detection tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DetectOrganizationalBlindnessAsync_OvervaluedDepartment_IdentifiesBlindSpot()
    {
        // Arrange — Engineering perceived = 0.9, actual = 0.4 (discrepancy > 0.3)
        var data = new OrgDataSnapshot
        {
            PerceivedValueScores = new Dictionary<string, double> { { "Engineering", 0.9 } },
            ActualImpactScores = new Dictionary<string, double> { { "Engineering", 0.4 } },
            ResourceAllocation = new Dictionary<string, double>(),
            RecognitionMetrics = new Dictionary<string, double>()
        };

        SetupRepository("org-1", data);
        var request = CreateRequest("org-1");

        // Act
        var result = await _sut.DetectOrganizationalBlindnessAsync(request);

        // Assert
        result.IdentifiedBlindSpots.Should().Contain(s => s.Contains("Overvaluing 'Engineering'"));
    }

    [Fact]
    public async Task DetectOrganizationalBlindnessAsync_UndervaluedDepartment_IdentifiesBlindSpot()
    {
        // Arrange — Marketing perceived = 0.2, actual = 0.8 (discrepancy < -0.3)
        var data = new OrgDataSnapshot
        {
            PerceivedValueScores = new Dictionary<string, double> { { "Marketing", 0.2 } },
            ActualImpactScores = new Dictionary<string, double> { { "Marketing", 0.8 } },
            ResourceAllocation = new Dictionary<string, double>(),
            RecognitionMetrics = new Dictionary<string, double>()
        };

        SetupRepository("org-2", data);
        var request = CreateRequest("org-2");

        // Act
        var result = await _sut.DetectOrganizationalBlindnessAsync(request);

        // Assert
        result.IdentifiedBlindSpots.Should().Contain(s => s.Contains("Undervaluing 'Marketing'"));
    }

    [Fact]
    public async Task DetectOrganizationalBlindnessAsync_OverallocatedResources_IdentifiesBlindSpot()
    {
        // Arrange — Resources high but impact low
        var data = new OrgDataSnapshot
        {
            PerceivedValueScores = new Dictionary<string, double>(),
            ActualImpactScores = new Dictionary<string, double> { { "Sales", 0.3 } },
            ResourceAllocation = new Dictionary<string, double> { { "Sales", 0.9 } },
            RecognitionMetrics = new Dictionary<string, double>()
        };

        SetupRepository("org-3", data);
        var request = CreateRequest("org-3");

        // Act
        var result = await _sut.DetectOrganizationalBlindnessAsync(request);

        // Assert
        result.IdentifiedBlindSpots.Should().Contain(s => s.Contains("Overallocating resources to 'Sales'"));
    }

    [Fact]
    public async Task DetectOrganizationalBlindnessAsync_UnderresourcedDepartment_IdentifiesBlindSpot()
    {
        // Arrange — Resources low but impact high
        var data = new OrgDataSnapshot
        {
            PerceivedValueScores = new Dictionary<string, double>(),
            ActualImpactScores = new Dictionary<string, double> { { "R&D", 0.9 } },
            ResourceAllocation = new Dictionary<string, double> { { "R&D", 0.2 } },
            RecognitionMetrics = new Dictionary<string, double>()
        };

        SetupRepository("org-4", data);
        var request = CreateRequest("org-4");

        // Act
        var result = await _sut.DetectOrganizationalBlindnessAsync(request);

        // Assert
        result.IdentifiedBlindSpots.Should().Contain(s => s.Contains("Underresourcing 'R&D'"));
    }

    [Fact]
    public async Task DetectOrganizationalBlindnessAsync_OverrecognizedDepartment_IdentifiesBlindSpot()
    {
        // Arrange — Recognition high but impact low
        var data = new OrgDataSnapshot
        {
            PerceivedValueScores = new Dictionary<string, double>(),
            ActualImpactScores = new Dictionary<string, double> { { "PR", 0.2 } },
            ResourceAllocation = new Dictionary<string, double>(),
            RecognitionMetrics = new Dictionary<string, double> { { "PR", 0.8 } }
        };

        SetupRepository("org-5", data);
        var request = CreateRequest("org-5");

        // Act
        var result = await _sut.DetectOrganizationalBlindnessAsync(request);

        // Assert
        result.IdentifiedBlindSpots.Should().Contain(s => s.Contains("Overrecognizing 'PR'"));
    }

    // -----------------------------------------------------------------------
    // Risk scoring tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DetectOrganizationalBlindnessAsync_NoDiscrepancies_ReturnsZeroRisk()
    {
        // Arrange — Perfect alignment
        var data = new OrgDataSnapshot
        {
            PerceivedValueScores = new Dictionary<string, double> { { "Eng", 0.5 } },
            ActualImpactScores = new Dictionary<string, double> { { "Eng", 0.5 } },
            ResourceAllocation = new Dictionary<string, double> { { "Eng", 0.5 } },
            RecognitionMetrics = new Dictionary<string, double> { { "Eng", 0.5 } }
        };

        SetupRepository("org-aligned", data);
        var request = CreateRequest("org-aligned");

        // Act
        var result = await _sut.DetectOrganizationalBlindnessAsync(request);

        // Assert
        result.BlindnessRiskScore.Should().Be(0.0);
        result.IdentifiedBlindSpots.Should().Contain(s => s.Contains("No significant value blindness detected"));
    }

    [Fact]
    public async Task DetectOrganizationalBlindnessAsync_RiskScoreCappedAtOne()
    {
        // Arrange — Extreme discrepancies across all dimensions
        var data = new OrgDataSnapshot
        {
            PerceivedValueScores = new Dictionary<string, double> { { "A", 1.0 } },
            ActualImpactScores = new Dictionary<string, double> { { "A", 0.0 } },
            ResourceAllocation = new Dictionary<string, double> { { "A", 1.0 } },
            RecognitionMetrics = new Dictionary<string, double> { { "A", 1.0 } }
        };

        SetupRepository("org-extreme", data);
        var request = CreateRequest("org-extreme");

        // Act
        var result = await _sut.DetectOrganizationalBlindnessAsync(request);

        // Assert
        result.BlindnessRiskScore.Should().BeLessThanOrEqualTo(1.0);
    }

    // -----------------------------------------------------------------------
    // Error handling and metadata tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DetectOrganizationalBlindnessAsync_NullData_ThrowsInvalidOperationException()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetOrgDataSnapshotAsync("org-null", It.IsAny<string[]>(), "tenant-1"))
            .ReturnsAsync((OrgDataSnapshot)null!);

        var request = CreateRequest("org-null");

        // Act
        var act = () => _sut.DetectOrganizationalBlindnessAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No organizational data found*");
    }

    [Fact]
    public async Task DetectOrganizationalBlindnessAsync_ValidRequest_IncludesModelVersionAndCorrelationId()
    {
        // Arrange
        var data = new OrgDataSnapshot
        {
            PerceivedValueScores = new Dictionary<string, double>(),
            ActualImpactScores = new Dictionary<string, double>(),
            ResourceAllocation = new Dictionary<string, double>(),
            RecognitionMetrics = new Dictionary<string, double>()
        };

        SetupRepository("org-meta", data);
        var request = CreateRequest("org-meta");

        // Act
        var result = await _sut.DetectOrganizationalBlindnessAsync(request);

        // Assert
        result.ModelVersion.Should().Be("OrgBlindness-v1.1");
        result.CorrelationId.Should().Be("corr-456");
        result.OrganizationId.Should().Be("org-meta");
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private void SetupRepository(string orgId, OrgDataSnapshot data)
    {
        _repoMock
            .Setup(r => r.GetOrgDataSnapshotAsync(orgId, It.IsAny<string[]>(), "tenant-1"))
            .ReturnsAsync(data);
    }

    private static OrgBlindnessDetectionRequest CreateRequest(string orgId)
    {
        return new OrgBlindnessDetectionRequest
        {
            OrganizationId = orgId,
            DepartmentFilters = Array.Empty<string>(),
            Provenance = new ProvenanceContext
            {
                TenantId = "tenant-1",
                ActorId = "test-actor",
                CorrelationId = "corr-456"
            }
        };
    }
}
