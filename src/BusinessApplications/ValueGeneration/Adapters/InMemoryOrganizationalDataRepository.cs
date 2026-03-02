using System.Collections.Concurrent;
using CognitiveMesh.ReasoningLayer.ValueGeneration.Engines;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.BusinessApplications.ValueGeneration.Adapters;

/// <summary>
/// In-memory implementation of <see cref="IOrganizationalDataRepository"/>
/// for development, testing, and local environments.  Data is held in a
/// <see cref="ConcurrentDictionary{TKey,TValue}"/> and is not persisted
/// between application restarts.
/// </summary>
public class InMemoryOrganizationalDataRepository : IOrganizationalDataRepository
{
    private readonly ILogger<InMemoryOrganizationalDataRepository> _logger;
    private readonly ConcurrentDictionary<string, OrgDataSnapshot> _store = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryOrganizationalDataRepository"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public InMemoryOrganizationalDataRepository(ILogger<InMemoryOrganizationalDataRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<OrgDataSnapshot> GetOrgDataSnapshotAsync(string organizationId, string[] departmentFilters, string tenantId)
    {
        _logger.LogDebug(
            "Retrieving organizational data snapshot for org '{OrganizationId}' in tenant '{TenantId}'.",
            organizationId, tenantId);

        var key = $"{tenantId}:{organizationId}";
        var data = _store.GetOrAdd(key, _ => new OrgDataSnapshot
        {
            PerceivedValueScores = new Dictionary<string, double>
            {
                { "Engineering", 0.8 },
                { "Marketing", 0.6 }
            },
            ActualImpactScores = new Dictionary<string, double>
            {
                { "Engineering", 0.7 },
                { "Marketing", 0.5 }
            },
            ResourceAllocation = new Dictionary<string, double>
            {
                { "Engineering", 0.6 },
                { "Marketing", 0.4 }
            },
            RecognitionMetrics = new Dictionary<string, double>
            {
                { "Engineering", 0.7 },
                { "Marketing", 0.5 }
            }
        });

        return Task.FromResult(data);
    }

    /// <summary>
    /// Seeds the in-memory store with organizational data for the given organization.
    /// </summary>
    /// <param name="organizationId">The organization identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="data">The organizational data snapshot to store.</param>
    public void Seed(string organizationId, string tenantId, OrgDataSnapshot data)
    {
        var key = $"{tenantId}:{organizationId}";
        _store[key] = data;
    }
}
