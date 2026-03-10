using System.Text.Json;
using AgencyLayer.Agents.Ports;
using AgencyLayer.AgentTeamFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.AgentTeamFramework.Pipeline;

/// <summary>
/// Executes agent pipeline steps with standardized serialization, error handling,
/// and logging. Eliminates the repeated serialize→execute→deserialize pattern
/// across agent team engines.
/// </summary>
public class AgentPipelineExecutor
{
    private readonly ISpecializedAgentPort _agentPort;
    private readonly ILogger<AgentPipelineExecutor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentPipelineExecutor"/> class.
    /// </summary>
    /// <param name="agentPort">The specialized agent port for task execution.</param>
    /// <param name="logger">Logger instance.</param>
    public AgentPipelineExecutor(
        ISpecializedAgentPort agentPort,
        ILogger<AgentPipelineExecutor> logger)
    {
        _agentPort = agentPort ?? throw new ArgumentNullException(nameof(agentPort));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes a single pipeline step: serializes context, calls the agent, and deserializes the output.
    /// </summary>
    /// <typeparam name="TOutput">The expected output type (must have a parameterless constructor).</typeparam>
    /// <param name="agentId">The agent to execute the step.</param>
    /// <param name="description">Task description for the agent.</param>
    /// <param name="contextObject">Object to serialize as the agent's context (from prior pipeline stages).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized output, or a default instance if deserialization fails.</returns>
    public async Task<TOutput> ExecuteStepAsync<TOutput>(
        string agentId,
        string description,
        object contextObject,
        CancellationToken cancellationToken = default) where TOutput : new()
    {
        var context = JsonSerializer.Serialize(contextObject, AgentJsonDefaults.CamelCaseIndented);

        var result = await ExecuteAgentAsync(agentId, description, context, cancellationToken)
            .ConfigureAwait(false);

        return DeserializeOrDefault<TOutput>(result.Output, typeof(TOutput).Name);
    }

    /// <summary>
    /// Executes a single pipeline step with a raw string context.
    /// </summary>
    /// <typeparam name="TOutput">The expected output type (must have a parameterless constructor).</typeparam>
    /// <param name="agentId">The agent to execute the step.</param>
    /// <param name="description">Task description for the agent.</param>
    /// <param name="context">Pre-serialized context string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized output, or a default instance if deserialization fails.</returns>
    public async Task<TOutput> ExecuteStepAsync<TOutput>(
        string agentId,
        string description,
        string context,
        CancellationToken cancellationToken = default) where TOutput : new()
    {
        var result = await ExecuteAgentAsync(agentId, description, context, cancellationToken)
            .ConfigureAwait(false);

        return DeserializeOrDefault<TOutput>(result.Output, typeof(TOutput).Name);
    }

    /// <summary>
    /// Executes an agent and returns the raw result without deserialization.
    /// Useful for the final pipeline stage where custom merging is needed.
    /// </summary>
    /// <param name="agentId">The agent to execute.</param>
    /// <param name="description">Task description for the agent.</param>
    /// <param name="contextObject">Object to serialize as context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The raw task result from the agent.</returns>
    public async Task<SpecializedTaskResult> ExecuteRawAsync(
        string agentId,
        string description,
        object contextObject,
        CancellationToken cancellationToken = default)
    {
        var context = JsonSerializer.Serialize(contextObject, AgentJsonDefaults.CamelCaseIndented);

        return await ExecuteAgentAsync(agentId, description, context, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Deserializes JSON output to the specified type, returning a default instance on failure.
    /// </summary>
    /// <typeparam name="T">Target type (must have a parameterless constructor).</typeparam>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="typeName">Type name for logging.</param>
    /// <returns>The deserialized object or a new default instance.</returns>
    public T DeserializeOrDefault<T>(string? json, string typeName) where T : new()
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogWarning("Agent returned empty output for {TypeName} — using default", typeName);
            return new T();
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, AgentJsonDefaults.CamelCaseIndented) ?? new T();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize {TypeName} from agent output — using default", typeName);
            return new T();
        }
    }

    private async Task<SpecializedTaskResult> ExecuteAgentAsync(
        string agentId,
        string description,
        string context,
        CancellationToken cancellationToken)
    {
        var task = new SpecializedTask
        {
            Description = description,
            ExpectedOutput = "Structured JSON matching the specified schema",
            Context = context
        };

        var result = await _agentPort.ExecuteTaskAsync(agentId, task, cancellationToken)
            .ConfigureAwait(false);

        if (!result.Success)
        {
            _logger.LogError("Agent {AgentId} failed: {Error}", agentId, result.ErrorMessage);
            throw new InvalidOperationException(
                $"Agent '{agentId}' failed: {result.ErrorMessage}");
        }

        _logger.LogDebug("Agent {AgentId} completed — {Tokens} tokens used", agentId, result.TokensUsed);
        return result;
    }
}
