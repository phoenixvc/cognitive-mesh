using System.Collections.Concurrent;
using AgencyLayer.MultiAgentOrchestration.Engines;
using AgencyLayer.MultiAgentOrchestration.Ports;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.MultiAgentOrchestration.Adapters;

/// <summary>
/// In-memory implementation of <see cref="IAgentKnowledgeRepository"/> for storing
/// agent definitions and learning insights. Uses <see cref="ConcurrentDictionary{TKey,TValue}"/>
/// for thread-safe storage and efficient retrieval by key.
/// </summary>
public class InMemoryAgentKnowledgeRepository : IAgentKnowledgeRepository
{
    private readonly ConcurrentDictionary<string, AgentDefinition> _definitions = new();
    private readonly ConcurrentDictionary<string, AgentLearningInsight> _insights = new();
    private readonly ILogger<InMemoryAgentKnowledgeRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryAgentKnowledgeRepository"/> class.
    /// </summary>
    /// <param name="logger">Logger for structured diagnostic output.</param>
    public InMemoryAgentKnowledgeRepository(ILogger<InMemoryAgentKnowledgeRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task StoreAgentDefinitionAsync(AgentDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentException.ThrowIfNullOrWhiteSpace(definition.AgentType);

        var isUpdate = _definitions.ContainsKey(definition.AgentType);
        _definitions[definition.AgentType] = definition;

        _logger.LogDebug(
            "{Operation} agent definition for type: {AgentType} (capabilities: {Capabilities})",
            isUpdate ? "Updated" : "Stored",
            definition.AgentType,
            string.Join(", ", definition.Capabilities));

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<AgentDefinition?> GetAgentDefinitionAsync(string agentType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentType);

        _definitions.TryGetValue(agentType, out var definition);
        if (definition == null)
        {
            _logger.LogWarning("Agent definition not found for type: {AgentType}", agentType);
        }
        else
        {
            _logger.LogDebug("Retrieved agent definition for type: {AgentType}", agentType);
        }
        return Task.FromResult(definition);
    }

    /// <inheritdoc/>
    public Task StoreLearningInsightAsync(AgentLearningInsight insight)
    {
        ArgumentNullException.ThrowIfNull(insight);
        ArgumentException.ThrowIfNullOrWhiteSpace(insight.InsightId);

        _insights[insight.InsightId] = insight;

        _logger.LogDebug(
            "Stored learning insight: {InsightId} (type={InsightType}, confidence={Confidence:F2}, agent={AgentType})",
            insight.InsightId, insight.InsightType, insight.ConfidenceScore, insight.GeneratingAgentType);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<AgentLearningInsight>> GetRelevantInsightsAsync(string taskGoal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskGoal);

        var goalTokens = taskGoal
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => t.Length > 2)
            .Select(t => t.ToUpperInvariant())
            .ToHashSet();

        var relevant = _insights.Values
            .Select(insight =>
            {
                double relevanceScore = 0.0;

                // Exact type match in goal text
                if (!string.IsNullOrEmpty(insight.InsightType) &&
                    taskGoal.Contains(insight.InsightType, StringComparison.OrdinalIgnoreCase))
                {
                    relevanceScore += 1.0;
                }

                // Token overlap between goal and insight type
                if (!string.IsNullOrEmpty(insight.InsightType))
                {
                    var insightTokens = insight.InsightType
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(t => t.ToUpperInvariant())
                        .ToHashSet();

                    var overlap = goalTokens.Intersect(insightTokens).Count();
                    if (insightTokens.Count > 0)
                    {
                        relevanceScore += (double)overlap / insightTokens.Count * 0.5;
                    }
                }

                // High-confidence insights are always somewhat relevant
                if (insight.ConfidenceScore >= 0.8)
                {
                    relevanceScore += 0.3;
                }

                return new { Insight = insight, Relevance = relevanceScore };
            })
            .Where(x => x.Relevance > 0.0)
            .OrderByDescending(x => x.Relevance)
            .ThenByDescending(x => x.Insight.ConfidenceScore)
            .Take(10)
            .Select(x => x.Insight);

        _logger.LogDebug(
            "Retrieved {Count} relevant insights for goal: {TaskGoal}",
            relevant.Count(), taskGoal);

        return Task.FromResult(relevant);
    }
}
