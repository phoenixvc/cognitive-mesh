using AgencyLayer.AgentTeamFramework.Ports;
using AgencyLayer.RoadmapCrew.Models;

namespace AgencyLayer.RoadmapCrew.Ports;

/// <summary>
/// Port for the RoadmapCrew agent team — produces strategic roadmaps
/// by coordinating six specialized agents in a sequential pipeline:
/// VisionKeeper → MarketScanner → PriorityRanker → DependencyMapper → MilestoneTracker → RoadmapSynthesizer.
/// </summary>
public interface IRoadmapCrewPort : IAgentTeamPort
{
    /// <summary>
    /// Generates a complete strategic roadmap by running all six agents sequentially.
    /// </summary>
    /// <param name="request">The roadmap generation request with proposed items and vision context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A complete roadmap document with prioritized backlog, dependency graph, and risk register.</returns>
    Task<RoadmapDocument> GenerateRoadmapAsync(
        RoadmapCrewRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs only the VisionKeeper agent to assess strategic alignment of proposed items.
    /// Useful for quick filtering without the full pipeline.
    /// </summary>
    /// <param name="items">Work items to assess.</param>
    /// <param name="vision">Strategic vision context.</param>
    /// <param name="alignmentThreshold">Minimum alignment score (0-10). Default: 4.0.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Vision assessment with approved, rejected, and escalated items.</returns>
    Task<VisionAssessment> AssessAlignmentAsync(
        IReadOnlyList<WorkItem> items,
        VisionContext vision,
        double alignmentThreshold = 4.0,
        CancellationToken cancellationToken = default);
}
