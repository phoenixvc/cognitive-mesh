using AgencyLayer.Orchestration.Checkpointing;
using AgencyLayer.Orchestration.Execution;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.Integration;

/// <summary>
/// Integration tests for DurableWorkflowEngine crash recovery.
/// Verifies that workflows can be interrupted, checkpointed, and resumed
/// from the last successful step using real InMemoryCheckpointManager.
/// </summary>
public class DurableWorkflowCrashRecoveryTests
{
    private readonly DurableWorkflowEngine _engine;
    private readonly InMemoryCheckpointManager _checkpointManager;

    public DurableWorkflowCrashRecoveryTests()
    {
        _checkpointManager = new InMemoryCheckpointManager(
            Mock.Of<ILogger<InMemoryCheckpointManager>>());
        _engine = new DurableWorkflowEngine(
            _checkpointManager,
            Mock.Of<ILogger<DurableWorkflowEngine>>());
    }

    [Fact]
    public async Task ExecuteWorkflow_ThreeSteps_AllCheckpointsPersisted()
    {
        // Arrange
        var stepResults = new List<string>();
        var workflow = CreateWorkflow("wf-all-checkpoints", 3, (ctx, ct) =>
        {
            stepResults.Add($"step-{ctx.StepNumber}");
            return Task.FromResult(new WorkflowStepResult
            {
                Success = true,
                Output = $"output-{ctx.StepNumber}",
                StateUpdates = new Dictionary<string, object> { [$"step{ctx.StepNumber}Done"] = true }
            });
        });

        // Act
        var result = await _engine.ExecuteWorkflowAsync(workflow);

        // Assert
        result.Success.Should().BeTrue();
        result.CompletedSteps.Should().Be(3);
        result.Checkpoints.Should().HaveCount(3);
        stepResults.Should().Equal("step-0", "step-1", "step-2");

        // Verify checkpoints are in the checkpoint manager
        var checkpoints = (await _checkpointManager.GetWorkflowCheckpointsAsync(workflow.WorkflowId)).ToList();
        checkpoints.Should().HaveCount(3);
        checkpoints.Should().AllSatisfy(cp =>
            cp.Status.Should().Be(ExecutionStepStatus.Completed));
    }

    [Fact]
    public async Task ResumeWorkflow_AfterFailureAtStep2_ContinuesFromStep2()
    {
        // Arrange — first execution fails at step 2
        var callCount = 0;
        var workflow = CreateWorkflow("wf-resume-from-2", 4, (ctx, ct) =>
        {
            callCount++;
            if (ctx.StepNumber == 2 && callCount <= 6) // Fail on first run through step 2 (all 4 attempts with MaxRetryPerStep=3)
            {
                return Task.FromResult(new WorkflowStepResult
                {
                    Success = false,
                    ErrorMessage = "Simulated transient failure"
                });
            }

            return Task.FromResult(new WorkflowStepResult
            {
                Success = true,
                Output = $"output-{ctx.StepNumber}",
                StateUpdates = new Dictionary<string, object> { [$"step{ctx.StepNumber}"] = "done" }
            });
        });

        // First execution — should fail at step 2
        var firstResult = await _engine.ExecuteWorkflowAsync(workflow);
        firstResult.Success.Should().BeFalse();
        firstResult.CompletedSteps.Should().Be(2); // Steps 0 and 1 completed

        // Verify the checkpoint chain: step 0 (ok), step 1 (ok), step 2 (failed)
        var checkpoints = (await _checkpointManager.GetWorkflowCheckpointsAsync(workflow.WorkflowId)).ToList();
        checkpoints.Should().HaveCount(3);
        checkpoints[0].Status.Should().Be(ExecutionStepStatus.Completed);
        checkpoints[1].Status.Should().Be(ExecutionStepStatus.Completed);
        checkpoints[2].Status.Should().Be(ExecutionStepStatus.Failed);

        // Act — resume (callCount is now >5, so step 2 will succeed)
        var resumeResult = await _engine.ResumeWorkflowAsync(workflow.WorkflowId);

        // Assert — should complete from step 2 onward
        resumeResult.Success.Should().BeTrue();
        resumeResult.CompletedSteps.Should().BeGreaterThanOrEqualTo(2); // At least steps 2,3 completed on resume
    }

    [Fact]
    public async Task ResumeWorkflow_NoCheckpoints_ThrowsInvalidOperation()
    {
        // Arrange — register a workflow but don't execute it
        var workflow = CreateWorkflow("wf-no-checkpoints", 2, (ctx, ct) =>
            Task.FromResult(new WorkflowStepResult { Success = true, Output = "ok" }));

        // Execute and complete so the workflow is registered, then purge checkpoints
        await _engine.ExecuteWorkflowAsync(workflow);
        await _checkpointManager.PurgeWorkflowCheckpointsAsync(workflow.WorkflowId);

        // Act & Assert
        var act = () => _engine.ResumeWorkflowAsync(workflow.WorkflowId);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No checkpoints found*");
    }

