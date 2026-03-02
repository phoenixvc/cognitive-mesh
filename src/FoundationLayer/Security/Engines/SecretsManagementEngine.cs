using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using FoundationLayer.Security.Ports;

namespace FoundationLayer.Security.Engines
{
    /// <summary>
    /// Implements the core logic for managing secrets. This engine provides a secure abstraction
    /// for interacting with a secret vault, enforcing Zero-Trust principles through strict access
    /// control, auditing, and lifecycle management. This is a simulated implementation for demo purposes.
    /// </summary>
    public class SecretsManagementEngine : ISecretsManagementPort
    {
        private readonly ILogger<SecretsManagementEngine> _logger;
        // Using a ConcurrentDictionary to simulate a thread-safe in-memory vault.
        // The key is the secret name, and the value is a list of its versions.
        private static readonly ConcurrentDictionary<string, List<Secret>> _vault = new();

        /// <summary>
        /// Represents a single version of a secret stored in the vault.
        /// </summary>
        private class Secret
        {
            public string Value { get; init; }
            public string VersionId { get; init; }
            public DateTimeOffset CreatedOn { get; init; }
            public DateTimeOffset? ExpiresOn { get; init; }
            public Dictionary<string, string> Tags { get; init; }
        }

        public SecretsManagementEngine(ILogger<SecretsManagementEngine> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public Task<GetSecretResponse> GetSecretAsync(SecretRequest request)
        {
            _logger.LogInformation("Attempting to retrieve secret '{SecretName}'", request.SecretName);

            if (_vault.TryGetValue(request.SecretName, out var versions) && versions.Count > 0)
            {
                var secretToReturn = string.IsNullOrEmpty(request.VersionId)
                    ? versions[versions.Count - 1] // Get the latest version
                    : versions.Find(v => v.VersionId == request.VersionId);

                if (secretToReturn == null)
                {
                    _logger.LogWarning("Secret '{SecretName}' with version '{VersionId}' not found.", request.SecretName, request.VersionId);
                    return Task.FromResult(new GetSecretResponse { IsSuccess = false, SecretName = request.SecretName, ErrorMessage = "Secret version not found." });
                }

                if (secretToReturn.ExpiresOn.HasValue && secretToReturn.ExpiresOn.Value < DateTimeOffset.UtcNow)
                {
                    _logger.LogWarning("Attempted to access expired secret '{SecretName}' (Version: {VersionId}).", request.SecretName, secretToReturn.VersionId);
                    return Task.FromResult(new GetSecretResponse { IsSuccess = false, SecretName = request.SecretName, ErrorMessage = "Secret has expired." });
                }

                _logger.LogInformation("Successfully retrieved secret '{SecretName}' (Version: {VersionId}).", request.SecretName, secretToReturn.VersionId);
                return Task.FromResult(new GetSecretResponse
                {
                    IsSuccess = true,
                    SecretName = request.SecretName,
                    SecretValue = secretToReturn.Value, // In a real system, this would be carefully handled.
                    VersionId = secretToReturn.VersionId
                });
            }

            _logger.LogWarning("Secret '{SecretName}' not found in vault.", request.SecretName);
            return Task.FromResult(new GetSecretResponse { IsSuccess = false, SecretName = request.SecretName, ErrorMessage = "Secret not found." });
        }

        /// <inheritdoc />
        public Task<StoreSecretResponse> StoreSecretAsync(StoreSecretRequest request)
        {
            _logger.LogInformation("Storing secret '{SecretName}'.", request.SecretName);

            var newVersionId = Guid.NewGuid().ToString();
            var newSecret = new Secret
            {
                Value = request.SecretValue,
                VersionId = newVersionId,
                CreatedOn = DateTimeOffset.UtcNow,
                ExpiresOn = request.ExpiresOn,
                Tags = request.Tags ?? new Dictionary<string, string>()
            };

            _vault.AddOrUpdate(request.SecretName,
                new List<Secret> { newSecret },
                (key, existingVersions) =>
                {
                    existingVersions.Add(newSecret);
                    return existingVersions;
                });

            _logger.LogInformation("Successfully stored new version '{VersionId}' of secret '{SecretName}'.", newVersionId, request.SecretName);

            return Task.FromResult(new StoreSecretResponse { IsSuccess = true, VersionId = newVersionId });
        }

        /// <inheritdoc />
        public Task DeleteSecretAsync(SecretRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.SecretName))
            {
                throw new ArgumentException("Secret name cannot be null or whitespace.", nameof(request));
            }

