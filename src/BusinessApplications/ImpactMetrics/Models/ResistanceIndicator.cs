namespace CognitiveMesh.BusinessApplications.ImpactMetrics.Models;

/// <summary>
/// Enumerates the types of resistance patterns that can be detected.
/// </summary>
public enum ResistanceType
{
    /// <summary>Users are avoiding using AI tools entirely.</summary>
    Avoidance,

    /// <summary>Users are frequently overriding AI recommendations.</summary>
    Override,

    /// <summary>Users are providing negative feedback about AI tools.</summary>
    NegativeFeedback,

    /// <summary>A spike in help requests indicates users are struggling.</summary>
    HelpSpike,

    /// <summary>Users are disengaging from AI-enabled workflows.</summary>
    Disengagement
}

/// <summary>
/// Represents a detected pattern of resistance to AI adoption.
/// </summary>
/// <param name="IndicatorType">The category of resistance detected.</param>
/// <param name="Severity">Severity of the resistance indicator on a 0-1 scale.</param>
/// <param name="AffectedUserCount">The number of users exhibiting this resistance pattern.</param>
/// <param name="FirstDetectedAt">When this resistance pattern was first observed.</param>
/// <param name="Description">A human-readable description of the resistance pattern.</param>
public record ResistanceIndicator(
    ResistanceType IndicatorType,
    double Severity,
    int AffectedUserCount,
    DateTimeOffset FirstDetectedAt,
    string Description);
