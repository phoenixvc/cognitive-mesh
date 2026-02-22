namespace AgencyLayer.CognitiveSandwich.Models;

/// <summary>
/// Represents the result of a cognitive debt assessment for a specific phase,
/// measuring the degree of over-reliance on AI within the Cognitive Sandwich process.
/// </summary>
public class CognitiveDebtAssessment
{
    /// <summary>
    /// Identifier of the process being assessed.
    /// </summary>
    public string ProcessId { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the phase being assessed.
    /// </summary>
    public string PhaseId { get; set; } = string.Empty;

    /// <summary>
    /// Cognitive debt score on a scale of 0 (no debt) to 100 (maximum debt).
    /// Higher values indicate greater over-reliance on AI with insufficient human oversight.
    /// </summary>
    public double DebtScore { get; set; }

    /// <summary>
    /// Indicates whether the debt score exceeds the configured threshold for the process.
    /// </summary>
    public bool IsBreached { get; set; }

    /// <summary>
    /// Actionable recommendations for reducing cognitive debt.
    /// </summary>
    public IReadOnlyList<string> Recommendations { get; set; } = [];

    /// <summary>
    /// Timestamp when the assessment was performed.
    /// </summary>
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
}
