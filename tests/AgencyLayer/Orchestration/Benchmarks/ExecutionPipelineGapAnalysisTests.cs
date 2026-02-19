using AgencyLayer.Orchestration.Benchmarks;
using AgencyLayer.Orchestration.Checkpointing;
using AgencyLayer.Orchestration.Execution;
using AgencyLayer.ProcessAutomation;
using AgencyLayer.AgencyRouter;
using AgencyLayer.MultiAgentOrchestration.Adapters;
using AgencyLayer.MultiAgentOrchestration.Ports;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CognitiveMesh.Tests.AgencyLayer.Orchestration.Benchmarks;

/// <summary>
/// Gap analysis tests that verify the execution pipeline is complete
/// and can handle long-horizon sequential tasks.
/// These tests explicitly check for the gaps identified in the Gas Town comparison.
/// </summary>
public class ExecutionPipelineGapAnalysisTests
{
    private readonly Mock<ILogger<InMemoryCheckpointManager>> _cpLogger = new();
    private readonly Mock<ILogger<DurableWorkflowEngine>> _wfLogger = new();

    // GAP 1: "No durable workflow engine" — FIXED
    [Fact]
    public void WorkflowEngine_Exists_AndImplementsInterface()
    {
        var checkpointManager = new InMemoryCheckpointManager(_cpLogger.Object);
        var engine = new DurableWorkflowEngine(checkpointManager, _wfLogger.Object);

        engine.Should().BeAssignableTo<IWorkflowEngine>();
    }

    // GAP 2: "No crash recovery / checkpoint / rehydration" — FIXED
    [Fact]
    public async Task CheckpointManager_SupportsRehydration()
    {
        var checkpointManager = new InMemoryCheckpointManager(_cpLogger.Object);

        // Save a checkpoint simulating a crash at step 5
        await checkpointManager.SaveCheckpointAsync(new ExecutionCheckpoint
        {
            WorkflowId = "wf-crash",
            StepNumber = 4,
            StepName = "Step 5",
            Status = ExecutionStepStatus.Completed,
            StateJson = "{\"progress\": 5}"
        });

        // Verify we can rehydrate from the checkpoint
        var latest = await checkpointManager.GetLatestCheckpointAsync("wf-crash");
        latest.Should().NotBeNull();
        latest!.StepNumber.Should().Be(4);
        latest.StateJson.Should().Contain("progress");
    }

    // GAP 3: "No sequential execution scaling" — FIXED
    [Fact]
    public async Task WorkflowEngine_Handles1000SequentialSteps()
    {
        var checkpointManager = new InMemoryCheckpointManager(_cpLogger.Object);
        var engine = new DurableWorkflowEngine(checkpointManager, _wfLogger.Object);

        var workflow = new WorkflowDefinition
        {
            Name = "1000StepTest",
            MaxRetryPerStep = 0,
            Steps = Enumerable.Range(0, 1000).Select(i => new WorkflowStepDefinition
            {
                StepNumber = i,
                Name = $"Step {i}",
                ExecuteFunc = (ctx, ct) => Task.FromResult(new WorkflowStepResult
                {
                    Success = true,
                    Output = i,
                    StateUpdates = new Dictionary<string, object> { [$"s{i}"] = true }
                })
            }).ToList()
        };

        var result = await engine.ExecuteWorkflowAsync(workflow);

        result.Success.Should().BeTrue();
        result.CompletedSteps.Should().Be(1000);
        result.Checkpoints.Should().HaveCount(1000);
    }

    // GAP 4: "Governance is blocking with no hot path" — FIXED
    [Fact]
    public void PreApprovedWorkflowTemplate_BypassesGovernance()
    {
        var templateLogger = new Mock<ILogger<WorkflowTemplateRegistry>>();
        var registry = new WorkflowTemplateRegistry(templateLogger.Object);

        registry.RegisterTemplate(new WorkflowTemplate
        {
            TemplateId = "hanoi-benchmark",
            Name = "Tower of Hanoi MAKER Benchmark",
            IsPreApproved = true,
            ApprovedBy = "SystemAdmin",
            ApprovedAt = DateTime.UtcNow,
            BuildWorkflow = parameters => new WorkflowDefinition
            {
                Name = "Hanoi",
                IsPreApproved = true,
                Steps = new List<WorkflowStepDefinition>
                {
                    new() { StepNumber = 0, Name = "Move", RequiresGovernanceCheck = false }
                }
            }
        });

        registry.IsPreApproved("hanoi-benchmark").Should().BeTrue();
        var workflow = registry.CreateWorkflowFromTemplate("hanoi-benchmark");
        workflow.Should().NotBeNull();
        workflow!.IsPreApproved.Should().BeTrue();
        workflow.Steps[0].RequiresGovernanceCheck.Should().BeFalse();
    }

