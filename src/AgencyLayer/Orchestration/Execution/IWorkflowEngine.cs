using AgencyLayer.Orchestration.Checkpointing;

namespace AgencyLayer.Orchestration.Execution;

/// <summary>
/// Durable workflow engine that executes sequential step chains with checkpointing,
/// retry, and crash recovery. This is the core component for achieving high MAKER scores.
/// </summary>
public interface IWorkflowEngine
{
    /// <summary>
    /// Executes a workflow from the beginning or resumes from the last checkpoint.
    /// </summary>
    Task<WorkflowResult> ExecuteWorkflowAsync(WorkflowDefinition workflow, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a workflow from its last successful checkpoint.
    /// </summary>
    Task<WorkflowResult> ResumeWorkflowAsync(string workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current status of a running or completed workflow.
    /// </summary>
    Task<WorkflowStatus> GetWorkflowStatusAsync(string workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a running workflow.
    /// </summary>
    Task CancelWorkflowAsync(string workflowId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a workflow as a sequence of executable steps.
/// </summary>
public class WorkflowDefinition
{
    public string WorkflowId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<WorkflowStepDefinition> Steps { get; set; } = new();
    public bool IsPreApproved { get; set; }
    public int MaxRetryPerStep { get; set; } = 3;
    public TimeSpan StepTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public Dictionary<string, object> InitialContext { get; set; } = new();
}

/// <summary>
/// Defines a single step in a workflow.
/// </summary>
public class WorkflowStepDefinition
{
    public int StepNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Func<WorkflowStepContext, CancellationToken, Task<WorkflowStepResult>>? ExecuteFunc { get; set; }
    public bool RequiresGovernanceCheck { get; set; } = true;
}

/// <summary>
/// Context passed to each workflow step, containing accumulated state.
/// </summary>
public class WorkflowStepContext
{
    public string WorkflowId { get; set; } = string.Empty;
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public Dictionary<string, object> State { get; set; } = new();
    public object? PreviousStepOutput { get; set; }
}

/// <summary>
/// Result returned by a single workflow step.
/// </summary>
public class WorkflowStepResult
{
    public bool Success { get; set; }
    public object? Output { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> StateUpdates { get; set; } = new();
}

/// <summary>
/// Final result of a complete workflow execution.
/// </summary>
public class WorkflowResult
{
    public string WorkflowId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int TotalSteps { get; set; }
    public int CompletedSteps { get; set; }
    public int FailedSteps { get; set; }
    public object? FinalOutput { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public List<ExecutionCheckpoint> Checkpoints { get; set; } = new();
}

/// <summary>
/// Status of a workflow execution.
/// </summary>
public class WorkflowStatus
{
    public string WorkflowId { get; set; } = string.Empty;
    public WorkflowState State { get; set; }
    public int TotalSteps { get; set; }
    public int CurrentStep { get; set; }
    public string CurrentStepName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// State of a workflow execution lifecycle.
/// </summary>
public enum WorkflowState
{
    /// <summary>Workflow has not started.</summary>
    Pending,
    /// <summary>Workflow is actively executing steps.</summary>
    Running,
    /// <summary>All steps completed successfully.</summary>
    Completed,
    /// <summary>Workflow failed at a step after exhausting retries.</summary>
    Failed,
    /// <summary>Workflow was cancelled by the caller.</summary>
    Cancelled,
    /// <summary>Workflow is suspended and can be resumed.</summary>
    Suspended
}
