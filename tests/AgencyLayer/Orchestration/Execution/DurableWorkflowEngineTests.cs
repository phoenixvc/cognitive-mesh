using AgencyLayer.Orchestration.Checkpointing;
using AgencyLayer.Orchestration.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CognitiveMesh.Tests.AgencyLayer.Orchestration.Execution;

public class DurableWorkflowEngineTests
{
    private readonly DurableWorkflowEngine _sut;
    private readonly InMemoryCheckpointManager _checkpointManager;
    private readonly Mock<ILogger<DurableWorkflowEngine>> _logger = new();
    private readonly Mock<ILogger<InMemoryCheckpointManager>> _cpLogger = new();

    public DurableWorkflowEngineTests()
    {
        _checkpointManager = new InMemoryCheckpointManager(_cpLogger.Object);
        _sut = new DurableWorkflowEngine(_checkpointManager, _logger.Object);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_SingleStep_Succeeds()
    {
        var workflow = new WorkflowDefinition
        {
            Name = "SingleStep",
            Steps = new List<WorkflowStepDefinition>
            {
                new()
                {
                    StepNumber = 0,
                    Name = "Step 1",
                    ExecuteFunc = (ctx, ct) => Task.FromResult(new WorkflowStepResult
                    {
                        Success = true,
                        Output = "Hello"
                    })
                }
            }
        };

        var result = await _sut.ExecuteWorkflowAsync(workflow);

        result.Success.Should().BeTrue();
        result.CompletedSteps.Should().Be(1);
        result.TotalSteps.Should().Be(1);
        result.FinalOutput.Should().Be("Hello");
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_MultipleSteps_PassesStateBetweenSteps()
    {
        var workflow = new WorkflowDefinition
        {
            Name = "MultiStep",
            Steps = new List<WorkflowStepDefinition>
            {
                new()
                {
                    StepNumber = 0,
                    Name = "Init",
                    ExecuteFunc = (ctx, ct) => Task.FromResult(new WorkflowStepResult
                    {
                        Success = true,
                        Output = 1,
                        StateUpdates = new Dictionary<string, object> { ["counter"] = 1 }
                    })
                },
                new()
                {
                    StepNumber = 1,
                    Name = "Increment",
                    ExecuteFunc = (ctx, ct) =>
                    {
                        var counter = Convert.ToInt32(ctx.State["counter"]);
                        return Task.FromResult(new WorkflowStepResult
                        {
                            Success = true,
                            Output = counter + 1,
                            StateUpdates = new Dictionary<string, object> { ["counter"] = counter + 1 }
                        });
                    }
                },
                new()
                {
                    StepNumber = 2,
                    Name = "Final",
                    ExecuteFunc = (ctx, ct) =>
                    {
                        var counter = Convert.ToInt32(ctx.State["counter"]);
                        return Task.FromResult(new WorkflowStepResult
                        {
                            Success = true,
                            Output = $"Final value: {counter}"
                        });
                    }
                }
            }
        };

        var result = await _sut.ExecuteWorkflowAsync(workflow);

        result.Success.Should().BeTrue();
        result.CompletedSteps.Should().Be(3);
        result.FinalOutput.Should().Be("Final value: 2");
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_FailAtStep_StopsAndReportsFailure()
    {
        var workflow = new WorkflowDefinition
        {
            Name = "FailAtStep2",
            MaxRetryPerStep = 0,
            Steps = new List<WorkflowStepDefinition>
            {
                new()
                {
                    StepNumber = 0,
                    Name = "Step 1",
                    ExecuteFunc = (ctx, ct) => Task.FromResult(new WorkflowStepResult { Success = true })
                },
                new()
                {
                    StepNumber = 1,
                    Name = "Step 2 - Fail",
                    ExecuteFunc = (ctx, ct) => Task.FromResult(new WorkflowStepResult { Success = false, ErrorMessage = "Boom" })
                },
                new()
                {
                    StepNumber = 2,
                    Name = "Step 3 - Never Reached",
                    ExecuteFunc = (ctx, ct) => Task.FromResult(new WorkflowStepResult { Success = true })
                }
            }
        };

        var result = await _sut.ExecuteWorkflowAsync(workflow);

        result.Success.Should().BeFalse();
        result.CompletedSteps.Should().Be(1);
        result.FailedSteps.Should().Be(1);
        result.ErrorMessage.Should().Contain("Boom");
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithRetry_RetriesOnFailure()
    {
        int attempts = 0;
        var workflow = new WorkflowDefinition
        {
            Name = "RetryTest",
            MaxRetryPerStep = 2,
            Steps = new List<WorkflowStepDefinition>
            {
                new()
                {
                    StepNumber = 0,
                    Name = "Flaky Step",
                    ExecuteFunc = (ctx, ct) =>
                    {
                        attempts++;
                        bool succeed = attempts >= 3; // Succeed on 3rd attempt
                        return Task.FromResult(new WorkflowStepResult
                        {
                            Success = succeed,
                            Output = succeed ? "OK" : null,
                            ErrorMessage = succeed ? null : $"Attempt {attempts} failed"
                        });
                    }
                }
            }
        };

        var result = await _sut.ExecuteWorkflowAsync(workflow);

        result.Success.Should().BeTrue();
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_CreatesCheckpointsForEachStep()
    {
        var workflow = new WorkflowDefinition
        {
            Name = "CheckpointTest",
            Steps = Enumerable.Range(0, 5).Select(i => new WorkflowStepDefinition
            {
                StepNumber = i,
                Name = $"Step {i}",
                ExecuteFunc = (ctx, ct) => Task.FromResult(new WorkflowStepResult { Success = true, Output = i })
            }).ToList()
        };

        var result = await _sut.ExecuteWorkflowAsync(workflow);

        result.Success.Should().BeTrue();
        result.Checkpoints.Should().HaveCount(5);

        var checkpoints = (await _checkpointManager.GetWorkflowCheckpointsAsync(workflow.WorkflowId)).ToList();
        checkpoints.Should().HaveCount(5);
        checkpoints.All(c => c.Status == ExecutionStepStatus.Completed).Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_Cancellation_ThrowsOperationCanceled()
    {
        var cts = new CancellationTokenSource();
        var workflow = new WorkflowDefinition
        {
            Name = "CancelTest",
            Steps = new List<WorkflowStepDefinition>
            {
                new()
                {
                    StepNumber = 0,
                    Name = "Cancel Trigger",
                    ExecuteFunc = (ctx, ct) =>
                    {
                        cts.Cancel(); // Cancel during execution
                        return Task.FromResult(new WorkflowStepResult { Success = true });
                    }
                },
                new()
                {
                    StepNumber = 1,
                    Name = "Should Not Execute",
                    ExecuteFunc = (ctx, ct) => Task.FromResult(new WorkflowStepResult { Success = true })
                }
            }
        };

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _sut.ExecuteWorkflowAsync(workflow, cts.Token));
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_EmptyWorkflow_ThrowsArgumentException()
    {
        var workflow = new WorkflowDefinition { Name = "Empty", Steps = new List<WorkflowStepDefinition>() };

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.ExecuteWorkflowAsync(workflow));
    }

    [Fact]
    public async Task GetWorkflowStatusAsync_ReturnsCorrectStatus()
    {
        var workflow = new WorkflowDefinition
        {
            Name = "StatusTest",
            Steps = new List<WorkflowStepDefinition>
            {
                new()
                {
                    StepNumber = 0,
                    Name = "Step",
                    ExecuteFunc = (ctx, ct) => Task.FromResult(new WorkflowStepResult { Success = true })
                }
            }
        };

        await _sut.ExecuteWorkflowAsync(workflow);

        var status = await _sut.GetWorkflowStatusAsync(workflow.WorkflowId);
        status.State.Should().Be(WorkflowState.Completed);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_100Steps_CompletesSuccessfully()
    {
        var workflow = new WorkflowDefinition
        {
            Name = "100StepTest",
            Steps = Enumerable.Range(0, 100).Select(i => new WorkflowStepDefinition
            {
                StepNumber = i,
                Name = $"Step {i}",
                ExecuteFunc = (ctx, ct) => Task.FromResult(new WorkflowStepResult
                {
                    Success = true,
                    Output = i,
                    StateUpdates = new Dictionary<string, object> { [$"step_{i}"] = true }
                })
            }).ToList()
        };

        var result = await _sut.ExecuteWorkflowAsync(workflow);

        result.Success.Should().BeTrue();
        result.CompletedSteps.Should().Be(100);
        result.Checkpoints.Should().HaveCount(100);
    }
}
