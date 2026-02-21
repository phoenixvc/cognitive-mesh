using FoundationLayer.EnterpriseConnectors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CognitiveMesh.Tests.BusinessApplications.KnowledgeManagement;

/// <summary>
/// Unit tests for <see cref="KnowledgeManager"/>, covering constructor behavior,
/// CRUD operations (Add, Retrieve, Update, Delete), feature flag routing,
/// and error handling for each knowledge lifecycle method.
/// </summary>
public class KnowledgeManagerTests
{
    private readonly Mock<ILogger<KnowledgeManager>> _loggerMock;

    public KnowledgeManagerTests()
    {
        _loggerMock = new Mock<ILogger<KnowledgeManager>>();
    }

    // -----------------------------------------------------------------------
    // Helper: create a FeatureFlagManager with specific flags enabled
    // -----------------------------------------------------------------------

    private static FeatureFlagManager CreateFeatureFlagManager(Dictionary<string, string?>? flags = null)
    {
        var configData = flags ?? new Dictionary<string, string?>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
        return new FeatureFlagManager(configuration);
    }

    private KnowledgeManager CreateSut(Dictionary<string, string?>? flags = null)
    {
        var featureFlagManager = CreateFeatureFlagManager(flags);
        return new KnowledgeManager(_loggerMock.Object, featureFlagManager);
    }

    // -----------------------------------------------------------------------
    // Constructor tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_ValidDependencies_DoesNotThrow()
    {
        var act = () => CreateSut();

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_ValidDependencies_CreatesInstance()
    {
        var sut = CreateSut();

        sut.Should().NotBeNull();
    }

    // -----------------------------------------------------------------------
    // AddKnowledgeAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AddKnowledgeAsync_ADKEnabled_ReturnsTrue()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_ADK"] = "true"
        });

        var result = await sut.AddKnowledgeAsync("k-001", "Test content");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task AddKnowledgeAsync_LangGraphEnabled_ReturnsTrue()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_LangGraph"] = "true"
        });

        var result = await sut.AddKnowledgeAsync("k-002", "LangGraph content");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task AddKnowledgeAsync_CrewAIEnabled_ReturnsTrue()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_CrewAI"] = "true"
        });

        var result = await sut.AddKnowledgeAsync("k-003", "CrewAI content");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task AddKnowledgeAsync_SemanticKernelEnabled_ReturnsTrue()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_SemanticKernel"] = "true"
        });

        var result = await sut.AddKnowledgeAsync("k-004", "SK content");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task AddKnowledgeAsync_NoFeatureEnabled_ReturnsFalse()
    {
        var sut = CreateSut();

        var result = await sut.AddKnowledgeAsync("k-none", "No feature content");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddKnowledgeAsync_ADKPrioritizedOverLangGraph_ReturnsTrue()
    {
        // ADK is checked first in AddKnowledgeAsync, so when both are enabled, ADK path is taken
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_ADK"] = "true",
            ["FeatureFlags:enable_LangGraph"] = "true"
        });

        var result = await sut.AddKnowledgeAsync("k-priority", "Priority test");

        result.Should().BeTrue();
    }

    // -----------------------------------------------------------------------
    // RetrieveKnowledgeAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RetrieveKnowledgeAsync_LangGraphEnabled_ReturnsContent()
    {
        // LangGraph is checked first in RetrieveKnowledgeAsync
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_LangGraph"] = "true"
        });

        var result = await sut.RetrieveKnowledgeAsync("k-001");

        result.Should().NotBeNull();
        result.Should().Contain("Sample content");
    }

    [Fact]
    public async Task RetrieveKnowledgeAsync_ADKEnabled_ReturnsContentWithADK()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_ADK"] = "true"
        });

        var result = await sut.RetrieveKnowledgeAsync("k-002");

        result.Should().NotBeNull();
        result.Should().Contain("ADK");
    }

    [Fact]
    public async Task RetrieveKnowledgeAsync_CrewAIEnabled_ReturnsContentWithCrewAI()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_CrewAI"] = "true"
        });

        var result = await sut.RetrieveKnowledgeAsync("k-003");

        result.Should().NotBeNull();
        result.Should().Contain("CrewAI");
    }

    [Fact]
    public async Task RetrieveKnowledgeAsync_NoFeatureEnabled_ReturnsNull()
    {
        var sut = CreateSut();

        var result = await sut.RetrieveKnowledgeAsync("k-none");

        result.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // UpdateKnowledgeAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UpdateKnowledgeAsync_CrewAIEnabled_ReturnsTrue()
    {
        // CrewAI is checked first in UpdateKnowledgeAsync
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_CrewAI"] = "true"
        });

        var result = await sut.UpdateKnowledgeAsync("k-001", "Updated content");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateKnowledgeAsync_ADKEnabled_ReturnsTrue()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_ADK"] = "true"
        });

        var result = await sut.UpdateKnowledgeAsync("k-002", "ADK updated content");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateKnowledgeAsync_NoFeatureEnabled_ReturnsFalse()
    {
        var sut = CreateSut();

        var result = await sut.UpdateKnowledgeAsync("k-none", "No feature update");

        result.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // DeleteKnowledgeAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DeleteKnowledgeAsync_SemanticKernelEnabled_ReturnsTrue()
    {
        // SemanticKernel is checked first in DeleteKnowledgeAsync
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_SemanticKernel"] = "true"
        });

        var result = await sut.DeleteKnowledgeAsync("k-001");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteKnowledgeAsync_ADKEnabled_ReturnsTrue()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_ADK"] = "true"
        });

        var result = await sut.DeleteKnowledgeAsync("k-002");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteKnowledgeAsync_LangGraphEnabled_ReturnsTrue()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_LangGraph"] = "true"
        });

        var result = await sut.DeleteKnowledgeAsync("k-003");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteKnowledgeAsync_NoFeatureEnabled_ReturnsFalse()
    {
        var sut = CreateSut();

        var result = await sut.DeleteKnowledgeAsync("k-none");

        result.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // AutoGen / Smolagents / AutoGPT coverage
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AddKnowledgeAsync_AutoGenEnabled_ReturnsTrue()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_AutoGen"] = "true"
        });

        var result = await sut.AddKnowledgeAsync("k-ag", "AutoGen content");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task AddKnowledgeAsync_SmolagentsEnabled_ReturnsTrue()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_Smolagents"] = "true"
        });

        var result = await sut.AddKnowledgeAsync("k-sm", "Smolagents content");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task AddKnowledgeAsync_AutoGPTEnabled_ReturnsTrue()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_AutoGPT"] = "true"
        });

        var result = await sut.AddKnowledgeAsync("k-agpt", "AutoGPT content");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task RetrieveKnowledgeAsync_AutoGenEnabled_ReturnsContentWithAutoGen()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_AutoGen"] = "true"
        });

        var result = await sut.RetrieveKnowledgeAsync("k-ag");

        result.Should().NotBeNull();
        result.Should().Contain("AutoGen");
    }

    [Fact]
    public async Task DeleteKnowledgeAsync_AutoGPTEnabled_ReturnsTrue()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_AutoGPT"] = "true"
        });

        var result = await sut.DeleteKnowledgeAsync("k-agpt");

        result.Should().BeTrue();
    }
}
