namespace AgencyLayer.CognitiveSovereignty.Models;

/// <summary>
/// Defines the spectrum of sovereignty modes that determine the balance
/// between autonomous agentic workflows and human cognitive control.
/// </summary>
public enum SovereigntyMode
{
    /// <summary>
    /// The system operates with full autonomous agentic capability.
    /// Human input is limited to problem statement; agents handle execution.
    /// Autonomy level: 1.0.
    /// </summary>
    FullAutonomy,

    /// <summary>
    /// Agents operate autonomously with human oversight at critical decision points.
    /// Autonomy level: 0.75.
    /// </summary>
    GuidedAutonomy,

    /// <summary>
    /// Equal partnership between human and agent. Both contribute to decisions
    /// and outputs, with shared authorship tracked transparently.
    /// Autonomy level: 0.5.
    /// </summary>
    CoAuthorship,

    /// <summary>
    /// Human drives the workflow with agent suggestions and support.
    /// Agents provide recommendations but do not act autonomously.
    /// Autonomy level: 0.25.
    /// </summary>
    HumanLed,

    /// <summary>
    /// Full human control with no autonomous agent actions.
    /// Agents are disabled or provide only passive information display.
    /// Autonomy level: 0.0.
    /// </summary>
    FullManual
}
