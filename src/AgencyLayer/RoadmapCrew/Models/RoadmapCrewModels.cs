using System.Text.Json.Serialization;

namespace AgencyLayer.RoadmapCrew.Models;

// ── Enums ──────────────────────────────────────────────────────────────

/// <summary>
/// Confidence level for market signals and estimates.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConfidenceLevel
{
    /// <summary>High confidence — strong evidence.</summary>
    High,
    /// <summary>Medium confidence — some supporting evidence.</summary>
    Medium,
    /// <summary>Low confidence — limited or anecdotal evidence.</summary>
    Low
}

/// <summary>
/// Type of dependency between roadmap items.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DependencyType
{
    /// <summary>This item blocks another.</summary>
    Blocks,
    /// <summary>This item is blocked by another.</summary>
    BlockedBy,
    /// <summary>This item enhances another.</summary>
    Enhances,
    /// <summary>This item requires another.</summary>
    Requires
}

/// <summary>
/// Status of a milestone.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MilestoneStatus
{
    /// <summary>Milestone is on track.</summary>
    OnTrack,
    /// <summary>Milestone is at risk.</summary>
    AtRisk,
    /// <summary>Milestone is blocked.</summary>
    Blocked,
    /// <summary>Milestone is complete.</summary>
    Complete
}

/// <summary>
/// Risk level classification.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RiskLevel
{
    /// <summary>Critical risk — immediate action required.</summary>
    Critical,
    /// <summary>High risk — needs mitigation plan.</summary>
    High,
    /// <summary>Medium risk — monitor closely.</summary>
    Medium,
    /// <summary>Low risk — acceptable.</summary>
    Low
}

// ── Input Models ───────────────────────────────────────────────────────

/// <summary>
/// A proposed work item to be evaluated by RoadmapCrew.
/// </summary>
public class WorkItem
{
    /// <summary>Unique identifier for the work item.</summary>
    public required string Id { get; init; }

    /// <summary>Work item title.</summary>
    public required string Title { get; init; }

    /// <summary>Detailed description of the work item.</summary>
    public required string Description { get; init; }

    /// <summary>Category or theme (e.g., "infrastructure", "feature", "tech-debt").</summary>
    public string? Category { get; init; }

    /// <summary>Estimated effort in story points or person-days.</summary>
    public double? EstimatedEffort { get; init; }

    /// <summary>Proposed owner or team.</summary>
    public string? Owner { get; init; }
}

/// <summary>
/// Strategic vision context for alignment scoring.
/// </summary>
public class VisionContext
{
    /// <summary>The current vision statement.</summary>
    public required string VisionStatement { get; init; }

    /// <summary>Current OKRs (Objectives and Key Results).</summary>
    public IReadOnlyList<string> OKRs { get; init; } = [];

    /// <summary>Strategic themes guiding the roadmap.</summary>
    public IReadOnlyList<string> StrategicThemes { get; init; } = [];
}

/// <summary>
/// Request to generate a strategic roadmap.
/// </summary>
public class RoadmapCrewRequest
{
    /// <summary>Project or portfolio identifier.</summary>
    public required string ProjectId { get; init; }

    /// <summary>Proposed work items to evaluate and prioritize.</summary>
    public required IReadOnlyList<WorkItem> ProposedItems { get; init; }

    /// <summary>Strategic vision context for alignment scoring.</summary>
    public required VisionContext Vision { get; init; }

    /// <summary>Industry domain context for market scanning.</summary>
    public string? IndustryContext { get; init; }

    /// <summary>Known competitors for market analysis.</summary>
    public IReadOnlyList<string> Competitors { get; init; } = [];

    /// <summary>Team capacity data for feasibility assessment.</summary>
    public string? TeamCapacity { get; init; }

    /// <summary>Historical velocity metrics for calibration.</summary>
    public string? HistoricalVelocity { get; init; }

    /// <summary>Minimum alignment score threshold (0-10). Items below this are rejected.</summary>
    public double AlignmentThreshold { get; init; } = 4.0;
}

// ── Intermediate Models (per-agent outputs) ────────────────────────────

/// <summary>
/// A work item annotated with strategic alignment scoring by VisionKeeper.
/// </summary>
public class AnnotatedWorkItem
{
    /// <summary>The original work item.</summary>
    public required WorkItem Item { get; init; }

    /// <summary>Strategic alignment score (0-10).</summary>
    public double AlignmentScore { get; init; }

    /// <summary>Justification for the alignment score.</summary>
    public required string AlignmentRationale { get; init; }
}

/// <summary>
/// Output from VisionKeeper — filtered and scored work items.
/// </summary>
public class VisionAssessment
{
    /// <summary>Items that passed alignment threshold with scores.</summary>
    public IReadOnlyList<AnnotatedWorkItem> ApprovedItems { get; init; } = [];

