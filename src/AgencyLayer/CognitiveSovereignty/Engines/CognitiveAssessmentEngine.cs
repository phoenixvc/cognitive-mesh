using AgencyLayer.CognitiveSovereignty.Models;
using AgencyLayer.CognitiveSovereignty.Ports;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.CognitiveSovereignty.Engines;

/// <summary>
/// Implements the CIA 2.0 (Cognitive Impact Assessment) formula from the
/// Cognitive Sovereignty AI Ethics framework (v4).
///
/// Formula:
///   CIA2.0 = (TI + APS + MAR + ACR) / 4 × RW-CIA × SFI × (1 – STG)
///
/// Where SFI (1.0 = no friction, 0.0 = fully inaccessible controls) is a direct
/// multiplier, and STG (0.0 = full comprehension) reduces via (1 – STG).
///
/// CSI (Collective Sovereignty Index) is derived from the base CIA score as a
/// leading indicator of team-level resilience vs. knowledge silos.
/// </summary>
public class CognitiveAssessmentEngine : ICognitiveAssessmentPort
{
    private readonly ILogger<CognitiveAssessmentEngine> _logger;

    public CognitiveAssessmentEngine(ILogger<CognitiveAssessmentEngine> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<CiaAssessmentResult> AssessAsync(CiaAssessmentRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateMetrics(request);

        // Step 1 — raw CIA 2.0: average of the four core metrics
        var raw = (request.TransparencyIndex
                 + request.AgencyPreservationScore
                 + request.MetacognitiveAwarenessRate
                 + request.AdaptiveControlRange) / 4.0;

        // Step 2 — adjusted CIA 2.0 per the published formula:
        //   CIA2.0 = raw × RW-CIA × SFI × (1 – STG)
        var adjusted = raw
            * request.RiskWeightedMultiplier
            * request.SovereigntyFrictionIndex
            * (1.0 - request.SovereigntyTransparencyGap);

        // Step 3 — CSI leading indicator: normalise adjusted back to [0,1]
        // by dividing out RW-CIA (which can push adjusted above 1.0)
        var csi = Math.Clamp(adjusted / request.RiskWeightedMultiplier, 0.0, 1.0);

        // Step 4 — derive recommended sovereignty mode
        var (mode, rationale) = ResolveMode(adjusted, request.TaskType);

        var result = new CiaAssessmentResult
        {
            RawCiaScore = Math.Round(raw, 4),
            AdjustedCiaScore = Math.Round(adjusted, 4),
            CollectiveSovereigntyIndex = Math.Round(csi, 4),
            RecommendedMode = mode,
            Rationale = rationale,
            TransparencyIndex = request.TransparencyIndex,
            AgencyPreservationScore = request.AgencyPreservationScore,
            MetacognitiveAwarenessRate = request.MetacognitiveAwarenessRate,
            AdaptiveControlRange = request.AdaptiveControlRange,
        };

        _logger.LogDebug(
            "CIA assessment: raw={Raw:F4} adjusted={Adjusted:F4} csi={Csi:F4} mode={Mode} taskType={TaskType}",
            result.RawCiaScore, result.AdjustedCiaScore, result.CollectiveSovereigntyIndex,
            result.RecommendedMode, request.TaskType ?? "unspecified");

        return Task.FromResult(result);
    }

    /// <summary>
    /// Maps the adjusted CIA score + task type to a sovereignty mode and rationale.
    ///
    /// High adjusted CIA (≥ 0.7) → human oversight required.
    /// Medium (0.4–0.7) → co-authorship or guided autonomy.
    /// Low (&lt; 0.4) → full autonomy (for autonomy-friendly task types).
    /// Creative tasks always floor at HumanLed regardless of score.
    /// </summary>
    private static (SovereigntyMode mode, string rationale) ResolveMode(double adjusted, string? taskType)
    {
        var isCreative = taskType is "CreativeWriting" or "Design" or "Strategy";
        var isAutonomyFriendly = taskType is "DataAnalysis" or "CodeGeneration" or "Summarisation";

        if (isCreative)
        {
            return (SovereigntyMode.HumanLed,
                $"Creative task type '{taskType}' requires high sovereignty to preserve authorship and originality.");
        }

        return adjusted switch
        {
            >= 0.7 => (SovereigntyMode.HumanLed,
                $"High adjusted CIA ({adjusted:F2}) — significant cognitive impact, human-led mode required."),
            >= 0.55 => (SovereigntyMode.CoAuthorship,
                $"Moderate-high adjusted CIA ({adjusted:F2}) — co-authorship balances efficiency with oversight."),
            >= 0.4 => (SovereigntyMode.GuidedAutonomy,
                $"Moderate adjusted CIA ({adjusted:F2}) — guided autonomy with human checkpoints."),
            _ when isAutonomyFriendly => (SovereigntyMode.FullAutonomy,
                $"Low adjusted CIA ({adjusted:F2}) with autonomy-friendly task type '{taskType}'."),
            _ => (SovereigntyMode.GuidedAutonomy,
                $"Low adjusted CIA ({adjusted:F2}), task type unspecified — defaulting to guided autonomy.")
        };
    }

    private static void ValidateMetrics(CiaAssessmentRequest r)
    {
        AssertUnitRange(r.TransparencyIndex, nameof(r.TransparencyIndex));
        AssertUnitRange(r.AgencyPreservationScore, nameof(r.AgencyPreservationScore));
        AssertUnitRange(r.MetacognitiveAwarenessRate, nameof(r.MetacognitiveAwarenessRate));
        AssertUnitRange(r.AdaptiveControlRange, nameof(r.AdaptiveControlRange));
        AssertUnitRange(r.SovereigntyFrictionIndex, nameof(r.SovereigntyFrictionIndex));
        AssertUnitRange(r.SovereigntyTransparencyGap, nameof(r.SovereigntyTransparencyGap));

        if (r.RiskWeightedMultiplier < 1.0)
            throw new ArgumentOutOfRangeException(nameof(r.RiskWeightedMultiplier),
                "Risk-weighted multiplier must be ≥ 1.0.");
    }

    private static void AssertUnitRange(double value, string name)
    {
        if (value is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(name,
                $"{name} must be between 0.0 and 1.0 (was {value}).");
    }
}
