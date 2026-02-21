using System.Text.Json;
using CognitiveMesh.ReasoningLayer.SecurityReasoning.Ports;
using MetacognitiveLayer.SecurityMonitoring;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.SecurityAgents;

/// <summary>
/// Port for network security operations such as IP blocking.
/// </summary>
public interface INetworkSecurityPort
{
    /// <summary>
    /// Blocks the specified IP address for the given reason.
    /// </summary>
    Task BlockIpAddressAsync(string ipAddress, string reason);
}

/// <summary>
/// Port for identity management operations such as account isolation.
/// </summary>
public interface IIdentityManagementPort
{
    /// <summary>
    /// Isolates the specified account for the given reason.
    /// </summary>
    Task IsolateAccountAsync(string subjectId, string reason);
}

/// <summary>
/// Port for forensic data collection and evidence storage.
/// </summary>
public interface IForensicDataPort
{
    /// <summary>
    /// Stores forensic evidence for the specified incident.
    /// </summary>
    Task<string> StoreEvidenceAsync(string incidentId, object evidence);
}

/// <summary>
/// Represents a request to execute an agent task.
/// </summary>
public class AgentTaskRequest
{
    /// <summary>Gets or sets the unique identifier of the agent.</summary>
    public string AgentId { get; set; }
    /// <summary>Gets or sets the description of the task to execute.</summary>
    public string TaskDescription { get; set; }
    /// <summary>Gets or sets the parameters for the task.</summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
    /// <summary>Gets or sets the priority of the task.</summary>
    public int Priority { get; set; }
}

/// <summary>
/// Represents the response from an agent task execution.
/// </summary>
public class AgentTaskResponse
{
    /// <summary>Gets or sets whether the task was successful.</summary>
    public bool IsSuccess { get; set; }
    /// <summary>Gets or sets the result message.</summary>
    public string Message { get; set; }
    /// <summary>Gets or sets the output data from the task.</summary>
    public Dictionary<string, object> Output { get; set; } = new();
}

/// <summary>
/// Defines the contract for an executable agent.
/// </summary>
public interface IAgent
{
    /// <summary>Gets the unique identifier of the agent.</summary>
    string AgentId { get; }
    /// <summary>
    /// Executes the specified task and returns the result.
    /// </summary>
    Task<AgentTaskResponse> ExecuteTaskAsync(AgentTaskRequest request);
}

/// <summary>
/// Port for orchestrating agent task execution.
/// </summary>
public interface IAgentOrchestrationPort
{
    /// <summary>
    /// Executes the specified agent task and returns the result.
    /// </summary>
    Task<AgentTaskResponse> ExecuteTaskAsync(AgentTaskRequest request);
}

/// <summary>
/// Represents a notification to be sent through one or more channels.
/// </summary>
public class Notification
{
    /// <summary>Gets or sets the notification subject.</summary>
    public string Subject { get; set; }
    /// <summary>Gets or sets the notification message body.</summary>
    public string Message { get; set; }
    /// <summary>Gets or sets the delivery channels (e.g., email, SMS).</summary>
    public List<string> Channels { get; set; }
    /// <summary>Gets or sets the notification recipients.</summary>
    public List<string> Recipients { get; set; }
    /// <summary>Gets or sets the timestamp when the notification was created.</summary>
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Port for sending notifications.
/// </summary>
public interface INotificationPort
{
    /// <summary>
    /// Sends the specified notification asynchronously.
    /// </summary>
    Task SendNotificationAsync(Notification notification);
}


/// <summary>
/// An autonomous agent responsible for executing automated incident response playbooks.
/// This agent performs immediate containment actions to mitigate threats, preserves forensic
/// evidence, and coordinates with other security systems to ensure a swift and effective response.
/// </summary>
public class AutomatedResponseAgent : IAgent
{
    public string AgentId => "AutomatedResponseAgent";

    private readonly ILogger<AutomatedResponseAgent> _logger;
    private readonly INetworkSecurityPort _networkSecurityPort;
    private readonly IIdentityManagementPort _identityManagementPort;
    private readonly IForensicDataPort _forensicDataPort;
    private readonly INotificationPort _notificationPort;
    private readonly ISecurityIncidentPort _securityIncidentPort;

    public AutomatedResponseAgent(
        ILogger<AutomatedResponseAgent> logger,
        INetworkSecurityPort networkSecurityPort,
        IIdentityManagementPort identityManagementPort,
        IForensicDataPort forensicDataPort,
        INotificationPort notificationPort,
        ISecurityIncidentPort securityIncidentPort)
    {
        _logger = logger;
        _networkSecurityPort = networkSecurityPort;
        _identityManagementPort = identityManagementPort;
        _forensicDataPort = forensicDataPort;
        _notificationPort = notificationPort;
        _securityIncidentPort = securityIncidentPort;
    }

