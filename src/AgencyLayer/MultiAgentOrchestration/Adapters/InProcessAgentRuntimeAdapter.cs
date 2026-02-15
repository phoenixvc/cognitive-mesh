using System.Collections.Concurrent;
using AgencyLayer.MultiAgentOrchestration.Engines;
using AgencyLayer.MultiAgentOrchestration.Ports;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.MultiAgentOrchestration.Adapters;

/// <summary>
/// In-process implementation of IAgentRuntimeAdapter that executes agent logic
/// within the current process using registered agent handlers.
/// For production distributed execution, replace with a Temporal/Durable Functions adapter.
/// </summary>
public class InProcessAgentRuntimeAdapter : IAgentRuntimeAdapter
{
    private readonly ILogger<InProcessAgentRuntimeAdapter> _logger;
    private readonly ConcurrentDictionary<string, Func<AgentTask, Task<object>>> _agentHandlers = new();
    private readonly ConcurrentDictionary<string, AgentInstanceInfo> _provisionedAgents = new();

    public InProcessAgentRuntimeAdapter(ILogger<InProcessAgentRuntimeAdapter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a handler function for a specific agent type.
    /// </summary>
    public void RegisterHandler(string agentType, Func<AgentTask, Task<object>> handler)
    {
        _agentHandlers[agentType] = handler;
        _logger.LogInformation("Registered handler for agent type: {AgentType}", agentType);
    }

    /// <inheritdoc/>
    public async Task<object> ExecuteAgentLogicAsync(string agentId, AgentTask subTask)
    {
        _logger.LogDebug("Executing agent logic for {AgentId}, goal: {Goal}", agentId, subTask.Goal);

        // Try to find a handler by agent type from provisioned instances
        if (_provisionedAgents.TryGetValue(agentId, out var agentInfo) &&
            _agentHandlers.TryGetValue(agentInfo.AgentType, out var handler))
        {
            return await handler(subTask);
        }

        // Fallback: try to extract agent type from context or use a default handler
        if (subTask.Context.TryGetValue("AgentType", out var agentTypeObj) &&
            _agentHandlers.TryGetValue(agentTypeObj.ToString()!, out var contextHandler))
        {
            return await contextHandler(subTask);
        }

        // Default: if there's a wildcard handler registered
        if (_agentHandlers.TryGetValue("*", out var wildcardHandler))
        {
            return await wildcardHandler(subTask);
        }

        _logger.LogWarning("No handler registered for agent {AgentId}. Returning goal acknowledgment.", agentId);
        return new { AgentId = agentId, Goal = subTask.Goal, Result = "Acknowledged", Status = "NoHandler" };
    }

    /// <inheritdoc/>
    public Task<string> ProvisionAgentInstanceAsync(DynamicAgentSpawnRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var safeAgentType = request.AgentType ?? string.Empty;
        var baseId = $"agent-{safeAgentType}-{Guid.NewGuid():N}";
        var agentId = baseId.Length > 40 ? baseId[..40] : baseId;
        _provisionedAgents[agentId] = new AgentInstanceInfo
        {
            AgentId = agentId,
            AgentType = safeAgentType,
            TenantId = request.TenantId ?? string.Empty,
            ProvisionedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Provisioned agent instance {AgentId} of type {AgentType}", agentId, safeAgentType);
        return Task.FromResult(agentId);
    }

    private class AgentInstanceInfo
    {
        public string AgentId { get; set; } = string.Empty;
        public string AgentType { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public DateTime ProvisionedAt { get; set; }
    }
}
