using CognitiveMesh.FoundationLayer.PolicyStore.Models;

namespace CognitiveMesh.FoundationLayer.PolicyStore.Ports;

/// <summary>
/// Defines the port for retrieving and managing remediation policies.
/// </summary>
public interface IRemediationPolicyPort
{
    /// <summary>
    /// Retrieves the active remediation policy for the specified incident category and severity.
    /// </summary>
    /// <param name="incidentCategory">The incident category to look up.</param>
    /// <param name="severity">The severity level to look up.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The matching <see cref="RemediationPolicy"/>.</returns>
    Task<RemediationPolicy> GetPolicyAsync(string incidentCategory, string severity, CancellationToken ct = default);

    /// <summary>
    /// Lists all active remediation policies.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A collection of active <see cref="RemediationPolicy"/> instances.</returns>
    Task<IEnumerable<RemediationPolicy>> ListPoliciesAsync(CancellationToken ct = default);

    /// <summary>
    /// Creates or updates a remediation policy. If a policy with the same identifier already exists,
    /// it is deactivated and a new version is created.
    /// </summary>
    /// <param name="policy">The policy to upsert.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The persisted <see cref="RemediationPolicy"/>.</returns>
    Task<RemediationPolicy> UpsertPolicyAsync(RemediationPolicy policy, CancellationToken ct = default);

    /// <summary>
    /// Soft-deletes a remediation policy by marking all versions as inactive.
    /// </summary>
    /// <param name="id">The unique identifier of the policy to delete.</param>
    /// <param name="ct">A cancellation token.</param>
    Task DeletePolicyAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the full version history of a remediation policy, ordered by descending version number.
    /// </summary>
    /// <param name="id">The unique identifier of the policy.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A collection of <see cref="RemediationPolicy"/> versions.</returns>
    Task<IEnumerable<RemediationPolicy>> GetPolicyHistoryAsync(Guid id, CancellationToken ct = default);
}