    [Fact]
    public async Task ResumeWorkflow_UnregisteredWorkflow_ThrowsInvalidOperation()
    {
        // Act & Assert
        var act = () => _engine.ResumeWorkflowAsync("nonexistent-workflow");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task ExecuteWorkflow_StepPassesContextToNextStep()
    {
        // Arrange — each step reads previous output and accumulates state
        var receivedOutputs = new List<object?>();
        var receivedStates = new List<Dictionary<string, object>>();

        var workflow = CreateWorkflow("wf-context-flow", 3, (ctx, ct) =>
        {
            receivedOutputs.Add(ctx.PreviousStepOutput);
            receivedStates.Add(new Dictionary<string, object>(ctx.State));

            return Task.FromResult(new WorkflowStepResult
            {
                Success = true,
                Output = $"output-{ctx.StepNumber}",
                StateUpdates = new Dictionary<string, object>
                {
                    [$"key{ctx.StepNumber}"] = $"value{ctx.StepNumber}"
                }
            });
        });

        // Act
        var result = await _engine.ExecuteWorkflowAsync(workflow);

        // Assert
        result.Success.Should().BeTrue();
        receivedOutputs[0].Should().BeNull(); // First step has no previous output
        receivedOutputs[1]!.ToString().Should().Be("output-0");
        receivedOutputs[2]!.ToString().Should().Be("output-1");

        // State accumulates across steps
        receivedStates[0].Should().NotContainKey("key0"); // State not yet updated before step 0 runs
        receivedStates[1].Should().ContainKey("key0");
        receivedStates[2].Should().ContainKeys("key0", "key1");
    }

    [Fact]
    public async Task ExecuteWorkflow_WithRetry_SucceedsAfterTransientFailure()
    {
        // Arrange — step 1 fails twice then succeeds
        var step1Attempts = 0;

        var workflow = new WorkflowDefinition
        {
            WorkflowId = "wf-retry-success",
            Name = "Retry Success Test",
            MaxRetryPerStep = 3,
            StepTimeout = TimeSpan.FromSeconds(10),
            Steps = new List<WorkflowStepDefinition>
            {
                new()
                {
                    StepNumber = 0,
                    Name = "Init",
                    ExecuteFunc = (ctx, ct) => Task.FromResult(new WorkflowStepResult
                    {
                        Success = true, Output = "initialized"
                    })
                },
                new()
                {
                    StepNumber = 1,
                    Name = "Flaky Step",
                    ExecuteFunc = (ctx, ct) =>
                    {
                        step1Attempts++;
                        if (step1Attempts <= 2)
                        {
                            return Task.FromResult(new WorkflowStepResult
                            {
                                Success = false, ErrorMessage = "Transient error"
                            });
                        }
                        return Task.FromResult(new WorkflowStepResult
                        {
                            Success = true, Output = "recovered"
                        });
                    }
                },
                new()
                {
                    StepNumber = 2,
                    Name = "Final",
                    ExecuteFunc = (ctx, ct) => Task.FromResult(new WorkflowStepResult
                    {
                        Success = true, Output = "done"
                    })
                }
            }
        };

        // Act
        var result = await _engine.ExecuteWorkflowAsync(workflow);

        // Assert
        result.Success.Should().BeTrue();
        result.CompletedSteps.Should().Be(3);
        step1Attempts.Should().Be(3); // 2 failures + 1 success
    }

    [Fact]
    public async Task ExecuteWorkflow_RetryExhaustion_ProducesFailedCheckpoint()
    {
        // Arrange — step always fails
        var workflow = new WorkflowDefinition
        {
            WorkflowId = "wf-retry-exhaustion",
            Name = "Retry Exhaustion Test",
            MaxRetryPerStep = 2,
            StepTimeout = TimeSpan.FromSeconds(5),
            Steps = new List<WorkflowStepDefinition>
            {
                new()
                {
                    StepNumber = 0,
                    Name = "Setup",
                    ExecuteFunc = (ctx, ct) => Task.FromResult(new WorkflowStepResult
                    {
                        Success = true, Output = "ready"
                    })
                },
                new()
                {
                    StepNumber = 1,
                    Name = "Always Fails",
                    ExecuteFunc = (ctx, ct) => Task.FromResult(new WorkflowStepResult
                    {
                        Success = false, ErrorMessage = "Permanent failure"
                    })
                }
            }
        };

        // Act
        var result = await _engine.ExecuteWorkflowAsync(workflow);

        // Assert
        result.Success.Should().BeFalse();
        result.CompletedSteps.Should().Be(1); // Only step 0 completed
        result.FailedSteps.Should().Be(1);
        result.ErrorMessage.Should().Contain("Always Fails");

        // Verify checkpoint chain
        var checkpoints = (await _checkpointManager.GetWorkflowCheckpointsAsync(workflow.WorkflowId)).ToList();
        checkpoints.Should().HaveCount(2);
        checkpoints[0].Status.Should().Be(ExecutionStepStatus.Completed);
        checkpoints[1].Status.Should().Be(ExecutionStepStatus.Failed);
        checkpoints[1].ErrorMessage.Should().Contain("Permanent failure");
    }

    [Fact]
    public async Task CancelWorkflow_DuringExecution_SavesCancellationCheckpoint()
    {
        // Arrange — step 1 blocks until cancellation
        var step1Started = new TaskCompletionSource<bool>();

        var workflow = new WorkflowDefinition
        {
            WorkflowId = "wf-cancel",
            Name = "Cancellation Test",
            MaxRetryPerStep = 0,
            StepTimeout = TimeSpan.FromSeconds(30),
            Steps = new List<WorkflowStepDefinition>
            {
                new()
                {
                    StepNumber = 0,
                    Name = "Quick Step",
                    ExecuteFunc = (ctx, ct) => Task.FromResult(new WorkflowStepResult
                    {
                        Success = true, Output = "done"
                    })
                },
                new()
                {
                    StepNumber = 1,
                    Name = "Blocking Step",
                    ExecuteFunc = async (ctx, ct) =>
                    {
                        step1Started.SetResult(true);
                        await Task.Delay(TimeSpan.FromMinutes(5), ct); // Will be cancelled
                        return new WorkflowStepResult { Success = true };
                    }
                }
            }
        };

        // Act — start workflow and cancel after step 1 begins
        var executeTask = _engine.ExecuteWorkflowAsync(workflow);
        await step1Started.Task; // Wait for step 1 to start

        await _engine.CancelWorkflowAsync(workflow.WorkflowId);

        // Assert — workflow should throw OperationCanceledException
        var act = () => executeTask;
        await act.Should().ThrowAsync<OperationCanceledException>();

        // Verify status
        var status = await _engine.GetWorkflowStatusAsync(workflow.WorkflowId);
        status.State.Should().Be(WorkflowState.Cancelled);
    }

    [Fact]
    public async Task PurgeCheckpoints_AfterCompletion_CleansUpAllState()
    {
        // Arrange
        var workflow = CreateWorkflow("wf-purge-test", 3, (ctx, ct) =>
            Task.FromResult(new WorkflowStepResult { Success = true, Output = "ok" }));

        await _engine.ExecuteWorkflowAsync(workflow);

        // Pre-condition: checkpoints exist
        var before = (await _checkpointManager.GetWorkflowCheckpointsAsync(workflow.WorkflowId)).ToList();
        before.Should().HaveCount(3);

        // Act
        await _checkpointManager.PurgeWorkflowCheckpointsAsync(workflow.WorkflowId);

        // Assert
        var after = (await _checkpointManager.GetWorkflowCheckpointsAsync(workflow.WorkflowId)).ToList();
        after.Should().BeEmpty();
    }

    [Fact]
    public async Task ConcurrentWorkflows_IndependentCheckpointChains()
    {
        // Arrange — two workflows running concurrently
        var wf1 = CreateWorkflow("wf-concurrent-1", 3, async (ctx, ct) =>
        {
            await Task.Delay(10, ct);
            return new WorkflowStepResult
            {
                Success = true,
                Output = $"wf1-step{ctx.StepNumber}",
                StateUpdates = new Dictionary<string, object> { [$"wf1_{ctx.StepNumber}"] = true }
            };
        });

        var wf2 = CreateWorkflow("wf-concurrent-2", 3, async (ctx, ct) =>
        {
            await Task.Delay(10, ct);
            return new WorkflowStepResult
            {
                Success = true,
                Output = $"wf2-step{ctx.StepNumber}",
                StateUpdates = new Dictionary<string, object> { [$"wf2_{ctx.StepNumber}"] = true }
            };
        });

        // Act
        var results = await Task.WhenAll(
            _engine.ExecuteWorkflowAsync(wf1),
            _engine.ExecuteWorkflowAsync(wf2));

        // Assert — both succeed independently
        results[0].Success.Should().BeTrue();
        results[0].WorkflowId.Should().Be("wf-concurrent-1");
        results[1].Success.Should().BeTrue();
        results[1].WorkflowId.Should().Be("wf-concurrent-2");

        // Checkpoint chains are isolated
        var cp1 = (await _checkpointManager.GetWorkflowCheckpointsAsync("wf-concurrent-1")).ToList();
        var cp2 = (await _checkpointManager.GetWorkflowCheckpointsAsync("wf-concurrent-2")).ToList();
        cp1.Should().HaveCount(3);
        cp2.Should().HaveCount(3);
        cp1.Should().OnlyContain(cp => cp.WorkflowId == "wf-concurrent-1");
        cp2.Should().OnlyContain(cp => cp.WorkflowId == "wf-concurrent-2");
    }

    private static WorkflowDefinition CreateWorkflow(
        string workflowId,
        int stepCount,
        Func<WorkflowStepContext, CancellationToken, Task<WorkflowStepResult>> stepFunc)
    {
        return new WorkflowDefinition
        {
            WorkflowId = workflowId,
            Name = $"Test Workflow {workflowId}",
            MaxRetryPerStep = 3,
            StepTimeout = TimeSpan.FromSeconds(10),
            Steps = Enumerable.Range(0, stepCount)
                .Select(i => new WorkflowStepDefinition
                {
                    StepNumber = i,
                    Name = $"Step {i}",
                    ExecuteFunc = stepFunc
                })
                .ToList()
        };
    }
}
