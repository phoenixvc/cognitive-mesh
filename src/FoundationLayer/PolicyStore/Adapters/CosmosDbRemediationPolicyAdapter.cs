using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using CognitiveMesh.FoundationLayer.PolicyStore.Models;
using CognitiveMesh.FoundationLayer.PolicyStore.Options;
using CognitiveMesh.FoundationLayer.PolicyStore.Ports;

namespace CognitiveMesh.FoundationLayer.PolicyStore.Adapters;

/// <summary>
/// Cosmos DB implementation of <see cref="IRemediationPolicyPort"/> that provides
/// policy retrieval, upsert, deletion, and version history with in-memory caching.
/// </summary>
public sealed class CosmosDbRemediationPolicyAdapter : IRemediationPolicyPort
{
    private const string PolicyCacheKey = "policies:all";

    private readonly CosmosClient _cosmosClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CosmosDbRemediationPolicyAdapter> _logger;
    private readonly PolicyStoreOptions _options;

    /// <summary>
    /// Initialises a new instance of the <see cref="CosmosDbRemediationPolicyAdapter"/> class.
    /// </summary>
    /// <param name="options">The policy store configuration options.</param>
    /// <param name="cache">The in-memory cache instance.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is <see langword="null"/>.</exception>
    public CosmosDbRemediationPolicyAdapter(
        IOptions<PolicyStoreOptions> options,
        IMemoryCache cache,
        ILogger<CosmosDbRemediationPolicyAdapter> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _cache = cache;
        _logger = logger;
        _cosmosClient = new CosmosClient(_options.CosmosDbConnectionString);
    }

    /// <inheritdoc />
    public async Task<RemediationPolicy> GetPolicyAsync(
        string incidentCategory,
        string severity,
        CancellationToken ct = default)
    {
        var cacheKey = $"policy:{incidentCategory}:{severity}";

        if (_cache.TryGetValue(cacheKey, out RemediationPolicy? cached) && cached is not null)
        {
            _logger.LogDebug("Cache hit for policy {Category}/{Severity}", incidentCategory, severity);
            return cached;
        }

        try
        {
            var container = GetPolicyContainer();
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.IncidentCategory = @category AND c.Severity = @severity AND c.IsActive = true")
                .WithParameter("@category", incidentCategory)
                .WithParameter("@severity", severity);

            using var iterator = container.GetItemQueryIterator<RemediationPolicy>(
                query,
                requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(incidentCategory) });

            var results = new List<RemediationPolicy>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(ct).ConfigureAwait(false);
                results.AddRange(response);
            }

            var policy = results.OrderByDescending(p => p.Version).FirstOrDefault();

            if (policy is null)
            {
                _logger.LogWarning(
                    "No policy found for {Category}/{Severity}; returning default",
                    incidentCategory,
                    severity);
                return BuildDefaultPolicy(incidentCategory, severity);
            }