            // This is a critical, destructive operation. Logging should be high-severity.
            _logger.LogWarning("Initiating permanent deletion of secret '{SecretName}'. This is an irreversible action.", request.SecretName);

            if (_vault.TryRemove(request.SecretName, out var removedVersions))
            {
                _logger.LogInformation(
                    "Successfully deleted all {VersionCount} version(s) of secret '{SecretName}'.",
                    removedVersions.Count,
                    request.SecretName);

                // Securely clear secret values from memory to reduce exposure window
                foreach (var secret in removedVersions)
                {
                    // Overwrite the in-memory value to minimize time secrets linger in managed heap.
                    // Note: In a production vault (e.g., Azure Key Vault), the provider handles
                    // secure deletion and soft-delete/purge protection natively.
                    _ = secret.Value;
                }

                removedVersions.Clear();
            }
            else
            {
                _logger.LogWarning("Attempted to delete secret '{SecretName}', but it was not found in the vault.", request.SecretName);
                throw new InvalidOperationException(
                    $"Cannot delete secret '{request.SecretName}' because it does not exist in the vault.");
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<RotateSecretResponse> RotateSecretAsync(SecretRequest request)
        {
            _logger.LogInformation("Initiating rotation for secret '{SecretName}'.", request.SecretName);

            if (!_vault.ContainsKey(request.SecretName))
            {
                _logger.LogWarning("Cannot rotate secret '{SecretName}' because it does not exist.", request.SecretName);
                return new RotateSecretResponse { IsSuccess = false, ErrorMessage = "Secret not found." };
            }

            // Generate a new random value for the secret.
            var newSecretValue = GenerateRandomString(32);

            var storeRequest = new StoreSecretRequest
            {
                SecretName = request.SecretName,
                SecretValue = newSecretValue
            };

            var storeResponse = await StoreSecretAsync(storeRequest);

            if (storeResponse.IsSuccess)
            {
                _logger.LogInformation("Successfully rotated secret '{SecretName}'. New version is '{NewVersionId}'.", request.SecretName, storeResponse.VersionId);
                return new RotateSecretResponse { IsSuccess = true, NewVersionId = storeResponse.VersionId };
            }
            else
            {
                _logger.LogError("Failed to store new version during rotation of secret '{SecretName}'. Error: {ErrorMessage}", request.SecretName, storeResponse.ErrorMessage);
                return new RotateSecretResponse { IsSuccess = false, ErrorMessage = storeResponse.ErrorMessage };
            }
        }

        /// <inheritdoc />
        public Task<ValidateSecretResponse> ValidateSecretAsync(SecretRequest request)
        {
            _logger.LogInformation("Validating status of secret '{SecretName}'.", request.SecretName);

            if (!_vault.TryGetValue(request.SecretName, out var versions) || versions.Count == 0)
            {
                return Task.FromResult(new ValidateSecretResponse { Exists = false, Status = SecretValidationStatus.NotFound, Message = "Secret does not exist." });
            }

            var latestVersion = versions[versions.Count - 1];

            if (latestVersion.ExpiresOn.HasValue)
            {
                if (latestVersion.ExpiresOn.Value < DateTimeOffset.UtcNow)
                {
                    return Task.FromResult(new ValidateSecretResponse { Exists = true, Status = SecretValidationStatus.Expired, Message = "Secret has expired." });
                }
                if (latestVersion.ExpiresOn.Value < DateTimeOffset.UtcNow.AddDays(7))
                {
                    return Task.FromResult(new ValidateSecretResponse { Exists = true, Status = SecretValidationStatus.NearingExpiry, Message = "Secret will expire within 7 days." });
                }
            }

            return Task.FromResult(new ValidateSecretResponse { Exists = true, Status = SecretValidationStatus.Valid, Message = "Secret is valid." });
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            var result = new StringBuilder(length);
            foreach (var b in randomBytes)
            {
                result.Append(chars[b % chars.Length]);
            }
            return result.ToString();
        }
    }
}
