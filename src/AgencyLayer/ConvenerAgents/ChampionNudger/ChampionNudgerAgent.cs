using Microsoft.Extensions.Logging;

namespace AgencyLayer.ConvenerAgents.ChampionNudger;
// --- Supporting Interfaces & Models ---
// Note: In a complete Onion Architecture, these contracts would reside in their respective layers
// (e.g., Core/Interfaces, Application/DTOs) to maintain strict separation of concerns.
// They are included here to make this file self-contained and understandable.

/// <summary>
/// Defines the contract for a service that can send notifications.
/// This would be implemented in the Infrastructure layer.
/// </summary>
public interface INotificationService
{
    Task SendNotificationAsync(string userId, string subject, string message);
}

/// <summary>
/// Defines the contract for a service that interacts with collaboration platforms.
/// </summary>
public interface ICollaborationPlatformService
{
    Task<string> CreateCollaborationSpaceAsync(string spaceName, IEnumerable<string> memberUserIds);
}

/// <summary>
/// Defines the contract for a service that checks user consent for automated actions.
/// </summary>
public interface IConsentService
{
    Task<bool> HasConsentAsync(string userId, string consentType);
}

/// <summary>
/// Base class for all community events handled by the agent.
/// </summary>
public abstract class CommunityEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
    public string TenantId { get; set; }
}

/// <summary>
/// Published when a new project requires a champion with a specific skill.
/// </summary>
public class ProjectNeedsChampionEvent : CommunityEvent
{
    public string ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string RequiredSkill { get; set; }
    public string RequestingUserId { get; set; }
    public IEnumerable<string> PotentialChampionUserIds { get; set; }
}

/// <summary>
/// Published when a user requests collaboration with a champion.
/// </summary>
public class CollaborationRequestedEvent : CommunityEvent
{
    public string RequestingUserId { get; set; }
    public string ChampionUserId { get; set; }
    public string ContextMessage { get; set; }
}

/// <summary>
/// Published when a potential psychological safety concern is detected.
/// </summary>
public class SafetyConcernDetectedEvent : CommunityEvent
{
    public string ChannelId { get; set; }
    public string ConcernDetails { get; set; }
    public string EscalationTargetUserId { get; set; } // e.g., HR or community manager
}


// --- Agent Implementation ---
/// <summary>
/// An autonomous agent responsible for event-driven automation related to champion engagement.
/// It listens for community events and triggers workflows based on predefined rules and user consent.
/// </summary>
public class ChampionNudgerAgent
{
    private readonly ILogger<ChampionNudgerAgent> _logger;
    private readonly INotificationService _notificationService;
    private readonly ICollaborationPlatformService _collaborationPlatformService;
    private readonly IConsentService _consentService;

    public ChampionNudgerAgent(
        ILogger<ChampionNudgerAgent> logger,
        INotificationService notificationService,
        ICollaborationPlatformService collaborationPlatformService,
        IConsentService consentService)
    {
        _logger = logger;
        _notificationService = notificationService;
        _collaborationPlatformService = collaborationPlatformService;
        _consentService = consentService;
    }

    /// <summary>
    /// The main entry point for processing incoming community events.
    /// This method would typically be called by a message queue subscriber (e.g., Azure Service Bus).
    /// </summary>
    /// <param name="communityEvent">The event to process.</param>
    public async Task HandleCommunityEventAsync(CommunityEvent communityEvent)
    {
        _logger.LogInformation("ChampionNudgerAgent received event {EventId} of type {EventType}.", communityEvent.EventId, communityEvent.GetType().Name);

        // Use pattern matching to route the event to the correct handler.
        switch (communityEvent)
        {
            case ProjectNeedsChampionEvent e:
                await HandleProjectNeedsChampionAsync(e);
                break;
            case CollaborationRequestedEvent e:
                await HandleCollaborationRequestedAsync(e);
                break;
            case SafetyConcernDetectedEvent e:
                await HandleSafetyConcernDetectedAsync(e);
                break;
            default:
                _logger.LogWarning("Unhandled event type: {EventType}", communityEvent.GetType().Name);
                break;
        }
    }

