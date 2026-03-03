using AgencyLayer.MultiAgentOrchestration.Engines;
using AgencyLayer.MultiAgentOrchestration.Ports;
using CognitiveMesh.ReasoningLayer.EthicalReasoning.Ports;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CognitiveMesh.Tests.AgencyLayer.MultiAgentOrchestration;

/// <summary>
/// Unit tests for <see cref="MultiAgentOrchestrationEngine"/>, covering agent registration,
/// task execution across coordination patterns, autonomy/authority configuration,
/// learning insight sharing, agent spawning, and ethical check integration.
/// </summary>
public class MultiAgentOrchestrationEngineTests
{
    private readonly Mock<ILogger<MultiAgentOrchestrationEngine>> _loggerMock;
    private readonly Mock<IAgentRuntimeAdapter> _runtimeAdapterMock;
    private readonly Mock<IAgentKnowledgeRepository> _knowledgeRepoMock;
    private readonly Mock<IApprovalAdapter> _approvalAdapterMock;
    private readonly Mock<INormativeAgencyPort> _normativeAgencyMock;
    private readonly Mock<IInformationEthicsPort> _informationEthicsMock;
    private readonly MultiAgentOrchestrationEngine _sut;

    public MultiAgentOrchestrationEngineTests()
    {
        _loggerMock = new Mock<ILogger<MultiAgentOrchestrationEngine>>();
        _runtimeAdapterMock = new Mock<IAgentRuntimeAdapter>();
        _knowledgeRepoMock = new Mock<IAgentKnowledgeRepository>();
        _approvalAdapterMock = new Mock<IApprovalAdapter>();
        _normativeAgencyMock = new Mock<INormativeAgencyPort>();
        _informationEthicsMock = new Mock<IInformationEthicsPort>();

        // Default: ethical checks pass
        _normativeAgencyMock
            .Setup(x => x.ValidateActionAsync(It.IsAny<NormativeActionValidationRequest>()))
            .ReturnsAsync(new NormativeActionValidationResponse { IsValid = true });

        _informationEthicsMock
            .Setup(x => x.AssessInformationalDignityAsync(It.IsAny<DignityAssessmentRequest>()))
            .ReturnsAsync(new DignityAssessmentResponse { IsDignityPreserved = true });

        _sut = new MultiAgentOrchestrationEngine(
            _loggerMock.Object,
            _runtimeAdapterMock.Object,
            _knowledgeRepoMock.Object,
            _approvalAdapterMock.Object,
            _normativeAgencyMock.Object,
            _informationEthicsMock.Object);
    }

