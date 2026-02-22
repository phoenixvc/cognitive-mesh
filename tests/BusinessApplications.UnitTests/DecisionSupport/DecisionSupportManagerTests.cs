using CognitiveMesh.BusinessApplications.DecisionSupport;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CognitiveMesh.Tests.BusinessApplications.DecisionSupport;

/// <summary>
/// Unit tests for <see cref="DecisionSupportManager"/>, covering constructor behavior,
/// decision analysis, risk evaluation, recommendation generation, outcome simulation,
/// and IDisposable implementation.
/// </summary>
public class DecisionSupportManagerTests : IDisposable
{
    private readonly Mock<ILogger<DecisionSupportManager>> _loggerMock;
    private readonly DecisionSupportManager _sut;

    public DecisionSupportManagerTests()
    {
        _loggerMock = new Mock<ILogger<DecisionSupportManager>>();
        _sut = new DecisionSupportManager(_loggerMock.Object);
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    // -----------------------------------------------------------------------
    // Constructor tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_WithLogger_InitializesSuccessfully()
    {
        using var manager = new DecisionSupportManager(_loggerMock.Object);

        manager.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NullLogger_InitializesWithoutThrowing()
    {
        // DecisionSupportManager accepts null logger (optional parameter)
        using var manager = new DecisionSupportManager(null);

        manager.Should().NotBeNull();
    }

    // -----------------------------------------------------------------------
    // AnalyzeDecisionOptionsAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AnalyzeDecisionOptionsAsync_ValidInputs_ReturnsAnalysisResult()
    {
        var options = new List<Dictionary<string, object>>
        {
            new() { ["name"] = "Option A", ["cost"] = 100.0 },
            new() { ["name"] = "Option B", ["cost"] = 200.0 }
        };
        var criteria = new Dictionary<string, object> { ["weight_cost"] = 0.6 };

        var result = await _sut.AnalyzeDecisionOptionsAsync(
            "Budget allocation", options, criteria, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().ContainKey("bestOption");
        result.Should().ContainKey("scores");
        result.Should().ContainKey("recommendations");
    }

    [Fact]
    public async Task AnalyzeDecisionOptionsAsync_WithOptions_BestOptionIsNotNegativeOne()
    {
        var options = new List<Dictionary<string, object>>
        {
            new() { ["name"] = "Option A" }
        };

        var result = await _sut.AnalyzeDecisionOptionsAsync(
            "Simple decision", options, null, CancellationToken.None);

        result["bestOption"].Should().Be(0);
    }

    [Fact]
    public async Task AnalyzeDecisionOptionsAsync_EmptyOptions_BestOptionIsNegativeOne()
    {
        var options = new List<Dictionary<string, object>>();

        var result = await _sut.AnalyzeDecisionOptionsAsync(
            "No options decision", options, null, CancellationToken.None);

        result["bestOption"].Should().Be(-1);
    }

    [Fact]
    public async Task AnalyzeDecisionOptionsAsync_NullCriteria_ReturnsResult()
    {
        var options = new List<Dictionary<string, object>>
        {
            new() { ["name"] = "Option A" }
        };

        var result = await _sut.AnalyzeDecisionOptionsAsync(
            "Decision without criteria", options, null, CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AnalyzeDecisionOptionsAsync_MultipleOptions_ReturnsFirstAsBest()
    {
        var options = new List<Dictionary<string, object>>
        {
            new() { ["name"] = "Option A" },
            new() { ["name"] = "Option B" },
            new() { ["name"] = "Option C" }
        };

        var result = await _sut.AnalyzeDecisionOptionsAsync(
            "Multi-option decision", options, null, CancellationToken.None);

        // With the current implementation, bestOption should be 0 when options exist
        result["bestOption"].Should().Be(0);
    }

    // -----------------------------------------------------------------------
    // EvaluateRiskAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task EvaluateRiskAsync_ValidInputs_ReturnsRiskAssessment()
    {
        var parameters = new Dictionary<string, object>
        {
            ["probability"] = 0.3,
            ["impact"] = "high"
        };

        var result = await _sut.EvaluateRiskAsync(
            "Market expansion", parameters, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().ContainKey("riskLevel");
        result.Should().ContainKey("riskScore");
        result.Should().ContainKey("mitigationStrategies");
    }

    [Fact]
    public async Task EvaluateRiskAsync_ValidInputs_ReturnsLowRiskLevel()
    {
        var parameters = new Dictionary<string, object> { ["factor"] = "test" };

        var result = await _sut.EvaluateRiskAsync(
            "Low-risk scenario", parameters, CancellationToken.None);

        result["riskLevel"].Should().Be("low");
    }

    [Fact]
    public async Task EvaluateRiskAsync_ValidInputs_ReturnsRiskScore()
    {
        var parameters = new Dictionary<string, object> { ["factor"] = "test" };

        var result = await _sut.EvaluateRiskAsync(
            "Score scenario", parameters, CancellationToken.None);

        result["riskScore"].Should().Be(0.1);
    }

    [Fact]
    public async Task EvaluateRiskAsync_EmptyParameters_ReturnsResult()
    {
        var parameters = new Dictionary<string, object>();

        var result = await _sut.EvaluateRiskAsync(
            "Empty params", parameters, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().ContainKey("riskLevel");
    }

    // -----------------------------------------------------------------------
    // GenerateRecommendationsAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GenerateRecommendationsAsync_ValidInputs_ReturnsRecommendations()
    {
        var data = new Dictionary<string, object>
        {
            ["revenue"] = 1000000.0,
            ["growth_rate"] = 0.15
        };

        var result = await _sut.GenerateRecommendationsAsync(
            "Revenue optimization", data, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().ContainKey("recommendations");
        result.Should().ContainKey("confidenceScores");
        result.Should().ContainKey("supportingEvidence");
    }

    [Fact]
    public async Task GenerateRecommendationsAsync_EmptyData_ReturnsEmptyRecommendations()
    {
        var data = new Dictionary<string, object>();

        var result = await _sut.GenerateRecommendationsAsync(
            "Empty context", data, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().ContainKey("recommendations");
    }

    [Fact]
    public async Task GenerateRecommendationsAsync_ValidInputs_ContainsAllExpectedKeys()
    {
        var data = new Dictionary<string, object> { ["metric"] = 42 };

        var result = await _sut.GenerateRecommendationsAsync(
            "Full key check", data, CancellationToken.None);

        result.Keys.Should().Contain("recommendations");
        result.Keys.Should().Contain("confidenceScores");
        result.Keys.Should().Contain("supportingEvidence");
    }

    // -----------------------------------------------------------------------
    // SimulateOutcomesAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SimulateOutcomesAsync_ValidInputs_ReturnsSimulationResult()
    {
        var parameters = new Dictionary<string, object>
        {
            ["iterations"] = 1000,
            ["timeHorizon"] = "1y"
        };

        var result = await _sut.SimulateOutcomesAsync(
            "Market entry simulation", parameters, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().ContainKey("mostLikelyOutcome");
        result.Should().ContainKey("probability");
        result.Should().ContainKey("alternativeScenarios");
    }

    [Fact]
    public async Task SimulateOutcomesAsync_ValidInputs_ProbabilityIsOne()
    {
        var parameters = new Dictionary<string, object> { ["param"] = "value" };

        var result = await _sut.SimulateOutcomesAsync(
            "Deterministic scenario", parameters, CancellationToken.None);

        result["probability"].Should().Be(1.0);
    }

    [Fact]
    public async Task SimulateOutcomesAsync_EmptyParameters_ReturnsResult()
    {
        var parameters = new Dictionary<string, object>();

        var result = await _sut.SimulateOutcomesAsync(
            "Empty params simulation", parameters, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().ContainKey("mostLikelyOutcome");
    }

    // -----------------------------------------------------------------------
    // Dispose tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var manager = new DecisionSupportManager(_loggerMock.Object);

        var act = () =>
        {
            manager.Dispose();
            manager.Dispose();
        };

        act.Should().NotThrow();
    }

    // -----------------------------------------------------------------------
    // Interface compliance
    // -----------------------------------------------------------------------

    [Fact]
    public void DecisionSupportManager_ImplementsIDecisionSupportManager()
    {
        _sut.Should().BeAssignableTo<IDecisionSupportManager>();
    }

    [Fact]
    public void DecisionSupportManager_ImplementsIDisposable()
    {
        _sut.Should().BeAssignableTo<IDisposable>();
    }
}