    /// <summary>Items rejected for insufficient strategic alignment.</summary>
    public IReadOnlyList<AnnotatedWorkItem> RejectedItems { get; init; } = [];

    /// <summary>Items escalated for human review due to ambiguity.</summary>
    public IReadOnlyList<AnnotatedWorkItem> EscalatedItems { get; init; } = [];
}

/// <summary>
/// A market signal identified by MarketScanner.
/// </summary>
public class MarketSignal
{
    /// <summary>Description of the market signal.</summary>
    public required string Description { get; init; }

    /// <summary>Specific evidence or source.</summary>
    public string? Evidence { get; init; }

    /// <summary>Confidence level of the signal.</summary>
    public ConfidenceLevel Confidence { get; init; }
}

/// <summary>
/// A recommendation to adjust a work item's priority based on market factors.
/// </summary>
public class MarketAdjustment
{
    /// <summary>The work item ID this adjustment applies to.</summary>
    public required string WorkItemId { get; init; }

    /// <summary>Recommended action (accelerate or deprioritize).</summary>
    public required string Action { get; init; }

    /// <summary>Rationale for the adjustment.</summary>
    public required string Rationale { get; init; }

    /// <summary>Confidence level of the recommendation.</summary>
    public ConfidenceLevel Confidence { get; init; }
}

/// <summary>
/// Output from MarketScanner — market context and priority adjustments.
/// </summary>
public class MarketContextBrief
{
    /// <summary>Key market trends identified.</summary>
    public IReadOnlyList<MarketSignal> Trends { get; init; } = [];

    /// <summary>Items recommended for acceleration.</summary>
    public IReadOnlyList<MarketAdjustment> AccelerateItems { get; init; } = [];

    /// <summary>Items recommended for deprioritization.</summary>
    public IReadOnlyList<MarketAdjustment> DeprioritizeItems { get; init; } = [];

    /// <summary>New opportunities not currently on the roadmap.</summary>
    public IReadOnlyList<MarketSignal> NewOpportunities { get; init; } = [];
}

/// <summary>
/// A work item with RICE and WSJF priority scores.
/// </summary>
public class ScoredWorkItem
{
    /// <summary>The work item ID.</summary>
    public required string WorkItemId { get; init; }

    /// <summary>Work item title.</summary>
    public required string Title { get; init; }

    /// <summary>RICE score (Reach * Impact * Confidence / Effort).</summary>
    public double RiceScore { get; init; }

    /// <summary>WSJF score (Cost of Delay / Job Size).</summary>
    public double WsjfScore { get; init; }

    /// <summary>Detailed scoring rationale showing the math.</summary>
    public required string ScoringRationale { get; init; }

    /// <summary>Final rank position.</summary>
    public int Rank { get; init; }
}

/// <summary>
/// A resolved priority conflict between competing items.
/// </summary>
public class ConflictResolution
{
    /// <summary>IDs of items that were in conflict.</summary>
    public IReadOnlyList<string> ConflictingItemIds { get; init; } = [];

    /// <summary>How the conflict was resolved.</summary>
    public required string Resolution { get; init; }

    /// <summary>Tie-breaking rule applied.</summary>
    public required string TieBreaker { get; init; }
}

/// <summary>
/// Output from PriorityRanker — scored and ranked work items.
/// </summary>
public class PriorityRanking
{
    /// <summary>Work items ranked by priority with dual scores.</summary>
    public IReadOnlyList<ScoredWorkItem> RankedItems { get; init; } = [];

    /// <summary>Conflict resolutions applied during ranking.</summary>
    public IReadOnlyList<ConflictResolution> ConflictResolutions { get; init; } = [];
}

/// <summary>
/// An edge in the dependency graph.
/// </summary>
public class DependencyEdge
{
    /// <summary>Source work item ID.</summary>
    public required string FromItemId { get; init; }

    /// <summary>Target work item ID.</summary>
    public required string ToItemId { get; init; }

    /// <summary>Type of dependency.</summary>
    public DependencyType Type { get; init; }

    /// <summary>Whether this is a hard (must) or soft (benefits from) dependency.</summary>
    public bool IsHard { get; init; } = true;
}

/// <summary>
/// A risk flag identified during dependency analysis.
/// </summary>
public class DependencyRisk
{
    /// <summary>Work item ID(s) involved.</summary>
    public IReadOnlyList<string> AffectedItemIds { get; init; } = [];

    /// <summary>Description of the risk.</summary>
    public required string Description { get; init; }

    /// <summary>Risk level.</summary>
    public RiskLevel Level { get; init; }
}

