using FoundationLayer.ConvenerData;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.BusinessApplications.ConvenerServices.UseCases;

/// <summary>
/// Request DTO for the champion discovery use case.
/// </summary>
public class DiscoverChampionsRequest
{
    /// <summary>Tenant ID for data scoping.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Optional skill filter to narrow champion discovery.</summary>
    public string? SkillFilter { get; set; }

    /// <summary>Maximum number of champions to return.</summary>
    public int MaxResults { get; set; } = 10;
}

/// <summary>
/// Response DTO from the champion discovery use case.
/// </summary>
public class DiscoverChampionsResponse
{
    /// <summary>The discovered and ranked champions.</summary>
    public List<ChampionSummary> Champions { get; set; } = new();

    /// <summary>Total champions evaluated before filtering.</summary>
    public int TotalEvaluated { get; set; }
}

/// <summary>
/// Summary view of a single champion for API responses.
/// </summary>
public class ChampionSummary
{
    /// <summary>User ID of the champion.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Calculated influence score.</summary>
    public double InfluenceScore { get; set; }

    /// <summary>Skills attributed to the champion.</summary>
    public List<string> Skills { get; set; } = new();

    /// <summary>Number of significant interactions logged.</summary>
    public int InteractionCount { get; set; }

    /// <summary>Last recorded activity date.</summary>
    public DateTimeOffset LastActiveDate { get; set; }
}

/// <summary>
/// Orchestrates champion discovery by coordinating the ChampionScorer with
/// data retrieval and filtering. This is the application-level use case
/// consumed by the ConvenerController.
/// </summary>
public class DiscoverChampionsUseCase
{
    private readonly ILogger<DiscoverChampionsUseCase> _logger;
    private readonly IChampionDiscoveryPort _championDiscoveryPort;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscoverChampionsUseCase"/> class.
    /// </summary>
    /// <param name="logger">Logger for structured diagnostics.</param>
    /// <param name="championDiscoveryPort">Port for champion data retrieval and scoring.</param>
    public DiscoverChampionsUseCase(
        ILogger<DiscoverChampionsUseCase> logger,
        IChampionDiscoveryPort championDiscoveryPort)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _championDiscoveryPort = championDiscoveryPort ?? throw new ArgumentNullException(nameof(championDiscoveryPort));
    }

    /// <summary>
    /// Executes the champion discovery workflow: fetch candidates, score, rank, and return.
    /// </summary>
    /// <param name="request">The discovery request with tenant scoping and filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A response containing ranked champion summaries.</returns>
    public async Task<DiscoverChampionsResponse> ExecuteAsync(
        DiscoverChampionsRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Discovering champions for Tenant '{TenantId}', Skill='{Skill}', Max={Max}",
            request.TenantId, request.SkillFilter ?? "any", request.MaxResults);

        var candidates = await _championDiscoveryPort.GetChampionCandidatesAsync(
            request.TenantId, request.SkillFilter, cancellationToken);

        var candidateList = candidates.ToList();
        _logger.LogDebug("Found {Count} champion candidates", candidateList.Count);

        var ranked = await _championDiscoveryPort.ScoreAndRankAsync(
            candidateList, cancellationToken);

        var topChampions = ranked
            .Take(request.MaxResults)
            .Select(c => new ChampionSummary
            {
                UserId = c.UserId,
                InfluenceScore = c.InfluenceScore,
                Skills = c.Skills.Select(s => s.Name).ToList(),
                InteractionCount = c.InteractionCount,
                LastActiveDate = c.LastActiveDate
            })
            .ToList();

        return new DiscoverChampionsResponse
        {
            Champions = topChampions,
            TotalEvaluated = candidateList.Count
        };
    }
}

/// <summary>
/// Port interface for champion discovery operations.
/// Abstracts the data retrieval and scoring logic from the use case.
/// </summary>
public interface IChampionDiscoveryPort
{
    /// <summary>
    /// Retrieves champion candidates for a tenant, optionally filtered by skill.
    /// </summary>
    Task<IEnumerable<Champion>> GetChampionCandidatesAsync(
        string tenantId, string? skillFilter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Scores and ranks champions by influence.
    /// </summary>
    Task<IEnumerable<Champion>> ScoreAndRankAsync(
        IEnumerable<Champion> candidates, CancellationToken cancellationToken = default);
}
