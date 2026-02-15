using AgencyLayer.MultiAgentOrchestration.Ports;
using AgencyLayer.Orchestration.Execution;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.AgencyRouter;

/// <summary>
/// Routes incoming tasks to the appropriate execution path:
/// - Direct agent execution for simple tasks
/// - Workflow engine for multi-step sequential tasks
/// - Pre-approved hot path for deterministic workflows (MAKER benchmark compatible)
/// </summary>
public class TaskRouter
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IMultiAgentOrchestrationPort _orchestrationPort;
    private readonly ILogger<TaskRouter> _logger;

    public TaskRouter(
        IWorkflowEngine workflowEngine,
        IMultiAgentOrchestrationPort orchestrationPort,
        ILogger<TaskRouter> logger)
    {
        _workflowEngine = workflowEngine ?? throw new ArgumentNullException(nameof(workflowEngine));
        _orchestrationPort = orchestrationPort ?? throw new ArgumentNullException(nameof(orchestrationPort));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Routes a task to the appropriate execution path based on its characteristics.
    /// </summary>
    public async Task<TaskRoutingResult> RouteTaskAsync(TaskRoutingRequest request, CancellationToken cancellationToken = default)
    {
        if (request.WorkflowDefinition != null)
        {
            _logger.LogInformation("Routing to workflow engine: {WorkflowName}", request.WorkflowDefinition.Name);
            var result = await _workflowEngine.ExecuteWorkflowAsync(request.WorkflowDefinition, cancellationToken);
            return new TaskRoutingResult
            {
                Success = result.Success,
                Output = result.FinalOutput,
                CompletedSteps = result.CompletedSteps,
                TotalSteps = result.TotalSteps,
                Duration = result.TotalDuration,
                RoutedTo = "WorkflowEngine"
            };
        }

        if (request.AgentExecutionRequest != null)
        {
            _logger.LogInformation("Routing to multi-agent orchestration: {Goal}", request.AgentExecutionRequest.Task.Goal);
            var result = await _orchestrationPort.ExecuteTaskAsync(request.AgentExecutionRequest);
            return new TaskRoutingResult
            {
                Success = result.IsSuccess,
                Output = result.Result,
                CompletedSteps = result.IsSuccess ? 1 : 0,
                TotalSteps = 1,
                RoutedTo = "MultiAgentOrchestration"
            };
        }

        return new TaskRoutingResult { Success = false, ErrorMessage = "No valid execution path specified." };
    }
}

/// <summary>
/// Request to route a task to the appropriate execution path.
/// </summary>
public class TaskRoutingRequest
{
    /// <summary>Workflow definition for multi-step sequential execution.</summary>
    public WorkflowDefinition? WorkflowDefinition { get; set; }

    /// <summary>Agent execution request for single-step multi-agent coordination.</summary>
    public AgentExecutionRequest? AgentExecutionRequest { get; set; }
}

/// <summary>
/// Result of a routed task execution.
/// </summary>
public class TaskRoutingResult
{
    /// <summary>Whether the routed task completed successfully.</summary>
    public bool Success { get; set; }

    /// <summary>Final output of the executed task.</summary>
    public object? Output { get; set; }

    /// <summary>Error message if the task failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Number of steps completed.</summary>
    public int CompletedSteps { get; set; }

    /// <summary>Total number of steps in the task.</summary>
    public int TotalSteps { get; set; }

    /// <summary>Total execution duration.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Which execution path was used (WorkflowEngine or MultiAgentOrchestration).</summary>
    public string RoutedTo { get; set; } = string.Empty;
}