/// <summary>
/// Output from DependencyMapper — dependency graph with critical path.
/// </summary>
public class DependencyAnalysis
{
    /// <summary>All dependency edges between work items.</summary>
    public IReadOnlyList<DependencyEdge> Edges { get; init; } = [];

    /// <summary>Work item IDs on the critical path (longest dependency chain).</summary>
    public IReadOnlyList<string> CriticalPath { get; init; } = [];

    /// <summary>Risk flags from the dependency analysis.</summary>
    public IReadOnlyList<DependencyRisk> RiskFlags { get; init; } = [];

    /// <summary>Items that could be parallelized to reduce critical path length.</summary>
    public IReadOnlyList<string> ParallelizableItems { get; init; } = [];
}

/// <summary>
/// Assessment of a single milestone's feasibility.
/// </summary>
public class MilestoneAssessment
{
    /// <summary>Work item ID for this milestone.</summary>
    public required string WorkItemId { get; init; }

    /// <summary>Current status assessment.</summary>
    public MilestoneStatus Status { get; init; }

    /// <summary>Risk level.</summary>
    public RiskLevel RiskLevel { get; init; }

    /// <summary>Rationale for the assessment.</summary>
    public required string Rationale { get; init; }

    /// <summary>Proposed scope adjustments if timeline is infeasible.</summary>
    public IReadOnlyList<string> ScopeAdjustments { get; init; } = [];
}

/// <summary>
/// Output from MilestoneTracker — feasibility assessment with calibrated timeline.
/// </summary>
public class FeasibilityAssessment
{
    /// <summary>Per-milestone feasibility assessments.</summary>
    public IReadOnlyList<MilestoneAssessment> Assessments { get; init; } = [];

    /// <summary>Calibrated timeline adjustments based on historical velocity.</summary>
    public string? CalibratedTimeline { get; init; }

    /// <summary>Planning fallacy correction factor applied.</summary>
    public double CorrectionFactor { get; init; } = 1.0;

    /// <summary>Overall health metric: completed / planned ratio.</summary>
    public double? HealthMetric { get; init; }
}

// ── Output Model ───────────────────────────────────────────────────────

/// <summary>
/// A quarterly plan within the roadmap.
/// </summary>
public class QuarterlyPlan
{
    /// <summary>Quarter identifier (e.g., "2026-Q2").</summary>
    public required string Quarter { get; init; }

    /// <summary>Work item IDs scheduled for this quarter.</summary>
    public IReadOnlyList<string> ScheduledItems { get; init; } = [];

    /// <summary>Key deliverables for the quarter.</summary>
    public IReadOnlyList<string> Deliverables { get; init; } = [];

    /// <summary>Capacity allocation notes.</summary>
    public string? CapacityNotes { get; init; }
}

/// <summary>
/// A risk entry in the roadmap risk register.
/// </summary>
public class RiskEntry
{
    /// <summary>Risk description.</summary>
    public required string Description { get; init; }

    /// <summary>Risk level.</summary>
    public RiskLevel Level { get; init; }

    /// <summary>Proposed mitigation.</summary>
    public string? Mitigation { get; init; }

    /// <summary>Affected work item IDs.</summary>
    public IReadOnlyList<string> AffectedItemIds { get; init; } = [];
}

/// <summary>
/// The final roadmap document produced by RoadmapSynthesizer.
/// </summary>
public class RoadmapDocument
{
    /// <summary>Executive summary suitable for leadership review.</summary>
    public required string ExecutiveSummary { get; init; }

    /// <summary>Quarterly plans with deliverables and capacity.</summary>
    public IReadOnlyList<QuarterlyPlan> QuarterlyPlans { get; init; } = [];

    /// <summary>Prioritized backlog with scores.</summary>
    public required PriorityRanking PrioritizedBacklog { get; init; }

    /// <summary>Dependency graph with critical path.</summary>
    public required DependencyAnalysis DependencyGraph { get; init; }

    /// <summary>Risk register.</summary>
    public IReadOnlyList<RiskEntry> RiskRegister { get; init; } = [];

    /// <summary>Items requiring human judgment before proceeding.</summary>
    public IReadOnlyList<string> DecisionsNeeded { get; init; } = [];

    /// <summary>Capacity plan and allocation notes.</summary>
    public string? CapacityPlan { get; init; }

    /// <summary>Vision assessment from VisionKeeper.</summary>
    public required VisionAssessment VisionAssessment { get; init; }

    /// <summary>Market context from MarketScanner.</summary>
    public required MarketContextBrief MarketContext { get; init; }

    /// <summary>Feasibility assessment from MilestoneTracker.</summary>
    public required FeasibilityAssessment FeasibilityAssessment { get; init; }
}
