using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveMesh.FoundationLayer.Security.Ports.Models
{
    /// <summary>
    /// Base request model for operations targeting a specific secret.
    /// </summary>
    public class SecretRequest
    {
        /// <summary>
        /// The unique name or identifier for the secret in the vault.
        /// </summary>
        public string SecretName { get; set; }

        /// <summary>
        /// Optional. The specific version of the secret to target. If null, the latest version is assumed.
        /// </summary>
        public string VersionId { get; set; }
    }

    /// <summary>
    /// Represents the response for a secret retrieval operation.
    /// </summary>
    public class GetSecretResponse
    {
        public bool IsSuccess { get; set; }
        /// <summary>
        /// The name of the secret.
        /// </summary>
        public string SecretName { get; set; }
        /// <summary>
        /// The retrieved secret value.
        /// </summary>
        public string SecretValue { get; set; }
        /// <summary>
        /// The version identifier of the retrieved secret.
        /// </summary>
        public string VersionId { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Represents a request to store (create or update) a secret in the vault.
    /// </summary>
    public class StoreSecretRequest
    {
        /// <summary>
        /// The unique name for the secret.
        /// </summary>
        public string SecretName { get; set; }
        /// <summary>
        /// The value of the secret to be stored.
        /// </summary>
        public string SecretValue { get; set; }
        /// <summary>
        /// Optional. Metadata tags to associate with the secret.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new();
        /// <summary>
        /// Optional. The expiration date for the secret.
        /// </summary>
        public DateTimeOffset? ExpiresOn { get; set; }
    }

    /// <summary>
    /// Represents the response from a store operation.
    /// </summary>
    public class StoreSecretResponse
    {
        public bool IsSuccess { get; set; }
        /// <summary>
        /// The version identifier of the newly created or updated secret.
        /// </summary>
        public string VersionId { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Represents the response from a secret rotation operation.
    /// </summary>
    public class RotateSecretResponse
    {
        public bool IsSuccess { get; set; }
        /// <summary>
        /// The version identifier of the new secret created during rotation.
        /// </summary>
        public string NewVersionId { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Defines the possible health statuses of a secret.
    /// </summary>
    public enum SecretValidationStatus
    {
        Unknown,
        Valid,
        Expired,
        NearingExpiry,
        NotFound
    }

    /// <summary>
    /// Represents the response from a secret validation check.
    /// </summary>
    public class ValidateSecretResponse
    {
        public bool Exists { get; set; }
        public SecretValidationStatus Status { get; set; }
        public string Message { get; set; }
    }
}

namespace CognitiveMesh.FoundationLayer.Security.Ports
{
    using CognitiveMesh.FoundationLayer.Security.Ports.Models;

    /// <summary>
    /// Defines the contract for the Secrets Management Port, a critical component of the
    /// Security & Zero-Trust Infrastructure Framework. This port provides a secure abstraction
    /// for all lifecycle operations on secrets, such as API keys, connection strings, and certificates.
    /// </summary>
    public interface ISecretsManagementPort
    {
        /// <summary>
        /// Securely retrieves the value of a secret from the vault.
        /// This operation must be audited, and access is governed by Zero-Trust policies.
        /// </summary>
        /// <param name="request">The request identifying the secret to retrieve.</param>
        /// <returns>A response containing the secret's value and metadata, or an error if access is denied or the secret does not exist.</returns>
        Task<GetSecretResponse> GetSecretAsync(SecretRequest request);

        /// <summary>
        /// Securely stores a secret in the vault. This can be used for both creating a new secret
        /// and creating a new version of an existing secret.
        /// </summary>
        /// <param name="request">The request containing the secret's name, value, and optional metadata.</param>
        /// <returns>A response indicating the success of the operation and the new version ID of the secret.</returns>
        Task<StoreSecretResponse> StoreSecretAsync(StoreSecretRequest request);

        /// <summary>
        /// Permanently deletes a secret from the vault. This is an irreversible operation
        /// and requires a high level of privilege.
        /// </summary>
        /// <param name="request">The request identifying the secret to delete.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        Task DeleteSecretAsync(SecretRequest request);

        /// <summary>
        /// Initiates the rotation of a secret. This process generates a new version of the secret
        /// with a new value, following configured rotation policies. The old version may be
        /// disabled or deprecated.
        /// </summary>
        /// <param name="request">The request identifying the secret to rotate.</param>
        /// <returns>A response indicating the success of the rotation and the new version ID.</returns>
        Task<RotateSecretResponse> RotateSecretAsync(SecretRequest request);

        /// <summary>
        /// Validates the status of a secret without retrieving its value. Checks for existence,
        /// expiration, and other policy-defined health metrics.
        /// </summary>
        /// <param name="request">The request identifying the secret to validate.</param>
        /// <returns>A response containing the validation status of the secret.</returns>
        Task<ValidateSecretResponse> ValidateSecretAsync(SecretRequest request);
    }
}
