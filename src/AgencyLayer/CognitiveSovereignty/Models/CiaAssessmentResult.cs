namespace AgencyLayer.CognitiveSovereignty.Models;

/// <summary>
/// Output of a CIA 2.0 computation, containing both the raw and adjusted scores
/// plus derived sovereignty signals for downstream routing.
/// </summary>
public class CiaAssessmentResult
{
    // ── Computed scores ─────────────────────────────────────────────────────

    /// <summary>
    /// Raw CIA 2.0 score: average of the four core metrics (TI + APS + MAR + ACR) / 4.
    /// Range: 0.0–1.0.
    /// </summary>
    public double RawCiaScore { get; set; }

    /// <summary>
    /// Adjusted CIA 2.0 score after applying contextual multipliers:
    /// CIA2.0 × RW-CIA × (1 – SFI) × (1 – STG).
    /// Range: 0.0–∞ (can exceed 1.0 for high-risk contexts with RW-CIA > 1.0).
    /// </summary>
    public double AdjustedCiaScore { get; set; }

    /// <summary>
    /// Collective Sovereignty Impact (CSI): leading indicator (0.0–1.0) of whether
    /// team-level AI use creates resilience (high) or knowledge silos (low).
    /// Derived from the adjusted CIA score — not a direct user input.
    /// </summary>
    public double CollectiveSovereigntyIndex { get; set; }

    // ── Derived sovereignty guidance ────────────────────────────────────────

    /// <summary>
    /// Recommended sovereignty mode based on the adjusted CIA score and task type.
    /// </summary>
    public SovereigntyMode RecommendedMode { get; set; }

    /// <summary>
    /// Human-readable rationale for the recommended sovereignty mode.
    /// </summary>
    public string Rationale { get; set; } = string.Empty;

    // ── Score breakdown for diagnostics ────────────────────────────────────

    /// <summary>
    /// Individual metric scores mirrored from the request, for traceability.
    /// </summary>
    public double TransparencyIndex { get; set; }
    public double AgencyPreservationScore { get; set; }
    public double MetacognitiveAwarenessRate { get; set; }
    public double AdaptiveControlRange { get; set; }
}