    /// <inheritdoc />
    public async Task<AgentTaskResponse> ExecuteTaskAsync(AgentTaskRequest request)
    {
        _logger.LogInformation("AutomatedResponseAgent starting task for incident: {IncidentId}", request.Parameters.GetValueOrDefault("incidentId"));

        if (!ValidateParameters(request.Parameters, out var incidentId, out var events, out var recommendedActions))
        {
            var errorMsg = "Invalid or missing parameters for AutomatedResponseAgent.";
            _logger.LogError(errorMsg);
            return new AgentTaskResponse { IsSuccess = false, Message = errorMsg };
        }

        var actionsTaken = new List<string>();

        // 1. Execute Immediate Containment Actions
        foreach (var action in recommendedActions)
        {
            try
            {
                if (action.Contains("Block user account", StringComparison.OrdinalIgnoreCase) || action.Contains("Lock user account", StringComparison.OrdinalIgnoreCase))
                {
                    var subjectId = events.FirstOrDefault()?.Data.GetValueOrDefault("subjectId")?.ToString();
                    if (!string.IsNullOrEmpty(subjectId))
                    {
                        await _identityManagementPort.IsolateAccountAsync(subjectId, $"Automated response to incident {incidentId}.");
                        var actionMsg = $"Isolated account for subject '{subjectId}'.";
                        _logger.LogWarning(actionMsg);
                        actionsTaken.Add(actionMsg);
                    }
                }

                if (action.Contains("Block IP", StringComparison.OrdinalIgnoreCase))
                {
                    var ipAddress = events.FirstOrDefault()?.Data.GetValueOrDefault("ipAddress")?.ToString();
                    if (!string.IsNullOrEmpty(ipAddress))
                    {
                        await _networkSecurityPort.BlockIpAddressAsync(ipAddress, $"Automated response to incident {incidentId}.");
                        var actionMsg = $"Blocked malicious IP address '{ipAddress}'.";
                        _logger.LogWarning(actionMsg);
                        actionsTaken.Add(actionMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing containment action '{Action}' for incident {IncidentId}.", action, incidentId);
                actionsTaken.Add($"Failed to execute action '{action}': {ex.Message}");
            }
        }

        // 2. Preserve Forensic Evidence
        var evidenceId = await PreserveEvidence(incidentId, events, actionsTaken);
        actionsTaken.Add($"Preserved forensic evidence bundle with ID: {evidenceId}.");

        // 3. Coordinate and Report
        await NotifyStakeholders(incidentId, actionsTaken);
        await UpdateIncidentStatus(incidentId);

        var successMsg = $"Automated response playbook completed for incident {incidentId}.";
        _logger.LogInformation(successMsg);

        return new AgentTaskResponse
        {
            IsSuccess = true,
            Message = successMsg,
            Output = new Dictionary<string, object>
            {
                { "actionsTaken", actionsTaken },
                { "evidenceBundleId", evidenceId }
            }
        };
    }

    private bool ValidateParameters(
        Dictionary<string, object> parameters,
        out string incidentId,
        out List<SecurityEvent> events,
        out List<string> recommendedActions)
    {
        incidentId = parameters.GetValueOrDefault("incidentId")?.ToString();
            
        var eventsObj = parameters.GetValueOrDefault("correlatedEvents");
        events = eventsObj is JsonElement je ? JsonSerializer.Deserialize<List<SecurityEvent>>(je.GetRawText()) : eventsObj as List<SecurityEvent>;

        var actionsObj = parameters.GetValueOrDefault("recommendedActions");
        recommendedActions = actionsObj is JsonElement jeActions ? JsonSerializer.Deserialize<List<string>>(jeActions.GetRawText()) : actionsObj as List<string>;

        return !string.IsNullOrEmpty(incidentId) && events != null && recommendedActions != null;
    }

    private async Task<string> PreserveEvidence(string incidentId, List<SecurityEvent> events, List<string> actionsTaken)
    {
        _logger.LogInformation("Preserving forensic evidence for incident {IncidentId}.", incidentId);
        var evidenceBundle = new
        {
            IncidentId = incidentId,
            SnapshotTimestamp = DateTimeOffset.UtcNow,
            CorrelatedEvents = events,
            AutomatedActionsTaken = actionsTaken,
            SystemState = new { /* In a real system, collect relevant state snapshots */ }
        };

        try
        {
            var evidenceId = await _forensicDataPort.StoreEvidenceAsync(incidentId, evidenceBundle);
            _logger.LogInformation("Successfully stored evidence bundle for incident {IncidentId} with ID {EvidenceId}.", incidentId, evidenceId);
            return evidenceId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store forensic evidence for incident {IncidentId}.", incidentId);
            return "failed-to-store";
        }
    }

    private async Task NotifyStakeholders(string incidentId, List<string> actionsTaken)
    {
        var notification = new Notification
        {
            Subject = $"[Automated Response] Actions Taken for Incident {incidentId}",
            Message = "The AutomatedResponseAgent has taken the following actions:\n" + string.Join("\n- ", actionsTaken),
            Channels = new List<string> { "Slack" },
            Recipients = new List<string> { "#security-incidents" },
            Timestamp = DateTimeOffset.UtcNow
        };
        await _notificationPort.SendNotificationAsync(notification);
    }

    private async Task UpdateIncidentStatus(string incidentId)
    {
        var incident = await _securityIncidentPort.GetIncidentDetailsAsync(incidentId);
        if (incident != null)
        {
            incident.Status = IncidentStatus.Contained;
            _logger.LogInformation("Updating incident {IncidentId} status to 'Contained'.", incidentId);
            // In a real system, this would persist the change.
            // await _securityIncidentPort.UpdateIncidentAsync(incident);
        }
    }
}