using AgencyLayer.Agents.Ports;
using AgencyLayer.AgentTeamFramework.Configuration;
using AgencyLayer.AgentTeamFramework.Engines;
using AgencyLayer.AgentTeamFramework.Pipeline;
using AgencyLayer.RoadmapCrew.Models;
using AgencyLayer.RoadmapCrew.Ports;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.RoadmapCrew.Engines;

/// <summary>
/// Orchestrates the RoadmapCrew six-agent sequential pipeline to produce strategic roadmaps.
/// Each agent receives the accumulated context from prior agents and produces structured output
/// that feeds into the next stage.
/// </summary>
public class RoadmapCrewEngine : AgentTeamEngineBase, IRoadmapCrewPort
{
    // ── Agent IDs ───────────────────────────────────────────────────────

    private const string VisionKeeperId = "roadmapcrew-vision-keeper";
    private const string MarketScannerId = "roadmapcrew-market-scanner";
    private const string PriorityRankerId = "roadmapcrew-priority-ranker";
    private const string DependencyMapperId = "roadmapcrew-dependency-mapper";
    private const string MilestoneTrackerId = "roadmapcrew-milestone-tracker";
    private const string RoadmapSynthesizerId = "roadmapcrew-roadmap-synthesizer";

    // ── System Prompts ──────────────────────────────────────────────────

    private const string VisionKeeperPrompt = """
        You are VisionKeeper, the strategic alignment guardian for RoadmapCrew.

        Your responsibilities:
        - Maintain and reference the current vision statement, OKRs, and strategic themes
        - Evaluate every proposed work item against strategic alignment criteria
        - Assign an alignment score (0-10) to each item with explicit justification
        - Reject items that score below the threshold with clear reasoning
        - Escalate items that are strategically ambiguous for human review

        Rules:
        - Never approve an item without articulating which strategic goal it serves
        - Flag items that serve short-term needs but conflict with long-term direction
        - Distinguish between "important" and "strategically aligned" — they are not the same
        - When uncertain, err on the side of escalation rather than rejection

        Output format: JSON matching the VisionAssessment schema with approvedItems, rejectedItems, and escalatedItems.
        """;

    private const string MarketScannerPrompt = """
        You are MarketScanner, the external intelligence agent for RoadmapCrew.

        Your responsibilities:
        - Scan technology trends, competitor movements, and market shifts relevant to our domain
        - Produce a market context brief summarizing factors that should influence roadmap priorities
        - Flag items that should be accelerated due to market opportunity or competitive pressure
        - Flag items that should be deprioritized due to market changes making them less relevant
        - Identify emerging opportunities not yet on the roadmap

        Rules:
        - Distinguish between hype and genuine market shifts — apply a maturity filter
        - Cite specific signals (competitor launches, technology adoption curves, regulatory changes)
        - Do not recommend chasing every trend — recommend only when strategic alignment exists
        - Provide a confidence level (high/medium/low) for each market signal

        Output format: JSON matching the MarketContextBrief schema with trends, accelerateItems, deprioritizeItems, and newOpportunities.
        """;

    private const string PriorityRankerPrompt = """
        You are PriorityRanker, the prioritization engine for RoadmapCrew.

        Your responsibilities:
        - Score every validated work item using RICE (Reach, Impact, Confidence, Effort) and WSJF (Weighted Shortest Job First)
        - Produce a ranked priority list with scores and scoring rationale
        - Resolve priority conflicts when multiple items compete for the same resources or time slot
        - Apply tie-breaking rules: strategic alignment > dependency risk > market urgency > effort (smallest first)

        Rules:
        - Always show your scoring math — no opaque rankings
        - When estimating Reach and Impact, state your assumptions explicitly
        - Confidence scores must reflect actual certainty, not optimism
        - Flag items where Effort estimates have high variance — these need decomposition before commitment

        Output format: JSON matching the PriorityRanking schema with rankedItems and conflictResolutions.
        """;

