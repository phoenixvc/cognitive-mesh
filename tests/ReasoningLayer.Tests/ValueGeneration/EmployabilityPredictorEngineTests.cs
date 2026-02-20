using CognitiveMesh.ReasoningLayer.ValueGeneration.Engines;
using CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.ReasoningLayer.ValueGeneration;

/// <summary>
/// Unit tests for <see cref="EmployabilityPredictorEngine"/>, covering
/// skill gap analysis, risk classification, consent enforcement, and
/// manual review triggering.
/// </summary>
public class EmployabilityPredictorEngineTests
{
    private readonly Mock<ILogger<EmployabilityPredictorEngine>> _loggerMock;
    private readonly Mock<IEmployabilityDataRepository> _repoMock;
    private readonly Mock<IConsentVerifier> _consentMock;
    private readonly Mock<IManualReviewRequester> _reviewMock;
    private readonly EmployabilityPredictorEngine _sut;

    public EmployabilityPredictorEngineTests()
    {
        _loggerMock = new Mock<ILogger<EmployabilityPredictorEngine>>();
        _repoMock = new Mock<IEmployabilityDataRepository>();
        _consentMock = new Mock<IConsentVerifier>();
        _reviewMock = new Mock<IManualReviewRequester>();

        _sut = new EmployabilityPredictorEngine(
            _loggerMock.Object,
            _repoMock.Object,
            _consentMock.Object,
            _reviewMock.Object);
    }

    // -----------------------------------------------------------------------
    // Constructor null-guard tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new EmployabilityPredictorEngine(
            null!, _repoMock.Object, _consentMock.Object, _reviewMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        var act = () => new EmployabilityPredictorEngine(
            _loggerMock.Object, null!, _consentMock.Object, _reviewMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("employabilityDataRepository");
    }

