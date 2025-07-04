using CognitiveMesh.UILayer.AgencyWidgets.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveMesh.UILayer.AgencyWidgets.Adapters
{
    // --- Supporting DTOs for Adapter Interfaces ---

    /// <summary>
    /// Represents a real-time notification pushed from the backend.
    /// </summary>
    public class Notification
    {
        public string NotificationId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Severity { get; set; } // "Info", "Warning", "Error", "Critical"
        public DateTimeOffset Timestamp { get; set; }
    }

    /// <summary>
    /// Represents the current theme and accessibility settings for the UI.
    /// </summary>
    public class ThemeSettings
    {
        public string Name { get; set; } // "Light", "Dark"
        public bool HighContrastEnabled { get; set; }
        public string LanguageCode { get; set; } // e.g., "en-US"
    }

    // ---------------------------------------------------------------------
    //  New View-Models for Value-Generation Widgets (lightweight / UI side)
    // ---------------------------------------------------------------------

    /// <summary>
    /// A condensed view-model for displaying the “$200 Test” or full value
    /// diagnostic results in the UI layer.  This intentionally contains only
    /// presentation-ready data (keeping the heavy backend DTOs out of the UI
    /// assembly to maintain clean layering).
    /// </summary>
    public class ValueDiagnosticViewModel
    {
        public string TargetId { get; set; }
        public string TargetType { get; set; } // "User" | "Team"
        public double ValueScore { get; set; }
        public string ValueProfile { get; set; }
        public IReadOnlyList<string> Strengths { get; set; }
        public IReadOnlyList<string> DevelopmentOpportunities { get; set; }
    }

    /// <summary>
    /// View-model for organisational blindness trends visualisation.
    /// </summary>
    public class OrgBlindnessTrendViewModel
    {
        public double BlindnessRiskScore { get; set; }
        public IReadOnlyList<string> TopBlindSpots { get; set; }
    }

    /// <summary>
    /// View-model for employability score widget.
    /// </summary>
    public class EmployabilityScoreViewModel
    {
        public string UserId { get; set; }
        public double RiskScore { get; set; }
        public string RiskLevel { get; set; } // Low | Medium | High
    }

    // ---------------------------------------------------------------------
    //  Agentic-AI  (Agent Registry / Authority)  View-Models
    // ---------------------------------------------------------------------

    /// <summary>
    /// Lightweight projection of an agent definition for UI consumption.
    /// </summary>
    public class AgentViewModel
    {
        public string AgentId { get; set; }
        public string AgentType { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }          // Active | Deprecated | Retired
        public string Version { get; set; }
        public string DefaultAutonomy { get; set; } // RecommendOnly | ActWithConfirmation | FullyAutonomous
    }

    /// <summary>
    /// View-model representing the current authority scope of an agent.
    /// </summary>
    public class AuthorityScopeViewModel
    {
        public IReadOnlyList<string> AllowedApiEndpoints { get; set; }
        public double? MaxResourceConsumption { get; set; }
        public double? MaxBudget { get; set; }
        public IReadOnlyList<string> DataAccessPolicies { get; set; }
    }

    /// <summary>
    /// View-model for a single audit / event log row displayed in the Audit/Event Log Overlay.
    /// </summary>
    public class AuditEventViewModel
    {
        public string AuditId { get; set; }
        public string AgentId { get; set; }
        public string ActionType { get; set; }
        public string UserId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Outcome { get; set; }   // Success | Failure | Escalated
        public string CorrelationId { get; set; }
    }

    // ---------------------------------------------------------------------
    //  Consent helpers
    // ---------------------------------------------------------------------

    /// <summary>
    /// Enumerates well-known consent types.  Widgets should use these values
    /// rather than hard-coding strings when interacting with the ConsentAdapter
    /// to avoid typos and aid refactoring.
    /// </summary>
    public enum ConsentType
    {
        /// <summary>
        /// Consent allowing collection of data required to run the value
        /// diagnostic (e.g., “$200 Test”) for a user or team.
        /// </summary>
        ValueDiagnosticDataCollection,

        /// <summary>
        /// Consent allowing the system to compute an employability-risk
        /// assessment for a user.
        /// </summary>
        EmployabilityAnalysis
    }


    // --- Adapter Port Interfaces ---

    /// <summary>
    /// Port for fetching data from the backend Adaptive Agency Router.
    /// Implementations of this adapter are responsible for handling API calls, caching,
    /// retries, circuit breakers, and offline fallback as specified in the PRD.
    /// </summary>
    public interface IDataAPIAdapterPort
    {
        /// <summary>
        /// Fetches the current agency mode status for a given task context.
        /// </summary>
        /// <param name="taskId">The unique identifier for the task context.</param>
        /// <returns>A WidgetState containing the AgencyModeStatus. The state reflects whether the data is fresh, stale (from cache), or if an error occurred.</returns>
        /// <remarks>
        /// **PRD Compliance:** Implements retry logic (3x exponential backoff with jitter), circuit breaker pattern, and offline caching.
        /// **SLA:** Must respond in <250ms (P99) under normal conditions.
        /// </remarks>
        Task<WidgetState<AgencyModeStatus>> GetAgencyModeStatusAsync(string taskId);

        /// <summary>
        /// Retrieves the policy decision table that explains the current agency mode routing.
        /// </summary>
        /// <param name="taskId">The unique identifier for the task context to get the policy for.</param>
        /// <returns>A WidgetState containing a list of applicable policy decisions.</returns>
        Task<WidgetState<List<PolicyDecision>>> GetPolicyDecisionTableAsync(string taskId);

        // -----------------------------------------------------------------
        //            Value-Generation specific data-fetch endpoints
        // -----------------------------------------------------------------

        /// <summary>
        /// Retrieves value diagnostic data for a given user or team.
        /// </summary>
        Task<WidgetState<ValueDiagnosticViewModel>> GetValueDiagnosticDataAsync(
            string targetId,
            string targetType,
            string tenantId);

        /// <summary>
        /// Retrieves organisational blindness trends for an organisation.
        /// </summary>
        Task<WidgetState<OrgBlindnessTrendViewModel>> GetOrgBlindnessTrendsAsync(
            string organizationId,
            string[] departmentFilters,
            string tenantId);

        /// <summary>
        /// Retrieves an employability score for the specified user.  Adapter
        /// implementations must verify that <see cref="ConsentType.EmployabilityAnalysis"/>
        /// has been granted before calling the backend.
        /// </summary>
        Task<WidgetState<EmployabilityScoreViewModel>> GetEmployabilityScoreAsync(
            string userId,
            string tenantId);

        /// <summary>
        /// Submits a completed “$200 Test” questionnaire for the current user.
        /// </summary>
        Task<bool> SubmitTwoHundredDollarTestAsync(
            string userId,
            IDictionary<string, object> responses,
            string tenantId);

        // -----------------------------------------------------------------
        //                  Agent Registry & Authority Endpoints
        // -----------------------------------------------------------------

        /// <summary>
        /// Retrieves a list of agents in the registry.
        /// </summary>
        Task<WidgetState<IEnumerable<AgentViewModel>>> GetAgentRegistryAsync(
            bool includeRetired,
            string tenantId);

        /// <summary>
        /// Retrieves detailed information for a single agent.
        /// </summary>
        Task<WidgetState<AgentViewModel>> GetAgentDetailsAsync(
            string agentId,
            string tenantId);

        /// <summary>
        /// Retrieves the current authority scope for the specified agent.
        /// </summary>
        Task<WidgetState<AuthorityScopeViewModel>> GetAuthorityScopeAsync(
            string agentId,
            string tenantId);

        /// <summary>
        /// Updates the authority scope for the specified agent (admin-only operation).
        /// </summary>
        Task<bool> UpdateAuthorityScopeAsync(
            string agentId,
            AuthorityScopeViewModel newScope,
            string tenantId,
            string reason);

        /// <summary>
        /// Performs an authority override for the specified agent.
        /// </summary>
        Task<bool> OverrideAuthorityAsync(
            string agentId,
            AuthorityScopeViewModel overrideScope,
            TimeSpan duration,
            string tenantId,
            string reason);

        /// <summary>
        /// Validates whether a proposed agent action is within authority.
        /// </summary>
        Task<bool> ValidateActionAuthorityAsync(
            string agentId,
            string actionType,
            IDictionary<string, object> parameters,
            string tenantId);

        /// <summary>
        /// Retrieves a paged set of audit events related to agents.
        /// </summary>
        Task<WidgetState<IEnumerable<AuditEventViewModel>>> GetAgentAuditEventsAsync(
            string agentId,
            DateTimeOffset? since,
            int pageSize,
            string tenantId);
    }

    /// <summary>
    /// Port for handling all user consent and override actions.
    /// Implementations must ensure that all actions are securely transmitted and logged for audit purposes.
    /// </summary>
    public interface IConsentAdapter
    {
        /// <summary>
        /// Submits a user's consent decision to the backend.
        /// </summary>
        /// <param name="consentRequest">The consent decision to be recorded.</param>
        /// <returns>A task that resolves to true if the submission was successfully accepted by the backend; otherwise, false.</returns>
        /// <remarks>
        /// **PRD Compliance:** The adapter must handle transient network errors with a retry policy.
        /// The operation is critical and must be logged for audit.
        /// </remarks>
        Task<bool> SubmitConsentAsync(ConsentRequest consentRequest);

        /// <summary>
        /// Submits a user or admin override to the agency mode for a specific task.
        /// </summary>
        /// <param name="overrideRequest">The override request to be applied.</param>
        /// <returns>A task that resolves to true if the override was successfully applied; otherwise, false.</returns>
        Task<bool> SubmitOverrideAsync(OverrideRequest overrideRequest);

        // -----------------------------------------------------------------
        //                Value-Generation helper methods
        // -----------------------------------------------------------------

        /// <summary>
        /// Checks whether a user already has an active consent of the specified
        /// <paramref name="consentType"/>.  Widgets should call this to decide
        /// whether to show the consent overlay.
        /// </summary>
        Task<bool> HasActiveConsentAsync(string userId, ConsentType consentType);

        // -----------------------------------------------------------------
        //                Agent-Specific Consent Methods
        // -----------------------------------------------------------------

        /// <summary>
        /// Records consent for a specific agent action.
        /// </summary>
        Task<bool> RecordAgentConsentAsync(
            ConsentRequest consentRequest,
            string agentId,
            string agentActionType);

        /// <summary>
        /// Validates whether consent exists for a given agent and consent type.
        /// </summary>
        Task<bool> ValidateAgentConsentAsync(
            string userId,
            string tenantId,
            string agentId,
            string consentType);

        /// <summary>
        /// Revokes consent previously granted for agent operations.
        /// </summary>
        Task<bool> RevokeAgentConsentAsync(
            string userId,
            string tenantId,
            string agentId,
            string consentType);
    }

    /// <summary>
    /// Port for receiving real-time push notifications from the backend.
    /// This adapter is responsible for establishing and maintaining a connection (e.g., WebSocket, SignalR)
    /// to the mesh's notification service.
    /// </summary>
    public interface INotificationAdapter
    {
        /// <summary>
        /// Event that is raised when a new notification is received from the backend.
        /// UI components can subscribe to this event to display real-time alerts and updates.
        /// </summary>
        event Action<Notification> OnNotificationReceived;

        /// <summary>
        /// Initiates the connection to the notification service and starts listening for events.
        /// </summary>
        Task StartListeningAsync();

        /// <summary>
        /// Closes the connection to the notification service.
        /// </summary>
        Task StopListeningAsync();

        // -----------------------------------------------------------------
        //                Value-Generation notifications
        // -----------------------------------------------------------------

        /// <summary>
        /// Sends (or queues) a notification to the specified <paramref name="userId"/>
        /// about newly available value-diagnostic results.
        /// </summary>
        Task SendValueDiagnosticNotificationAsync(
            string userId,
            double valueScore,
            string valueProfile);

        /// <summary>
        /// Sends an employability alert (risk/opportunity) to the given user.
        /// </summary>
        Task SendEmployabilityAlertAsync(
            string userId,
            string riskLevel,
            double riskScore);
    }

    /// <summary>
    /// Port for sending telemetry data to the backend for compliance, auditing, and analytics.
    /// Implementations must handle batching and offline queuing to ensure data is not lost.
    /// </summary>
    public interface ITelemetryAdapter
    {
        /// <summary>
        /// Logs a single telemetry event. This is a "fire-and-forget" operation from the caller's perspective.
        /// The adapter implementation is responsible for queuing, batching, and sending the data.
        /// </summary>
        /// <param name="telemetryEvent">The event to be logged.</param>
        /// <returns>A task that completes once the event has been queued for processing.</returns>
        /// <remarks>
        /// **PRD Compliance:** Events are batched and flushed every <10 seconds. Implements an offline queue for resilience.
        /// </remarks>
        Task LogEventAsync(TelemetryEvent telemetryEvent);
    }

    /// <summary>
    /// Centralised constants used for telemetry event “Action” values so that
    /// producers and consumers remain in sync without magic strings.
    /// </summary>
    public static class TelemetryEventTypes
    {
        public const string ValueDiagnosticViewed = "ValueDiagnosticViewed";
        public const string EmployabilityConsentGranted = "EmployabilityConsentGranted";
        public const string OrgBlindnessReportGenerated = "OrgBlindnessReportGenerated";

        // -------- Agentic-AI specific events --------
        public const string AgentInvoked = "AgentInvoked";
        public const string AuthorityConsentGranted = "AuthorityConsentGranted";
        public const string AgentActionAudited = "AgentActionAudited";
    }

    /// <summary>
    /// Port for managing UI theme and accessibility settings.
    /// This adapter ensures all UI components adhere to a consistent visual style and meet accessibility standards.
    /// </summary>
    public interface IThemeAdapter
    {
        /// <summary>
        /// Retrieves the current theme and accessibility settings for the application.
        /// </summary>
        /// <returns>A task that resolves to the current ThemeSettings.</returns>
        Task<ThemeSettings> GetCurrentThemeAsync();

        /// <summary>
        /// Event that is raised when the theme or accessibility settings change.
        /// UI components should subscribe to this event to dynamically update their appearance.
        /// </summary>
        event Action<ThemeSettings> OnThemeChanged;
    }
}