    private const string DependencyMapperPrompt = """
        You are DependencyMapper, the dependency analysis agent for RoadmapCrew.

        Your responsibilities:
        - Map dependencies between all roadmap items (blocks, blocked-by, enhances, requires)
        - Identify the critical path — the longest chain of dependent items that determines minimum timeline
        - Flag circular dependencies, high-fan-in items (bottlenecks), and items with unresolved external dependencies
        - Produce a dependency graph with risk annotations

        Rules:
        - Distinguish between hard dependencies (must complete before) and soft dependencies (benefits from)
        - Flag any item with more than 3 hard dependencies as high-risk
        - Identify items that could be parallelized to reduce critical path length
        - External dependencies (third-party APIs, vendor deliverables) must be flagged with explicit risk level

        Output format: JSON matching the DependencyAnalysis schema with edges, criticalPath, riskFlags, and parallelizableItems.
        """;

    private const string MilestoneTrackerPrompt = """
        You are MilestoneTracker, the feasibility and progress tracking agent for RoadmapCrew.

        Your responsibilities:
        - Validate proposed timelines against team capacity, historical velocity, and known constraints
        - Flag milestones that are at risk based on dependency analysis and current progress
        - Calibrate future estimates using historical data (actual vs. estimated for past items)
        - Propose scope adjustments when timelines are infeasible

        Rules:
        - Apply a planning fallacy correction factor based on historical overrun data
        - Never mark a milestone as "on track" without evidence — default to "at risk" when data is insufficient
        - Propose scope reduction before timeline extension when both are options
        - Track the ratio of completed items to planned items per quarter as a health metric

        Output format: JSON matching the FeasibilityAssessment schema with assessments, calibratedTimeline, correctionFactor, and healthMetric.
        """;

    private const string RoadmapSynthesizerPrompt = """
        You are RoadmapSynthesizer, the final output agent for RoadmapCrew.

        Your responsibilities:
        - Consolidate outputs from all RoadmapCrew agents into a single, coherent roadmap document
        - Produce an executive summary suitable for leadership review
        - Structure the roadmap by quarter with clear deliverables, owners, and success criteria
        - Include the dependency graph, risk register, and capacity allocation

        Rules:
        - The executive summary must fit on one page and be understandable without the detail sections
        - Every roadmap item must have: description, priority score, dependencies, milestone, owner, and success criteria
        - Flag any item that lacks sufficient definition to be actionable
        - Include a "decisions needed" section for items requiring human judgment before they can proceed

        Output format: JSON matching the RoadmapDocument schema with executiveSummary, quarterlyPlans, riskRegister, decisionsNeeded, and capacityPlan.
        """;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoadmapCrewEngine"/> class.
    /// </summary>
    /// <param name="agentPort">The specialized agent port for agent registration and task execution.</param>
    /// <param name="pipeline">The pipeline executor for standardized step execution.</param>
    /// <param name="logger">Logger instance.</param>
    public RoadmapCrewEngine(
        ISpecializedAgentPort agentPort,
        AgentPipelineExecutor pipeline,
        ILogger<RoadmapCrewEngine> logger)
        : base(agentPort, pipeline, logger)
    {
    }

    /// <inheritdoc />
    public override string TeamId => "roadmapcrew";

    /// <inheritdoc />
    protected override IReadOnlyList<AgentDefinitionRecord> DefineAgents() =>
    [
        new AgentDefinitionRecord
        {
            AgentId = VisionKeeperId,
            Name = "VisionKeeper",
            Type = SpecializedAgentType.Planner,
            SystemPrompt = VisionKeeperPrompt,
            Temperature = 0.3,
            Domains = ["strategy", "okr", "vision-alignment"],
            Goals = ["Filter work items by strategic alignment", "Prevent strategic drift"]
        },
        new AgentDefinitionRecord
        {
            AgentId = MarketScannerId,
            Name = "MarketScanner",
            Type = SpecializedAgentType.Analyst,
            SystemPrompt = MarketScannerPrompt,
            Temperature = 0.5,
            Domains = ["market-intelligence", "competitive-analysis", "technology-trends"],
            Goals = ["Identify market signals affecting roadmap", "Recommend priority adjustments"]
        },
        new AgentDefinitionRecord
        {
            AgentId = PriorityRankerId,
            Name = "PriorityRanker",
            Type = SpecializedAgentType.Planner,
            SystemPrompt = PriorityRankerPrompt,
            Temperature = 0.2,
            Domains = ["prioritization", "rice-scoring", "wsjf"],
            Goals = ["Produce defensible priority rankings", "Resolve priority conflicts transparently"]
        },
        new AgentDefinitionRecord
        {
            AgentId = DependencyMapperId,
            Name = "DependencyMapper",
            Type = SpecializedAgentType.Analyst,
            SystemPrompt = DependencyMapperPrompt,
            Temperature = 0.2,
            Domains = ["dependency-analysis", "critical-path", "graph-theory"],
            Goals = ["Map all dependencies accurately", "Identify critical path and bottlenecks"]
        },
        new AgentDefinitionRecord
        {
            AgentId = MilestoneTrackerId,
            Name = "MilestoneTracker",
            Type = SpecializedAgentType.Analyst,
            SystemPrompt = MilestoneTrackerPrompt,
            Temperature = 0.3,
            Domains = ["project-management", "velocity-tracking", "feasibility"],
            Goals = ["Validate timeline feasibility", "Calibrate estimates using historical data"]
        },
        new AgentDefinitionRecord
        {
            AgentId = RoadmapSynthesizerId,
            Name = "RoadmapSynthesizer",
            Type = SpecializedAgentType.Writer,
            SystemPrompt = RoadmapSynthesizerPrompt,
            Temperature = 0.4,
            Domains = ["technical-writing", "roadmap-synthesis", "executive-communication"],
            Goals = ["Produce actionable roadmap document", "Ensure completeness and coherence"]
        }
    ];

