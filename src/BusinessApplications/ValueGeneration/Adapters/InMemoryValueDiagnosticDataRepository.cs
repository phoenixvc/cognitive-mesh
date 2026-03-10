using System.Collections.Concurrent;
using CognitiveMesh.ReasoningLayer.ValueGeneration.Engines;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.BusinessApplications.ValueGeneration.Adapters;

/// <summary>
/// In-memory implementation of <see cref="IValueDiagnosticDataRepository"/>
/// for development, testing, and local environments.  Data is held in a
/// <see cref="ConcurrentDictionary{TKey,TValue}"/> and is not persisted
/// between application restarts.
/// </summary>
public class InMemoryValueDiagnosticDataRepository : IValueDiagnosticDataRepository
{
    private readonly ILogger<InMemoryValueDiagnosticDataRepository> _logger;
    private readonly ConcurrentDictionary<string, ValueDiagnosticData> _store = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryValueDiagnosticDataRepository"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public InMemoryValueDiagnosticDataRepository(ILogger<InMemoryValueDiagnosticDataRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<ValueDiagnosticData> GetValueDiagnosticDataAsync(string targetId, string tenantId)
    {
        _logger.LogDebug(
            "Retrieving value diagnostic data for target '{TargetId}' in tenant '{TenantId}'.",
            targetId, tenantId);

        var key = $"{tenantId}:{targetId}";
        var data = _store.GetOrAdd(key, _ => new ValueDiagnosticData
        {
            AverageImpactScore = 0.65,
            HighValueContributions = 4,
            CreativityEvents = 3
        });

        return Task.FromResult(data);
    }

    /// <inheritdoc />
    public Task<OrgDataSnapshot> GetOrgDataSnapshotAsync(string organizationId, string[] departmentFilters, string tenantId)
    {
        _logger.LogDebug(
            "Retrieving org data snapshot for organization '{OrganizationId}' in tenant '{TenantId}'.",
            organizationId, tenantId);

        var snapshot = new OrgDataSnapshot
        {
            PerceivedValueScores = new Dictionary<string, double> { { "Engineering", 0.8 }, { "Marketing", 0.6 } },
            ActualImpactScores = new Dictionary<string, double> { { "Engineering", 0.7 }, { "Marketing", 0.75 } },
            ResourceAllocation = new Dictionary<string, double> { { "Engineering", 0.5 }, { "Marketing", 0.3 } },
            RecognitionMetrics = new Dictionary<string, double> { { "Engineering", 0.9 }, { "Marketing", 0.4 } }
        };

        return Task.FromResult(snapshot);
    }

    /// <summary>
    /// Seeds the in-memory store with diagnostic data for the given target.
    /// </summary>
    /// <param name="targetId">The target identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="data">The diagnostic data to store.</param>
    public void Seed(string targetId, string tenantId, ValueDiagnosticData data)
    {
        var key = $"{tenantId}:{targetId}";
        _store[key] = data;
    }
}
