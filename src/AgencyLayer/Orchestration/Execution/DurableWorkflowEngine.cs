using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using AgencyLayer.Orchestration.Checkpointing;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.Orchestration.Execution;

/// <summary>
/// Durable workflow engine with checkpointing, retry, and crash recovery.
/// Executes sequential step chains and persists state at each checkpoint.
/// Designed for high MAKER scores on long-horizon sequential tasks.
/// </summary>
public class DurableWorkflowEngine : IWorkflowEngine
{
    private readonly ICheckpointManager _checkpointManager;
    private readonly ILogger<DurableWorkflowEngine> _logger;
    private readonly ConcurrentDictionary<string, WorkflowStatus> _activeWorkflows = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellationTokens = new();
    private readonly ConcurrentDictionary<string, WorkflowDefinition> _workflowDefinitions = new();

    public DurableWorkflowEngine(
        ICheckpointManager checkpointManager,
        ILogger<DurableWorkflowEngine> logger)
    {
        _checkpointManager = checkpointManager ?? throw new ArgumentNullException(nameof(checkpointManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<WorkflowResult> ExecuteWorkflowAsync(WorkflowDefinition workflow, CancellationToken cancellationToken = default)
    {
        if (workflow == null) throw new ArgumentNullException(nameof(workflow));
        if (workflow.Steps.Count == 0) throw new ArgumentException("Workflow must have at least one step.", nameof(workflow));

        _workflowDefinitions[workflow.WorkflowId] = workflow;
        if (_cancellationTokens.TryRemove(workflow.WorkflowId, out var previousCts))
        {
            previousCts.Dispose();
        }
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _cancellationTokens[workflow.WorkflowId] = cts;

        var status = new WorkflowStatus
        {
            WorkflowId = workflow.WorkflowId,
            State = WorkflowState.Running,
            TotalSteps = workflow.Steps.Count,
            CurrentStep = 0,
            StartedAt = DateTime.UtcNow
        };
        _activeWorkflows[workflow.WorkflowId] = status;

        _logger.LogInformation(
            "Starting workflow {WorkflowId} '{Name}' with {StepCount} steps",
            workflow.WorkflowId, workflow.Name, workflow.Steps.Count);

        return await ExecuteFromStepAsync(workflow, 0, new Dictionary<string, object>(workflow.InitialContext), null, cts.Token);
    }

    public async Task<WorkflowResult> ResumeWorkflowAsync(string workflowId, CancellationToken cancellationToken = default)
    {
        if (!_workflowDefinitions.TryGetValue(workflowId, out var workflow))
            throw new InvalidOperationException($"Workflow {workflowId} not found. Cannot resume a workflow that hasn't been registered.");

        var latestCheckpoint = await _checkpointManager.GetLatestCheckpointAsync(workflowId, cancellationToken);
        if (latestCheckpoint == null)
            throw new InvalidOperationException($"No checkpoints found for workflow {workflowId}. Cannot resume.");

        if (_cancellationTokens.TryRemove(workflowId, out var previousCts))
        {
            previousCts.Dispose();
        }
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _cancellationTokens[workflowId] = cts;

        // Resume from the step after the last successful checkpoint
        int resumeStep = latestCheckpoint.Status == ExecutionStepStatus.Completed
            ? latestCheckpoint.StepNumber + 1
            : latestCheckpoint.StepNumber; // Retry the failed step

        var state = latestCheckpoint.DeserializeState<Dictionary<string, object>>() ?? new Dictionary<string, object>();
        var previousOutput = latestCheckpoint.Status == ExecutionStepStatus.Completed
            ? latestCheckpoint.DeserializeOutput<object>()
            : null;

        _logger.LogInformation(
            "Resuming workflow {WorkflowId} from step {ResumeStep} (last checkpoint: step {CheckpointStep}, status {CheckpointStatus})",
            workflowId, resumeStep, latestCheckpoint.StepNumber, latestCheckpoint.Status);

        if (_activeWorkflows.TryGetValue(workflowId, out var status))
        {
            status.State = WorkflowState.Running;
            status.CurrentStep = resumeStep;
        }

        return await ExecuteFromStepAsync(workflow, resumeStep, state, previousOutput, cts.Token);
    }

    public Task<WorkflowStatus> GetWorkflowStatusAsync(string workflowId, CancellationToken cancellationToken = default)
    {
        _activeWorkflows.TryGetValue(workflowId, out var status);
        return Task.FromResult(status ?? new WorkflowStatus { WorkflowId = workflowId, State = WorkflowState.Pending });
    }

    public Task CancelWorkflowAsync(string workflowId, CancellationToken cancellationToken = default)
    {
        if (_cancellationTokens.TryGetValue(workflowId, out var cts))
        {
            cts.Cancel();
            UpdateWorkflowState(workflowId, WorkflowState.Cancelled);
            _logger.LogInformation("Workflow {WorkflowId} cancelled", workflowId);
        }
        return Task.CompletedTask;
    }

    private async Task<WorkflowResult> ExecuteFromStepAsync(
        WorkflowDefinition workflow,
        int startStep,
        Dictionary<string, object> state,
        object? previousOutput,
        CancellationToken cancellationToken)
    {
        var overallStopwatch = Stopwatch.StartNew();
        var checkpoints = new List<ExecutionCheckpoint>();
        int completedSteps = startStep;
        int failedSteps = 0;
        object? currentOutput = previousOutput;

        var orderedSteps = workflow.Steps.OrderBy(s => s.StepNumber).ToList();

        try
        {
            for (int i = 0; i < orderedSteps.Count; i++)
            {
                var step = orderedSteps[i];
                if (step.StepNumber < startStep) continue;

                cancellationToken.ThrowIfCancellationRequested();

                if (_activeWorkflows.TryGetValue(workflow.WorkflowId, out var status))
                {
                    status.CurrentStep = step.StepNumber;
                    status.CurrentStepName = step.Name;
                }

                var stepStopwatch = Stopwatch.StartNew();
                var stepResult = await ExecuteStepWithRetryAsync(workflow, step, state, currentOutput, cancellationToken);
                stepStopwatch.Stop();

                var checkpoint = new ExecutionCheckpoint
                {
                    WorkflowId = workflow.WorkflowId,
                    StepNumber = step.StepNumber,
                    StepName = step.Name,
                    Status = stepResult.Success ? ExecutionStepStatus.Completed : ExecutionStepStatus.Failed,
                    StateJson = JsonSerializer.Serialize(state),
                    InputJson = JsonSerializer.Serialize(new { PreviousOutput = currentOutput }),
                    OutputJson = stepResult.Output != null ? JsonSerializer.Serialize(stepResult.Output) : "{}",
                    ErrorMessage = stepResult.ErrorMessage,
                    ExecutionDuration = stepStopwatch.Elapsed
                };

                await _checkpointManager.SaveCheckpointAsync(checkpoint, cancellationToken);
                checkpoints.Add(checkpoint);

                if (stepResult.Success)
                {
                    // Merge state updates
                    foreach (var kvp in stepResult.StateUpdates)
                    {
                        state[kvp.Key] = kvp.Value;
                    }
                    currentOutput = stepResult.Output;
                    completedSteps++;
                }
                else
                {
                    failedSteps++;
                    _logger.LogError(
                        "Workflow {WorkflowId} failed at step {StepNumber}/{StepName}: {Error}",
                        workflow.WorkflowId, step.StepNumber, step.Name, stepResult.ErrorMessage);

                    overallStopwatch.Stop();
                    UpdateWorkflowState(workflow.WorkflowId, WorkflowState.Failed);

                    return new WorkflowResult
                    {
                        WorkflowId = workflow.WorkflowId,
                        Success = false,
                        TotalSteps = orderedSteps.Count,
                        CompletedSteps = completedSteps,
                        FailedSteps = failedSteps,
                        ErrorMessage = $"Failed at step {step.StepNumber} ({step.Name}): {stepResult.ErrorMessage}",
                        TotalDuration = overallStopwatch.Elapsed,
                        Checkpoints = checkpoints
                    };
                }
            }
        }
        catch (OperationCanceledException)
        {
            overallStopwatch.Stop();
            UpdateWorkflowState(workflow.WorkflowId, WorkflowState.Cancelled);

            if (checkpoints.Count > 0)
            {
                var cancellationCheckpoint = new ExecutionCheckpoint
                {
                    WorkflowId = workflow.WorkflowId,
                    StepNumber = completedSteps,
                    StepName = "Cancelled",
                    Status = ExecutionStepStatus.Failed,
                    StateJson = JsonSerializer.Serialize(state),
                    InputJson = "{}",
                    OutputJson = "{}",
                    ErrorMessage = "Workflow cancelled via CancellationToken",
                    ExecutionDuration = overallStopwatch.Elapsed
                };
                await _checkpointManager.SaveCheckpointAsync(cancellationCheckpoint, CancellationToken.None);
            }

            throw;
        }

        overallStopwatch.Stop();
        UpdateWorkflowState(workflow.WorkflowId, WorkflowState.Completed);

        _logger.LogInformation(
            "Workflow {WorkflowId} completed successfully: {CompletedSteps}/{TotalSteps} steps in {Duration}",
            workflow.WorkflowId, completedSteps, orderedSteps.Count, overallStopwatch.Elapsed);

        return new WorkflowResult
        {
            WorkflowId = workflow.WorkflowId,
            Success = true,
            TotalSteps = orderedSteps.Count,
            CompletedSteps = completedSteps,
            FailedSteps = 0,
            FinalOutput = currentOutput,
            TotalDuration = overallStopwatch.Elapsed,
            Checkpoints = checkpoints
        };
    }

    private async Task<WorkflowStepResult> ExecuteStepWithRetryAsync(
        WorkflowDefinition workflow,
        WorkflowStepDefinition step,
        Dictionary<string, object> state,
        object? previousOutput,
        CancellationToken cancellationToken)
    {
        if (step.ExecuteFunc == null)
            return new WorkflowStepResult { Success = false, ErrorMessage = $"Step {step.StepNumber} ({step.Name}) has no execution function." };

        var context = new WorkflowStepContext
        {
            WorkflowId = workflow.WorkflowId,
            StepNumber = step.StepNumber,
            StepName = step.Name,
            State = new Dictionary<string, object>(state),
            PreviousStepOutput = previousOutput
        };

        int maxRetries = workflow.MaxRetryPerStep;
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var stepCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                stepCts.CancelAfter(workflow.StepTimeout);

                var result = await step.ExecuteFunc(context, stepCts.Token);

                if (result.Success)
                {
                    _logger.LogDebug(
                        "Step {StepNumber}/{StepName} completed on attempt {Attempt}",
                        step.StepNumber, step.Name, attempt + 1);
                    return result;
                }

                if (attempt < maxRetries)
                {
                    var backoff = TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100);
                    _logger.LogWarning(
                        "Step {StepNumber}/{StepName} returned failure on attempt {Attempt}, retrying after {Backoff}ms: {Error}",
                        step.StepNumber, step.Name, attempt + 1, backoff.TotalMilliseconds, result.ErrorMessage);
                    await Task.Delay(backoff, cancellationToken);
                }
                else
                {
                    return result;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw; // Propagate external cancellation
            }
            catch (OperationCanceledException)
            {
                if (attempt >= maxRetries)
                    return new WorkflowStepResult { Success = false, ErrorMessage = $"Step {step.Name} timed out after {workflow.StepTimeout}" };

                var backoff = TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100);
                _logger.LogWarning(
                    "Step {StepNumber}/{StepName} timed out on attempt {Attempt}, retrying after {Backoff}ms",
                    step.StepNumber, step.Name, attempt + 1, backoff.TotalMilliseconds);
                await Task.Delay(backoff, cancellationToken);
            }
            catch (Exception ex)
            {
                if (attempt >= maxRetries)
                    return new WorkflowStepResult { Success = false, ErrorMessage = $"Step {step.Name} threw exception after {attempt + 1} attempts: {ex.Message}" };

                var backoff = TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100);
                _logger.LogWarning(ex,
                    "Step {StepNumber}/{StepName} threw exception on attempt {Attempt}, retrying after {Backoff}ms",
                    step.StepNumber, step.Name, attempt + 1, backoff.TotalMilliseconds);
                await Task.Delay(backoff, cancellationToken);
            }
        }

        return new WorkflowStepResult { Success = false, ErrorMessage = "Exhausted all retry attempts" };
    }

    private void UpdateWorkflowState(string workflowId, WorkflowState state)
    {
        if (_activeWorkflows.TryGetValue(workflowId, out var status))
        {
            status.State = state;
            if (state is WorkflowState.Completed or WorkflowState.Failed or WorkflowState.Cancelled)
            {
                status.CompletedAt = DateTime.UtcNow;
                if (_cancellationTokens.TryRemove(workflowId, out var cts))
                {
                    cts.Dispose();
                }
            }
        }
    }
}
