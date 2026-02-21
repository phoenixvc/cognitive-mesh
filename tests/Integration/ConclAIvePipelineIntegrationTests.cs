using CognitiveMesh.ReasoningLayer.LLMReasoning.Abstractions;
using CognitiveMesh.ReasoningLayer.StructuredReasoning.Engines;
using CognitiveMesh.ReasoningLayer.StructuredReasoning.Models;
using CognitiveMesh.ReasoningLayer.StructuredReasoning.Ports;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.Integration;

/// <summary>
/// Integration tests for the ConclAIve structured reasoning pipeline.
/// Wires real ConclAIveOrchestrator with real reasoning engines and a
/// deterministic mock LLM to verify the full reasoning flow.
/// </summary>
public class ConclAIvePipelineIntegrationTests : IClassFixture<ConclAIveTestFixture>
{
    private readonly IConclAIveOrchestratorPort _orchestrator;
    private readonly ConclAIveTestFixture _fixture;

    public ConclAIvePipelineIntegrationTests(ConclAIveTestFixture fixture)
    {
        _fixture = fixture;
        _orchestrator = fixture.ServiceProvider.GetRequiredService<IConclAIveOrchestratorPort>();
    }

    [Fact]
    public async Task ReasonAsync_WithDebateRecipe_ProducesStructuredOutputWithTrace()
    {
        // Act
        var result = await _orchestrator.ReasonAsync(
            "Should we adopt microservices or monolith architecture?",
            ReasoningRecipeType.DebateAndVote,
            new Dictionary<string, string>
            {
                ["domain"] = "software architecture",
                ["team_size"] = "20"
            });

        // Assert
        result.Should().NotBeNull();
        result.RecipeType.Should().Be(ReasoningRecipeType.DebateAndVote);
        result.Conclusion.Should().NotBeNullOrWhiteSpace();
        result.Confidence.Should().BeInRange(0.0, 1.0);
        result.ReasoningTrace.Should().NotBeEmpty();
        result.SessionId.Should().NotBeNullOrWhiteSpace();
        result.Metadata.Should().ContainKey("question");
    }

    [Fact]
    public async Task ReasonAsync_WithSequentialRecipe_DecomposesAndIntegrates()
    {
        // Act
        var result = await _orchestrator.ReasonAsync(
            "What is the optimal cloud migration strategy for a legacy banking application?",
            ReasoningRecipeType.Sequential,
            new Dictionary<string, string>
            {
                ["industry"] = "financial services",
                ["constraints"] = "regulatory compliance required"
            });

        // Assert
        result.Should().NotBeNull();
        result.RecipeType.Should().Be(ReasoningRecipeType.Sequential);
        result.Conclusion.Should().NotBeNullOrWhiteSpace();
        result.Confidence.Should().BeInRange(0.0, 1.0);
        result.ReasoningTrace.Should().NotBeEmpty();
        result.Metadata.Should().ContainKey("question");
    }

