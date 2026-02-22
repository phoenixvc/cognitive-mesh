namespace CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;

/// <summary>
/// Represents the outcome classification of a learning event.
/// Used to categorize events for pattern analysis and mistake prevention.
/// </summary>
public enum LearningOutcome
{
    /// <summary>The action or decision led to a successful outcome.</summary>
    Success,

    /// <summary>The action or decision led to a failure or negative outcome.</summary>
    Failure,

    /// <summary>The outcome could not be clearly determined.</summary>
    Inconclusive,

    /// <summary>The event requires human review before classification.</summary>
    NeedsReview
}
