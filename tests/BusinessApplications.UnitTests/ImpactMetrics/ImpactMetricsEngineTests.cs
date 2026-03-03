using CognitiveMesh.BusinessApplications.ImpactMetrics.Engines;
using CognitiveMesh.BusinessApplications.ImpactMetrics.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.BusinessApplications.ImpactMetrics;

/// <summary>
/// Unit tests for <see cref="ImpactMetricsEngine"/> covering psychological safety
/// scoring, mission alignment, adoption telemetry, resistance detection, and
/// impact assessment calculations.
/// </summary>
public class ImpactMetricsEngineTests
{
    private readonly Mock<ILogger<ImpactMetricsEngine>> _loggerMock;
    private readonly ImpactMetricsEngine _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImpactMetricsEngineTests"/> class.
    /// </summary>
    public ImpactMetricsEngineTests()
    {
        _loggerMock = new Mock<ILogger<ImpactMetricsEngine>>();
        _sut = new ImpactMetricsEngine(_loggerMock.Object);
    }

    // -----------------------------------------------------------------------
    // Constructor null guards
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new ImpactMetricsEngine(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // -----------------------------------------------------------------------
    // CalculateSafetyScoreAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CalculateSafetyScoreAsync_AllDimensionsHigh_ScoreAbove80()
    {
        // Arrange
        var surveyScores = new Dictionary<SafetyDimension, double>
        {
            { SafetyDimension.TrustInAI, 90 },
            { SafetyDimension.FearOfReplacement, 90 },
            { SafetyDimension.ComfortWithAutomation, 90 },
            { SafetyDimension.WillingnessToExperiment, 90 },
            { SafetyDimension.TransparencyPerception, 90 },
            { SafetyDimension.ErrorTolerance, 90 }
        };

        // Act
        var result = await _sut.CalculateSafetyScoreAsync("team-1", "tenant-1", surveyScores);

        // Assert
        result.Should().NotBeNull();
        result.OverallScore.Should().BeGreaterThanOrEqualTo(80);
        result.TeamId.Should().Be("team-1");
        result.TenantId.Should().Be("tenant-1");
    }

    [Fact]
    public async Task CalculateSafetyScoreAsync_LowTrust_ReducedOverallScore()
    {
        // Arrange
        var surveyScores = new Dictionary<SafetyDimension, double>
        {
            { SafetyDimension.TrustInAI, 20 },
            { SafetyDimension.FearOfReplacement, 80 },
            { SafetyDimension.ComfortWithAutomation, 80 },
            { SafetyDimension.WillingnessToExperiment, 80 },
            { SafetyDimension.TransparencyPerception, 80 },
            { SafetyDimension.ErrorTolerance, 80 }
        };

        // Act
        var result = await _sut.CalculateSafetyScoreAsync("team-low-trust", "tenant-1", surveyScores);

        // Assert
        result.OverallScore.Should().BeLessThan(80);
    }

    [Fact]
    public async Task CalculateSafetyScoreAsync_AllDimensionsLow_ScoreBelow50()
    {
        // Arrange
        var surveyScores = new Dictionary<SafetyDimension, double>
        {
            { SafetyDimension.TrustInAI, 20 },
            { SafetyDimension.FearOfReplacement, 20 },
            { SafetyDimension.ComfortWithAutomation, 20 },
            { SafetyDimension.WillingnessToExperiment, 20 },
            { SafetyDimension.TransparencyPerception, 20 },
            { SafetyDimension.ErrorTolerance, 20 }
        };

        // Act
        var result = await _sut.CalculateSafetyScoreAsync("team-low", "tenant-1", surveyScores);

        // Assert
        result.OverallScore.Should().BeLessThan(50);
    }

    [Fact]
    public async Task CalculateSafetyScoreAsync_FewResponses_LowConfidence()
    {
        // Arrange — only 2 dimensions supplied (count = 2 < 10 threshold)
        var surveyScores = new Dictionary<SafetyDimension, double>
        {
            { SafetyDimension.TrustInAI, 80 },
            { SafetyDimension.ComfortWithAutomation, 80 }
        };

        // Act
        var result = await _sut.CalculateSafetyScoreAsync("team-few", "tenant-1", surveyScores);

        // Assert
        result.ConfidenceLevel.Should().Be(ConfidenceLevel.Low);
    }

    [Fact]
    public async Task CalculateSafetyScoreAsync_ManyResponses_HighConfidence()
    {
        // Arrange — provide all 6 dimensions plus seed behavioral telemetry for >50 total
        // First, seed telemetry events to boost the behavioral signal count
        for (int i = 0; i < 50; i++)
        {
            await _sut.RecordActionAsync(new AdoptionTelemetry(
                TelemetryId: Guid.NewGuid().ToString(),
                UserId: $"user-{i}",
                TenantId: "tenant-conf",
                ToolId: "tool-1",
                Action: AdoptionAction.FeatureUse,
                Timestamp: DateTimeOffset.UtcNow,
                DurationMs: 100,
                Context: null));
        }

        var surveyScores = new Dictionary<SafetyDimension, double>
        {
            { SafetyDimension.TrustInAI, 80 },
            { SafetyDimension.FearOfReplacement, 80 },
            { SafetyDimension.ComfortWithAutomation, 80 },
            { SafetyDimension.WillingnessToExperiment, 80 },
            { SafetyDimension.TransparencyPerception, 80 },
            { SafetyDimension.ErrorTolerance, 80 }
        };

        // Act
        var result = await _sut.CalculateSafetyScoreAsync("team-many", "tenant-conf", surveyScores);

        // Assert
        result.ConfidenceLevel.Should().Be(ConfidenceLevel.High);
    }

    [Fact]
    public async Task CalculateSafetyScoreAsync_MediumResponses_MediumConfidence()
    {
        // Arrange — seed telemetry to bring total between 10 and 50
        for (int i = 0; i < 10; i++)
        {
            await _sut.RecordActionAsync(new AdoptionTelemetry(
                TelemetryId: Guid.NewGuid().ToString(),
                UserId: $"user-{i}",
                TenantId: "tenant-med",
                ToolId: "tool-1",
                Action: AdoptionAction.FeatureUse,
                Timestamp: DateTimeOffset.UtcNow,
                DurationMs: 100,
                Context: null));
        }

        var surveyScores = new Dictionary<SafetyDimension, double>
        {
            { SafetyDimension.TrustInAI, 80 },
            { SafetyDimension.FearOfReplacement, 80 },
            { SafetyDimension.ComfortWithAutomation, 80 },
            { SafetyDimension.WillingnessToExperiment, 80 },
            { SafetyDimension.TransparencyPerception, 80 },
            { SafetyDimension.ErrorTolerance, 80 }
        };

        // Act
        var result = await _sut.CalculateSafetyScoreAsync("team-med", "tenant-med", surveyScores);

        // Assert
        result.ConfidenceLevel.Should().Be(ConfidenceLevel.Medium);
    }

    [Fact]
    public async Task CalculateSafetyScoreAsync_ReturnsDimensionBreakdown()
    {
        // Arrange
        var surveyScores = new Dictionary<SafetyDimension, double>
        {
            { SafetyDimension.TrustInAI, 90 },
            { SafetyDimension.FearOfReplacement, 70 },
            { SafetyDimension.ComfortWithAutomation, 85 },
            { SafetyDimension.WillingnessToExperiment, 60 },
            { SafetyDimension.TransparencyPerception, 75 },
            { SafetyDimension.ErrorTolerance, 80 }
        };

        // Act
        var result = await _sut.CalculateSafetyScoreAsync("team-dim", "tenant-1", surveyScores);

        // Assert
        result.Dimensions.Should().HaveCount(6);
        result.Dimensions.Should().ContainKey(SafetyDimension.TrustInAI);
        result.Dimensions.Should().ContainKey(SafetyDimension.ErrorTolerance);
    }

    [Fact]
    public async Task CalculateSafetyScoreAsync_NullTeamId_ThrowsArgumentException()
    {
        var surveyScores = new Dictionary<SafetyDimension, double>();

        var act = async () => await _sut.CalculateSafetyScoreAsync(null!, "tenant-1", surveyScores);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // -----------------------------------------------------------------------
    // GetHistoricalScoresAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetHistoricalScoresAsync_AfterCalculation_ReturnsScores()
    {
        // Arrange
        var surveyScores = new Dictionary<SafetyDimension, double>
        {
            { SafetyDimension.TrustInAI, 80 }
        };
        await _sut.CalculateSafetyScoreAsync("team-hist", "tenant-hist", surveyScores);

        // Act
        var result = await _sut.GetHistoricalScoresAsync("team-hist", "tenant-hist");

        // Assert
        result.Should().HaveCount(1);
        result[0].TeamId.Should().Be("team-hist");
    }

    [Fact]
    public async Task GetHistoricalScoresAsync_NoScores_ReturnsEmpty()
    {
        var result = await _sut.GetHistoricalScoresAsync("no-team", "no-tenant");

        result.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // GetDimensionBreakdownAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetDimensionBreakdownAsync_AfterCalculation_ReturnsDimensions()
    {
        // Arrange
        var surveyScores = new Dictionary<SafetyDimension, double>
        {
            { SafetyDimension.TrustInAI, 95 },
            { SafetyDimension.ComfortWithAutomation, 85 }
        };
        await _sut.CalculateSafetyScoreAsync("team-bd", "tenant-bd", surveyScores);

        // Act
        var result = await _sut.GetDimensionBreakdownAsync("team-bd", "tenant-bd");

        // Assert
        result.Should().NotBeNull();
        result!.Should().ContainKey(SafetyDimension.TrustInAI);
    }

    [Fact]
    public async Task GetDimensionBreakdownAsync_NoScores_ReturnsNull()
    {
        var result = await _sut.GetDimensionBreakdownAsync("missing", "missing");

        result.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // AssessAlignmentAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AssessAlignmentAsync_MatchingKeywords_HighScore()
    {
        // Arrange
        var missionStatement = "We are committed to innovation, transparency, and customer satisfaction";
        var decisionContext = "This decision promotes innovation and enhances customer satisfaction through transparent processes";

        // Act
        var result = await _sut.AssessAlignmentAsync("dec-1", decisionContext, missionStatement);

        // Assert
        result.Should().NotBeNull();
        result.AlignmentScore.Should().BeGreaterThan(0.3);
        result.ValueMatches.Should().NotBeEmpty();
        result.DecisionId.Should().Be("dec-1");
    }

    [Fact]
    public async Task AssessAlignmentAsync_ConflictingActions_ConflictsDetected()
    {
        // Arrange
        var missionStatement = "We believe in transparency and trust";
        var decisionContext = "This action is against transparency and ignoring trust principles";

        // Act
        var result = await _sut.AssessAlignmentAsync("dec-conflict", decisionContext, missionStatement);

        // Assert
        result.Conflicts.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AssessAlignmentAsync_NoOverlap_LowScore()
    {
        // Arrange
        var missionStatement = "We champion environmental sustainability and green energy";
        var decisionContext = "Reduced quarterly marketing budget for office supplies";

        // Act
        var result = await _sut.AssessAlignmentAsync("dec-low", decisionContext, missionStatement);

        // Assert
        result.AlignmentScore.Should().BeLessThanOrEqualTo(0.5);
    }

    [Fact]
    public async Task AssessAlignmentAsync_NullDecisionId_ThrowsArgumentException()
    {
        var act = async () => await _sut.AssessAlignmentAsync(null!, "context", "mission");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // -----------------------------------------------------------------------
    // GetAlignmentTrendAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAlignmentTrendAsync_AfterAssessments_ReturnsTrend()
    {
        // Arrange
        await _sut.AssessAlignmentAsync("dec-t1", "innovation in products", "We value innovation");
        await _sut.AssessAlignmentAsync("dec-t2", "customer-focused design", "We value innovation");

        // Act
        var result = await _sut.GetAlignmentTrendAsync("tenant-trend");

        // Assert
        result.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    // -----------------------------------------------------------------------
    // RecordActionAsync and GetUsageSummaryAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RecordActionAsync_ValidTelemetry_StoresEvent()
    {
        // Arrange
        var telemetry = new AdoptionTelemetry(
            TelemetryId: "tel-1",
            UserId: "user-1",
            TenantId: "tenant-tel",
            ToolId: "tool-1",
            Action: AdoptionAction.FeatureUse,
            Timestamp: DateTimeOffset.UtcNow,
            DurationMs: 500,
            Context: null);

        // Act
        await _sut.RecordActionAsync(telemetry);
        var result = await _sut.GetUsageSummaryAsync("tenant-tel");

        // Assert
        result.Should().HaveCount(1);
        result[0].UserId.Should().Be("user-1");
        result[0].Action.Should().Be(AdoptionAction.FeatureUse);
    }

    [Fact]
    public async Task GetUsageSummaryAsync_NoEvents_ReturnsEmpty()
    {
        var result = await _sut.GetUsageSummaryAsync("empty-tenant");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task RecordActionAsync_NullTelemetry_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.RecordActionAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // -----------------------------------------------------------------------
    // DetectResistancePatternsAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DetectResistancePatternsAsync_HighOverrideRate_FlagsOverrideResistance()
    {
        // Arrange — seed events with high override rate (>30%)
        var tenantId = "tenant-override";
        for (int i = 0; i < 4; i++)
        {
            await _sut.RecordActionAsync(new AdoptionTelemetry(
                TelemetryId: Guid.NewGuid().ToString(),
                UserId: $"user-{i}",
                TenantId: tenantId,
                ToolId: "tool-1",
                Action: AdoptionAction.Override,
                Timestamp: DateTimeOffset.UtcNow,
                DurationMs: null,
                Context: null));
        }
        // Add 1 feature use so override rate = 4/5 = 80%
        await _sut.RecordActionAsync(new AdoptionTelemetry(
            TelemetryId: Guid.NewGuid().ToString(),
            UserId: "user-5",
            TenantId: tenantId,
            ToolId: "tool-1",
            Action: AdoptionAction.FeatureUse,
            Timestamp: DateTimeOffset.UtcNow,
            DurationMs: null,
            Context: null));

        // Act
        var result = await _sut.DetectResistancePatternsAsync(tenantId);

        // Assert
        result.Should().Contain(r => r.IndicatorType == ResistanceType.Override);
    }

    [Fact]
    public async Task DetectResistancePatternsAsync_DecliningUsage_FlagsAvoidance()
    {
        // Arrange — more ignores than uses
        var tenantId = "tenant-avoidance";
        for (int i = 0; i < 5; i++)
        {
            await _sut.RecordActionAsync(new AdoptionTelemetry(
                TelemetryId: Guid.NewGuid().ToString(),
                UserId: $"user-{i}",
                TenantId: tenantId,
                ToolId: "tool-1",
                Action: AdoptionAction.FeatureIgnore,
                Timestamp: DateTimeOffset.UtcNow,
                DurationMs: null,
                Context: null));
        }
        await _sut.RecordActionAsync(new AdoptionTelemetry(
            TelemetryId: Guid.NewGuid().ToString(),
            UserId: "user-active",
            TenantId: tenantId,
            ToolId: "tool-1",
            Action: AdoptionAction.FeatureUse,
            Timestamp: DateTimeOffset.UtcNow,
            DurationMs: null,
            Context: null));

        // Act
        var result = await _sut.DetectResistancePatternsAsync(tenantId);

        // Assert
        result.Should().Contain(r => r.IndicatorType == ResistanceType.Avoidance);
    }

    [Fact]
    public async Task DetectResistancePatternsAsync_HelpSpike_FlagsHelpSpike()
    {
        // Arrange — help requests > 25% of total
        var tenantId = "tenant-help";
        for (int i = 0; i < 3; i++)
        {
            await _sut.RecordActionAsync(new AdoptionTelemetry(
                TelemetryId: Guid.NewGuid().ToString(),
                UserId: $"user-{i}",
                TenantId: tenantId,
                ToolId: "tool-1",
                Action: AdoptionAction.HelpRequest,
                Timestamp: DateTimeOffset.UtcNow,
                DurationMs: null,
                Context: null));
        }
        await _sut.RecordActionAsync(new AdoptionTelemetry(
            TelemetryId: Guid.NewGuid().ToString(),
            UserId: "user-ok",
            TenantId: tenantId,
            ToolId: "tool-1",
            Action: AdoptionAction.FeatureUse,
            Timestamp: DateTimeOffset.UtcNow,
            DurationMs: null,
            Context: null));

        // Act
        var result = await _sut.DetectResistancePatternsAsync(tenantId);

        // Assert
        result.Should().Contain(r => r.IndicatorType == ResistanceType.HelpSpike);
    }

    [Fact]
    public async Task DetectResistancePatternsAsync_NoEvents_ReturnsEmpty()
    {
        var result = await _sut.DetectResistancePatternsAsync("no-events-tenant");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DetectResistancePatternsAsync_NegativeFeedback_FlagsNegativeFeedback()
    {
        // Arrange — majority negative feedback
        var tenantId = "tenant-neg";
        for (int i = 0; i < 4; i++)
        {
            await _sut.RecordActionAsync(new AdoptionTelemetry(
                TelemetryId: Guid.NewGuid().ToString(),
                UserId: $"user-{i}",
                TenantId: tenantId,
                ToolId: "tool-1",
                Action: AdoptionAction.Feedback,
                Timestamp: DateTimeOffset.UtcNow,
                DurationMs: null,
                Context: "negative: tool is not helpful"));
        }
        await _sut.RecordActionAsync(new AdoptionTelemetry(
            TelemetryId: Guid.NewGuid().ToString(),
            UserId: "user-happy",
            TenantId: tenantId,
            ToolId: "tool-1",
            Action: AdoptionAction.Feedback,
            Timestamp: DateTimeOffset.UtcNow,
            DurationMs: null,
            Context: "positive: works great"));

        // Act
        var result = await _sut.DetectResistancePatternsAsync(tenantId);

        // Assert
        result.Should().Contain(r => r.IndicatorType == ResistanceType.NegativeFeedback);
    }

    // -----------------------------------------------------------------------
    // GenerateAssessmentAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GenerateAssessmentAsync_CalculatesWeightedOverallScore()
    {
        // Arrange — seed telemetry
        var tenantId = "tenant-assess";
        await _sut.RecordActionAsync(new AdoptionTelemetry(
            TelemetryId: "t-1", UserId: "u-1", TenantId: tenantId, ToolId: "tool-1",
            Action: AdoptionAction.FeatureUse, Timestamp: DateTimeOffset.UtcNow, DurationMs: 100, Context: null));
        await _sut.RecordActionAsync(new AdoptionTelemetry(
            TelemetryId: "t-2", UserId: "u-2", TenantId: tenantId, ToolId: "tool-1",
            Action: AdoptionAction.WorkflowComplete, Timestamp: DateTimeOffset.UtcNow, DurationMs: 200, Context: null));

        var periodStart = DateTimeOffset.UtcNow.AddDays(-7);
        var periodEnd = DateTimeOffset.UtcNow;

        // Act
        var result = await _sut.GenerateAssessmentAsync(tenantId, periodStart, periodEnd);

        // Assert
        result.Should().NotBeNull();
        result.TenantId.Should().Be(tenantId);
        result.AdoptionRate.Should().BeGreaterThan(0);
        result.PeriodStart.Should().Be(periodStart);
        result.PeriodEnd.Should().Be(periodEnd);
    }

    [Fact]
    public async Task GenerateAssessmentAsync_NoTelemetry_ReturnsBaselineValues()
    {
        var periodStart = DateTimeOffset.UtcNow.AddDays(-7);
        var periodEnd = DateTimeOffset.UtcNow;

        var result = await _sut.GenerateAssessmentAsync("empty-assess", periodStart, periodEnd);

        result.Should().NotBeNull();
        result.AdoptionRate.Should().Be(0);
        result.ResistanceIndicators.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // GenerateReportAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GenerateReportAsync_IncludesAllMetrics()
    {
        // Arrange
        var tenantId = "tenant-report";

        // Seed safety score
        var surveyScores = new Dictionary<SafetyDimension, double>
        {
            { SafetyDimension.TrustInAI, 85 },
            { SafetyDimension.FearOfReplacement, 80 },
            { SafetyDimension.ComfortWithAutomation, 90 },
            { SafetyDimension.WillingnessToExperiment, 75 },
            { SafetyDimension.TransparencyPerception, 85 },
            { SafetyDimension.ErrorTolerance, 80 }
        };
        await _sut.CalculateSafetyScoreAsync("team-rpt", tenantId, surveyScores);

        // Seed alignment
        await _sut.AssessAlignmentAsync("dec-rpt", "promote innovation in products", "We value innovation and quality");

        // Seed telemetry
        await _sut.RecordActionAsync(new AdoptionTelemetry(
            TelemetryId: "tel-rpt", UserId: "u-rpt", TenantId: tenantId, ToolId: "tool-1",
            Action: AdoptionAction.FeatureUse, Timestamp: DateTimeOffset.UtcNow, DurationMs: 100, Context: null));

        var periodStart = DateTimeOffset.UtcNow.AddDays(-30);
        var periodEnd = DateTimeOffset.UtcNow;

        // Act
        var result = await _sut.GenerateReportAsync(tenantId, periodStart, periodEnd);

        // Assert
        result.Should().NotBeNull();
        result.TenantId.Should().Be(tenantId);
        result.SafetyScore.Should().BeGreaterThan(0);
        result.AlignmentScore.Should().BeGreaterThan(0);
        result.AdoptionRate.Should().BeGreaterThan(0);
        result.OverallImpactScore.Should().BeGreaterThan(0);
        result.Recommendations.Should().NotBeEmpty();
        result.GeneratedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GenerateReportAsync_LowMetrics_GeneratesImprovementRecommendations()
    {
        // Arrange — no data means baseline scores (low)
        var tenantId = "tenant-low-report";
        var periodStart = DateTimeOffset.UtcNow.AddDays(-30);
        var periodEnd = DateTimeOffset.UtcNow;

        // Act
        var result = await _sut.GenerateReportAsync(tenantId, periodStart, periodEnd);

        // Assert
        result.Recommendations.Should().NotBeEmpty();
        // With neutral baselines, we should get some improvement recommendations
    }

    [Fact]
    public async Task GenerateReportAsync_OverallImpactScoreBetween0And100()
    {
        var tenantId = "tenant-range";
        var periodStart = DateTimeOffset.UtcNow.AddDays(-7);
        var periodEnd = DateTimeOffset.UtcNow;

        var result = await _sut.GenerateReportAsync(tenantId, periodStart, periodEnd);

        result.OverallImpactScore.Should().BeGreaterThanOrEqualTo(0);
        result.OverallImpactScore.Should().BeLessThanOrEqualTo(100);
    }
}
