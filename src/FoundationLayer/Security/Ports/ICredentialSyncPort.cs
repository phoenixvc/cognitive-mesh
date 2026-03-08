namespace FoundationLayer.Security.Ports;

/// <summary>
/// Types of credentials that can be managed.
/// </summary>
public enum CredentialType
{
    /// <summary>API key or token.</summary>
    ApiKey,
    /// <summary>Username and password pair.</summary>
    UsernamePassword,
    /// <summary>OAuth2 client credentials.</summary>
    OAuth2ClientCredentials,
    /// <summary>X.509 certificate.</summary>
    Certificate,
    /// <summary>SSH private key.</summary>
    SshKey,
    /// <summary>Connection string (database, service bus, etc.).</summary>
    ConnectionString,
    /// <summary>Service principal or managed identity.</summary>
    ServicePrincipal,
    /// <summary>Custom credential type.</summary>
    Custom
}

/// <summary>
/// Status of a credential.
/// </summary>
public enum CredentialStatus
{
    /// <summary>Credential is active and valid.</summary>
    Active,
    /// <summary>Credential is pending rotation.</summary>
    PendingRotation,
    /// <summary>Credential has been rotated, old value still valid during grace period.</summary>
    Rotating,
    /// <summary>Credential has expired.</summary>
    Expired,
    /// <summary>Credential has been revoked.</summary>
    Revoked,
    /// <summary>Credential status is unknown.</summary>
    Unknown
}

/// <summary>
/// Represents a managed credential.
/// </summary>
public class ManagedCredential
{
    /// <summary>Unique identifier for the credential.</summary>
    public required string CredentialId { get; init; }

    /// <summary>Human-readable name for the credential.</summary>
    public required string Name { get; init; }

    /// <summary>Type of credential.</summary>
    public required CredentialType Type { get; init; }

    /// <summary>Current status of the credential.</summary>
    public CredentialStatus Status { get; init; } = CredentialStatus.Active;

    /// <summary>External vault reference (e.g., Azure Key Vault secret URL).</summary>
    public string? VaultReference { get; init; }

    /// <summary>When the credential was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>When the credential expires (null = never).</summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>When the credential was last rotated.</summary>
    public DateTimeOffset? LastRotatedAt { get; init; }

    /// <summary>Next scheduled rotation time.</summary>
    public DateTimeOffset? NextRotationAt { get; init; }

    /// <summary>Tags for categorization.</summary>
    public Dictionary<string, string> Tags { get; init; } = new();

    /// <summary>Services or agents that use this credential.</summary>
    public IReadOnlyList<string> UsedBy { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Request to sync credentials from an external vault.
/// </summary>
public class CredentialSyncRequest
{
    /// <summary>The vault provider (AzureKeyVault, HashiCorpVault, AWS SecretsManager).</summary>
    public required string VaultProvider { get; init; }

    /// <summary>The vault URL or identifier.</summary>
    public required string VaultUrl { get; init; }

    /// <summary>Specific credentials to sync (null = all).</summary>
    public IReadOnlyList<string>? CredentialNames { get; init; }

    /// <summary>Whether to force refresh even if not expired.</summary>
    public bool ForceRefresh { get; init; }
}

/// <summary>
/// Result of a credential sync operation.
/// </summary>
public class CredentialSyncResult
{
    /// <summary>Whether the sync was successful.</summary>
    public required bool Success { get; init; }

    /// <summary>Credentials that were synced.</summary>
    public IReadOnlyList<string> SyncedCredentials { get; init; } = Array.Empty<string>();

    /// <summary>Credentials that failed to sync.</summary>
    public Dictionary<string, string> FailedCredentials { get; init; } = new();

    /// <summary>When the sync completed.</summary>
    public DateTimeOffset CompletedAt { get; init; }
}

/// <summary>
/// Request to rotate a credential.
/// </summary>
public class CredentialRotationRequest
{
    /// <summary>The credential to rotate.</summary>
    public required string CredentialId { get; init; }

    /// <summary>Grace period where both old and new values are valid.</summary>
    public TimeSpan GracePeriod { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>Reason for rotation.</summary>
    public string? Reason { get; init; }

    /// <summary>Whether to notify dependent services.</summary>
    public bool NotifyDependents { get; init; } = true;
}

/// <summary>
/// Port for external credential synchronization and management.
/// Implements the "External Credential Sync" pattern.
/// </summary>
/// <remarks>
/// This port provides integration with external secret management systems
/// (Azure Key Vault, HashiCorp Vault, AWS Secrets Manager) for secure
/// credential storage, rotation, and synchronization.
/// </remarks>
public interface ICredentialSyncPort
{
    /// <summary>
    /// Retrieves the current value of a credential.
    /// </summary>
    /// <param name="credentialId">The credential identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The credential value (sensitive, handle securely).</returns>
    Task<string> GetCredentialValueAsync(
        string credentialId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metadata about a credential without retrieving its value.
    /// </summary>
    /// <param name="credentialId">The credential identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The credential metadata.</returns>
    Task<ManagedCredential?> GetCredentialMetadataAsync(
        string credentialId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all managed credentials.
    /// </summary>
    /// <param name="type">Filter by credential type (null = all).</param>
    /// <param name="status">Filter by status (null = all).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of credential metadata.</returns>
    Task<IReadOnlyList<ManagedCredential>> ListCredentialsAsync(
        CredentialType? type = null,
        CredentialStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs credentials from an external vault.
    /// </summary>
    /// <param name="request">The sync request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The sync result.</returns>
    Task<CredentialSyncResult> SyncFromVaultAsync(
        CredentialSyncRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates a credential to a new value.
    /// </summary>
    /// <param name="request">The rotation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated credential metadata.</returns>
    Task<ManagedCredential> RotateCredentialAsync(
        CredentialRotationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a credential immediately.
    /// </summary>
    /// <param name="credentialId">The credential to revoke.</param>
    /// <param name="reason">The reason for revocation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RevokeCredentialAsync(
        string credentialId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets credentials that are expiring within a time window.
    /// </summary>
    /// <param name="window">The time window to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of expiring credentials.</returns>
    Task<IReadOnlyList<ManagedCredential>> GetExpiringCredentialsAsync(
        TimeSpan window,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a credential is still valid and not compromised.
    /// </summary>
    /// <param name="credentialId">The credential to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if valid, false otherwise.</returns>
    Task<bool> ValidateCredentialAsync(
        string credentialId,
        CancellationToken cancellationToken = default);
}
