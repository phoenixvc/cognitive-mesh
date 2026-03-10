using System.Text.Json;
using AgencyLayer.Agents.Ports;
using AgencyLayer.AgentTeamFramework.Pipeline;
using AgencyLayer.RoadmapCrew.Engines;
using AgencyLayer.RoadmapCrew.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace RoadmapCrew.Tests;

public class RoadmapCrewEngineTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly Mock<ISpecializedAgentPort> _agentPortMock;
    private readonly Mock<ILogger<RoadmapCrewEngine>> _loggerMock;
    private readonly Mock<ILogger<AgentPipelineExecutor>> _pipelineLoggerMock;
    private readonly AgentPipelineExecutor _pipeline;
    private readonly RoadmapCrewEngine _engine;

    public RoadmapCrewEngineTests()
    {
        _agentPortMock = new Mock<ISpecializedAgentPort>();
        _loggerMock = new Mock<ILogger<RoadmapCrewEngine>>();
        _pipelineLoggerMock = new Mock<ILogger<AgentPipelineExecutor>>();

        // Default: all agent registrations succeed
        _agentPortMock
            .Setup(x => x.RegisterAgentAsync(It.IsAny<SpecializedAgentConfiguration>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _pipeline = new AgentPipelineExecutor(_agentPortMock.Object, _pipelineLoggerMock.Object);
        _engine = new RoadmapCrewEngine(_agentPortMock.Object, _pipeline, _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateRoadmapAsync_ValidRequest_RunsAllSixAgentsSequentially()
    {
        // Arrange
        var request = CreateSampleRequest();
        SetupAgentResponses();

        // Act
        var result = await _engine.GenerateRoadmapAsync(request);

        // Assert — all 6 agents were called
        _agentPortMock.Verify(
            x => x.ExecuteTaskAsync(
                It.Is<string>(id => id.StartsWith("roadmapcrew-")),
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(6));

        result.Should().NotBeNull();
        result.ExecutiveSummary.Should().NotBeNullOrEmpty();
        result.VisionAssessment.Should().NotBeNull();
        result.MarketContext.Should().NotBeNull();
        result.PrioritizedBacklog.Should().NotBeNull();
        result.DependencyGraph.Should().NotBeNull();
        result.FeasibilityAssessment.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateRoadmapAsync_VisionKeeperRejectsAllItems_ReturnsEmptyRoadmap()
    {
        // Arrange
        var request = CreateSampleRequest();

        // VisionKeeper returns empty approved list
        var emptyVision = new VisionAssessment
        {
            ApprovedItems = [],
            RejectedItems =
            [
                new AnnotatedWorkItem
                {
                    Item = request.ProposedItems[0],
                    AlignmentScore = 2.0,
                    AlignmentRationale = "Does not serve strategic goals"
                }
            ]
        };

        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-vision-keeper",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResult("roadmapcrew-vision-keeper", emptyVision));

        // Act
        var result = await _engine.GenerateRoadmapAsync(request);

        // Assert — only VisionKeeper was called (pipeline short-circuits)
        _agentPortMock.Verify(
            x => x.ExecuteTaskAsync(
                "roadmapcrew-vision-keeper",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _agentPortMock.Verify(
            x => x.ExecuteTaskAsync(
                "roadmapcrew-market-scanner",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        result.ExecutiveSummary.Should().Contain("rejected");
        result.PrioritizedBacklog.RankedItems.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateRoadmapAsync_CancellationRequested_ThrowsOperationCancelled()
    {
        // Arrange
        var request = CreateSampleRequest();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                It.IsAny<string>(),
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await _engine.Invoking(e => e.GenerateRoadmapAsync(request, cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task AssessAlignmentAsync_ItemsBelowThreshold_MarkedAsRejected()
    {
        // Arrange
        var items = new List<WorkItem>
        {
            new() { Id = "WI-1", Title = "Low-value task", Description = "Unaligned work" },
            new() { Id = "WI-2", Title = "Strategic initiative", Description = "Core platform feature" }
        };

        var vision = new VisionContext
        {
            VisionStatement = "Build the best AI platform",
            OKRs = ["Ship v2.0 by Q3"],
            StrategicThemes = ["Platform reliability"]
        };

        var assessment = new VisionAssessment
        {
            ApprovedItems =
            [
                new AnnotatedWorkItem
                {
                    Item = items[1],
                    AlignmentScore = 8.5,
                    AlignmentRationale = "Directly supports platform reliability theme"
                }
            ],
            RejectedItems =
            [
                new AnnotatedWorkItem
                {
                    Item = items[0],
                    AlignmentScore = 2.0,
                    AlignmentRationale = "No connection to OKRs or strategic themes"
                }
            ]
        };

        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-vision-keeper",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResult("roadmapcrew-vision-keeper", assessment));

        // Act
        var result = await _engine.AssessAlignmentAsync(items, vision, 4.0);

        // Assert
        result.ApprovedItems.Should().HaveCount(1);
        result.ApprovedItems[0].AlignmentScore.Should().Be(8.5);
        result.RejectedItems.Should().HaveCount(1);
        result.RejectedItems[0].AlignmentScore.Should().BeLessThan(4.0);
    }

    [Fact]
    public async Task GenerateRoadmapAsync_AgentFails_PropagatesError()
    {
        // Arrange
        var request = CreateSampleRequest();

        // VisionKeeper succeeds
        SetupVisionKeeperSuccess(request);

        // MarketScanner fails
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-market-scanner",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecializedTaskResult
            {
                TaskId = "task-fail",
                AgentId = "roadmapcrew-market-scanner",
                Success = false,
                ErrorMessage = "Model rate limit exceeded"
            });

        // Act & Assert
        await _engine.Invoking(e => e.GenerateRoadmapAsync(request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*market-scanner*rate limit*");
    }

    [Fact]
    public async Task InitializeAsync_RegistersAllSixAgents()
    {
        // Act
        await _engine.InitializeAsync();

        // Assert
        _agentPortMock.Verify(
            x => x.RegisterAgentAsync(It.IsAny<SpecializedAgentConfiguration>(), It.IsAny<CancellationToken>()),
            Times.Exactly(6));
    }

    [Fact]
    public async Task InitializeAsync_CalledTwice_RegistersOnlyOnce()
    {
        // Act
        await _engine.InitializeAsync();
        await _engine.InitializeAsync();

        // Assert — still only 6 registrations
        _agentPortMock.Verify(
            x => x.RegisterAgentAsync(It.IsAny<SpecializedAgentConfiguration>(), It.IsAny<CancellationToken>()),
            Times.Exactly(6));
    }

    [Fact]
    public void Constructor_NullAgentPort_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new RoadmapCrewEngine(null!, _pipeline, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("agentPort");
    }

    [Fact]
    public void Constructor_NullPipeline_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new RoadmapCrewEngine(_agentPortMock.Object, null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("pipeline");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new RoadmapCrewEngine(_agentPortMock.Object, _pipeline, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ── WS4: Additional Coverage ─────────────────────────────────────────

    [Fact]
    public async Task GenerateRoadmapAsync_AgentReturnsEmptyOutput_ContinuesWithDefaults()
    {
        // Arrange
        var request = CreateSampleRequest();
        SetupVisionKeeperSuccess(request);

        // MarketScanner returns empty string output
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-market-scanner",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecializedTaskResult
            {
                TaskId = "task-empty",
                AgentId = "roadmapcrew-market-scanner",
                Success = true,
                Output = ""
            });

        // Remaining agents return valid responses
        SetupRemainingAgentsAfterMarketScanner();

        // Act
        var result = await _engine.GenerateRoadmapAsync(request);

        // Assert — pipeline continues despite empty MarketScanner output
        result.Should().NotBeNull();
        result.MarketContext.Should().NotBeNull();
        result.MarketContext.Trends.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateRoadmapAsync_AgentReturnsMalformedJson_ContinuesWithDefaults()
    {
        // Arrange
        var request = CreateSampleRequest();
        SetupVisionKeeperSuccess(request);

        // MarketScanner returns garbage text
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-market-scanner",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecializedTaskResult
            {
                TaskId = "task-malformed",
                AgentId = "roadmapcrew-market-scanner",
                Success = true,
                Output = "This is not JSON at all {{{invalid"
            });

        SetupRemainingAgentsAfterMarketScanner();

        // Act
        var result = await _engine.GenerateRoadmapAsync(request);

        // Assert — graceful degradation
        result.Should().NotBeNull();
        result.MarketContext.Trends.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateRoadmapAsync_AgentReturnsWrongSchema_ContinuesWithDefaults()
    {
        // Arrange
        var request = CreateSampleRequest();
        SetupVisionKeeperSuccess(request);

        // MarketScanner returns valid JSON but wrong shape
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-market-scanner",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecializedTaskResult
            {
                TaskId = "task-wrong",
                AgentId = "roadmapcrew-market-scanner",
                Success = true,
                Output = """{"unexpectedField": "some value", "count": 42}"""
            });

        SetupRemainingAgentsAfterMarketScanner();

        // Act
        var result = await _engine.GenerateRoadmapAsync(request);

        // Assert — defaults used for unrecognized schema
        result.Should().NotBeNull();
        result.MarketContext.Trends.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateRoadmapAsync_ContextToMarketScanner_ContainsApprovedItems()
    {
        // Arrange
        var request = CreateSampleRequest();
        SetupAgentResponses();

        SpecializedTask? capturedTask = null;
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-market-scanner",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, SpecializedTask, CancellationToken>((_, task, _) => capturedTask = task)
            .ReturnsAsync(CreateSuccessResult("roadmapcrew-market-scanner", new MarketContextBrief
            {
                Trends = [new MarketSignal { Description = "Test trend", Confidence = ConfidenceLevel.High }]
            }));

        // Act
        await _engine.GenerateRoadmapAsync(request);

        // Assert — context passed to MarketScanner contains approved items from VisionKeeper
        capturedTask.Should().NotBeNull();
        capturedTask!.Context.Should().NotBeNullOrEmpty();
        capturedTask.Context.Should().Contain("approvedItems");
        capturedTask.Context.Should().Contain("Implement auth service");
    }

    [Fact]
    public async Task GenerateRoadmapAsync_ContextToSynthesizer_ContainsAllPriorOutputs()
    {
        // Arrange
        var request = CreateSampleRequest();
        SetupAgentResponses();

        SpecializedTask? capturedTask = null;
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-roadmap-synthesizer",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, SpecializedTask, CancellationToken>((_, task, _) => capturedTask = task)
            .ReturnsAsync(CreateSuccessResult("roadmapcrew-roadmap-synthesizer", new RoadmapDocument
            {
                ExecutiveSummary = "Test summary",
                VisionAssessment = new VisionAssessment(),
                MarketContext = new MarketContextBrief(),
                PrioritizedBacklog = new PriorityRanking(),
                DependencyGraph = new DependencyAnalysis(),
                FeasibilityAssessment = new FeasibilityAssessment()
            }));

        // Act
        await _engine.GenerateRoadmapAsync(request);

        // Assert — synthesizer receives context with all 5 prior agent outputs
        capturedTask.Should().NotBeNull();
        capturedTask!.Context.Should().Contain("visionAssessment");
        capturedTask.Context.Should().Contain("marketContext");
        capturedTask.Context.Should().Contain("priorityRanking");
        capturedTask.Context.Should().Contain("dependencyAnalysis");
        capturedTask.Context.Should().Contain("feasibilityAssessment");
    }

    [Fact]
    public async Task GenerateRoadmapAsync_ConcurrentCalls_BothComplete()
    {
        // Arrange
        var request = CreateSampleRequest();
        SetupAgentResponses();

        // Act — two concurrent pipeline runs
        var task1 = _engine.GenerateRoadmapAsync(request);
        var task2 = _engine.GenerateRoadmapAsync(request);

        var results = await Task.WhenAll(task1, task2);

        // Assert — both complete without error
        results.Should().HaveCount(2);
        results[0].Should().NotBeNull();
        results[1].Should().NotBeNull();
        results[0].ExecutiveSummary.Should().NotBeNullOrEmpty();
        results[1].ExecutiveSummary.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InitializeAsync_ConcurrentCalls_RegistersExactlySixAgents()
    {
        // Arrange — add a small delay to registration to widen the race window
        _agentPortMock
            .Setup(x => x.RegisterAgentAsync(It.IsAny<SpecializedAgentConfiguration>(), It.IsAny<CancellationToken>()))
            .Returns(async () => await Task.Delay(10));

        // Act — 10 concurrent init calls
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => _engine.InitializeAsync())
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert — still only 6 registrations (not 60)
        _agentPortMock.Verify(
            x => x.RegisterAgentAsync(It.IsAny<SpecializedAgentConfiguration>(), It.IsAny<CancellationToken>()),
            Times.Exactly(6));
    }

    [Fact]
    public async Task GenerateRoadmapAsync_SynthesizerPartialOutput_MergesWithPipelineData()
    {
        // Arrange
        var request = CreateSampleRequest();
        SetupAgentResponses();

        // Synthesizer returns only prose fields (no structured data)
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-roadmap-synthesizer",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecializedTaskResult
            {
                TaskId = "task-synth",
                AgentId = "roadmapcrew-roadmap-synthesizer",
                Success = true,
                Output = JsonSerializer.Serialize(new
                {
                    executiveSummary = "Synthesizer-generated summary.",
                    quarterlyPlans = new[] { new { quarter = "2026-Q3", scheduledItems = new[] { "WI-1" } } },
                    decisionsNeeded = new[] { "Choose auth provider" }
                }, JsonOptions)
            });

        // Act
        var result = await _engine.GenerateRoadmapAsync(request);

        // Assert — prose from synthesizer, structured data from pipeline stages
        result.ExecutiveSummary.Should().Be("Synthesizer-generated summary.");
        result.DecisionsNeeded.Should().Contain("Choose auth provider");
        result.QuarterlyPlans.Should().HaveCount(1);

        // Structured data should come from pipeline, not synthesizer
        result.VisionAssessment.ApprovedItems.Should().NotBeEmpty();
        result.PrioritizedBacklog.RankedItems.Should().NotBeEmpty();
        result.DependencyGraph.Edges.Should().NotBeEmpty();
        result.FeasibilityAssessment.Assessments.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AssessAlignmentAsync_OnlyCallsVisionKeeper()
    {
        // Arrange
        var items = new List<WorkItem>
        {
            new() { Id = "WI-1", Title = "Test item", Description = "Test" }
        };
        var vision = new VisionContext
        {
            VisionStatement = "Test vision"
        };

        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-vision-keeper",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResult("roadmapcrew-vision-keeper", new VisionAssessment
            {
                ApprovedItems = [new AnnotatedWorkItem { Item = items[0], AlignmentScore = 8.0, AlignmentRationale = "Good" }]
            }));

        // Act
        var result = await _engine.AssessAlignmentAsync(items, vision);

        // Assert — only VisionKeeper called, no other agents
        _agentPortMock.Verify(
            x => x.ExecuteTaskAsync(
                "roadmapcrew-vision-keeper",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _agentPortMock.Verify(
            x => x.ExecuteTaskAsync(
                It.Is<string>(id => id != "roadmapcrew-vision-keeper"),
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        result.ApprovedItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task GenerateRoadmapAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        await _engine.Invoking(e => e.GenerateRoadmapAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GenerateRoadmapAsync_TokenUsage_LoggedForEachAgent()
    {
        // Arrange
        var request = CreateSampleRequest();
        SetupAgentResponses();

        // Act
        await _engine.GenerateRoadmapAsync(request);

        // Assert — verify debug logging was called for each agent's token usage (on pipeline logger)
        _pipelineLoggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("tokens used")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(6));
    }

    [Fact]
    public void TeamId_ReturnsRoadmapcrew()
    {
        _engine.TeamId.Should().Be("roadmapcrew");
    }

    [Fact]
    public void AgentCount_ReturnsSix()
    {
        _engine.AgentCount.Should().Be(6);
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    private void SetupRemainingAgentsAfterMarketScanner()
    {
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-priority-ranker",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResult("roadmapcrew-priority-ranker", new PriorityRanking
            {
                RankedItems =
                [
                    new ScoredWorkItem { WorkItemId = "WI-1", Title = "Auth service", RiceScore = 85.0, WsjfScore = 12.5, ScoringRationale = "High impact", Rank = 1 }
                ]
            }));

        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-dependency-mapper",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResult("roadmapcrew-dependency-mapper", new DependencyAnalysis()));

        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-milestone-tracker",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResult("roadmapcrew-milestone-tracker", new FeasibilityAssessment()));

        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-roadmap-synthesizer",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResult("roadmapcrew-roadmap-synthesizer", new RoadmapDocument
            {
                ExecutiveSummary = "Test summary",
                VisionAssessment = new VisionAssessment(),
                MarketContext = new MarketContextBrief(),
                PrioritizedBacklog = new PriorityRanking(),
                DependencyGraph = new DependencyAnalysis(),
                FeasibilityAssessment = new FeasibilityAssessment()
            }));
    }

    private static RoadmapCrewRequest CreateSampleRequest() => new()
    {
        ProjectId = "test-project",
        ProposedItems =
        [
            new WorkItem
            {
                Id = "WI-1",
                Title = "Implement auth service",
                Description = "OAuth2 + JWT authentication",
                Category = "infrastructure",
                EstimatedEffort = 13,
                Owner = "Platform Team"
            },
            new WorkItem
            {
                Id = "WI-2",
                Title = "Add dashboard analytics",
                Description = "Real-time metrics dashboard",
                Category = "feature",
                EstimatedEffort = 8,
                Owner = "Product Team"
            }
        ],
        Vision = new VisionContext
        {
            VisionStatement = "Build the leading AI orchestration platform",
            OKRs = ["Launch v2.0 by Q3 2026", "Achieve 99.9% uptime"],
            StrategicThemes = ["Platform reliability", "Developer experience", "Enterprise readiness"]
        },
        IndustryContext = "AI/ML infrastructure",
        Competitors = ["LangChain", "CrewAI", "AutoGen"]
    };

    private void SetupAgentResponses()
    {
        var request = CreateSampleRequest();
        SetupVisionKeeperSuccess(request);

        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-market-scanner",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResult("roadmapcrew-market-scanner", new MarketContextBrief
            {
                Trends = [new MarketSignal { Description = "Agent frameworks consolidating", Confidence = ConfidenceLevel.High }],
                AccelerateItems = [new MarketAdjustment { WorkItemId = "WI-1", Action = "accelerate", Rationale = "Competitive pressure", Confidence = ConfidenceLevel.Medium }]
            }));

        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-priority-ranker",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResult("roadmapcrew-priority-ranker", new PriorityRanking
            {
                RankedItems =
                [
                    new ScoredWorkItem { WorkItemId = "WI-1", Title = "Implement auth service", RiceScore = 85.0, WsjfScore = 12.5, ScoringRationale = "High reach, high impact", Rank = 1 },
                    new ScoredWorkItem { WorkItemId = "WI-2", Title = "Add dashboard analytics", RiceScore = 60.0, WsjfScore = 8.0, ScoringRationale = "Medium reach, medium impact", Rank = 2 }
                ]
            }));

        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-dependency-mapper",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResult("roadmapcrew-dependency-mapper", new DependencyAnalysis
            {
                Edges = [new DependencyEdge { FromItemId = "WI-2", ToItemId = "WI-1", Type = DependencyType.Requires }],
                CriticalPath = ["WI-1", "WI-2"]
            }));

        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-milestone-tracker",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResult("roadmapcrew-milestone-tracker", new FeasibilityAssessment
            {
                Assessments =
                [
                    new MilestoneAssessment { WorkItemId = "WI-1", Status = MilestoneStatus.OnTrack, RiskLevel = RiskLevel.Low, Rationale = "Team has capacity" },
                    new MilestoneAssessment { WorkItemId = "WI-2", Status = MilestoneStatus.AtRisk, RiskLevel = RiskLevel.Medium, Rationale = "Depends on WI-1 completion" }
                ],
                CorrectionFactor = 1.3
            }));

        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-roadmap-synthesizer",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResult("roadmapcrew-roadmap-synthesizer", new RoadmapDocument
            {
                ExecutiveSummary = "Two items prioritized for Q2 2026. Auth service is critical path.",
                QuarterlyPlans = [new QuarterlyPlan { Quarter = "2026-Q2", ScheduledItems = ["WI-1", "WI-2"] }],
                RiskRegister = [new RiskEntry { Description = "WI-2 depends on WI-1", Level = RiskLevel.Medium }],
                DecisionsNeeded = ["Confirm auth provider choice"],
                VisionAssessment = new VisionAssessment(),
                MarketContext = new MarketContextBrief(),
                PrioritizedBacklog = new PriorityRanking(),
                DependencyGraph = new DependencyAnalysis(),
                FeasibilityAssessment = new FeasibilityAssessment()
            }));
    }

    private void SetupVisionKeeperSuccess(RoadmapCrewRequest request)
    {
        var assessment = new VisionAssessment
        {
            ApprovedItems = request.ProposedItems.Select(item => new AnnotatedWorkItem
            {
                Item = item,
                AlignmentScore = 7.5,
                AlignmentRationale = "Aligns with platform reliability theme"
            }).ToList()
        };

        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(
                "roadmapcrew-vision-keeper",
                It.IsAny<SpecializedTask>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResult("roadmapcrew-vision-keeper", assessment));
    }

    private static SpecializedTaskResult CreateSuccessResult<T>(string agentId, T output) => new()
    {
        TaskId = Guid.NewGuid().ToString(),
        AgentId = agentId,
        Success = true,
        Output = JsonSerializer.Serialize(output, JsonOptions),
        TokensUsed = 500
    };
}
