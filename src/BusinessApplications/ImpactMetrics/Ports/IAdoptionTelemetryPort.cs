using CognitiveMesh.BusinessApplications.ImpactMetrics.Models;

namespace CognitiveMesh.BusinessApplications.ImpactMetrics.Ports;

/// <summary>
/// Defines the contract for recording AI tool usage telemetry and detecting
/// resistance patterns in adoption behaviour.
/// </summary>
public interface IAdoptionTelemetryPort
{
    /// <summary>
    /// Records a single user interaction with an AI tool.
    /// </summary>
    /// <param name="telemetry">The telemetry event to record.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordActionAsync(
        AdoptionTelemetry telemetry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a summary of AI tool usage for a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant whose usage to summarise.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of telemetry events for the tenant.</returns>
    Task<IReadOnlyList<AdoptionTelemetry>> GetUsageSummaryAsync(
        string tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyses telemetry data to detect patterns of resistance to AI adoption.
    /// </summary>
    /// <param name="tenantId">The tenant to analyse for resistance patterns.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of detected resistance indicators.</returns>
    Task<IReadOnlyList<ResistanceIndicator>> DetectResistancePatternsAsync(
        string tenantId,
        CancellationToken cancellationToken = default);
}
