using CognitiveMesh.FoundationLayer.PolicyStore.Models;

namespace CognitiveMesh.AgencyLayer.SelfHealing.Ports;

/// <summary>
/// Defines the port for obtaining remediation decisions based on incident context.
/// </summary>
public interface IRemediationDecisionPort
{
    /// <summary>
    /// Retrieves the allowed remediation actions and their ranking weights for the
    /// specified incident category and severity.
    /// </summary>
    /// <param name="incidentCategory">The category of the incident.</param>
    /// <param name="severity">The severity level of the incident.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// A tuple containing the allowed <see cref="RemediationAction"/> flags and
    /// a dictionary of ranking weights keyed by action name.
    /// </returns>
    Task<(RemediationAction AllowedActions, Dictionary<string, double> RankingWeights)> GetAllowedActionsAsync(
        string incidentCategory,
        string severity,
        CancellationToken ct = default);
}
