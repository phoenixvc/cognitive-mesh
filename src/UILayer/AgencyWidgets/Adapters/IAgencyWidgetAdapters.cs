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