            _cache.Set(cacheKey, policy, _options.CacheTtl);
            return policy;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos DB error retrieving policy for {Category}/{Severity}", incidentCategory, severity);
            return BuildDefaultPolicy(incidentCategory, severity);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RemediationPolicy>> ListPoliciesAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(PolicyCacheKey, out IEnumerable<RemediationPolicy>? cached) && cached is not null)
        {
            _logger.LogDebug("Cache hit for all policies");
            return cached;
        }

        try
        {
            var container = GetPolicyContainer();
            var query = new QueryDefinition("SELECT * FROM c WHERE c.IsActive = true");

            using var iterator = container.GetItemQueryIterator<RemediationPolicy>(query);

            var results = new List<RemediationPolicy>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(ct).ConfigureAwait(false);
                results.AddRange(response);
            }

            _cache.Set(PolicyCacheKey, results.AsEnumerable(), _options.CacheTtl);
            return results;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos DB error listing policies");
            return [];
        }
    }

    /// <inheritdoc />
    public async Task<RemediationPolicy> UpsertPolicyAsync(
        RemediationPolicy policy,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var container = GetPolicyContainer();

        // Deactivate any existing active version
        var existingQuery = new QueryDefinition(
            "SELECT * FROM c WHERE c.id = @id AND c.IsActive = true")
            .WithParameter("@id", policy.Id.ToString());

        using var iterator = container.GetItemQueryIterator<RemediationPolicy>(
            existingQuery,
            requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(policy.IncidentCategory) });

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct).ConfigureAwait(false);
            foreach (var existing in response)
            {
                existing.IsActive = false;
                await container.UpsertItemAsync(
                    existing,
                    new PartitionKey(existing.IncidentCategory),
                    cancellationToken: ct).ConfigureAwait(false);
            }
        }

        // Increment version and persist the new policy
        policy.Version += 1;
        policy.UpdatedAt = DateTimeOffset.UtcNow;

        var result = await container.UpsertItemAsync(
            policy,
            new PartitionKey(policy.IncidentCategory),
            cancellationToken: ct).ConfigureAwait(false);

        // Write audit entry
        var auditEntry = new PolicyAuditEntry
        {
            Id = Guid.NewGuid(),
            PolicyId = policy.Id,
            Operation = "Upsert",
            Version = policy.Version,
            Actor = policy.CreatedBy,
            Timestamp = DateTimeOffset.UtcNow,
            IncidentCategory = policy.IncidentCategory
        };

        var auditContainer = GetAuditContainer();
        await auditContainer.UpsertItemAsync(
            auditEntry,
            new PartitionKey(auditEntry.IncidentCategory),
            cancellationToken: ct).ConfigureAwait(false);

        InvalidateCache(policy.IncidentCategory, policy.Severity);

        _logger.LogInformation(
            "Upserted policy {PolicyId} version {Version} for {Category}/{Severity}",
            policy.Id,
            policy.Version,
            policy.IncidentCategory,
            policy.Severity);

        return result.Resource;
    }

    /// <inheritdoc />
    public async Task DeletePolicyAsync(Guid id, CancellationToken ct = default)
    {
        var container = GetPolicyContainer();

        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", id.ToString());

        using var iterator = container.GetItemQueryIterator<RemediationPolicy>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct).ConfigureAwait(false);
            foreach (var policy in response)
            {
                policy.IsActive = false;
                await container.UpsertItemAsync(
                    policy,
                    new PartitionKey(policy.IncidentCategory),
                    cancellationToken: ct).ConfigureAwait(false);

                // Write audit entry
                var auditEntry = new PolicyAuditEntry
                {
                    Id = Guid.NewGuid(),
                    PolicyId = id,
                    Operation = "Delete",
                    Version = policy.Version,
                    Actor = "system",
                    Timestamp = DateTimeOffset.UtcNow,
                    IncidentCategory = policy.IncidentCategory
                };

                var auditContainer = GetAuditContainer();
                await auditContainer.UpsertItemAsync(
                    auditEntry,
                    new PartitionKey(auditEntry.IncidentCategory),
                    cancellationToken: ct).ConfigureAwait(false);

                InvalidateCache(policy.IncidentCategory, policy.Severity);
            }
        }

        _logger.LogInformation("Deleted (soft) policy {PolicyId}", id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RemediationPolicy>> GetPolicyHistoryAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var container = GetPolicyContainer();

        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", id.ToString());

        using var iterator = container.GetItemQueryIterator<RemediationPolicy>(query);

        var results = new List<RemediationPolicy>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct).ConfigureAwait(false);
            results.AddRange(response);
        }

        return results.OrderByDescending(p => p.Version);
    }

    private static RemediationPolicy BuildDefaultPolicy(string incidentCategory, string severity) =>
        new()
        {
            Id = Guid.NewGuid(),
            Version = 1,
            IsActive = true,
            IncidentCategory = incidentCategory,
            Severity = severity,
            AllowedActions = RemediationAction.Retry | RemediationAction.Escalate,
            RankingWeights = new Dictionary<string, double>
            {
                ["Retry"] = 0.7,
                ["Escalate"] = 0.3
            },
            MaxRetries = 3,
            RepeatedFailureEscalationThreshold = 5,
            HumanInLoopRequired = false,
            ApproverRoles = [],
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "system"
        };

    private Container GetPolicyContainer() =>
        _cosmosClient.GetContainer(_options.DatabaseId, _options.PolicyContainerId);

    private Container GetAuditContainer() =>
        _cosmosClient.GetContainer(_options.DatabaseId, _options.AuditContainerId);

    private void InvalidateCache(string incidentCategory, string severity)
    {
        _cache.Remove($"policy:{incidentCategory}:{severity}");
        _cache.Remove(PolicyCacheKey);
    }
}
