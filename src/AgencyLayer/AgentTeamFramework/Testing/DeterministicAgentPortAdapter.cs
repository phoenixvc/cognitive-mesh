using System.Collections.Concurrent;
using AgencyLayer.Agents.Ports;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.AgentTeamFramework.Testing;

/// <summary>
/// A test-friendly <see cref="ISpecializedAgentPort"/> implementation that returns
/// pre-configured responses for each agent. Eliminates LLM calls during testing by
/// mapping agent IDs to deterministic output functions.
/// </summary>
/// <remarks>
/// Usage in tests:
/// <code>
/// var adapter = new DeterministicAgentPortAdapter(logger);
/// adapter.SetResponse("agent-1", task => "{\"name\": \"test\"}");
/// adapter.SetResponse("agent-2", task => "{\"value\": 42}");
/// var engine = new MyTeamEngine(adapter, new AgentPipelineExecutor(adapter, pipelineLogger), engineLogger);
/// </code>
/// </remarks>
public sealed class DeterministicAgentPortAdapter : ISpecializedAgentPort
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, SpecializedAgentConfiguration> _agents = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Func<SpecializedTask, string>> _responses = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, List<SpecializedTask>> _executedTasks = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="DeterministicAgentPortAdapter"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public DeterministicAgentPortAdapter(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the total number of tasks executed across all agents.
    /// </summary>
    public int TotalTasksExecuted => _executedTasks.Values.Sum(v => v.Count);

    /// <summary>
    /// Configures a deterministic response for a given agent.
    /// </summary>
    /// <param name="agentId">The agent identifier.</param>
    /// <param name="responseFactory">A function that produces the output string given a task.</param>
    public void SetResponse(string agentId, Func<SpecializedTask, string> responseFactory)
    {
        _responses[agentId] = responseFactory ?? throw new ArgumentNullException(nameof(responseFactory));
    }

    /// <summary>
    /// Configures a static response string for a given agent.
    /// </summary>
    /// <param name="agentId">The agent identifier.</param>
    /// <param name="output">The output string to return for every task.</param>
    public void SetResponse(string agentId, string output)
    {
        _responses[agentId] = _ => output;
    }

    /// <summary>
    /// Gets all tasks that were executed by a specific agent, in order.
    /// </summary>
    /// <param name="agentId">The agent identifier.</param>
    /// <returns>The list of tasks executed, or empty if none.</returns>
    public IReadOnlyList<SpecializedTask> GetExecutedTasks(string agentId)
    {
        return _executedTasks.TryGetValue(agentId, out var tasks)
            ? tasks.AsReadOnly()
            : Array.Empty<SpecializedTask>();
    }

    /// <inheritdoc />
    public Task RegisterAgentAsync(SpecializedAgentConfiguration configuration, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _agents[configuration.AgentId] = configuration;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<SpecializedAgentConfiguration?> GetAgentAsync(string agentId, CancellationToken cancellationToken = default)
    {
        _agents.TryGetValue(agentId, out var config);
        return Task.FromResult(config);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<SpecializedAgentConfiguration>> ListAgentsAsync(SpecializedAgentType? type = null, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SpecializedAgentConfiguration> result = type.HasValue
            ? _agents.Values.Where(a => a.Type == type.Value).ToList().AsReadOnly()
            : _agents.Values.ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<SpecializedTaskResult> ExecuteTaskAsync(string agentId, SpecializedTask task, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);
        ArgumentNullException.ThrowIfNull(task);
        cancellationToken.ThrowIfCancellationRequested();

        // Record the executed task
        _executedTasks.AddOrUpdate(
            agentId,
            _ => [task],
            (_, list) => { list.Add(task); return list; });

        // Get response or default
        var output = _responses.TryGetValue(agentId, out var factory)
            ? factory(task)
            : "{}";

        _logger.LogDebug("DeterministicAdapter: Agent {AgentId} executed task {TaskId}", agentId, task.TaskId);

        return Task.FromResult(new SpecializedTaskResult
        {
            TaskId = task.TaskId,
            AgentId = agentId,
            Success = true,
            Output = output,
            TokensUsed = 0
        });
    }

    /// <inheritdoc />
    public Task<string?> FindBestAgentAsync(SpecializedTask task, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_agents.Keys.FirstOrDefault());
    }

    /// <inheritdoc />
    public async Task<SpecializedTaskResult> DelegateTaskAsync(SpecializedTask task, CancellationToken cancellationToken = default)
    {
        var best = await FindBestAgentAsync(task, cancellationToken).ConfigureAwait(false);
        if (best is null)
            return new SpecializedTaskResult { TaskId = task.TaskId, AgentId = "none", Success = false, ErrorMessage = "No agents registered." };
        return await ExecuteTaskAsync(best, task, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<string> CreateTeamAsync(IEnumerable<string> agentIds, string teamGoal, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"test-team-{Guid.NewGuid():N}");
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<SpecializedTaskResult>> ExecuteWithTeamAsync(string teamId, IEnumerable<SpecializedTask> tasks, bool parallel = false, CancellationToken cancellationToken = default)
    {
        var results = new List<SpecializedTaskResult>();
        var agentIds = _agents.Keys.ToList();
        var taskList = tasks.ToList();

        for (var i = 0; i < taskList.Count; i++)
        {
            var agentId = agentIds[i % agentIds.Count];
            results.Add(ExecuteTaskAsync(agentId, taskList[i], cancellationToken).GetAwaiter().GetResult());
        }

        return Task.FromResult<IReadOnlyList<SpecializedTaskResult>>(results.AsReadOnly());
    }

    /// <inheritdoc />
    public Task UnregisterAgentAsync(string agentId, CancellationToken cancellationToken = default)
    {
        _agents.TryRemove(agentId, out _);
        return Task.CompletedTask;
    }
}