    /// <summary>
    /// Handles events where a project needs champions with a specific skill.
    /// It notifies potential champions if they have consented to be contacted.
    /// </summary>
    private async Task HandleProjectNeedsChampionAsync(ProjectNeedsChampionEvent e)
    {
        const string consentType = "NotifyOnProjectOpportunities";
        _logger.LogInformation("Handling ProjectNeedsChampionEvent for project '{ProjectName}'. Notifying {ChampionCount} potential champions.", e.ProjectName, e.PotentialChampionUserIds.Count());

        foreach (var championId in e.PotentialChampionUserIds)
        {
            if (await _consentService.HasConsentAsync(championId, consentType))
            {
                var subject = $"Opportunity: Your expertise in '{e.RequiredSkill}' is needed!";
                var message = $"Hello! The project '{e.ProjectName}' is looking for a champion with your skills in '{e.RequiredSkill}'. Please reach out to user {e.RequestingUserId} if you are interested in helping.";
                    
                await _notificationService.SendNotificationAsync(championId, subject, message);
                _logger.LogInformation("Notified champion {ChampionId} about project '{ProjectName}'.", championId, e.ProjectName);
            }
            else
            {
                _logger.LogInformation("Skipped notification for champion {ChampionId} due to lack of consent for '{ConsentType}'.", championId, consentType);
            }
        }
    }

    /// <summary>
    /// Handles events where one user requests to collaborate with another.
    /// It creates a collaboration space if both parties have consented.
    /// </summary>
    private async Task HandleCollaborationRequestedAsync(CollaborationRequestedEvent e)
    {
        const string consentType = "AutoCreateCollaborationSpaces";
        _logger.LogInformation("Handling CollaborationRequestedEvent from {RequestingUser} to {ChampionUser}.", e.RequestingUserId, e.ChampionUserId);

        bool requesterHasConsented = await _consentService.HasConsentAsync(e.RequestingUserId, consentType);
        bool championHasConsented = await _consentService.HasConsentAsync(e.ChampionUserId, consentType);

        if (requesterHasConsented && championHasConsented)
        {
            var spaceName = $"collab-{e.RequestingUserId}-{e.ChampionUserId}-{DateTime.UtcNow:yyyyMMdd}";
            var members = new List<string> { e.RequestingUserId, e.ChampionUserId };
                
            var spaceUrl = await _collaborationPlatformService.CreateCollaborationSpaceAsync(spaceName, members);
            _logger.LogInformation("Created collaboration space '{SpaceName}' for users {RequestingUser} and {ChampionUser}. URL: {SpaceUrl}", spaceName, e.RequestingUserId, e.ChampionUserId, spaceUrl);

            // Notify both users about the new space.
            var message = $"A collaboration space has been created for you regarding your request: {e.ContextMessage}. You can access it here: {spaceUrl}";
            await _notificationService.SendNotificationAsync(e.RequestingUserId, "Collaboration Space Created", message);
            await _notificationService.SendNotificationAsync(e.ChampionUserId, "Collaboration Space Created", message);
        }
        else
        {
            _logger.LogWarning("Could not create collaboration space due to lack of consent. Requester Consent: {RequesterConsent}, Champion Consent: {ChampionConsent}", requesterHasConsented, championHasConsented);
        }
    }

    /// <summary>
    /// Handles events related to psychological safety concerns.
    /// It escalates the concern by notifying a designated person (e.g., HR or admin).
    /// </summary>
    private async Task HandleSafetyConcernDetectedAsync(SafetyConcernDetectedEvent e)
    {
        // Consent check might be waived for critical safety escalations, depending on policy.
        // For this example, we assume escalation is a mandatory process.
        _logger.LogWarning("Handling SafetyConcernDetectedEvent for channel '{ChannelId}'. Escalating to {EscalationTarget}.", e.ChannelId, e.EscalationTargetUserId);

        var subject = $"[URGENT] Psychological Safety Concern Detected in Channel: {e.ChannelId}";
        var message = $"A potential psychological safety concern was flagged in channel {e.ChannelId}. Details: '{e.ConcernDetails}'. Please review immediately.";

        await _notificationService.SendNotificationAsync(e.EscalationTargetUserId, subject, message);
        _logger.LogInformation("Escalation notification sent to {EscalationTarget}.", e.EscalationTargetUserId);
    }
}