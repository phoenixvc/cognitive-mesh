using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.Orchestration.Checkpointing;

/// <summary>
/// In-memory checkpoint manager with optional persistence callback.
/// Suitable for single-process execution. For distributed workflows,
/// replace with DuckDB or CosmosDB-backed implementation.
/// </summary>
public class InMemoryCheckpointManager : ICheckpointManager
{
    private readonly ConcurrentDictionary<string, SortedList<int, ExecutionCheckpoint>> _checkpoints = new();
    private readonly ILogger<InMemoryCheckpointManager> _logger;
    private readonly Func<ExecutionCheckpoint, Task>? _persistCallback;

    public InMemoryCheckpointManager(
        ILogger<InMemoryCheckpointManager> logger,
        Func<ExecutionCheckpoint, Task>? persistCallback = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _persistCallback = persistCallback;
    }

    public async Task SaveCheckpointAsync(ExecutionCheckpoint checkpoint, CancellationToken cancellationToken = default)
    {
        var workflowCheckpoints = _checkpoints.GetOrAdd(checkpoint.WorkflowId, _ => new SortedList<int, ExecutionCheckpoint>());

        lock (workflowCheckpoints)
        {
            workflowCheckpoints[checkpoint.StepNumber] = checkpoint;
        }

        _logger.LogDebug(
            "Checkpoint saved: Workflow={WorkflowId}, Step={StepNumber}/{StepName}, Status={Status}",
            checkpoint.WorkflowId, checkpoint.StepNumber, checkpoint.StepName, checkpoint.Status);

        if (_persistCallback != null)
        {
            await _persistCallback(checkpoint);
        }
    }

    public Task<ExecutionCheckpoint?> GetCheckpointAsync(string workflowId, string checkpointId, CancellationToken cancellationToken = default)
    {
        if (!_checkpoints.TryGetValue(workflowId, out var workflowCheckpoints))
            return Task.FromResult<ExecutionCheckpoint?>(null);

        ExecutionCheckpoint? result;
        lock (workflowCheckpoints)
        {
            result = workflowCheckpoints.Values.FirstOrDefault(c => c.CheckpointId == checkpointId);
        }
        return Task.FromResult(result);
    }

    public Task<ExecutionCheckpoint?> GetLatestCheckpointAsync(string workflowId, CancellationToken cancellationToken = default)
    {
        if (!_checkpoints.TryGetValue(workflowId, out var workflowCheckpoints))
            return Task.FromResult<ExecutionCheckpoint?>(null);

        ExecutionCheckpoint? result;
        lock (workflowCheckpoints)
        {
            result = workflowCheckpoints.Count > 0 ? workflowCheckpoints.Values.Last() : null;
        }
        return Task.FromResult(result);
    }

    public Task<IEnumerable<ExecutionCheckpoint>> GetWorkflowCheckpointsAsync(string workflowId, CancellationToken cancellationToken = default)
    {
        if (!_checkpoints.TryGetValue(workflowId, out var workflowCheckpoints))
            return Task.FromResult(Enumerable.Empty<ExecutionCheckpoint>());

        IEnumerable<ExecutionCheckpoint> result;
        lock (workflowCheckpoints)
        {
            result = workflowCheckpoints.Values.ToList();
        }
        return Task.FromResult(result);
    }

    public Task PurgeWorkflowCheckpointsAsync(string workflowId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        cancellationToken.ThrowIfCancellationRequested();

        if (_checkpoints.TryRemove(workflowId, out var removed))
        {
            int count;
            lock (removed)
            {
                count = removed.Count;
                removed.Clear();
            }

            _logger.LogInformation(
                "Purged {CheckpointCount} checkpoints for workflow {WorkflowId}",
                count, workflowId);
        }
        else
        {
            _logger.LogDebug(
                "No checkpoints found to purge for workflow {WorkflowId}", workflowId);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the total number of checkpoints across all workflows (for diagnostics).
    /// </summary>
    public int TotalCheckpointCount => _checkpoints.Values.Sum(wf => { lock (wf) { return wf.Count; } });
}
