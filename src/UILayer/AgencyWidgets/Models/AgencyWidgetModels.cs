using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CognitiveMesh.UILayer.AgencyWidgets.Models
{
    /// <summary>
    /// Defines the degree of independent decision-making an agent can exercise.
    /// This is a foundational concept from the Agent Framework, adapted for UI consumption
    /// and displayed in the Agency Mode Banner and AAA Control Center.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AutonomyLevel
    {
        /// <summary>
        /// A special mode where human authorship is paramount, and AI assistance is minimal and explicit.
        /// Represents the highest level of Cognitive Sovereignty.
        /// </summary>
        SovereigntyFirst,

        /// <summary>
        /// The agent can only analyze and provide recommendations. A human must approve any action.
        /// </summary>
        RecommendOnly,

        /// <summary>
        /// The agent can propose and prepare an action but requires explicit user confirmation before execution.
        /// This is a balanced, hybrid mode.
        /// </summary>
        ActWithConfirmation,

        /// <summary>
        /// The agent can act independently within its defined authority scope without requiring confirmation for each action.
        /// Represents the highest level of agentic autonomy.
        /// </summary>
        FullyAutonomous
    }

    /// <summary>
    /// Represents the current authorship and control status, designed to be displayed in the Sovereignty Status Overlay.
    /// </summary>
    public class SovereigntyStatus
    {
        /// <summary>
        /// A human-readable string describing the current authorship state.
        /// e.g., "Human-Authored", "AI-Assisted", "Agent-Driven"
        /// </summary>
        public string AuthorshipState { get; set; }

        /// <summary>
        /// Indicates if a user or admin has manually overridden the system's default agency mode.
        /// </summary>
        public bool IsOverrideActive { get; set; }

        /// <summary>
        /// The reason provided for the active override, if applicable.
        /// </summary>
        public string OverrideReason { get; set; }
    }

    /// <summary>
    /// A DTO representing the real-time state shown in the Agency Mode Banner.
    /// It provides an at-a-glance summary of the current operational mode.
    /// </summary>
    public class AgencyModeStatus
    {
        public AutonomyLevel CurrentAutonomyLevel { get; set; }
        public string AuthorityScopeSummary { get; set; } // e.g., "Limited to read-only actions"
        public SovereigntyStatus Sovereignty { get; set; }
        public DateTimeOffset LastChecked { get; set; }
    }
    
    /// <summary>
    /// A view model representing the detailed authority scope of an agent,
    /// designed for display in the AAA Control Center.
    /// </summary>
    public class AuthorityScopeViewModel
    {
        public List<string> AllowedApiEndpoints { get; set; } = new();
        public string MaxResourceConsumption { get; set; }
        public string MaxBudget { get; set; }
        public List<string> DataAccessPolicies { get; set; } = new();
    }

    /// <summary>
    /// Represents the contextual information used by the backend router to make a decision.
    /// This is displayed in the AAA Control Center and Policy Decision Table Viewer to provide transparency.
    /// </summary>
    public class PolicyContext
    {
        public string TaskType { get; set; }
        public double CognitiveImpactAssessmentScore { get; set; }
        public double CognitiveSovereigntyIndexScore { get; set; }
    }

    /// <summary>
    /// Represents a single rule from the Policy Decision Table Viewer, explaining how a decision was made.
    /// </summary>
    public class PolicyDecision
    {
        public string MatchedCondition { get; set; } // e.g., "CIA > 0.7"
        public AutonomyLevel ResultingAutonomy { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// A model for requesting user consent, designed to be rendered by the Consent/Notification Modal.
    /// </summary>
    public class ConsentRequest
    {
        public string RequestId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ActionToApprove { get; set; }
        public List<string> RequiredPermissions { get; set; } = new();
    }

    /// <summary>
    /// A model for submitting an override request from the AAA Control Center.
    /// </summary>
    public class OverrideRequest
    {
        public string TaskId { get; set; }
        public AutonomyLevel RequestedAutonomyLevel { get; set; }
        public string Justification { get; set; }
    }

    /// <summary>
    /// A standardized error format for all UI components, as specified in the PRD.
    /// Used by the shared overlay/pattern library.
    /// </summary>
    public class ErrorEnvelope
    {
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public string CorrelationID { get; set; }
        public bool CanRetry { get; set; }
    }

    /// <summary>
    /// A generic container for widget state, supporting offline caching and data staleness detection.
    /// </summary>
    /// <typeparam name="T">The type of the widget's data payload.</typeparam>
    public class WidgetState<T>
    {
        public T Data { get; set; }
        public bool IsStale { get; set; }
        public DateTimeOffset LastSyncTimestamp { get; set; }
        public ErrorEnvelope LastError { get; set; }
    }

    /// <summary>
    /// A structured model for logging user interactions and system events via the TelemetryAdapter.
    /// </summary>
    public class TelemetryEvent
    {
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public string WidgetId { get; set; } // e.g., "experimental-velocity-widget"
        public string PanelId { get; set; } // e.g., "AAAControlCenter"
        public string UserId { get; set; }
        public string CorrelationID { get; set; }
        public string Action { get; set; } // e.g., "OverrideApplied", "ConsentGranted", "ApiCallFailed"
        public string ErrorCode { get; set; } // Null if no error
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
