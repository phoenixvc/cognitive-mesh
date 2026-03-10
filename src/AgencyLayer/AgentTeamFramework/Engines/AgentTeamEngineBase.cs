using AgencyLayer.Agents.Ports;
using AgencyLayer.AgentTeamFramework.Configuration;
using AgencyLayer.AgentTeamFramework.Pipeline;
using AgencyLayer.AgentTeamFramework.Ports;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.AgentTeamFramework.Engines;

/// <summary>
/// Abstract base class for agent team engines. Provides thread-safe initialization,
/// declarative agent registration via <see cref="DefineAgents"/>, and access to
/// the <see cref="AgentPipelineExecutor"/> for standardized step execution.
/// </summary>
public abstract class AgentTeamEngineBase : IAgentTeamPort
{
    private readonly ISpecializedAgentPort _agentPort;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private volatile bool _initialized;

    /// <summary>
    /// The pipeline executor for running agent steps with standardized serialization.
    /// </summary>
    protected AgentPipelineExecutor Pipeline { get; }

    /// <summary>
    /// Logger instance for the derived team engine.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentTeamEngineBase"/> class.
    /// </summary>
    /// <param name="agentPort">The specialized agent port for agent registration and task execution.</param>
    /// <param name="pipeline">The pipeline executor for standardized step execution.</param>
    /// <param name="logger">Logger instance.</param>
    protected AgentTeamEngineBase(
        ISpecializedAgentPort agentPort,
        AgentPipelineExecutor pipeline,
        ILogger logger)
    {
        _agentPort = agentPort ?? throw new ArgumentNullException(nameof(agentPort));
        Pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public abstract string TeamId { get; }

    /// <inheritdoc />
    public int AgentCount => DefineAgents().Count;

    /// <inheritdoc />
    public bool IsInitialized => _initialized;

    /// <summary>
    /// Returns the agent definitions for this team. Called during initialization
    /// to register all agents. Override this to declare your team's agents as data.
    /// </summary>
    /// <returns>A list of agent definitions to register.</returns>
    protected abstract IReadOnlyList<AgentDefinitionRecord> DefineAgents();

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }

        await _initLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_initialized)
            {
                return;
            }

            var agents = DefineAgents();
            Logger.LogInformation(
                "Initializing agent team {TeamId} — registering {AgentCount} agents",
                TeamId,
                agents.Count);

            await Task.WhenAll(
                agents.Select(a => _agentPort.RegisterAgentAsync(ToConfiguration(a), cancellationToken)))
                .ConfigureAwait(false);

            _initialized = true;
            Logger.LogInformation(
                "Agent team {TeamId} initialization complete — {AgentCount} agents registered",
                TeamId,
                agents.Count);
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// Ensures the team is initialized before executing pipeline operations.
    /// Call this at the start of every public method that uses agents.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    protected async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            await InitializeAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static SpecializedAgentConfiguration ToConfiguration(AgentDefinitionRecord def) => new()
    {
        AgentId = def.AgentId,
        Name = def.Name,
        Type = def.Type,
        SystemPrompt = def.SystemPrompt,
        Temperature = def.Temperature,
        Domains = def.Domains,
        Goals = def.Goals,
        Model = def.Model,
        MaxTokens = def.MaxTokens,
        Tools = def.Tools,
        Capabilities = def.Capabilities,
        Backstory = def.Backstory
    };
}
