using AgencyLayer.Orchestration.Checkpointing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CognitiveMesh.Tests.AgencyLayer.Orchestration.Checkpointing;

public class InMemoryCheckpointManagerTests
{
    private readonly InMemoryCheckpointManager _sut;
    private readonly Mock<ILogger<InMemoryCheckpointManager>> _logger = new();

    public InMemoryCheckpointManagerTests()
    {
        _sut = new InMemoryCheckpointManager(_logger.Object);
    }

    [Fact]
    public async Task SaveCheckpointAsync_StoresCheckpoint()
    {
        var checkpoint = new ExecutionCheckpoint
        {
            WorkflowId = "wf-1",
            StepNumber = 0,
            StepName = "Step 1",
            Status = ExecutionStepStatus.Completed,
            StateJson = "{\"key\": \"value\"}"
        };

        await _sut.SaveCheckpointAsync(checkpoint);

        var retrieved = await _sut.GetCheckpointAsync("wf-1", checkpoint.CheckpointId);
        retrieved.Should().NotBeNull();
        retrieved!.StepName.Should().Be("Step 1");
        retrieved.StateJson.Should().Be("{\"key\": \"value\"}");
    }

    [Fact]
    public async Task GetLatestCheckpointAsync_ReturnsHighestStepNumber()
    {
        await _sut.SaveCheckpointAsync(new ExecutionCheckpoint { WorkflowId = "wf-1", StepNumber = 0, StepName = "First" });
        await _sut.SaveCheckpointAsync(new ExecutionCheckpoint { WorkflowId = "wf-1", StepNumber = 1, StepName = "Second" });
        await _sut.SaveCheckpointAsync(new ExecutionCheckpoint { WorkflowId = "wf-1", StepNumber = 2, StepName = "Third" });

        var latest = await _sut.GetLatestCheckpointAsync("wf-1");
        latest.Should().NotBeNull();
        latest!.StepName.Should().Be("Third");
        latest.StepNumber.Should().Be(2);
    }

    [Fact]
    public async Task GetWorkflowCheckpointsAsync_ReturnsAllInOrder()
    {
        await _sut.SaveCheckpointAsync(new ExecutionCheckpoint { WorkflowId = "wf-1", StepNumber = 2, StepName = "Third" });
        await _sut.SaveCheckpointAsync(new ExecutionCheckpoint { WorkflowId = "wf-1", StepNumber = 0, StepName = "First" });
        await _sut.SaveCheckpointAsync(new ExecutionCheckpoint { WorkflowId = "wf-1", StepNumber = 1, StepName = "Second" });

        var all = (await _sut.GetWorkflowCheckpointsAsync("wf-1")).ToList();
        all.Should().HaveCount(3);
        all[0].StepName.Should().Be("First");
        all[1].StepName.Should().Be("Second");
        all[2].StepName.Should().Be("Third");
    }

    [Fact]
    public async Task PurgeWorkflowCheckpointsAsync_RemovesAll()
    {
        await _sut.SaveCheckpointAsync(new ExecutionCheckpoint { WorkflowId = "wf-1", StepNumber = 0 });
        await _sut.SaveCheckpointAsync(new ExecutionCheckpoint { WorkflowId = "wf-1", StepNumber = 1 });

        await _sut.PurgeWorkflowCheckpointsAsync("wf-1");

        var latest = await _sut.GetLatestCheckpointAsync("wf-1");
        latest.Should().BeNull();
    }

    [Fact]
    public async Task GetCheckpointAsync_NonExistentWorkflow_ReturnsNull()
    {
        var result = await _sut.GetCheckpointAsync("non-existent", "cp-1");
        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveCheckpointAsync_WithPersistCallback_InvokesCallback()
    {
        ExecutionCheckpoint? callbackCheckpoint = null;
        var manager = new InMemoryCheckpointManager(_logger.Object, cp =>
        {
            callbackCheckpoint = cp;
            return Task.CompletedTask;
        });

        var checkpoint = new ExecutionCheckpoint { WorkflowId = "wf-1", StepNumber = 0 };
        await manager.SaveCheckpointAsync(checkpoint);

        callbackCheckpoint.Should().NotBeNull();
        callbackCheckpoint!.WorkflowId.Should().Be("wf-1");
    }

    [Fact]
    public async Task TotalCheckpointCount_TracksAcrossWorkflows()
    {
        await _sut.SaveCheckpointAsync(new ExecutionCheckpoint { WorkflowId = "wf-1", StepNumber = 0 });
        await _sut.SaveCheckpointAsync(new ExecutionCheckpoint { WorkflowId = "wf-1", StepNumber = 1 });
        await _sut.SaveCheckpointAsync(new ExecutionCheckpoint { WorkflowId = "wf-2", StepNumber = 0 });

        _sut.TotalCheckpointCount.Should().Be(3);
    }

    [Fact]
    public async Task IsolatesWorkflows()
    {
        await _sut.SaveCheckpointAsync(new ExecutionCheckpoint { WorkflowId = "wf-1", StepNumber = 0, StepName = "WF1-Step" });
        await _sut.SaveCheckpointAsync(new ExecutionCheckpoint { WorkflowId = "wf-2", StepNumber = 0, StepName = "WF2-Step" });

        var wf1Latest = await _sut.GetLatestCheckpointAsync("wf-1");
        var wf2Latest = await _sut.GetLatestCheckpointAsync("wf-2");

        wf1Latest!.StepName.Should().Be("WF1-Step");
        wf2Latest!.StepName.Should().Be("WF2-Step");
    }
}
