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

public class TaskRoutingRequest
{
    public WorkflowDefinition? WorkflowDefinition { get; set; }
    public AgentExecutionRequest? AgentExecutionRequest { get; set; }
}

public class TaskRoutingResult
{
    public bool Success { get; set; }
    public object? Output { get; set; }
    public string? ErrorMessage { get; set; }
    public int CompletedSteps { get; set; }
    public int TotalSteps { get; set; }
    public TimeSpan Duration { get; set; }
    public string RoutedTo { get; set; } = string.Empty;
}
