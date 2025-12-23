using System.Collections.Concurrent;
using CognitiveMesh.ReasoningLayer.SecurityReasoning.Ports;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.SecurityMonitoring;

public enum IncidentStatus
{
    New,
    UnderInvestigation,
    Contained,
    Remediated,
    Resolved,
    Closed
}

public enum IncidentSeverity
{
    Informational,
    Low,
    Medium,
    High,
    Critical
}

public class SecurityIncident
{
    public string IncidentId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; }
    public IncidentStatus Status { get; set; }
    public IncidentSeverity Severity { get; set; }
    public DateTimeOffset FirstSeen { get; set; }
    public DateTimeOffset LastSeen { get; set; }
    public List<SecurityEvent> CorrelatedEvents { get; set; } = new();
    public List<string> RecommendedActions { get; set; } = new();
    public string Summary { get; set; }
}

public interface ISecurityIncidentPort
{
    Task HandleSecurityEventAsync(SecurityEvent securityEvent);
    Task<SecurityIncident> GetIncidentDetailsAsync(string incidentId);
    Task<IEnumerable<SecurityIncident>> GetActiveIncidentsAsync();
}

/// <summary>
/// Implements the core logic for security incident monitoring. This service aggregates raw security events,
/// correlates them into actionable incidents, analyzes their severity using threat intelligence,
/// and orchestrates the appropriate response by notifying stakeholders or dispatching automated agents.
/// </summary>
public class SecurityIncidentMonitor : ISecurityIncidentPort
{
    private readonly ILogger<SecurityIncidentMonitor> _logger;
    private readonly IThreatIntelligencePort _threatIntelPort;
    private readonly INotificationPort _notificationPort;
    private readonly IAgentOrchestrationPort _agentOrchestrationPort;

    // In-memory store for ongoing incidents. In a real system, this would be a distributed cache or database.
    private static readonly ConcurrentDictionary<string, SecurityIncident> _activeIncidents = new();

    public SecurityIncidentMonitor(
        ILogger<SecurityIncidentMonitor> logger,
        IThreatIntelligencePort threatIntelPort,
        INotificationPort notificationPort,
        IAgentOrchestrationPort agentOrchestrationPort)
    {
        _logger = logger;
        _threatIntelPort = threatIntelPort;
        _notificationPort = notificationPort;
        _agentOrchestrationPort = agentOrchestrationPort;
    }

    public async Task HandleSecurityEventAsync(SecurityEvent securityEvent)
    {
        _logger.LogDebug("Processing security event {EventId} of type '{EventType}' from source '{Source}'.",
            securityEvent.EventId, securityEvent.EventType, securityEvent.Source);

        // Simple correlation logic based on SubjectId. A real system would use more complex rules.
        var correlationKey = securityEvent.Data.GetValueOrDefault("subjectId")?.ToString();
        if (string.IsNullOrEmpty(correlationKey))
        {
            _logger.LogWarning("Security event {EventId} lacks a correlation key ('subjectId') and will be treated as a standalone incident.", securityEvent.EventId);
            correlationKey = $"standalone-{securityEvent.EventId}";
        }

        var incident = _activeIncidents.GetOrAdd(correlationKey, key => new SecurityIncident
        {
            FirstSeen = securityEvent.Timestamp,
            Status = IncidentStatus.New,
            Severity = IncidentSeverity.Informational,
            Title = $"New activity detected for subject: {key}"
        });

        lock (incident)
        {
            incident.CorrelatedEvents.Add(securityEvent);
            incident.LastSeen = securityEvent.Timestamp;
        }

        await AnalyzeAndEscalateIncidentAsync(incident);
    }

    public Task<SecurityIncident> GetIncidentDetailsAsync(string incidentId)
    {
        var incident = _activeIncidents.Values.FirstOrDefault(i => i.IncidentId == incidentId);
        return Task.FromResult(incident);
    }

    public Task<IEnumerable<SecurityIncident>> GetActiveIncidentsAsync()
    {
        var active = _activeIncidents.Values.Where(i => i.Status != IncidentStatus.Closed && i.Status != IncidentStatus.Resolved).ToList();
        return Task.FromResult<IEnumerable<SecurityIncident>>(active);
    }

