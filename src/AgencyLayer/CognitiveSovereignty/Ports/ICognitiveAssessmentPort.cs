using AgencyLayer.CognitiveSovereignty.Models;

namespace AgencyLayer.CognitiveSovereignty.Ports;

/// <summary>
/// Port for computing CIA 2.0 (Cognitive Impact Assessment) and CSI
/// (Collective Sovereignty Index) scores based on the framework defined in
/// the Cognitive Sovereignty AI Ethics paper (v4).
///
/// Callers provide the four observable interface metrics; this port applies
/// the CIA 2.0 formula and returns the adjusted score plus a recommended
/// sovereignty mode for downstream routing.
/// </summary>
public interface ICognitiveAssessmentPort
{
    /// <summary>
    /// Computes a CIA 2.0 assessment given the observed interface metrics and
    /// contextual adjustments provided by the caller.
    /// </summary>
    /// <param name="request">The four core CIA metrics plus contextual adjustments.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Raw score, adjusted score, CSI leading indicator, and recommended sovereignty mode.
    /// </returns>
    Task<CiaAssessmentResult> AssessAsync(CiaAssessmentRequest request, CancellationToken ct = default);
}
