namespace CognitiveMesh.FoundationLayer.PolicyStore.Options;

/// <summary>
/// Configuration options for the Cosmos DB–backed policy store.
/// </summary>
public sealed class PolicyStoreOptions
{
    /// <summary>Gets or sets the Cosmos DB connection string.</summary>
    public string CosmosDbConnectionString { get; set; } = string.Empty;

    /// <summary>Gets or sets the Cosmos DB database identifier.</summary>
    public string DatabaseId { get; set; } = "CognitiveMesh";

    /// <summary>Gets or sets the container identifier for remediation policies.</summary>
    public string PolicyContainerId { get; set; } = "RemediationPolicies";

    /// <summary>Gets or sets the container identifier for policy audit log entries.</summary>
    public string AuditContainerId { get; set; } = "PolicyAuditLog";

    /// <summary>Gets or sets the duration that cached policy data remains valid.</summary>
    public TimeSpan CacheTtl { get; set; } = TimeSpan.FromMinutes(5);
}