    [Fact]
    public void Constructor_NullConsentVerifier_ThrowsArgumentNullException()
    {
        var act = () => new EmployabilityPredictorEngine(
            _loggerMock.Object, _repoMock.Object, null!, _reviewMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("consentVerifier");
    }

    [Fact]
    public void Constructor_NullManualReviewRequester_ThrowsArgumentNullException()
    {
        var act = () => new EmployabilityPredictorEngine(
            _loggerMock.Object, _repoMock.Object, _consentMock.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("manualReviewRequester");
    }

    // -----------------------------------------------------------------------
    // Consent enforcement tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CheckEmployabilityAsync_NoConsent_ThrowsConsentRequiredException()
    {
        // Arrange
        _consentMock
            .Setup(c => c.VerifyConsentExistsAsync("user-1", "tenant-1", "EmployabilityAnalysis"))
            .ReturnsAsync(false);

        var request = CreateRequest("user-1", "tenant-1");

        // Act
        var act = () => _sut.CheckEmployabilityAsync(request);

        // Assert
        await act.Should().ThrowAsync<ConsentRequiredException>()
            .WithMessage("*EmployabilityAnalysis*");
    }

    [Fact]
    public async Task CheckEmployabilityAsync_WithConsent_Proceeds()
    {
        // Arrange
        SetupConsentGranted("user-ok", "tenant-1");
        SetupEmployabilityData("user-ok", "tenant-1", CreateLowRiskData());

        var request = CreateRequest("user-ok", "tenant-1");

        // Act
        var result = await _sut.CheckEmployabilityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be("user-ok");
    }

    // -----------------------------------------------------------------------
    // Skill gap analysis tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CheckEmployabilityAsync_SkillGap_IncludesRiskFactor()
    {
        // Arrange — user has C# but market wants AI and K8s
        var data = new EmployabilityData
        {
            UserSkills = new List<string> { "C#" },
            MarketTrendingSkills = new List<string> { "C#", "AI", "Kubernetes" },
            UserCreativeOutputScore = 0.5,
            ProjectsCompleted = 3,
            CollaborationScore = 0.6,
            SkillRelevanceScores = new Dictionary<string, double> { { "C#", 0.8 } }
        };

        SetupConsentGranted("user-gap", "tenant-1");
        SetupEmployabilityData("user-gap", "tenant-1", data);

        var request = CreateRequest("user-gap", "tenant-1");

        // Act
        var result = await _sut.CheckEmployabilityAsync(request);

        // Assert
        result.RiskFactors.Should().Contain(f => f.Contains("Skill gap identified"));
        result.RecommendedActions.Should().Contain(a => a.Contains("Explore training"));
    }

    [Fact]
    public async Task CheckEmployabilityAsync_NoSkillGap_DoesNotFlagSkillRisk()
    {
        // Arrange — user has all trending skills
        var data = new EmployabilityData
        {
            UserSkills = new List<string> { "C#", "AI", "Kubernetes" },
            MarketTrendingSkills = new List<string> { "C#", "AI", "Kubernetes" },
            UserCreativeOutputScore = 0.5,
            ProjectsCompleted = 3,
            CollaborationScore = 0.6,
            SkillRelevanceScores = new Dictionary<string, double> { { "C#", 0.8 } }
        };

        SetupConsentGranted("user-noskillgap", "tenant-1");
        SetupEmployabilityData("user-noskillgap", "tenant-1", data);

        var request = CreateRequest("user-noskillgap", "tenant-1");

        // Act
        var result = await _sut.CheckEmployabilityAsync(request);

        // Assert
        result.RiskFactors.Should().NotContain(f => f.Contains("Skill gap"));
    }

    // -----------------------------------------------------------------------
    // Risk classification tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CheckEmployabilityAsync_AllRiskFactors_ClassifiedAsHigh()
    {
        // Arrange — Low creative output, low collaboration, low projects, skill gap, low relevance
        var data = new EmployabilityData
        {
            UserSkills = new List<string>(),
            MarketTrendingSkills = new List<string> { "AI", "ML", "Cloud" },
            UserCreativeOutputScore = 0.1,
            ProjectsCompleted = 1,
            CollaborationScore = 0.2,
            SkillRelevanceScores = new Dictionary<string, double> { { "Legacy", 0.1 } }
        };

        SetupConsentGranted("user-highrisk", "tenant-1");
        SetupEmployabilityData("user-highrisk", "tenant-1", data);

        var request = CreateRequest("user-highrisk", "tenant-1");

        // Act
        var result = await _sut.CheckEmployabilityAsync(request);

        // Assert
        result.RiskLevel.Should().Be("High");
        result.EmployabilityRiskScore.Should().BeGreaterThan(0.6);
    }

    [Fact]
    public async Task CheckEmployabilityAsync_LowRisk_ClassifiedAsLow()
    {
        // Arrange
        SetupConsentGranted("user-low", "tenant-1");
        SetupEmployabilityData("user-low", "tenant-1", CreateLowRiskData());

        var request = CreateRequest("user-low", "tenant-1");

        // Act
        var result = await _sut.CheckEmployabilityAsync(request);

        // Assert
        result.RiskLevel.Should().Be("Low");
        result.EmployabilityRiskScore.Should().BeLessThanOrEqualTo(0.3);
    }

    [Fact]
    public async Task CheckEmployabilityAsync_MediumRisk_ClassifiedAsMedium()
    {
        // Arrange — some risk factors present
        var data = new EmployabilityData
        {
            UserSkills = new List<string> { "C#" },
            MarketTrendingSkills = new List<string> { "C#", "AI", "ML" },
            UserCreativeOutputScore = 0.5,
            ProjectsCompleted = 3,
            CollaborationScore = 0.3, // Low collaboration adds 0.2
            SkillRelevanceScores = new Dictionary<string, double> { { "C#", 0.8 } }
        };

        SetupConsentGranted("user-medium", "tenant-1");
        SetupEmployabilityData("user-medium", "tenant-1", data);

        var request = CreateRequest("user-medium", "tenant-1");

        // Act
        var result = await _sut.CheckEmployabilityAsync(request);

        // Assert
        result.RiskLevel.Should().Be("Medium");
    }

    // -----------------------------------------------------------------------
    // Manual review triggering tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CheckEmployabilityAsync_HighRisk_TriggersManualReview()
    {
        // Arrange
        var data = new EmployabilityData
        {
            UserSkills = new List<string>(),
            MarketTrendingSkills = new List<string> { "AI", "ML", "Cloud" },
            UserCreativeOutputScore = 0.1,
            ProjectsCompleted = 1,
            CollaborationScore = 0.2,
            SkillRelevanceScores = new Dictionary<string, double> { { "Legacy", 0.1 } }
        };

        SetupConsentGranted("user-review", "tenant-1");
        SetupEmployabilityData("user-review", "tenant-1", data);

        var request = CreateRequest("user-review", "tenant-1");

        // Act
        await _sut.CheckEmployabilityAsync(request);

        // Assert
        _reviewMock.Verify(
            r => r.RequestManualReviewAsync(
                "user-review",
                "tenant-1",
                "EmployabilityHighRisk",
                It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckEmployabilityAsync_LowRisk_DoesNotTriggerManualReview()
    {
        // Arrange
        SetupConsentGranted("user-noreview", "tenant-1");
        SetupEmployabilityData("user-noreview", "tenant-1", CreateLowRiskData());

        var request = CreateRequest("user-noreview", "tenant-1");

        // Act
        await _sut.CheckEmployabilityAsync(request);

        // Assert
        _reviewMock.Verify(
            r => r.RequestManualReviewAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()),
            Times.Never);
    }

    // -----------------------------------------------------------------------
    // Error handling tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CheckEmployabilityAsync_NullData_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupConsentGranted("user-nulldata", "tenant-1");

        _repoMock
            .Setup(r => r.GetEmployabilityDataAsync("user-nulldata", "tenant-1"))
            .ReturnsAsync((EmployabilityData)null!);

        var request = CreateRequest("user-nulldata", "tenant-1");

        // Act
        var act = () => _sut.CheckEmployabilityAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No employability data found*");
    }

    // -----------------------------------------------------------------------
    // Response metadata tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CheckEmployabilityAsync_ValidRequest_IncludesModelVersionAndCorrelationId()
    {
        // Arrange
        SetupConsentGranted("user-meta", "tenant-1");
        SetupEmployabilityData("user-meta", "tenant-1", CreateLowRiskData());

        var request = CreateRequest("user-meta", "tenant-1");

        // Act
        var result = await _sut.CheckEmployabilityAsync(request);

        // Assert
        result.ModelVersion.Should().Be("EmployabilityPredictor-v1.0");
        result.CorrelationId.Should().Be("corr-789");
        result.UserId.Should().Be("user-meta");
    }

    [Fact]
    public async Task CheckEmployabilityAsync_RiskScoreCappedAtOne()
    {
        // Arrange — stacked risk factors that would push score beyond 1.0
        var data = new EmployabilityData
        {
            UserSkills = new List<string>(),
            MarketTrendingSkills = new List<string> { "A", "B", "C", "D", "E" },
            UserCreativeOutputScore = 0.0,
            ProjectsCompleted = 0,
            CollaborationScore = 0.0,
            SkillRelevanceScores = new Dictionary<string, double> { { "Obsolete", 0.0 } }
        };

        SetupConsentGranted("user-cap", "tenant-1");
        SetupEmployabilityData("user-cap", "tenant-1", data);

        var request = CreateRequest("user-cap", "tenant-1");

        // Act
        var result = await _sut.CheckEmployabilityAsync(request);

        // Assert
        result.EmployabilityRiskScore.Should().BeLessThanOrEqualTo(1.0);
    }

    [Fact]
    public async Task CheckEmployabilityAsync_NoRiskFactors_ReturnsDefaultRecommendation()
    {
        // Arrange — everything is great
        SetupConsentGranted("user-great", "tenant-1");
        SetupEmployabilityData("user-great", "tenant-1", CreateLowRiskData());

        var request = CreateRequest("user-great", "tenant-1");

        // Act
        var result = await _sut.CheckEmployabilityAsync(request);

        // Assert
        result.RecommendedActions.Should().NotBeEmpty();
        result.RecommendedActions.Should().Contain(a =>
            a.Contains("Continue developing your existing skills"));
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private void SetupConsentGranted(string userId, string tenantId)
    {
        _consentMock
            .Setup(c => c.VerifyConsentExistsAsync(userId, tenantId, "EmployabilityAnalysis"))
            .ReturnsAsync(true);
    }

    private void SetupEmployabilityData(string userId, string tenantId, EmployabilityData data)
    {
        _repoMock
            .Setup(r => r.GetEmployabilityDataAsync(userId, tenantId))
            .ReturnsAsync(data);
    }

    private static EmployabilityData CreateLowRiskData()
    {
        return new EmployabilityData
        {
            UserSkills = new List<string> { "C#", "Azure", "AI", "Kubernetes" },
            MarketTrendingSkills = new List<string> { "C#", "Azure", "AI", "Kubernetes" },
            UserCreativeOutputScore = 0.8,
            ProjectsCompleted = 5,
            CollaborationScore = 0.7,
            SkillRelevanceScores = new Dictionary<string, double>
            {
                { "C#", 0.8 },
                { "Azure", 0.9 },
                { "AI", 0.95 }
            }
        };
    }

    private static EmployabilityCheckRequest CreateRequest(string userId, string tenantId)
    {
        return new EmployabilityCheckRequest
        {
            UserId = userId,
            Provenance = new ProvenanceContext
            {
                TenantId = tenantId,
                ActorId = "test-actor",
                CorrelationId = "corr-789"
            }
        };
    }
}