    [Fact]
    public async Task ReasonAsync_WithStrategicSimulationRecipe_ExploresScenarios()
    {
        // Act
        var result = await _orchestrator.ReasonAsync(
            "How will AI regulation affect SaaS pricing models in the next 5 years?",
            ReasoningRecipeType.StrategicSimulation,
            new Dictionary<string, string>
            {
                ["market"] = "enterprise SaaS",
                ["region"] = "EU"
            });

        // Assert
        result.Should().NotBeNull();
        result.RecipeType.Should().Be(ReasoningRecipeType.StrategicSimulation);
        result.Conclusion.Should().NotBeNullOrWhiteSpace();
        result.Confidence.Should().BeInRange(0.0, 1.0);
        result.ReasoningTrace.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ReasonAsync_AutoSelection_PicksRecipeBasedOnQuery()
    {
        // Act — let the orchestrator auto-select the recipe
        var result = await _orchestrator.ReasonAsync(
            "Compare the pros and cons of Kubernetes vs serverless for batch processing workloads");

        // Assert — should produce a valid result regardless of recipe chosen
        result.Should().NotBeNull();
        result.Conclusion.Should().NotBeNullOrWhiteSpace();
        result.ReasoningTrace.Should().NotBeEmpty();
        result.RecipeType.Should().BeOneOf(
            ReasoningRecipeType.DebateAndVote,
            ReasoningRecipeType.Sequential,
            ReasoningRecipeType.StrategicSimulation);
    }

    [Fact]
    public async Task ReasonAsync_MultipleSequentialCalls_ProduceIndependentSessions()
    {
        // Act
        var result1 = await _orchestrator.ReasonAsync("What is the best database for time-series data?",
            ReasoningRecipeType.Sequential);
        var result2 = await _orchestrator.ReasonAsync("Should we build or buy an observability platform?",
            ReasoningRecipeType.DebateAndVote);

        // Assert — each gets its own session ID and trace
        result1.SessionId.Should().NotBe(result2.SessionId);
        result1.RecipeType.Should().Be(ReasoningRecipeType.Sequential);
        result2.RecipeType.Should().Be(ReasoningRecipeType.DebateAndVote);
        result1.ReasoningTrace.Should().NotIntersectWith(result2.ReasoningTrace);
    }

    [Fact]
    public async Task ReasonAsync_DebateRecipe_TraceContainsMultiplePerspectives()
    {
        // Act
        var result = await _orchestrator.ReasonAsync(
            "Is remote work or hybrid work better for engineering productivity?",
            ReasoningRecipeType.DebateAndVote);

        // Assert — debate should have multiple perspective steps in the trace
        result.ReasoningTrace.Should().HaveCountGreaterThanOrEqualTo(2,
            "Debate reasoning should generate arguments from multiple perspectives");
    }

    [Fact]
    public async Task ReasonAsync_WithEmptyContext_StillProducesResult()
    {
        // Act
        var result = await _orchestrator.ReasonAsync(
            "What is the safest investment strategy?",
            ReasoningRecipeType.Sequential,
            new Dictionary<string, string>());

        // Assert
        result.Should().NotBeNull();
        result.Conclusion.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ReasonAsync_Performance_CompletesWithinSLA()
    {
        // Arrange — the mock LLM is instant, so this tests orchestration overhead
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await _orchestrator.ReasonAsync(
            "Quick analysis needed",
            ReasoningRecipeType.Sequential);
        sw.Stop();

        // Assert — orchestration overhead should be minimal with mock LLM
        result.Should().NotBeNull();
        sw.ElapsedMilliseconds.Should().BeLessThan(5000,
            "Orchestration overhead with mock LLM should be under 5 seconds");
    }
}

/// <summary>
/// Shared fixture for ConclAIve integration tests.
/// Wires real engines with a deterministic mock LLM client.
/// </summary>
public class ConclAIveTestFixture
{
    /// <summary>
    /// The DI service provider for resolving ConclAIve components.
    /// </summary>
    public ServiceProvider ServiceProvider { get; }

    public ConclAIveTestFixture()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // Mock LLM client that returns deterministic responses
        var mockLlm = new Mock<ILLMClient>();

        // GenerateCompletionAsync — returns structured text for various prompts
        mockLlm.Setup(l => l.GenerateCompletionAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<float>(),
                It.IsAny<IEnumerable<string>?>(), It.IsAny<CancellationToken>()))
            .Returns<string, int, float, IEnumerable<string>?, CancellationToken>((prompt, _, _, _, _) =>
            {
                // Return contextual responses based on prompt content
                if (prompt.Contains("perspective", StringComparison.OrdinalIgnoreCase)
                    || prompt.Contains("argument", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult("This perspective emphasizes cost-effectiveness and scalability. " +
                        "Key argument: modern architectures provide better long-term value. " +
                        "1. Reduced operational costs\n2. Improved scalability\n3. Better developer experience\n" +
                        "Confidence: 75");
                }

                if (prompt.Contains("synthesis", StringComparison.OrdinalIgnoreCase)
                    || prompt.Contains("conclude", StringComparison.OrdinalIgnoreCase)
                    || prompt.Contains("integrate", StringComparison.OrdinalIgnoreCase)
                    || prompt.Contains("final", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult("After careful analysis, the recommended approach balances " +
                        "practicality with innovation. The key factors are cost, scalability, and team capabilities. " +
                        "Confidence: 80");
                }

                if (prompt.Contains("decompose", StringComparison.OrdinalIgnoreCase)
                    || prompt.Contains("phase", StringComparison.OrdinalIgnoreCase)
                    || prompt.Contains("step", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult("1. Requirements Analysis\n2. Technical Evaluation\n3. Risk Assessment\n4. Final Recommendation");
                }

                if (prompt.Contains("scenario", StringComparison.OrdinalIgnoreCase)
                    || prompt.Contains("simulation", StringComparison.OrdinalIgnoreCase)
                    || prompt.Contains("explore", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult("Scenario: Moderate regulation with gradual adoption. " +
                        "Probability: 60%. Risk factors: compliance costs, talent shortage. " +
                        "Opportunities: first-mover advantage, premium pricing. Confidence: 70");
                }

                if (prompt.Contains("recipe", StringComparison.OrdinalIgnoreCase)
                    || prompt.Contains("approach", StringComparison.OrdinalIgnoreCase)
                    || prompt.Contains("appropriate", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult("DebateAndVote");
                }

                // Default response
                return Task.FromResult("Analysis complete. The recommended approach is a balanced strategy " +
                    "considering all factors. Confidence: 72");
            });

        // GenerateMultipleCompletionsAsync — for debate perspectives
        mockLlm.Setup(l => l.GenerateMultipleCompletionsAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<float>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                "Perspective A: Focus on cost reduction and efficiency. Confidence: 70",
                "Perspective B: Focus on innovation and growth. Confidence: 75",
                "Perspective C: Focus on risk mitigation and compliance. Confidence: 80"
            });

        mockLlm.Setup(l => l.GetTokenCount(It.IsAny<string>()))
            .Returns<string>(text => text.Length / 4);

        mockLlm.SetupGet(l => l.ModelName).Returns("mock-reasoning-model");

        services.AddSingleton(mockLlm.Object);

        // Wire real engines
        services.AddSingleton<IDebateReasoningPort, DebateReasoningEngine>();
        services.AddSingleton<ISequentialReasoningPort, SequentialReasoningEngine>();
        services.AddSingleton<IStrategicSimulationPort, StrategicSimulationEngine>();
        services.AddSingleton<IConclAIveOrchestratorPort, ConclAIveOrchestrator>();

        ServiceProvider = services.BuildServiceProvider();
    }
}