    /// <inheritdoc />
    public async Task<RoadmapDocument> GenerateRoadmapAsync(
        RoadmapCrewRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        Logger.LogInformation(
            "Starting RoadmapCrew pipeline for project {ProjectId} with {ItemCount} proposed items",
            request.ProjectId,
            request.ProposedItems.Count);

        // Step 1: VisionKeeper — strategic alignment filtering
        var visionAssessment = await Pipeline.ExecuteStepAsync<VisionAssessment>(
            VisionKeeperId,
            "Evaluate each proposed work item against the strategic vision, OKRs, and themes. " +
            $"Assign alignment scores (0-10) and reject items below {request.AlignmentThreshold}. " +
            "Return a VisionAssessment JSON with approvedItems, rejectedItems, and escalatedItems.",
            new { request.Vision, request.ProposedItems, request.AlignmentThreshold },
            cancellationToken).ConfigureAwait(false);

        if (visionAssessment.ApprovedItems.Count == 0)
        {
            Logger.LogWarning("VisionKeeper rejected all items — producing empty roadmap");
            return CreateEmptyRoadmap(visionAssessment);
        }

        // Step 2: MarketScanner — external intelligence
        var marketBrief = await Pipeline.ExecuteStepAsync<MarketContextBrief>(
            MarketScannerId,
            "Scan market trends, competitor movements, and technology shifts. " +
            "Produce a MarketContextBrief JSON with trends, accelerateItems, deprioritizeItems, and newOpportunities.",
            new { ApprovedItems = visionAssessment.ApprovedItems, request.IndustryContext, request.Competitors },
            cancellationToken).ConfigureAwait(false);

        // Step 3: PriorityRanker — RICE/WSJF scoring
        var priorityRanking = await Pipeline.ExecuteStepAsync<PriorityRanking>(
            PriorityRankerId,
            "Score all validated items using RICE and WSJF frameworks. " +
            "Resolve priority conflicts using tie-breaking rules: strategic alignment > dependency risk > market urgency > effort. " +
            "Return a PriorityRanking JSON with rankedItems and conflictResolutions.",
            new { visionAssessment.ApprovedItems, MarketContext = marketBrief },
            cancellationToken).ConfigureAwait(false);

        // Step 4: DependencyMapper — dependency graph and critical path
        var dependencyAnalysis = await Pipeline.ExecuteStepAsync<DependencyAnalysis>(
            DependencyMapperId,
            "Map dependencies between all roadmap items (blocks, blocked-by, enhances, requires). " +
            "Identify the critical path and flag circular dependencies, bottlenecks, and external dependencies. " +
            "Return a DependencyAnalysis JSON with edges, criticalPath, riskFlags, and parallelizableItems.",
            new { priorityRanking.RankedItems },
            cancellationToken).ConfigureAwait(false);

        // Step 5: MilestoneTracker — feasibility assessment
        var feasibilityAssessment = await Pipeline.ExecuteStepAsync<FeasibilityAssessment>(
            MilestoneTrackerId,
            "Validate proposed timelines against capacity and historical velocity. " +
            "Apply planning fallacy correction. Flag at-risk milestones. " +
            "Return a FeasibilityAssessment JSON with assessments, calibratedTimeline, correctionFactor, and healthMetric.",
            new { priorityRanking.RankedItems, DependencyAnalysis = dependencyAnalysis, request.TeamCapacity, request.HistoricalVelocity },
            cancellationToken).ConfigureAwait(false);

        // Step 6: RoadmapSynthesizer — final document (custom merge: prose from LLM, structured data from pipeline)
        var synthesizerResult = await Pipeline.ExecuteRawAsync(
            RoadmapSynthesizerId,
            "Consolidate all agent outputs into a final roadmap document. " +
            "Include: executive summary (one page), quarterly plans, prioritized backlog, dependency graph, " +
            "risk register, decisions needed, and capacity plan. " +
            "Return a RoadmapDocument JSON.",
            new
            {
                VisionAssessment = visionAssessment,
                MarketContext = marketBrief,
                PriorityRanking = priorityRanking,
                DependencyAnalysis = dependencyAnalysis,
                FeasibilityAssessment = feasibilityAssessment
            },
            cancellationToken).ConfigureAwait(false);

        var partial = Pipeline.DeserializeOrDefault<SynthesizerPartialOutput>(
            synthesizerResult.Output, "SynthesizerOutput");

        var roadmapDocument = new RoadmapDocument
        {
            ExecutiveSummary = partial.ExecutiveSummary ?? "Roadmap generated from pipeline data.",
            QuarterlyPlans = partial.QuarterlyPlans ?? [],
            RiskRegister = partial.RiskRegister ?? [],
            DecisionsNeeded = partial.DecisionsNeeded ?? [],
            CapacityPlan = partial.CapacityPlan,
            VisionAssessment = visionAssessment,
            MarketContext = marketBrief,
            PrioritizedBacklog = priorityRanking,
            DependencyGraph = dependencyAnalysis,
            FeasibilityAssessment = feasibilityAssessment
        };

        Logger.LogInformation(
            "RoadmapCrew pipeline complete for project {ProjectId} — {RankedCount} items ranked, {RiskCount} risks identified",
            request.ProjectId,
            priorityRanking.RankedItems.Count,
            roadmapDocument.RiskRegister.Count);

        return roadmapDocument;
    }

