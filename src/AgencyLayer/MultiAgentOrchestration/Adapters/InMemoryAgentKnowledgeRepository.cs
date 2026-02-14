using System.Collections.Concurrent;
using AgencyLayer.MultiAgentOrchestration.Engines;
using AgencyLayer.MultiAgentOrchestration.Ports;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.MultiAgentOrchestration.Adapters;

/// <summary>
/// In-memory implementation of IAgentKnowledgeRepository for storing
/// agent definitions and learning insights.
/// </summary>
public class InMemoryAgentKnowledgeRepository : IAgentKnowledgeRepository
{
    private readonly ConcurrentDictionary<string, AgentDefinition> _definitions = new();
    private readonly ConcurrentBag<AgentLearningInsight> _insights = new();
    private readonly ILogger<InMemoryAgentKnowledgeRepository> _logger;

    public InMemoryAgentKnowledgeRepository(ILogger<InMemoryAgentKnowledgeRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task StoreAgentDefinitionAsync(AgentDefinition definition)
    {
        _definitions[definition.AgentType] = definition;
        _logger.LogDebug("Stored agent definition for type: {AgentType}", definition.AgentType);
        return Task.CompletedTask;
    }

    public Task<AgentDefinition> GetAgentDefinitionAsync(string agentType)
    {
        _definitions.TryGetValue(agentType, out var definition);
        return Task.FromResult(definition!);
    }

    public Task StoreLearningInsightAsync(AgentLearningInsight insight)
    {
        _insights.Add(insight);
        _logger.LogDebug("Stored learning insight: {InsightId} ({InsightType})", insight.InsightId, insight.InsightType);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<AgentLearningInsight>> GetRelevantInsightsAsync(string taskGoal)
    {
        // Simple keyword matching — a real implementation would use vector search
        var relevant = _insights
            .Where(i => i.InsightType != null &&
                (taskGoal.Contains(i.InsightType, StringComparison.OrdinalIgnoreCase) ||
                 i.ConfidenceScore >= 0.8))
            .OrderByDescending(i => i.ConfidenceScore)
            .Take(10);

        return Task.FromResult(relevant);
    }
}
