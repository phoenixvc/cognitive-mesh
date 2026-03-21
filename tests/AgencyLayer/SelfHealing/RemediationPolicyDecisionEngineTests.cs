using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using CognitiveMesh.AgencyLayer.SelfHealing.Engines;
using CognitiveMesh.FoundationLayer.PolicyStore.Models;
using CognitiveMesh.FoundationLayer.PolicyStore.Ports;

namespace CognitiveMesh.AgencyLayer.SelfHealing.Tests;

public sealed class RemediationPolicyDecisionEngineTests
{
    private readonly Mock<IRemediationPolicyPort> _policyPortMock = new();
    private RemediationPolicyDecisionEngine CreateSut() =>
        new(_policyPortMock.Object, NullLogger<RemediationPolicyDecisionEngine>.Instance);

    [Fact]
    public async Task GetAllowedActionsAsync_ValidPolicy_ReturnsActionsAndWeights()
    {
        var policy = new RemediationPolicy
        {
            IncidentCategory = "infrastructure", Severity = "high",
            AllowedActions = RemediationAction.Retry | RemediationAction.Escalate,
            RankingWeights = new Dictionary<string, double> { ["Retry"] = 0.7, ["Escalate"] = 0.3 }
        };
        _policyPortMock.Setup(p => p.GetPolicyAsync("infrastructure", "high", It.IsAny<CancellationToken>())).ReturnsAsync(policy);
        var (actions, weights) = await CreateSut().GetAllowedActionsAsync("infrastructure", "high");
        actions.Should().HaveFlag(RemediationAction.Retry);
        actions.Should().HaveFlag(RemediationAction.Escalate);
        weights["Retry"].Should().Be(0.7);
        weights["Escalate"].Should().Be(0.3);
    }

    [Fact]
    public async Task GetAllowedActionsAsync_PolicyPortFallback_ReturnsPermissiveActions()
    {
        var fallback = new RemediationPolicy
        {
            IncidentCategory = "application", Severity = "medium",
            AllowedActions = RemediationAction.Retry | RemediationAction.Escalate,
            RankingWeights = new Dictionary<string, double> { ["Retry"] = 0.7, ["Escalate"] = 0.3 }
        };
        _policyPortMock.Setup(p => p.GetPolicyAsync("application", "medium", It.IsAny<CancellationToken>())).ReturnsAsync(fallback);
        var (actions, _) = await CreateSut().GetAllowedActionsAsync("application", "medium");
        actions.Should().NotBe(RemediationAction.None);
    }

    [Fact]
    public async Task GetAllowedActionsAsync_SecurityCritical_ContainsEscalate()
    {
        var policy = new RemediationPolicy
        {
            IncidentCategory = "security", Severity = "critical",
            AllowedActions = RemediationAction.Escalate,
            RankingWeights = new Dictionary<string, double> { ["Escalate"] = 1.0 }
        };
        _policyPortMock.Setup(p => p.GetPolicyAsync("security", "critical", It.IsAny<CancellationToken>())).ReturnsAsync(policy);
        var (actions, weights) = await CreateSut().GetAllowedActionsAsync("security", "critical");
        actions.Should().HaveFlag(RemediationAction.Escalate);
        actions.Should().NotHaveFlag(RemediationAction.Retry);
        weights["Escalate"].Should().Be(1.0);
    }

    [Fact]
    public async Task GetAllowedActionsAsync_NullCategory_ThrowsArgumentException()
    {
        var act = async () => await CreateSut().GetAllowedActionsAsync(null!, "high");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetAllowedActionsAsync_EmptySeverity_ThrowsArgumentException()
    {
        var act = async () => await CreateSut().GetAllowedActionsAsync("infrastructure", "");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetAllowedActionsAsync_CallsDelegatesPortExactlyOnce()
    {
        var policy = new RemediationPolicy
        {
            IncidentCategory = "data", Severity = "low",
            AllowedActions = RemediationAction.Retry,
            RankingWeights = new Dictionary<string, double> { ["Retry"] = 0.6 }
        };
        _policyPortMock.Setup(p => p.GetPolicyAsync("data", "low", It.IsAny<CancellationToken>())).ReturnsAsync(policy);
        await CreateSut().GetAllowedActionsAsync("data", "low");
        _policyPortMock.Verify(p => p.GetPolicyAsync("data", "low", It.IsAny<CancellationToken>()), Times.Once);
    }
}