    /// <inheritdoc />
    public async Task<VisionAssessment> AssessAlignmentAsync(
        IReadOnlyList<WorkItem> items,
        VisionContext vision,
        double alignmentThreshold = 4.0,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(vision);
        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        return await Pipeline.ExecuteStepAsync<VisionAssessment>(
            VisionKeeperId,
            "Evaluate each proposed work item against the strategic vision, OKRs, and themes. " +
            $"Assign alignment scores (0-10) and reject items below {alignmentThreshold}. " +
            "Return a VisionAssessment JSON with approvedItems, rejectedItems, and escalatedItems.",
            new { Vision = vision, ProposedItems = items, AlignmentThreshold = alignmentThreshold },
            cancellationToken).ConfigureAwait(false);
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    private static RoadmapDocument CreateEmptyRoadmap(VisionAssessment visionAssessment) =>
        new()
        {
            ExecutiveSummary = "No items passed strategic alignment review. All proposed items were rejected or escalated by VisionKeeper.",
            VisionAssessment = visionAssessment,
            MarketContext = new MarketContextBrief(),
            PrioritizedBacklog = new PriorityRanking(),
            DependencyGraph = new DependencyAnalysis(),
            FeasibilityAssessment = new FeasibilityAssessment()
        };

    /// <summary>
    /// Partial output from RoadmapSynthesizer — only the prose fields it generates.
    /// Structured data (VisionAssessment, PrioritizedBacklog, etc.) comes from earlier pipeline stages.
    /// </summary>
    private record SynthesizerPartialOutput
    {
        public string? ExecutiveSummary { get; init; }
        public IReadOnlyList<QuarterlyPlan>? QuarterlyPlans { get; init; }
        public IReadOnlyList<RiskEntry>? RiskRegister { get; init; }
        public IReadOnlyList<string>? DecisionsNeeded { get; init; }
        public string? CapacityPlan { get; init; }
    }
}
