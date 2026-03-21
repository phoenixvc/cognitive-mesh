namespace AgencyLayer.CognitiveSovereignty.Models;

/// <summary>
/// Input for a CIA 2.0 (Cognitive Impact Assessment) score computation.
/// All four core metrics are scored 0.0–1.0 by the caller, who observes the
/// AI system's interface and interaction characteristics directly.
/// </summary>
public class CiaAssessmentRequest
{
    // ── Core CIA 2.0 metrics ────────────────────────────────────────────────

    /// <summary>
    /// Transparency Index (TI): how clearly AI involvement is signalled to the user.
    /// 1.0 = every AI action is visible and attributed; 0.0 = fully opaque.
    /// </summary>
    public double TransparencyIndex { get; set; }

    /// <summary>
    /// Agency Preservation Score (APS): how easily the user can override, reject,
    /// or modify AI actions. 1.0 = one-click override anywhere; 0.0 = no override path.
    /// </summary>
    public double AgencyPreservationScore { get; set; }

    /// <summary>
    /// Metacognitive Awareness Rate (MAR): how often the system prompts users to
    /// reflect, question, or verify AI output. 1.0 = consistent prompts; 0.0 = never.
    /// </summary>
    public double MetacognitiveAwarenessRate { get; set; }

    /// <summary>
    /// Adaptive Control Range (ACR): depth of personalisation the user has over AI
    /// frequency, scope, and settings. 1.0 = full per-task control; 0.0 = fixed behaviour.
    /// </summary>
    public double AdaptiveControlRange { get; set; }

    // ── Contextual adjustments ──────────────────────────────────────────────

    /// <summary>
    /// Risk-Weighted Multiplier (RW-CIA): stakes of the task context.
    /// Defaults to 1.0 (standard). Use 1.5 for production code, 2.0 for security tasks.
    /// Must be ≥ 1.0.
    /// </summary>
    public double RiskWeightedMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Sovereignty Friction Index (SFI): accessibility of user controls (0.0–1.0).
    /// 1.0 = fully accessible (no friction); 0.0 = controls completely inaccessible.
    /// Applied directly as a multiplier: CIA × SFI.
    /// Example: controls requiring 5 clicks → SFI ≈ 0.8 (20% friction penalty).
    /// </summary>
    public double SovereigntyFrictionIndex { get; set; } = 1.0;

    /// <summary>
    /// Sovereignty Transparency Gap (STG): gap between what is shown and what users
    /// actually comprehend (0.0–1.0). 0.0 = full comprehension; 1.0 = no comprehension.
    /// Applied as (1 – STG).
    /// </summary>
    public double SovereigntyTransparencyGap { get; set; } = 0.0;

    /// <summary>
    /// Task type context, used as a leading indicator alongside computed scores.
    /// E.g. "CreativeWriting", "DataAnalysis", "CodeGeneration".
    /// </summary>
    public string? TaskType { get; set; }
}
