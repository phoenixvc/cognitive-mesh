using CognitiveMesh.BusinessApplications.AdaptiveBalance.Models;
using CognitiveMesh.BusinessApplications.AdaptiveBalance.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.BusinessApplications.AdaptiveBalance;

/// <summary>
/// Unit tests for <see cref="AdaptiveBalanceService"/> covering balance retrieval,
/// overrides, spectrum history, learning evidence, and reflexion status.
/// </summary>
public class AdaptiveBalanceServiceTests
{
    private readonly Mock<ILogger<AdaptiveBalanceService>> _loggerMock;
    private readonly AdaptiveBalanceService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdaptiveBalanceServiceTests"/> class.
    /// </summary>
    public AdaptiveBalanceServiceTests()
    {
        _loggerMock = new Mock<ILogger<AdaptiveBalanceService>>();
        _sut = new AdaptiveBalanceService(_loggerMock.Object);
    }

    // -----------------------------------------------------------------------
    // Constructor
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new AdaptiveBalanceService(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // -----------------------------------------------------------------------
    // GetBalanceAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetBalanceAsync_DefaultState_ReturnsFiveDimensionsAtHalf()
    {
        var request = new BalanceRequest { Context = new Dictionary<string, string>() };

        var result = await _sut.GetBalanceAsync(request);

        result.Should().NotBeNull();
        result.Dimensions.Should().HaveCount(5);
        result.Dimensions.Should().AllSatisfy(d =>
        {
            d.Value.Should().Be(0.5);
            d.LowerBound.Should().Be(0.4);
            d.UpperBound.Should().Be(0.6);
        });
        result.GeneratedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetBalanceAsync_ContainsAllExpectedDimensions()
    {
        var request = new BalanceRequest();

        var result = await _sut.GetBalanceAsync(request);

        var dimensionNames = result.Dimensions.Select(d => d.Dimension).ToList();
        dimensionNames.Should().Contain("Profit");
        dimensionNames.Should().Contain("Risk");
        dimensionNames.Should().Contain("Agreeableness");
        dimensionNames.Should().Contain("IdentityGrounding");
        dimensionNames.Should().Contain("LearningRate");
    }

    [Fact]
    public async Task GetBalanceAsync_WithContext_ReturnsBalanceSuccessfully()
    {
        var request = new BalanceRequest
        {
            Context = new Dictionary<string, string>
            {
                { "scenario", "high-risk" },
                { "urgency", "high" }
            }
        };

        var result = await _sut.GetBalanceAsync(request);

        result.Should().NotBeNull();
        result.Dimensions.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetBalanceAsync_NullRequest_ThrowsArgumentNullException()
    {
        var act = () => _sut.GetBalanceAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // -----------------------------------------------------------------------
    // ApplyOverrideAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ApplyOverrideAsync_ValidRequest_UpdatesDimensionValue()
    {
        var request = new OverrideRequest
        {
            Dimension = "Profit",
            NewValue = 0.8,
            Rationale = "Increased focus on profitability",
            OverriddenBy = "admin-1"
        };

        var result = await _sut.ApplyOverrideAsync(request);

        result.Should().NotBeNull();
        result.OverrideId.Should().NotBe(Guid.Empty);
        result.Dimension.Should().Be("Profit");
        result.OldValue.Should().Be(0.5);
        result.NewValue.Should().Be(0.8);
        result.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        result.Message.Should().NotBeNullOrEmpty();

        // Verify the balance reflects the new value
        var balance = await _sut.GetBalanceAsync(new BalanceRequest());
        var profit = balance.Dimensions.First(d => d.Dimension == "Profit");
        profit.Value.Should().Be(0.8);
    }

    [Fact]
    public async Task ApplyOverrideAsync_ValueBelowRange_ThrowsArgumentOutOfRangeException()
    {
        var request = new OverrideRequest
        {
            Dimension = "Risk",
            NewValue = -0.1,
            Rationale = "Invalid",
            OverriddenBy = "admin-1"
        };

        var act = () => _sut.ApplyOverrideAsync(request);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task ApplyOverrideAsync_ValueAboveRange_ThrowsArgumentOutOfRangeException()
    {
        var request = new OverrideRequest
        {
            Dimension = "Risk",
            NewValue = 1.1,
            Rationale = "Invalid",
            OverriddenBy = "admin-1"
        };

        var act = () => _sut.ApplyOverrideAsync(request);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task ApplyOverrideAsync_EmptyDimension_ThrowsArgumentException()
    {
        var request = new OverrideRequest
        {
            Dimension = "",
            NewValue = 0.5,
            Rationale = "Test",
            OverriddenBy = "admin-1"
        };

        var act = () => _sut.ApplyOverrideAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ApplyOverrideAsync_NullRequest_ThrowsArgumentNullException()
    {
        var act = () => _sut.ApplyOverrideAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ApplyOverrideAsync_BoundaryValueZero_Succeeds()
    {
        var request = new OverrideRequest
        {
            Dimension = "Risk",
            NewValue = 0.0,
            Rationale = "Minimum risk",
            OverriddenBy = "admin-1"
        };

        var result = await _sut.ApplyOverrideAsync(request);

        result.NewValue.Should().Be(0.0);
    }

    [Fact]
    public async Task ApplyOverrideAsync_BoundaryValueOne_Succeeds()
    {
        var request = new OverrideRequest
        {
            Dimension = "Risk",
            NewValue = 1.0,
            Rationale = "Maximum risk",
            OverriddenBy = "admin-1"
        };

        var result = await _sut.ApplyOverrideAsync(request);

        result.NewValue.Should().Be(1.0);
    }

    // -----------------------------------------------------------------------
    // GetSpectrumHistoryAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetSpectrumHistoryAsync_DefaultDimension_ReturnsInitialEntry()
    {
        var result = await _sut.GetSpectrumHistoryAsync("Profit");

        result.Should().NotBeNull();
        result.Dimension.Should().Be("Profit");
        result.History.Should().HaveCount(1);
        result.History[0].Value.Should().Be(0.5);
        result.History[0].Rationale.Should().Be("Default initial position.");
    }

    [Fact]
    public async Task GetSpectrumHistoryAsync_AfterOverride_ContainsMultipleEntries()
    {
        await _sut.ApplyOverrideAsync(new OverrideRequest
        {
            Dimension = "Risk",
            NewValue = 0.7,
            Rationale = "Increased risk tolerance",
            OverriddenBy = "admin-1"
        });

        var result = await _sut.GetSpectrumHistoryAsync("Risk");

        result.History.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSpectrumHistoryAsync_NullDimension_ThrowsArgumentException()
    {
        var act = () => _sut.GetSpectrumHistoryAsync(null!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetSpectrumHistoryAsync_UnknownDimension_ReturnsEmptyHistory()
    {
        var result = await _sut.GetSpectrumHistoryAsync("NonExistentDimension");

        result.Dimension.Should().Be("NonExistentDimension");
        result.History.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // SubmitLearningEvidenceAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SubmitLearningEvidenceAsync_ValidRequest_ReturnsUniqueEventId()
    {
        var request = new LearningEvidenceRequest
        {
            PatternType = "Bias",
            Description = "Output bias detected",
            Evidence = "Comparison data",
            Outcome = "Corrected",
            SourceAgentId = "agent-1"
        };

        var result = await _sut.SubmitLearningEvidenceAsync(request);

        result.Should().NotBeNull();
        result.EventId.Should().NotBe(Guid.Empty);
        result.RecordedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        result.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SubmitLearningEvidenceAsync_MultipleSubmissions_ProduceDistinctIds()
    {
        var request1 = new LearningEvidenceRequest
        {
            PatternType = "Bias",
            Description = "First",
            Evidence = "Data1",
            Outcome = "Fixed",
            SourceAgentId = "agent-1"
        };

        var request2 = new LearningEvidenceRequest
        {
            PatternType = "Drift",
            Description = "Second",
            Evidence = "Data2",
            Outcome = "Monitored",
            SourceAgentId = "agent-2"
        };

        var result1 = await _sut.SubmitLearningEvidenceAsync(request1);
        var result2 = await _sut.SubmitLearningEvidenceAsync(request2);

        result1.EventId.Should().NotBe(result2.EventId);
    }

    [Fact]
    public async Task SubmitLearningEvidenceAsync_NullRequest_ThrowsArgumentNullException()
    {
        var act = () => _sut.SubmitLearningEvidenceAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // -----------------------------------------------------------------------
    // GetReflexionStatusAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetReflexionStatusAsync_NoResults_ReturnsEmptyWithZeroRates()
    {
        var result = await _sut.GetReflexionStatusAsync();

        result.Should().NotBeNull();
        result.RecentResults.Should().BeEmpty();
        result.HallucinationRate.Should().Be(0.0);
        result.AverageConfidence.Should().Be(0.0);
    }

    [Fact]
    public async Task GetReflexionStatusAsync_OverallConfidenceReflectsInBalance()
    {
        // With no reflexion results, confidence should default to 0.5
        var balance = await _sut.GetBalanceAsync(new BalanceRequest());

        balance.OverallConfidence.Should().Be(0.5);
    }
}
