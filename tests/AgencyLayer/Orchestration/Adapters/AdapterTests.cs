using AgencyLayer.MultiAgentOrchestration.Adapters;
using AgencyLayer.MultiAgentOrchestration.Ports;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CognitiveMesh.Tests.AgencyLayer.Orchestration.Adapters;

public class InProcessAgentRuntimeAdapterTests
{
    private readonly InProcessAgentRuntimeAdapter _sut;
    private readonly Mock<ILogger<InProcessAgentRuntimeAdapter>> _logger = new();

    public InProcessAgentRuntimeAdapterTests()
    {
        _sut = new InProcessAgentRuntimeAdapter(_logger.Object);
    }

    [Fact]
    public async Task ExecuteAgentLogicAsync_WithRegisteredHandler_ExecutesHandler()
    {
        _sut.RegisterHandler("TestAgent", task =>
            Task.FromResult<object>(new { Result = $"Processed: {task.Goal}" }));

        var spawnRequest = new DynamicAgentSpawnRequest { AgentType = "TestAgent", TenantId = "t1" };
        var agentId = await _sut.ProvisionAgentInstanceAsync(spawnRequest);

        var result = await _sut.ExecuteAgentLogicAsync(agentId, new AgentTask { Goal = "Test" });

        result.Should().NotBeNull();
        result.ToString().Should().Contain("Processed");
    }

    [Fact]
    public async Task ExecuteAgentLogicAsync_WithWildcardHandler_FallsBackToWildcard()
    {
        _sut.RegisterHandler("*", task =>
            Task.FromResult<object>(new { Wildcard = true, Goal = task.Goal }));

        var result = await _sut.ExecuteAgentLogicAsync("unknown-agent", new AgentTask { Goal = "Fallback" });

        result.Should().NotBeNull();
        result.ToString().Should().Contain("Wildcard");
    }

    [Fact]
    public async Task ExecuteAgentLogicAsync_NoHandler_ReturnsAcknowledgment()
    {
        var result = await _sut.ExecuteAgentLogicAsync("no-handler", new AgentTask { Goal = "Test" });
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ProvisionAgentInstanceAsync_ReturnsUniqueId()
    {
        var id1 = await _sut.ProvisionAgentInstanceAsync(new DynamicAgentSpawnRequest { AgentType = "A" });
        var id2 = await _sut.ProvisionAgentInstanceAsync(new DynamicAgentSpawnRequest { AgentType = "A" });

        id1.Should().NotBe(id2);
        id1.Should().Contain("agent-A-");
    }
}

public class InMemoryAgentKnowledgeRepositoryTests
{
    private readonly InMemoryAgentKnowledgeRepository _sut;
    private readonly Mock<ILogger<InMemoryAgentKnowledgeRepository>> _logger = new();

    public InMemoryAgentKnowledgeRepositoryTests()
    {
        _sut = new InMemoryAgentKnowledgeRepository(_logger.Object);
    }

    [Fact]
    public async Task StoreAndRetrieveAgentDefinition()
    {
        var definition = new AgentDefinition { AgentType = "TestAgent", Description = "A test agent" };
        await _sut.StoreAgentDefinitionAsync(definition);

        var retrieved = await _sut.GetAgentDefinitionAsync("TestAgent");
        retrieved.Should().NotBeNull();
        retrieved.Description.Should().Be("A test agent");
    }

    [Fact]
    public async Task StoreLearningInsight_CanBeRetrieved()
    {
        var insight = new AgentLearningInsight
        {
            InsightType = "TestInsight",
            ConfidenceScore = 0.9,
            InsightData = "Some data"
        };
        await _sut.StoreLearningInsightAsync(insight);

        var results = await _sut.GetRelevantInsightsAsync("TestInsight");
        results.Should().NotBeEmpty();
    }
}

public class AutoApprovalAdapterTests
{
    [Fact]
    public async Task AutoApproveAll_ReturnsTrue()
    {
        var logger = new Mock<ILogger<AutoApprovalAdapter>>();
        var adapter = new AutoApprovalAdapter(logger.Object, autoApproveAll: true);

        var result = await adapter.RequestApprovalAsync("user1", "test action", null!);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ManualCallback_DelegatesToCallback()
    {
        var logger = new Mock<ILogger<AutoApprovalAdapter>>();
        var adapter = new AutoApprovalAdapter(logger.Object, autoApproveAll: false,
            manualApprovalCallback: (user, desc, payload) => Task.FromResult(desc.Contains("approve")));

        var approved = await adapter.RequestApprovalAsync("user1", "please approve this", null!);
        var denied = await adapter.RequestApprovalAsync("user1", "reject this", null!);

        approved.Should().BeTrue();
        denied.Should().BeFalse();
    }

    [Fact]
    public async Task NoMechanism_DefaultsDeny()
    {
        var logger = new Mock<ILogger<AutoApprovalAdapter>>();
        var adapter = new AutoApprovalAdapter(logger.Object, autoApproveAll: false);

        var result = await adapter.RequestApprovalAsync("user1", "action", null!);
        result.Should().BeFalse();
    }
}
