using System.Collections.Concurrent;
using System.Diagnostics;
using AgencyLayer.Agents.Ports;
using AgencyLayer.AgentTeamFramework.Ports;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.AgentTeamFramework.Adapters;

/// <summary>
/// Implements <see cref="ISpecializedAgentPort"/> by bridging to <see cref="IAgentLLMPort"/>.
/// Registered agents are stored in-memory. Task execution sends the agent's system prompt
/// and the task description plus context to the LLM and returns the response.
/// </summary>
public sealed class LLMBackedSpecializedAgentAdapter : ISpecializedAgentPort
{
    private readonly IAgentLLMPort _llmPort;
    private readonly ILogger<LLMBackedSpecializedAgentAdapter> _logger;
    private readonly ConcurrentDictionary<string, SpecializedAgentConfiguration> _agents = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, List<string>> _teams = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="LLMBackedSpecializedAgentAdapter"/> class.
    /// </summary>
    /// <param name="llmPort">The LLM port used to generate completions.</param>
    /// <param name="logger">Logger instance.</param>
    public LLMBackedSpecializedAgentAdapter(IAgentLLMPort llmPort, ILogger<LLMBackedSpecializedAgentAdapter> logger)
    {
        _llmPort = llmPort ?? throw new ArgumentNullException(nameof(llmPort));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task RegisterAgentAsync(SpecializedAgentConfiguration configuration, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        cancellationToken.ThrowIfCancellationRequested();

        _agents[configuration.AgentId] = configuration;
        _logger.LogDebug("Registered agent {AgentId} ({Name}, {Type})", configuration.AgentId, configuration.Name, configuration.Type);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<SpecializedAgentConfiguration?> GetAgentAsync(string agentId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);
        cancellationToken.ThrowIfCancellationRequested();

        _agents.TryGetValue(agentId, out var config);
        return Task.FromResult(config);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<SpecializedAgentConfiguration>> ListAgentsAsync(SpecializedAgentType? type = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<SpecializedAgentConfiguration> result = type.HasValue
            ? _agents.Values.Where(a => a.Type == type.Value).ToList().AsReadOnly()
            : _agents.Values.ToList().AsReadOnly();

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public async Task<SpecializedTaskResult> ExecuteTaskAsync(string agentId, SpecializedTask task, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);
        ArgumentNullException.ThrowIfNull(task);

        if (!_agents.TryGetValue(agentId, out var agent))
        {
            return new SpecializedTaskResult
            {
                TaskId = task.TaskId,
                AgentId = agentId,
                Success = false,
                ErrorMessage = $"Agent '{agentId}' is not registered."
            };
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogDebug("Executing task {TaskId} with agent {AgentId}", task.TaskId, agentId);

            var userMessage = BuildUserMessage(task);

            var result = await _llmPort.CompleteAsync(
                agent.SystemPrompt,
                userMessage,
                agent.Temperature,
                agent.MaxTokens,
                cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();

            _logger.LogDebug(
                "Agent {AgentId} completed task {TaskId} in {Duration}ms ({TokensUsed} tokens)",
                agentId, task.TaskId, stopwatch.ElapsedMilliseconds, result.TokensUsed);

            return new SpecializedTaskResult
            {
                TaskId = task.TaskId,
                AgentId = agentId,
                Success = true,
                Output = result.Content,
                Duration = stopwatch.Elapsed,
                TokensUsed = result.TokensUsed
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Agent {AgentId} failed on task {TaskId}", agentId, task.TaskId);

            return new SpecializedTaskResult
            {
                TaskId = task.TaskId,
                AgentId = agentId,
                Success = false,
                ErrorMessage = ex.Message,
                Duration = stopwatch.Elapsed
            };
        }
    }

    /// <inheritdoc />
    public Task<string?> FindBestAgentAsync(SpecializedTask task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        cancellationToken.ThrowIfCancellationRequested();

        string? bestAgentId = null;
        var bestScore = 0;

        foreach (var agent in _agents.Values)
        {
            var score = 0;
            foreach (var required in task.RequiredCapabilities)
            {
                if (agent.Capabilities.Contains(required, StringComparer.OrdinalIgnoreCase) ||
                    agent.Domains.Contains(required, StringComparer.OrdinalIgnoreCase))
                {
                    score++;
                }
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestAgentId = agent.AgentId;
            }
        }

        bestAgentId ??= _agents.Keys.FirstOrDefault();
        return Task.FromResult(bestAgentId);
    }

    /// <inheritdoc />
    public async Task<SpecializedTaskResult> DelegateTaskAsync(SpecializedTask task, CancellationToken cancellationToken = default)
    {
        var bestAgent = await FindBestAgentAsync(task, cancellationToken).ConfigureAwait(false);

        if (bestAgent is null)
        {
            return new SpecializedTaskResult
            {
                TaskId = task.TaskId,
                AgentId = "none",
                Success = false,
                ErrorMessage = "No agents are registered to handle this task."
            };
        }

        return await ExecuteTaskAsync(bestAgent, task, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<string> CreateTeamAsync(IEnumerable<string> agentIds, string teamGoal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(agentIds);
        ArgumentException.ThrowIfNullOrWhiteSpace(teamGoal);
        cancellationToken.ThrowIfCancellationRequested();

        var ids = agentIds.ToList();
        var teamId = $"team-{Guid.NewGuid():N}";
        _teams[teamId] = ids;

        _logger.LogDebug("Created team {TeamId} with {AgentCount} agents for goal: {TeamGoal}", teamId, ids.Count, teamGoal);
        return Task.FromResult(teamId);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SpecializedTaskResult>> ExecuteWithTeamAsync(
        string teamId,
        IEnumerable<SpecializedTask> tasks,
        bool parallel = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(teamId);
        ArgumentNullException.ThrowIfNull(tasks);

        if (!_teams.TryGetValue(teamId, out var agentIds))
            throw new InvalidOperationException($"Team '{teamId}' does not exist.");

        var taskList = tasks.ToList();
        var results = new List<SpecializedTaskResult>();

        if (parallel)
        {
            var executions = taskList.Select((task, i) =>
            {
                var agentId = agentIds[i % agentIds.Count];
                return ExecuteTaskAsync(agentId, task, cancellationToken);
            });

            results.AddRange(await Task.WhenAll(executions).ConfigureAwait(false));
        }
        else
        {
            foreach (var (task, i) in taskList.Select((t, i) => (t, i)))
            {
                var agentId = agentIds[i % agentIds.Count];
                var result = await ExecuteTaskAsync(agentId, task, cancellationToken).ConfigureAwait(false);
                results.Add(result);
            }
        }

        return results.AsReadOnly();
    }

    /// <inheritdoc />
    public Task UnregisterAgentAsync(string agentId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);
        cancellationToken.ThrowIfCancellationRequested();

        _agents.TryRemove(agentId, out _);
        _logger.LogDebug("Unregistered agent {AgentId}", agentId);
        return Task.CompletedTask;
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    private static string BuildUserMessage(SpecializedTask task)
    {
        if (string.IsNullOrWhiteSpace(task.Context))
            return task.Description;

        return $"""
            {task.Description}

            Context:
            {task.Context}
            """;
    }
}
