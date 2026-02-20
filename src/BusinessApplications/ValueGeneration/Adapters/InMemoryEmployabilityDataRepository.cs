using System.Collections.Concurrent;
using CognitiveMesh.ReasoningLayer.ValueGeneration.Engines;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.BusinessApplications.ValueGeneration.Adapters;

/// <summary>
/// In-memory implementation of <see cref="IEmployabilityDataRepository"/>
/// for development, testing, and local environments.  Data is held in a
/// <see cref="ConcurrentDictionary{TKey,TValue}"/> and is not persisted
/// between application restarts.
/// </summary>
public class InMemoryEmployabilityDataRepository : IEmployabilityDataRepository
{
    private readonly ILogger<InMemoryEmployabilityDataRepository> _logger;
    private readonly ConcurrentDictionary<string, EmployabilityData> _store = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryEmployabilityDataRepository"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public InMemoryEmployabilityDataRepository(ILogger<InMemoryEmployabilityDataRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<EmployabilityData> GetEmployabilityDataAsync(string userId, string tenantId)
    {
        _logger.LogDebug(
            "Retrieving employability data for user '{UserId}' in tenant '{TenantId}'.",
            userId, tenantId);

        var key = $"{tenantId}:{userId}";
        var data = _store.GetOrAdd(key, _ => new EmployabilityData
        {
            UserSkills = new List<string> { "C#", "Azure", "SQL" },
            MarketTrendingSkills = new List<string> { "C#", "Azure", "AI", "Kubernetes" },
            UserCreativeOutputScore = 0.5,
            ProjectsCompleted = 3,
            CollaborationScore = 0.6,
            SkillRelevanceScores = new Dictionary<string, double>
            {
                { "C#", 0.8 },
                { "Azure", 0.9 },
                { "SQL", 0.6 }
            }
        });

        return Task.FromResult(data);
    }

    /// <summary>
    /// Seeds the in-memory store with employability data for the given user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="data">The employability data to store.</param>
    public void Seed(string userId, string tenantId, EmployabilityData data)
    {
        var key = $"{tenantId}:{userId}";
        _store[key] = data;
    }
}
