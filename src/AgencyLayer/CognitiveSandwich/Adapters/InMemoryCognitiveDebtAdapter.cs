using System.Collections.Concurrent;
using AgencyLayer.CognitiveSandwich.Models;
using AgencyLayer.CognitiveSandwich.Ports;
using Microsoft.Extensions.Logging;
using static CognitiveMesh.Shared.LogSanitizer;

namespace AgencyLayer.CognitiveSandwich.Adapters;

/// <summary>
/// In-memory implementation of <see cref="ICognitiveDebtPort"/> using a
/// <see cref="ConcurrentDictionary{TKey,TValue}"/> for development and testing scenarios.
/// Tracks cognitive debt scores per process and supports threshold breach checks.
/// </summary>
public class InMemoryCognitiveDebtAdapter : ICognitiveDebtPort
{
    private readonly ConcurrentDictionary<string, CognitiveDebtAssessment> _assessments = new();
    private readonly ILogger<InMemoryCognitiveDebtAdapter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryCognitiveDebtAdapter"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public InMemoryCognitiveDebtAdapter(ILogger<InMemoryCognitiveDebtAdapter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<CognitiveDebtAssessment> AssessDebtAsync(string processId, string phaseId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processId);
        ArgumentException.ThrowIfNullOrWhiteSpace(phaseId);

        var key = $"{processId}:{phaseId}";

        var assessment = _assessments.GetOrAdd(key, _ => new CognitiveDebtAssessment
        {
            ProcessId = processId,
            PhaseId = phaseId,
            DebtScore = 0.0,
            IsBreached = false,
            Recommendations = [],
            AssessedAt = DateTime.UtcNow
        });

        _logger.LogDebug(
            "Assessed cognitive debt for process {ProcessId}, phase {PhaseId}: score={DebtScore}",
            Sanitize(processId), Sanitize(phaseId), assessment.DebtScore);

        return Task.FromResult(assessment);
    }

    /// <inheritdoc />
    public Task<bool> IsThresholdBreachedAsync(string processId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processId);

        // In the default in-memory adapter, threshold is never breached
        // unless an assessment was explicitly stored with IsBreached = true
        var breached = _assessments.Values
            .Any(a => a.ProcessId == processId && a.IsBreached);

        _logger.LogDebug(
            "Threshold breach check for process {ProcessId}: breached={Breached}",
            Sanitize(processId), breached);

        return Task.FromResult(breached);
    }
}
