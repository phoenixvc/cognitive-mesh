using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AgencyLayer.AgentTeamFramework.Prompts;

/// <summary>
/// Loads prompt templates from YAML files following the convention
/// <c>{basePath}/{teamId}/Prompts/*.prompt.yaml</c>.
/// Files are cached after first load for the lifetime of the adapter instance.
/// </summary>
public sealed class YamlPromptRegistryAdapter : IPromptRegistryPort
{
    private readonly string _basePath;
    private readonly ILogger<YamlPromptRegistryAdapter> _logger;
    private readonly IDeserializer _deserializer;
    private readonly Dictionary<string, PromptTemplate> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _loadedTeams = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="YamlPromptRegistryAdapter"/> class.
    /// </summary>
    /// <param name="basePath">Root directory containing team subdirectories with prompt files.</param>
    /// <param name="logger">Logger instance.</param>
    public YamlPromptRegistryAdapter(string basePath, ILogger<YamlPromptRegistryAdapter> logger)
    {
        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <inheritdoc />
    public async Task<PromptTemplate?> GetPromptAsync(string promptId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(promptId);

        // Check cache first
        if (_cache.TryGetValue(promptId, out var cached))
            return cached;

        // Try to infer team from prompt ID (convention: "{teamid}-{agentname}")
        var dashIndex = promptId.IndexOf('-');
        if (dashIndex > 0)
        {
            var teamId = promptId[..dashIndex];
            await LoadTeamPromptsInternalAsync(teamId, cancellationToken).ConfigureAwait(false);

            if (_cache.TryGetValue(promptId, out cached))
                return cached;
        }

        _logger.LogDebug("Prompt {PromptId} not found in registry", promptId);
        return null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PromptTemplate>> GetTeamPromptsAsync(string teamId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(teamId);

        await LoadTeamPromptsInternalAsync(teamId, cancellationToken).ConfigureAwait(false);

        return _cache.Values
            .Where(p => p.Id.StartsWith(teamId, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string promptId, CancellationToken cancellationToken = default)
    {
        var prompt = await GetPromptAsync(promptId, cancellationToken).ConfigureAwait(false);
        return prompt is not null;
    }

    private async Task LoadTeamPromptsInternalAsync(string teamId, CancellationToken cancellationToken)
    {
        if (_loadedTeams.Contains(teamId))
            return;

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Double-check after acquiring lock
            if (_loadedTeams.Contains(teamId))
                return;

            var promptsDir = FindPromptsDirectory(teamId);
            if (promptsDir is null)
            {
                _logger.LogWarning("No Prompts directory found for team {TeamId} under {BasePath}", teamId, _basePath);
                _loadedTeams.Add(teamId);
                return;
            }

            var yamlFiles = Directory.GetFiles(promptsDir, "*.prompt.yaml");
            _logger.LogDebug("Loading {Count} prompt files for team {TeamId} from {Directory}", yamlFiles.Length, teamId, promptsDir);

            foreach (var file in yamlFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var yaml = await File.ReadAllTextAsync(file, cancellationToken).ConfigureAwait(false);
                    var template = _deserializer.Deserialize<PromptTemplate>(yaml);

                    if (string.IsNullOrWhiteSpace(template.Id))
                    {
                        _logger.LogWarning("Prompt file {File} has no id — skipping", file);
                        continue;
                    }

                    _cache[template.Id] = template;
                    _logger.LogDebug("Loaded prompt {PromptId} v{Version} from {File}", template.Id, template.Version, Path.GetFileName(file));
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Failed to parse prompt file {File}", file);
                }
            }

            _loadedTeams.Add(teamId);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Searches for the Prompts directory for a given team.
    /// Supports two conventions:
    /// <list type="bullet">
    ///   <item><c>{basePath}/{TeamDirectory}/Prompts/</c> (e.g., <c>src/AgencyLayer/RoadmapCrew/Prompts/</c>)</item>
    ///   <item><c>{basePath}/Prompts/</c> (flat layout)</item>
    /// </list>
    /// </summary>
    private string? FindPromptsDirectory(string teamId)
    {
        // Convention 1: Look for a directory matching the team name (case-insensitive)
        if (Directory.Exists(_basePath))
        {
            var teamDirs = Directory.GetDirectories(_basePath);
            foreach (var dir in teamDirs)
            {
                var dirName = Path.GetFileName(dir);
                if (dirName.Contains(teamId, StringComparison.OrdinalIgnoreCase))
                {
                    var promptsPath = Path.Combine(dir, "Prompts");
                    if (Directory.Exists(promptsPath))
                        return promptsPath;
                }
            }
        }

        // Convention 2: Direct Prompts subdirectory
        var directPath = Path.Combine(_basePath, "Prompts");
        if (Directory.Exists(directPath))
            return directPath;

        return null;
    }
}
