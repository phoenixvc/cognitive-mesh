using System.Collections.Concurrent;
using AgencyLayer.Agents.Ports;

namespace AgencyLayer.AgentTeamFramework.Testing;

/// <summary>
/// A decorator around <see cref="ISpecializedAgentPort"/> that records all method calls
/// for verification in tests. Delegates actual execution to the inner adapter.
/// </summary>
public sealed class RecordingAgentPortDecorator : ISpecializedAgentPort
{
    private readonly ISpecializedAgentPort _inner;
    private readonly ConcurrentBag<AgentPortCall> _calls = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordingAgentPortDecorator"/> class.
    /// </summary>
    /// <param name="inner">The inner adapter to delegate to.</param>
    public RecordingAgentPortDecorator(ISpecializedAgentPort inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    /// <summary>
    /// Gets all recorded calls in chronological order.
    /// </summary>
    public IReadOnlyList<AgentPortCall> Calls => _calls.Reverse().ToList().AsReadOnly();

    /// <summary>
    /// Gets all recorded calls of a specific method.
    /// </summary>
    /// <param name="methodName">The method name to filter by.</param>
    /// <returns>Filtered calls in chronological order.</returns>
    public IReadOnlyList<AgentPortCall> GetCalls(string methodName) =>
        Calls.Where(c => c.Method.Equals(methodName, StringComparison.OrdinalIgnoreCase)).ToList().AsReadOnly();

    /// <inheritdoc />
    public async Task RegisterAgentAsync(SpecializedAgentConfiguration configuration, CancellationToken cancellationToken = default)
    {
        Record(nameof(RegisterAgentAsync), configuration.AgentId);
        await _inner.RegisterAgentAsync(configuration, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpecializedAgentConfiguration?> GetAgentAsync(string agentId, CancellationToken cancellationToken = default)
    {
        Record(nameof(GetAgentAsync), agentId);
        return await _inner.GetAgentAsync(agentId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SpecializedAgentConfiguration>> ListAgentsAsync(SpecializedAgentType? type = null, CancellationToken cancellationToken = default)
    {
        Record(nameof(ListAgentsAsync), type?.ToString());
        return await _inner.ListAgentsAsync(type, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpecializedTaskResult> ExecuteTaskAsync(string agentId, SpecializedTask task, CancellationToken cancellationToken = default)
    {
        Record(nameof(ExecuteTaskAsync), agentId, task.TaskId);
        return await _inner.ExecuteTaskAsync(agentId, task, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<string?> FindBestAgentAsync(SpecializedTask task, CancellationToken cancellationToken = default)
    {
        Record(nameof(FindBestAgentAsync), task.TaskId);
        return await _inner.FindBestAgentAsync(task, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpecializedTaskResult> DelegateTaskAsync(SpecializedTask task, CancellationToken cancellationToken = default)
    {
        Record(nameof(DelegateTaskAsync), task.TaskId);
        return await _inner.DelegateTaskAsync(task, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<string> CreateTeamAsync(IEnumerable<string> agentIds, string teamGoal, CancellationToken cancellationToken = default)
    {
        Record(nameof(CreateTeamAsync), teamGoal);
        return await _inner.CreateTeamAsync(agentIds, teamGoal, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SpecializedTaskResult>> ExecuteWithTeamAsync(string teamId, IEnumerable<SpecializedTask> tasks, bool parallel = false, CancellationToken cancellationToken = default)
    {
        Record(nameof(ExecuteWithTeamAsync), teamId);
        return await _inner.ExecuteWithTeamAsync(teamId, tasks, parallel, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UnregisterAgentAsync(string agentId, CancellationToken cancellationToken = default)
    {
        Record(nameof(UnregisterAgentAsync), agentId);
        await _inner.UnregisterAgentAsync(agentId, cancellationToken).ConfigureAwait(false);
    }

    private void Record(string method, params string?[] args)
    {
        _calls.Add(new AgentPortCall(method, DateTimeOffset.UtcNow, args.Where(a => a is not null).ToArray()!));
    }
}

/// <summary>
/// Represents a recorded call to <see cref="ISpecializedAgentPort"/>.
/// </summary>
/// <param name="Method">The method name that was called.</param>
/// <param name="Timestamp">When the call was made.</param>
/// <param name="Args">Key arguments passed to the method.</param>
public record AgentPortCall(string Method, DateTimeOffset Timestamp, string[] Args);