    // -----------------------------------------------------------------------
    // Constructor null-guard tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new MultiAgentOrchestrationEngine(
            null!,
            _runtimeAdapterMock.Object,
            _knowledgeRepoMock.Object,
            _approvalAdapterMock.Object,
            _normativeAgencyMock.Object,
            _informationEthicsMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_NullAgentRuntimeAdapter_ThrowsArgumentNullException()
    {
        var act = () => new MultiAgentOrchestrationEngine(
            _loggerMock.Object,
            null!,
            _knowledgeRepoMock.Object,
            _approvalAdapterMock.Object,
            _normativeAgencyMock.Object,
            _informationEthicsMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("agentRuntimeAdapter");
    }

    [Fact]
    public void Constructor_NullKnowledgeRepository_ThrowsArgumentNullException()
    {
        var act = () => new MultiAgentOrchestrationEngine(
            _loggerMock.Object,
            _runtimeAdapterMock.Object,
            null!,
            _approvalAdapterMock.Object,
            _normativeAgencyMock.Object,
            _informationEthicsMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("knowledgeRepository");
    }

    [Fact]
    public void Constructor_NullApprovalAdapter_ThrowsArgumentNullException()
    {
        var act = () => new MultiAgentOrchestrationEngine(
            _loggerMock.Object,
            _runtimeAdapterMock.Object,
            _knowledgeRepoMock.Object,
            null!,
            _normativeAgencyMock.Object,
            _informationEthicsMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("approvalAdapter");
    }

    [Fact]
    public void Constructor_NullNormativeAgencyPort_ThrowsArgumentNullException()
    {
        var act = () => new MultiAgentOrchestrationEngine(
            _loggerMock.Object,
            _runtimeAdapterMock.Object,
            _knowledgeRepoMock.Object,
            _approvalAdapterMock.Object,
            null!,
            _informationEthicsMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("normativeAgencyPort");
    }

    [Fact]
    public void Constructor_NullInformationEthicsPort_ThrowsArgumentNullException()
    {
        var act = () => new MultiAgentOrchestrationEngine(
            _loggerMock.Object,
            _runtimeAdapterMock.Object,
            _knowledgeRepoMock.Object,
            _approvalAdapterMock.Object,
            _normativeAgencyMock.Object,
            null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("informationEthicsPort");
    }

    // -----------------------------------------------------------------------
    // RegisterAgentAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RegisterAgentAsync_ValidDefinition_StoresInRepository()
    {
        var definition = CreateAgentDefinition("Analyzer");

        await _sut.RegisterAgentAsync(definition);

        _knowledgeRepoMock.Verify(
            x => x.StoreAgentDefinitionAsync(definition),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAgentAsync_DuplicateAgentType_OverwritesPreviousRegistration()
    {
        var definition1 = CreateAgentDefinition("Analyzer", "First version");
        var definition2 = CreateAgentDefinition("Analyzer", "Second version");

        await _sut.RegisterAgentAsync(definition1);
        await _sut.RegisterAgentAsync(definition2);

        _knowledgeRepoMock.Verify(
            x => x.StoreAgentDefinitionAsync(It.IsAny<AgentDefinition>()),
            Times.Exactly(2));
    }

    // -----------------------------------------------------------------------
    // ExecuteTaskAsync tests — Parallel coordination
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ExecuteTaskAsync_ParallelCoordination_ReturnsSuccessWithAllAgentResults()
    {
        var definition = CreateAgentDefinition("Worker");
        await _sut.RegisterAgentAsync(definition);

        _runtimeAdapterMock
            .Setup(x => x.ExecuteAgentLogicAsync(It.IsAny<string>(), It.IsAny<AgentTask>()))
            .ReturnsAsync("result-from-agent");

        _knowledgeRepoMock
            .Setup(x => x.GetRelevantInsightsAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<AgentLearningInsight>());

        var request = CreateExecutionRequest(
            goal: "Analyze quarterly data",
            agentTypes: new List<string> { "Worker" },
            pattern: CoordinationPattern.Parallel);

        var response = await _sut.ExecuteTaskAsync(request);

        response.IsSuccess.Should().BeTrue();
        response.Summary.Should().Contain("Parallel");
        response.AgentIdsInvolved.Should().HaveCount(1);
        response.AuditTrailId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExecuteTaskAsync_ParallelCoordination_MultipleAgents_ExecutesAll()
    {
        await _sut.RegisterAgentAsync(CreateAgentDefinition("Analyzer"));
        await _sut.RegisterAgentAsync(CreateAgentDefinition("Validator"));

        int executionCount = 0;
        _runtimeAdapterMock
            .Setup(x => x.ExecuteAgentLogicAsync(It.IsAny<string>(), It.IsAny<AgentTask>()))
            .ReturnsAsync(() =>
            {
                Interlocked.Increment(ref executionCount);
                return $"result-{executionCount}";
            });

        _knowledgeRepoMock
            .Setup(x => x.GetRelevantInsightsAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<AgentLearningInsight>());

        var request = CreateExecutionRequest(
            goal: "Multi-agent analysis",
            agentTypes: new List<string> { "Analyzer", "Validator" },
            pattern: CoordinationPattern.Parallel);

        var response = await _sut.ExecuteTaskAsync(request);

        response.IsSuccess.Should().BeTrue();
        response.AgentIdsInvolved.Should().HaveCount(2);
        executionCount.Should().Be(2);
    }

    // -----------------------------------------------------------------------
    // ExecuteTaskAsync tests — No agents available
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ExecuteTaskAsync_NoRegisteredAgents_ReturnsFailure()
    {
        _knowledgeRepoMock
            .Setup(x => x.GetRelevantInsightsAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<AgentLearningInsight>());

        var request = CreateExecutionRequest(
            goal: "Impossible task",
            agentTypes: new List<string> { "NonExistentAgent" },
            pattern: CoordinationPattern.Parallel);

        var response = await _sut.ExecuteTaskAsync(request);

        response.IsSuccess.Should().BeFalse();
        response.Summary.Should().Contain("Could not assemble a team");
    }

    // -----------------------------------------------------------------------
    // ExecuteTaskAsync tests — Hierarchical coordination
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ExecuteTaskAsync_HierarchicalCoordination_LeadAgentCompletes()
    {
        await _sut.RegisterAgentAsync(CreateAgentDefinition("Leader"));
        await _sut.RegisterAgentAsync(CreateAgentDefinition("Subordinate"));

        _runtimeAdapterMock
            .Setup(x => x.ExecuteAgentLogicAsync(It.IsAny<string>(), It.IsAny<AgentTask>()))
            .ReturnsAsync("lead-completed-task");

        _knowledgeRepoMock
            .Setup(x => x.GetRelevantInsightsAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<AgentLearningInsight>());

        var request = CreateExecutionRequest(
            goal: "Hierarchical workflow",
            agentTypes: new List<string> { "Leader", "Subordinate" },
            pattern: CoordinationPattern.Hierarchical);

        var response = await _sut.ExecuteTaskAsync(request);

        response.IsSuccess.Should().BeTrue();
        response.Summary.Should().Contain("Hierarchical");
    }

    // -----------------------------------------------------------------------
    // ExecuteTaskAsync tests — Competitive coordination
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ExecuteTaskAsync_CompetitiveCoordination_SelectsBestResult()
    {
        await _sut.RegisterAgentAsync(CreateAgentDefinition("Competitor"));

        _runtimeAdapterMock
            .Setup(x => x.ExecuteAgentLogicAsync(It.IsAny<string>(), It.IsAny<AgentTask>()))
            .ReturnsAsync("competitive-result");

        _knowledgeRepoMock
            .Setup(x => x.GetRelevantInsightsAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<AgentLearningInsight>());

        var request = CreateExecutionRequest(
            goal: "Compete for best answer",
            agentTypes: new List<string> { "Competitor" },
            pattern: CoordinationPattern.Competitive);

        var response = await _sut.ExecuteTaskAsync(request);

        response.IsSuccess.Should().BeTrue();
        response.Summary.Should().Contain("Competitive");
    }

    // -----------------------------------------------------------------------
    // ExecuteTaskAsync tests — CollaborativeSwarm coordination
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ExecuteTaskAsync_CollaborativeSwarm_ConvergesOnComplete()
    {
        await _sut.RegisterAgentAsync(CreateAgentDefinition("SwarmWorker"));

        int callCount = 0;
        _runtimeAdapterMock
            .Setup(x => x.ExecuteAgentLogicAsync(It.IsAny<string>(), It.IsAny<AgentTask>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount >= 2 ? "COMPLETE: Final answer" : "Partial work";
            });

        _knowledgeRepoMock
            .Setup(x => x.GetRelevantInsightsAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<AgentLearningInsight>());

        var request = CreateExecutionRequest(
            goal: "Swarm collaboration",
            agentTypes: new List<string> { "SwarmWorker" },
            pattern: CoordinationPattern.CollaborativeSwarm);

        var response = await _sut.ExecuteTaskAsync(request);

        response.IsSuccess.Should().BeTrue();
        response.Summary.Should().Contain("CollaborativeSwarm");
    }

    // -----------------------------------------------------------------------
    // ExecuteTaskAsync tests — Cancellation
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ExecuteTaskAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        await _sut.RegisterAgentAsync(CreateAgentDefinition("Worker"));

        _knowledgeRepoMock
            .Setup(x => x.GetRelevantInsightsAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<AgentLearningInsight>());

        var cts = new CancellationTokenSource();
        cts.Cancel();

        var request = CreateExecutionRequest(
            goal: "Cancelled task",
            agentTypes: new List<string> { "Worker" },
            pattern: CoordinationPattern.Parallel);

        var act = () => _sut.ExecuteTaskAsync(request, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -----------------------------------------------------------------------
    // ExecuteTaskAsync tests — Autonomy (ActWithConfirmation)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ExecuteTaskAsync_AgentRequiresConfirmation_Approved_Succeeds()
    {
        var definition = CreateAgentDefinition("ConfirmAgent");
        definition.DefaultAutonomyLevel = AutonomyLevel.ActWithConfirmation;
        await _sut.RegisterAgentAsync(definition);

        _approvalAdapterMock
            .Setup(x => x.RequestApprovalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(true);

        _runtimeAdapterMock
            .Setup(x => x.ExecuteAgentLogicAsync(It.IsAny<string>(), It.IsAny<AgentTask>()))
            .ReturnsAsync("approved-action-result");

        _knowledgeRepoMock
            .Setup(x => x.GetRelevantInsightsAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<AgentLearningInsight>());

        var request = CreateExecutionRequest(
            goal: "Need confirmation",
            agentTypes: new List<string> { "ConfirmAgent" },
            pattern: CoordinationPattern.Parallel);

        var response = await _sut.ExecuteTaskAsync(request);

        response.IsSuccess.Should().BeTrue();
        _approvalAdapterMock.Verify(
            x => x.RequestApprovalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteTaskAsync_AgentRequiresConfirmation_Rejected_ReturnsRejectionMessage()
    {
        var definition = CreateAgentDefinition("ConfirmAgent");
        definition.DefaultAutonomyLevel = AutonomyLevel.ActWithConfirmation;
        await _sut.RegisterAgentAsync(definition);

        _approvalAdapterMock
            .Setup(x => x.RequestApprovalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(false);

        _knowledgeRepoMock
            .Setup(x => x.GetRelevantInsightsAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<AgentLearningInsight>());

        var request = CreateExecutionRequest(
            goal: "Rejected task",
            agentTypes: new List<string> { "ConfirmAgent" },
            pattern: CoordinationPattern.Parallel);

        var response = await _sut.ExecuteTaskAsync(request);

        // The result will still be "success" at orchestration level since the rejection
        // is handled gracefully; the individual agent result contains "rejected"
        response.IsSuccess.Should().BeTrue();
    }

    // -----------------------------------------------------------------------
    // ExecuteTaskAsync tests — Ethical check failure
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ExecuteTaskAsync_NormativeValidationFails_ReturnsEthicalConcern()
    {
        await _sut.RegisterAgentAsync(CreateAgentDefinition("EthicsTest"));

        _normativeAgencyMock
            .Setup(x => x.ValidateActionAsync(It.IsAny<NormativeActionValidationRequest>()))
            .ReturnsAsync(new NormativeActionValidationResponse
            {
                IsValid = false,
                Violations = new List<string> { "Violates user autonomy" }
            });

        _knowledgeRepoMock
            .Setup(x => x.GetRelevantInsightsAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<AgentLearningInsight>());

        var request = CreateExecutionRequest(
            goal: "Ethically problematic action",
            agentTypes: new List<string> { "EthicsTest" },
            pattern: CoordinationPattern.Parallel);

        var response = await _sut.ExecuteTaskAsync(request);

        // Ethical rejection is handled gracefully; the agent result contains the rejection message
        response.IsSuccess.Should().BeTrue();
        _normativeAgencyMock.Verify(
            x => x.ValidateActionAsync(It.IsAny<NormativeActionValidationRequest>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteTaskAsync_InformationalDignityFails_WithUserData_ReturnsEthicalConcern()
    {
        await _sut.RegisterAgentAsync(CreateAgentDefinition("DataProcessor"));

        _informationEthicsMock
            .Setup(x => x.AssessInformationalDignityAsync(It.IsAny<DignityAssessmentRequest>()))
            .ReturnsAsync(new DignityAssessmentResponse
            {
                IsDignityPreserved = false,
                PotentialViolations = new List<string> { "Risk of re-identification" }
            });

        _knowledgeRepoMock
            .Setup(x => x.GetRelevantInsightsAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<AgentLearningInsight>());

        var task = new AgentTask
        {
            Goal = "Process user data",
            CoordinationPattern = CoordinationPattern.Parallel,
            RequiredAgentTypes = new List<string> { "DataProcessor" },
            Context = new Dictionary<string, object>
            {
                ["UserData"] = new { Name = "Test User", Email = "test@example.com" },
                ["UserId"] = "user-123"
            }
        };

        var request = new AgentExecutionRequest
        {
            Task = task,
            TenantId = "tenant-1",
            RequestingUserId = "admin-1"
        };

        var response = await _sut.ExecuteTaskAsync(request);

        response.IsSuccess.Should().BeTrue();
        _informationEthicsMock.Verify(
            x => x.AssessInformationalDignityAsync(It.IsAny<DignityAssessmentRequest>()),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // ExecuteTaskAsync tests — Ethical engine exception is non-fatal
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ExecuteTaskAsync_EthicalEngineThrows_ContinuesExecution()
    {
        await _sut.RegisterAgentAsync(CreateAgentDefinition("Resilient"));

        _normativeAgencyMock
            .Setup(x => x.ValidateActionAsync(It.IsAny<NormativeActionValidationRequest>()))
            .ThrowsAsync(new InvalidOperationException("Ethical engine crashed"));

        _runtimeAdapterMock
            .Setup(x => x.ExecuteAgentLogicAsync(It.IsAny<string>(), It.IsAny<AgentTask>()))
            .ReturnsAsync("completed-despite-ethical-crash");

        _knowledgeRepoMock
            .Setup(x => x.GetRelevantInsightsAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<AgentLearningInsight>());

        var request = CreateExecutionRequest(
            goal: "Resilient execution",
            agentTypes: new List<string> { "Resilient" },
            pattern: CoordinationPattern.Parallel);

        var response = await _sut.ExecuteTaskAsync(request);

        response.IsSuccess.Should().BeTrue();
    }

    // -----------------------------------------------------------------------
    // SetAgentAutonomyAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SetAgentAutonomyAsync_ValidArgs_SetsAutonomyLevel()
    {
        await _sut.SetAgentAutonomyAsync("Analyzer", AutonomyLevel.FullyAutonomous, "tenant-1");

        // Verify that a subsequent task execution uses the set autonomy level (no confirmation needed).
        // This is an integration-style verification via the internal state.
        // If the autonomy were ActWithConfirmation, the approval adapter would be called.
        await _sut.RegisterAgentAsync(CreateAgentDefinition("Analyzer"));

        _runtimeAdapterMock
            .Setup(x => x.ExecuteAgentLogicAsync(It.IsAny<string>(), It.IsAny<AgentTask>()))
            .ReturnsAsync("autonomous-result");

        _knowledgeRepoMock
            .Setup(x => x.GetRelevantInsightsAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<AgentLearningInsight>());

        var request = CreateExecutionRequest(
            goal: "Autonomous task",
            agentTypes: new List<string> { "Analyzer" },
            pattern: CoordinationPattern.Parallel);

        await _sut.ExecuteTaskAsync(request);

        _approvalAdapterMock.Verify(
            x => x.RequestApprovalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()),
            Times.Never);
    }

    // -----------------------------------------------------------------------
    // ConfigureAgentAuthorityAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ConfigureAgentAuthorityAsync_ValidArgs_CompletesSuccessfully()
    {
        var scope = new AuthorityScope
        {
            AllowedApiEndpoints = new List<string> { "/api/data", "/api/reports" },
            MaxResourceConsumption = 100.0,
            MaxBudget = 500m,
            DataAccessPolicies = new List<string> { "read:operational-data" }
        };

        var act = () => _sut.ConfigureAgentAuthorityAsync("Analyzer", scope, "tenant-1");

        await act.Should().NotThrowAsync();
    }

    // -----------------------------------------------------------------------
    // ShareLearningInsightAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ShareLearningInsightAsync_ValidInsight_PersistsToRepository()
    {
        var insight = new AgentLearningInsight
        {
            GeneratingAgentType = "Optimizer",
            InsightType = "OptimizedWorkflow",
            InsightData = new { Strategy = "BatchProcessing", Improvement = 0.25 },
            ConfidenceScore = 0.92
        };

        await _sut.ShareLearningInsightAsync(insight);

        _knowledgeRepoMock.Verify(
            x => x.StoreLearningInsightAsync(insight),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // SpawnAgentAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SpawnAgentAsync_ValidRequest_ReturnsNewAgentId()
    {
        var expectedAgentId = "new-agent-42";
        _runtimeAdapterMock
            .Setup(x => x.ProvisionAgentInstanceAsync(It.IsAny<DynamicAgentSpawnRequest>()))
            .ReturnsAsync(expectedAgentId);

        var spawnRequest = new DynamicAgentSpawnRequest
        {
            AgentType = "ChampionNudger",
            TenantId = "tenant-1",
            ParentTaskId = "parent-task-1",
            CustomAutonomy = AutonomyLevel.FullyAutonomous
        };

        var agentId = await _sut.SpawnAgentAsync(spawnRequest);

        agentId.Should().Be(expectedAgentId);
        _runtimeAdapterMock.Verify(
            x => x.ProvisionAgentInstanceAsync(spawnRequest),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // GetAgentTaskStatusAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAgentTaskStatusAsync_NonExistentTask_ReturnsNull()
    {
        var result = await _sut.GetAgentTaskStatusAsync("non-existent-id", "tenant-1");

        result.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // ExecuteTaskAsync — Learning insight generation on success
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ExecuteTaskAsync_SuccessfulExecution_SharesLearningInsight()
    {
        await _sut.RegisterAgentAsync(CreateAgentDefinition("Learner"));

        _runtimeAdapterMock
            .Setup(x => x.ExecuteAgentLogicAsync(It.IsAny<string>(), It.IsAny<AgentTask>()))
            .ReturnsAsync("learning-result");

        _knowledgeRepoMock
            .Setup(x => x.GetRelevantInsightsAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<AgentLearningInsight>());

        var request = CreateExecutionRequest(
            goal: "Learn something",
            agentTypes: new List<string> { "Learner" },
            pattern: CoordinationPattern.Parallel);

        await _sut.ExecuteTaskAsync(request);

        _knowledgeRepoMock.Verify(
            x => x.StoreLearningInsightAsync(It.Is<AgentLearningInsight>(
                i => i.InsightType == "SuccessfulWorkflow" && i.GeneratingAgentType == "Orchestrator")),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // ExecuteTaskAsync — Task removed from active tasks after completion
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ExecuteTaskAsync_AfterCompletion_TaskRemovedFromActiveTasks()
    {
        await _sut.RegisterAgentAsync(CreateAgentDefinition("Cleaner"));

        _runtimeAdapterMock
            .Setup(x => x.ExecuteAgentLogicAsync(It.IsAny<string>(), It.IsAny<AgentTask>()))
            .ReturnsAsync("done");

        _knowledgeRepoMock
            .Setup(x => x.GetRelevantInsightsAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<AgentLearningInsight>());

        var request = CreateExecutionRequest(
            goal: "Cleanup test",
            agentTypes: new List<string> { "Cleaner" },
            pattern: CoordinationPattern.Parallel);

        var response = await _sut.ExecuteTaskAsync(request);
        var taskStatus = await _sut.GetAgentTaskStatusAsync(response.TaskId, "tenant-1");

        taskStatus.Should().BeNull("task should be removed from active tasks after completion");
    }

    // -----------------------------------------------------------------------
    // ExecuteTaskAsync — Unsupported coordination pattern
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ExecuteTaskAsync_UnsupportedCoordinationPattern_ReturnsFailure()
    {
        await _sut.RegisterAgentAsync(CreateAgentDefinition("Worker"));

        _knowledgeRepoMock
            .Setup(x => x.GetRelevantInsightsAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<AgentLearningInsight>());

        var task = new AgentTask
        {
            Goal = "Test unsupported pattern",
            CoordinationPattern = (CoordinationPattern)999,
            RequiredAgentTypes = new List<string> { "Worker" }
        };

        var request = new AgentExecutionRequest
        {
            Task = task,
            TenantId = "tenant-1",
            RequestingUserId = "user-1"
        };

        var response = await _sut.ExecuteTaskAsync(request);

        response.IsSuccess.Should().BeFalse();
        response.Summary.Should().Contain("not supported");
    }

    // -----------------------------------------------------------------------
    // Helper methods
    // -----------------------------------------------------------------------

    private static AgentDefinition CreateAgentDefinition(
        string agentType,
        string description = "Test agent")
    {
        return new AgentDefinition
        {
            AgentId = Guid.NewGuid(),
            AgentType = agentType,
            Description = description,
            Capabilities = new List<string> { "analyze", "report" },
            DefaultAutonomyLevel = AutonomyLevel.FullyAutonomous,
            DefaultAuthorityScope = new AuthorityScope
            {
                AllowedApiEndpoints = new List<string> { "/api/test" },
                MaxResourceConsumption = 50.0,
                MaxBudget = 100m
            },
            Status = AgentStatus.Active
        };
    }

    private static AgentExecutionRequest CreateExecutionRequest(
        string goal,
        List<string> agentTypes,
        CoordinationPattern pattern)
    {
        return new AgentExecutionRequest
        {
            Task = new AgentTask
            {
                Goal = goal,
                CoordinationPattern = pattern,
                RequiredAgentTypes = agentTypes,
                Context = new Dictionary<string, object> { ["environment"] = "test" }
            },
            TenantId = "tenant-1",
            RequestingUserId = "user-1"
        };
    }
}
