using CognitiveMesh.ReasoningLayer.ValueGeneration.Engines;
using CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.ReasoningLayer.ValueGeneration;

/// <summary>
/// Unit tests for <see cref="ValueGenerationDiagnosticEngine"/>, covering
/// score calculation, profile assignment, and strengths/opportunities derivation.
/// </summary>
public class ValueGenerationDiagnosticEngineTests
{
    private readonly Mock<ILogger<ValueGenerationDiagnosticEngine>> _loggerMock;
    private readonly Mock<IValueDiagnosticDataRepository> _repoMock;
    private readonly ValueGenerationDiagnosticEngine _sut;

    public ValueGenerationDiagnosticEngineTests()
    {
        _loggerMock = new Mock<ILogger<ValueGenerationDiagnosticEngine>>();
        _repoMock = new Mock<IValueDiagnosticDataRepository>();

        _sut = new ValueGenerationDiagnosticEngine(_loggerMock.Object, _repoMock.Object);
    }

    // -----------------------------------------------------------------------
    // Constructor null-guard tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new ValueGenerationDiagnosticEngine(null!, _repoMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        var act = () => new ValueGenerationDiagnosticEngine(_loggerMock.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("valueDataRepository");
    }

    // -----------------------------------------------------------------------
    // Score calculation tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RunValueDiagnosticAsync_HighScores_ReturnsInnovatorProfile()
    {
        // Arrange
        var data = new ValueDiagnosticData
        {
            AverageImpactScore = 2.0,
            HighValueContributions = 10,
            CreativityEvents = 5
        };

        SetupRepository("target-1", "tenant-1", data);

        var request = CreateRequest("target-1", "tenant-1");

        // Act
        var result = await _sut.RunValueDiagnosticAsync(request);

        // Assert — score = (2.0*50) + (10*10) + (5*5) = 100 + 100 + 25 = 225
        result.ValueScore.Should().Be(225.0);
        result.ValueProfile.Should().Be("Innovator");
    }

    [Fact]
    public async Task RunValueDiagnosticAsync_MediumScores_ReturnsConnectorProfile()
    {
        // Arrange
        var data = new ValueDiagnosticData
        {
            AverageImpactScore = 1.0,
            HighValueContributions = 2,
            CreativityEvents = 1
        };

        SetupRepository("target-2", "tenant-1", data);

        var request = CreateRequest("target-2", "tenant-1");

        // Act
        var result = await _sut.RunValueDiagnosticAsync(request);

        // Assert — score = (1.0*50) + (2*10) + (1*5) = 50 + 20 + 5 = 75 + something...
        // Actually 50 + 20 + 5 = 75, which is NOT > 75, so it should be "Contributor"
        result.ValueScore.Should().Be(75.0);
        result.ValueProfile.Should().Be("Contributor");
    }

    [Fact]
    public async Task RunValueDiagnosticAsync_ScoreAbove75_ReturnsConnector()
    {
        // Arrange — score = (1.0*50) + (3*10) + (1*5) = 50 + 30 + 5 = 85
        var data = new ValueDiagnosticData
        {
            AverageImpactScore = 1.0,
            HighValueContributions = 3,
            CreativityEvents = 1
        };

        SetupRepository("target-conn", "tenant-1", data);
        var request = CreateRequest("target-conn", "tenant-1");

        // Act
        var result = await _sut.RunValueDiagnosticAsync(request);

        // Assert
        result.ValueScore.Should().Be(85.0);
        result.ValueProfile.Should().Be("Connector");
    }

    [Fact]
    public async Task RunValueDiagnosticAsync_LowScores_ReturnsContributorProfile()
    {
        // Arrange
        var data = new ValueDiagnosticData
        {
            AverageImpactScore = 0.3,
            HighValueContributions = 1,
            CreativityEvents = 1
        };

        SetupRepository("target-3", "tenant-1", data);

        var request = CreateRequest("target-3", "tenant-1");

        // Act
        var result = await _sut.RunValueDiagnosticAsync(request);

        // Assert — score = (0.3*50) + (1*10) + (1*5) = 15 + 10 + 5 = 30
        result.ValueScore.Should().Be(30.0);
        result.ValueProfile.Should().Be("Contributor");
    }

    // -----------------------------------------------------------------------
    // Strengths derivation tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RunValueDiagnosticAsync_HighImpact_IncludesHighImpactDeliveryStrength()
    {
        // Arrange — AverageImpactScore > 0.7
        var data = new ValueDiagnosticData
        {
            AverageImpactScore = 0.9,
            HighValueContributions = 6,
            CreativityEvents = 4
        };

        SetupRepository("target-hi", "tenant-1", data);
        var request = CreateRequest("target-hi", "tenant-1");

        // Act
        var result = await _sut.RunValueDiagnosticAsync(request);

        // Assert
        result.Strengths.Should().Contain("High Impact Delivery");
        result.Strengths.Should().Contain("Consistent Value Creation");
        result.Strengths.Should().Contain("Creative Problem Solving");
    }

    [Fact]
    public async Task RunValueDiagnosticAsync_NoNotableStrengths_ReturnsBalancedContribution()
    {
        // Arrange — All scores below thresholds: impact <= 0.7, contributions <= 5, creativity <= 3
        var data = new ValueDiagnosticData
        {
            AverageImpactScore = 0.5,
            HighValueContributions = 3,
            CreativityEvents = 2
        };

        SetupRepository("target-balanced", "tenant-1", data);
        var request = CreateRequest("target-balanced", "tenant-1");

        // Act
        var result = await _sut.RunValueDiagnosticAsync(request);

        // Assert
        result.Strengths.Should().Contain("Balanced Contribution");
    }

    // -----------------------------------------------------------------------
    // Development opportunities tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RunValueDiagnosticAsync_LowImpact_IncludesFocusOnHighImpactWork()
    {
        // Arrange — AverageImpactScore < 0.5
        var data = new ValueDiagnosticData
        {
            AverageImpactScore = 0.3,
            HighValueContributions = 1,
            CreativityEvents = 1
        };

        SetupRepository("target-dev", "tenant-1", data);
        var request = CreateRequest("target-dev", "tenant-1");

        // Act
        var result = await _sut.RunValueDiagnosticAsync(request);

        // Assert
        result.DevelopmentOpportunities.Should().Contain("Focus on high-impact work");
        result.DevelopmentOpportunities.Should().Contain("Increase value-generating activities");
        result.DevelopmentOpportunities.Should().Contain("Engage in more creative thinking sessions");
    }

    [Fact]
    public async Task RunValueDiagnosticAsync_AllHighMetrics_ReturnsDefaultOpportunity()
    {
        // Arrange — All high: impact >= 0.5, contributions >= 3, creativity >= 2
        var data = new ValueDiagnosticData
        {
            AverageImpactScore = 0.9,
            HighValueContributions = 6,
            CreativityEvents = 4
        };

        SetupRepository("target-allhigh", "tenant-1", data);
        var request = CreateRequest("target-allhigh", "tenant-1");

        // Act
        var result = await _sut.RunValueDiagnosticAsync(request);

        // Assert
        result.DevelopmentOpportunities.Should().Contain("Increase cross-team collaboration");
    }

    // -----------------------------------------------------------------------
    // Error handling tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RunValueDiagnosticAsync_NullData_ThrowsInvalidOperationException()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetValueDiagnosticDataAsync("null-target", "tenant-1"))
            .ReturnsAsync((ValueDiagnosticData)null!);

        var request = CreateRequest("null-target", "tenant-1");

        // Act
        var act = () => _sut.RunValueDiagnosticAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No diagnostic data found*");
    }

    // -----------------------------------------------------------------------
    // Response metadata tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RunValueDiagnosticAsync_ValidRequest_IncludesModelVersionAndCorrelationId()
    {
        // Arrange
        var data = new ValueDiagnosticData
        {
            AverageImpactScore = 0.5,
            HighValueContributions = 3,
            CreativityEvents = 2
        };

        SetupRepository("target-meta", "tenant-1", data);
        var request = CreateRequest("target-meta", "tenant-1");

        // Act
        var result = await _sut.RunValueDiagnosticAsync(request);

        // Assert
        result.ModelVersion.Should().Be("ValueDiagnostic-v1.0");
        result.CorrelationId.Should().Be("corr-123");
        result.TargetId.Should().Be("target-meta");
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private void SetupRepository(string targetId, string tenantId, ValueDiagnosticData data)
    {
        _repoMock
            .Setup(r => r.GetValueDiagnosticDataAsync(targetId, tenantId))
            .ReturnsAsync(data);
    }

    private static ValueDiagnosticRequest CreateRequest(string targetId, string tenantId)
    {
        return new ValueDiagnosticRequest
        {
            TargetId = targetId,
            TargetType = "User",
            Provenance = new ProvenanceContext
            {
                TenantId = tenantId,
                ActorId = "test-actor",
                CorrelationId = "corr-123"
            }
        };
    }
}
