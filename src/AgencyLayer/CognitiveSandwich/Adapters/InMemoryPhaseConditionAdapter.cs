using AgencyLayer.CognitiveSandwich.Models;
using AgencyLayer.CognitiveSandwich.Ports;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.CognitiveSandwich.Adapters;

/// <summary>
/// In-memory implementation of <see cref="IPhaseConditionPort"/> for development
/// and testing scenarios. Returns default passing condition check results,
/// indicating all pre- and postconditions are met.
/// </summary>
public class InMemoryPhaseConditionAdapter : IPhaseConditionPort
{
    private readonly ILogger<InMemoryPhaseConditionAdapter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryPhaseConditionAdapter"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public InMemoryPhaseConditionAdapter(ILogger<InMemoryPhaseConditionAdapter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<ConditionCheckResult> CheckPreconditionsAsync(string processId, string phaseId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processId);
        ArgumentException.ThrowIfNullOrWhiteSpace(phaseId);

        _logger.LogDebug(
            "Checking preconditions for process {ProcessId}, phase {PhaseId} — returning all met (in-memory default)",
            processId, phaseId);

        var result = new ConditionCheckResult
        {
            AllMet = true,
            Results = []
        };

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<ConditionCheckResult> CheckPostconditionsAsync(string processId, string phaseId, PhaseOutput output, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processId);
        ArgumentException.ThrowIfNullOrWhiteSpace(phaseId);
        ArgumentNullException.ThrowIfNull(output);

        _logger.LogDebug(
            "Checking postconditions for process {ProcessId}, phase {PhaseId} — returning all met (in-memory default)",
            processId, phaseId);

        var result = new ConditionCheckResult
        {
            AllMet = true,
            Results = []
        };

        return Task.FromResult(result);
    }
}
