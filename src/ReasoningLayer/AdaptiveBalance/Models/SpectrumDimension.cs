namespace CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;

/// <summary>
/// Represents the dimensions of the adaptive balance spectrum.
/// Each dimension captures a different aspect of system behavior that can be tuned.
/// </summary>
public enum SpectrumDimension
{
    /// <summary>Revenue and profit optimization preference.</summary>
    Profit,

    /// <summary>Risk tolerance and aversion level.</summary>
    Risk,

    /// <summary>Tendency toward agreement and consensus.</summary>
    Agreeableness,

    /// <summary>Strength of identity and value grounding.</summary>
    IdentityGrounding,

    /// <summary>Rate at which the system adapts to new information.</summary>
    LearningRate
}