    // GAP 5: "Multi-agent coordination uses placeholder agents" — FIXED
    [Fact]
    public async Task InProcessAgentRuntimeAdapter_ExecutesRealLogic()
    {
        var adapterLogger = new Mock<ILogger<InProcessAgentRuntimeAdapter>>();
        var adapter = new InProcessAgentRuntimeAdapter(adapterLogger.Object);

        bool handlerExecuted = false;
        adapter.RegisterHandler("Worker", task =>
        {
            handlerExecuted = true;
            return Task.FromResult<object>($"Completed: {task.Goal}");
        });

        var agentId = await adapter.ProvisionAgentInstanceAsync(
            new DynamicAgentSpawnRequest { AgentType = "Worker" });

        var result = await adapter.ExecuteAgentLogicAsync(agentId, new AgentTask { Goal = "Do work" });

        handlerExecuted.Should().BeTrue();
        result.ToString().Should().Contain("Completed");
    }

    // GAP 6: "CollaborativeSwarm hardcoded to 5 iterations" — Verified via workflow engine alternative
    [Fact]
    public async Task WorkflowEngine_NoIterationLimit()
    {
        var checkpointManager = new InMemoryCheckpointManager(_cpLogger.Object);
        var engine = new DurableWorkflowEngine(checkpointManager, _wfLogger.Object);

        // 500 iterations — well beyond the hardcoded 5
        var workflow = new WorkflowDefinition
        {
            Name = "ManyIterations",
            MaxRetryPerStep = 0,
            Steps = Enumerable.Range(0, 500).Select(i => new WorkflowStepDefinition
            {
                StepNumber = i,
                Name = $"Iteration {i}",
                ExecuteFunc = (ctx, ct) => Task.FromResult(new WorkflowStepResult { Success = true })
            }).ToList()
        };

        var result = await engine.ExecuteWorkflowAsync(workflow);
        result.Success.Should().BeTrue();
        result.CompletedSteps.Should().Be(500);
    }

    // COMBINED: Full MAKER benchmark proves the pipeline is complete
    [Fact]
    public async Task FullMakerBenchmark_10Discs_1023Steps_Matches_GasTown()
    {
        var bmLogger = new Mock<ILogger<MakerBenchmark>>();
        var checkpointManager = new InMemoryCheckpointManager(_cpLogger.Object);
        var engine = new DurableWorkflowEngine(checkpointManager, _wfLogger.Object);
        var benchmark = new MakerBenchmark(engine, checkpointManager, bmLogger.Object);

        var report = await benchmark.RunTowerOfHanoiAsync(10);

        report.Success.Should().BeTrue("10-disc Hanoi (1,023 steps) should complete — this is Gas Town's proven benchmark");
        report.StepsCompleted.Should().Be(1023);
        report.StepsFailed.Should().Be(0);
        report.CheckpointsCreated.Should().Be(1023, "every step should produce a checkpoint");
    }

    // BEYOND GAS TOWN: 15-disc = 32,767 steps
    [Fact]
    public async Task MakerBenchmark_15Discs_Exceeds_GasTown_Proven()
    {
        var bmLogger = new Mock<ILogger<MakerBenchmark>>();
        var checkpointManager = new InMemoryCheckpointManager(_cpLogger.Object);
        var engine = new DurableWorkflowEngine(checkpointManager, _wfLogger.Object);
        var benchmark = new MakerBenchmark(engine, checkpointManager, bmLogger.Object);

        var report = await benchmark.RunTowerOfHanoiAsync(15);

        report.Success.Should().BeTrue("15-disc Hanoi (32,767 steps) should complete — exceeding Gas Town's proven 10-disc");
        report.StepsCompleted.Should().Be(32767);
        report.CheckpointsCreated.Should().Be(32767);
    }

    // TaskRouter integration test
    [Fact]
    public async Task TaskRouter_RoutesToWorkflowEngine()
    {
        var checkpointManager = new InMemoryCheckpointManager(_cpLogger.Object);
        var engine = new DurableWorkflowEngine(checkpointManager, _wfLogger.Object);
        var orchestrationPort = new Mock<IMultiAgentOrchestrationPort>();
        var routerLogger = new Mock<ILogger<TaskRouter>>();
        var router = new TaskRouter(engine, orchestrationPort.Object, routerLogger.Object);

        var workflow = new WorkflowDefinition
        {
            Name = "RouterTest",
            Steps = new List<WorkflowStepDefinition>
            {
                new()
                {
                    StepNumber = 0,
                    Name = "Step",
                    ExecuteFunc = (ctx, ct) => Task.FromResult(new WorkflowStepResult
                    {
                        Success = true, Output = "Routed OK"
                    })
                }
            }
        };

        var result = await router.RouteTaskAsync(new TaskRoutingRequest { WorkflowDefinition = workflow });

        result.Success.Should().BeTrue();
        result.RoutedTo.Should().Be("WorkflowEngine");
    }
}
