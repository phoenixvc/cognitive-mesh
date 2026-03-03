namespace CognitiveMesh.ReasoningLayer.NISTMaturity.Models;

/// <summary>
/// Represents the priority level of a maturity gap in the NIST AI RMF assessment.
/// Higher priority gaps require more urgent remediation.
/// </summary>
public enum GapPriority
{
    /// <summary>Critical gap requiring immediate attention (score of 1).</summary>
    Critical,

    /// <summary>High-priority gap requiring prompt remediation (score of 2).</summary>
    High,

    /// <summary>Medium-priority gap that should be addressed in the near term (score of 3).</summary>
    Medium,

    /// <summary>Low-priority gap for long-term improvement planning.</summary>
    Low
}