    private async Task AnalyzeAndEscalateIncidentAsync(SecurityIncident incident)
    {
        _logger.LogInformation("Analyzing incident {IncidentId} with {EventCount} correlated events.", incident.IncidentId, incident.CorrelatedEvents.Count);

        var analysisRequest = new ThreatAnalysisRequest
        {
            Events = incident.CorrelatedEvents,
            Context = new Dictionary<string, string> { { "incidentId", incident.IncidentId } }
        };

        var analysisResponse = await _threatIntelPort.AnalyzeThreatPatternsAsync(analysisRequest);

        var previousSeverity = incident.Severity;
        IncidentSeverity newSeverity;

        if (analysisResponse.IsThreatDetected)
        {
            incident.Summary = analysisResponse.ThreatDescription;
            incident.RecommendedActions = analysisResponse.RecommendedActions;
            newSeverity = analysisResponse.Severity switch
            {
                "Critical" => IncidentSeverity.Critical,
                "High" => IncidentSeverity.High,
                "Medium" => IncidentSeverity.Medium,
                _ => IncidentSeverity.Low
            };
        }
        else
        {
            // If no specific pattern is detected, base severity on the highest risk score of any single event.
            int maxRiskScore = 0;
            foreach (var evt in incident.CorrelatedEvents)
            {
                var riskRequest = new RiskScoringRequest
                {
                    SubjectId = evt.Data.GetValueOrDefault("subjectId")?.ToString(),
                    Action = evt.EventType,
                    ResourceId = evt.Data.GetValueOrDefault("resourceId")?.ToString(),
                    Context = evt.Data
                };
                var riskResponse = await _threatIntelPort.CalculateRiskScoreAsync(riskRequest);
                if (riskResponse.RiskScore > maxRiskScore)
                {
                    maxRiskScore = riskResponse.RiskScore;
                }
            }
            newSeverity = GetSeverityFromScore(maxRiskScore);
        }

        if (newSeverity > incident.Severity)
        {
            incident.Severity = newSeverity;
            _logger.LogInformation("Incident {IncidentId} severity updated from {PreviousSeverity} to {NewSeverity}.",
                incident.IncidentId, previousSeverity, newSeverity);
        }
            
        if (incident.Status == IncidentStatus.New && incident.Severity >= IncidentSeverity.Medium)
        {
            incident.Status = IncidentStatus.UnderInvestigation;
            await EscalateIncidentAsync(incident);
        }
    }

    private async Task EscalateIncidentAsync(SecurityIncident incident)
    {
        _logger.LogWarning("Escalating security incident {IncidentId} with severity '{Severity}'. Title: {Title}",
            incident.IncidentId, incident.Severity, incident.Title);

        // Update a conceptual dashboard (in a real system, this would be a separate service call).
        UpdateComplianceDashboard(incident);

        // Send notifications based on severity.
        var notification = new Notification
        {
            Subject = $"Security Alert [{incident.Severity}]: {incident.Title}",
            Message = $"Incident {incident.IncidentId} requires attention. Summary: {incident.Summary}",
            Timestamp = DateTimeOffset.UtcNow
        };

        if (incident.Severity >= IncidentSeverity.High)
        {
            notification.Channels = new List<string> { "Email", "SMS", "PagerDuty" };
            notification.Recipients = new List<string> { "security-oncall@cognitivemesh.com" };
        }
        else
        {
            notification.Channels = new List<string> { "Email", "Slack" };
            notification.Recipients = new List<string> { "security-team@cognitivemesh.com" };
        }
        await _notificationPort.SendNotificationAsync(notification);

        // Trigger automated response agents for high-severity incidents.
        if (incident.Severity >= IncidentSeverity.High && incident.Status == IncidentStatus.UnderInvestigation)
        {
            _logger.LogInformation("Dispatching AutomatedResponseAgent for incident {IncidentId}.", incident.IncidentId);
            var agentTask = new AgentTaskRequest
            {
                AgentId = "AutomatedResponseAgent",
                TaskDescription = "Perform initial containment and evidence gathering for a high-severity security incident.",
                Parameters = new Dictionary<string, object>
                {
                    { "incidentId", incident.IncidentId },
                    { "correlatedEvents", incident.CorrelatedEvents },
                    { "recommendedActions", incident.RecommendedActions }
                },
                Priority = 9 // High priority
            };
            await _agentOrchestrationPort.ExecuteTaskAsync(agentTask);
            incident.Status = IncidentStatus.Contained;
        }
    }

    private IncidentSeverity GetSeverityFromScore(int score)
    {
        if (score > 80) return IncidentSeverity.Critical;
        if (score > 60) return IncidentSeverity.High;
        if (score > 40) return IncidentSeverity.Medium;
        if (score > 20) return IncidentSeverity.Low;
        return IncidentSeverity.Informational;
    }

    private void UpdateComplianceDashboard(SecurityIncident incident)
    {
        // In a real system, this would call a port to a dashboarding service.
        _logger.LogInformation("Updating compliance dashboard: Incident {IncidentId} is now at status '{Status}' with severity '{Severity}'.",
            incident.IncidentId, incident.Status, incident.Severity);
    }
}