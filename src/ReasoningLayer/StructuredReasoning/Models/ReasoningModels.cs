using System;
using System.Collections.Generic;

namespace CognitiveMesh.ReasoningLayer.StructuredReasoning.Models
{
    /// <summary>
    /// Represents a structured reasoning output with auditable trace.
    /// </summary>
    public class ReasoningOutput
    {
        /// <summary>
        /// Unique identifier for this reasoning session.
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// The type of reasoning recipe used (Debate, Sequential, Strategic).
        /// </summary>
        public ReasoningRecipeType RecipeType { get; set; }

        /// <summary>
        /// The final conclusion or decision reached.
        /// </summary>
        public string Conclusion { get; set; } = string.Empty;

        /// <summary>
        /// The confidence level in the conclusion (0.0 to 1.0).
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Detailed trace of the reasoning process for auditability.
        /// </summary>
        public List<ReasoningStep> ReasoningTrace { get; set; } = new List<ReasoningStep>();

        /// <summary>
        /// Timestamp when the reasoning was performed.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Metadata about the reasoning process.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of ReasoningOutput with a unique session ID.
        /// </summary>
        public ReasoningOutput()
        {
            SessionId = Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// Types of reasoning recipes available in ConclAIve.
    /// </summary>
    public enum ReasoningRecipeType
    {
        /// <summary>
        /// Debate & Vote: Surface diverse angles, compare, select, and synthesize.
        /// </summary>
        DebateAndVote,

        /// <summary>
        /// Sequential Reasoning: Decompose into specialized steps, then integrate.
        /// </summary>
        Sequential,

        /// <summary>
        /// Strategic Simulation: Explore scenarios using patterns and constraints.
        /// </summary>
        StrategicSimulation
    }

    /// <summary>
    /// Represents a single step in the reasoning process.
    /// </summary>
    public class ReasoningStep
    {
        /// <summary>
        /// The order of this step in the reasoning sequence.
        /// </summary>
        public int StepNumber { get; set; }

        /// <summary>
        /// The name or title of this reasoning step.
        /// </summary>
        public string StepName { get; set; } = string.Empty;

        /// <summary>
        /// The input provided to this step.
        /// </summary>
        public string Input { get; set; } = string.Empty;

        /// <summary>
        /// The output produced by this step.
        /// </summary>
        public string Output { get; set; } = string.Empty;

        /// <summary>
        /// The agent or perspective that produced this step (for debate).
        /// </summary>
        public string? AgentId { get; set; }

        /// <summary>
        /// Timestamp when this step was executed.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Additional metadata for this step.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Request for debate and vote reasoning.
    /// </summary>
    public class DebateRequest
    {
        /// <summary>
        /// The question or problem to debate.
        /// </summary>
        public string Question { get; set; } = string.Empty;

        /// <summary>
        /// The perspectives or ideological viewpoints to consider.
        /// </summary>
        public List<string> Perspectives { get; set; } = new List<string>();

        /// <summary>
        /// Context and background information for the debate.
        /// </summary>
        public Dictionary<string, string> Context { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// The voting mechanism to use (e.g., "majority", "consensus", "weighted").
        /// </summary>
        public string VotingMechanism { get; set; } = "majority";
    }

    /// <summary>
    /// Request for sequential reasoning.
    /// </summary>
    public class SequentialReasoningRequest
    {
        /// <summary>
        /// The complex question to be decomposed.
        /// </summary>
        public string Question { get; set; } = string.Empty;

        /// <summary>
        /// The specialized phases or steps to execute in sequence.
        /// If empty, the engine will auto-decompose.
        /// </summary>
        public List<string> Phases { get; set; } = new List<string>();

        /// <summary>
        /// Context information for the reasoning.
        /// </summary>
        public Dictionary<string, string> Context { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Request for strategic simulation.
    /// </summary>
    public class StrategicSimulationRequest
    {
        /// <summary>
        /// The scenario or strategic question to explore.
        /// </summary>
        public string Scenario { get; set; } = string.Empty;

        /// <summary>
        /// Patterns or frameworks to apply (e.g., "SWOT", "Porter's Five Forces").
        /// </summary>
        public List<string> Patterns { get; set; } = new List<string>();

        /// <summary>
        /// Constraints that limit possible outcomes.
        /// </summary>
        public List<string> Constraints { get; set; } = new List<string>();

        /// <summary>
        /// Data points or facts to consider in the simulation.
        /// </summary>
        public Dictionary<string, string> DataPoints { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Number of scenarios to explore.
        /// </summary>
        public int ScenarioCount { get; set; } = 3;
    }

    /// <summary>
    /// A perspective or viewpoint in a debate.
    /// </summary>
    public class DebatePerspective
    {
        /// <summary>
        /// The name or label of this perspective.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The argument or position from this perspective.
        /// </summary>
        public string Argument { get; set; } = string.Empty;

        /// <summary>
        /// Supporting evidence or reasoning.
        /// </summary>
        public List<string> SupportingPoints { get; set; } = new List<string>();

        /// <summary>
        /// The vote weight for this perspective (if applicable).
        /// </summary>
        public double VoteWeight { get; set; } = 1.0;
    }

    /// <summary>
    /// A scenario explored in strategic simulation.
    /// </summary>
    public class ExploredScenario
    {
        /// <summary>
        /// The name or identifier of this scenario.
        /// </summary>
        public string ScenarioId { get; set; } = string.Empty;

        /// <summary>
        /// Description of the scenario.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The anticipated outcome of this scenario.
        /// </summary>
        public string AnticipatedOutcome { get; set; } = string.Empty;

        /// <summary>
        /// Probability or likelihood score (0.0 to 1.0).
        /// </summary>
        public double Probability { get; set; }

        /// <summary>
        /// Risk factors associated with this scenario.
        /// </summary>
        public List<string> RiskFactors { get; set; } = new List<string>();

        /// <summary>
        /// Opportunities in this scenario.
        /// </summary>
        public List<string> Opportunities { get; set; } = new List<string>();

        /// <summary>
        /// Initializes a new instance of ExploredScenario with a unique scenario ID.
        /// </summary>
        public ExploredScenario()
        {
            ScenarioId = Guid.NewGuid().ToString();
        }
    }
}
