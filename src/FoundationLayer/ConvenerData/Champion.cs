// --- Value Objects ---
// Note: In a production application, these value objects would each reside in their own file
// within the 'src/ConvenerLayer/Core/ValueObjects/' directory to adhere to clean architecture principles.

namespace FoundationLayer.ConvenerData;

/// <summary>
/// Represents a specific skill possessed by a champion.
/// This is an immutable value object.
/// </summary>
/// <param name="Name">The name of the skill (e.g., "MLOps", "DDD").</param>
/// <param name="Proficiency">An optional score indicating the champion's proficiency level in this skill.</param>
public record Skill(string Name, double? Proficiency = null);

/// <summary>
/// Represents a single, immutable event in the provenance trail of an entity.
/// It explains the "why" behind a state change or data point.
/// </summary>
/// <param name="Source">The system or service that generated the event (e.g., "CommunityPulseService", "ManualEndorsement").</param>
/// <param name="EventType">The type of event that occurred (e.g., "ScoreUpdated", "InteractionLogged").</param>
/// <param name="Details">A description of the event, providing context.</param>
/// <param name="Timestamp">The UTC timestamp when the event occurred.</param>
public record ProvenanceEntry(string Source, string EventType, string Details, DateTimeOffset Timestamp);


// --- Entity ---
/// <summary>
/// Represents a Knowledge Champion within the Cognitive Mesh ecosystem.
/// This is a root aggregate entity following Domain-Driven Design (DDD) principles.
/// Its state is managed through its constructor and public methods to ensure consistency.
/// </summary>
public class Champion
{
    /// <summary>
    /// The unique identifier for the Champion entity itself.
    /// </summary>
    public Guid ChampionId { get; private set; }

    /// <summary>
    /// The identifier for the user in the identity system who is the champion.
    /// </summary>
    public string UserId { get; private set; }

    /// <summary>
    /// The identifier for the tenant this champion belongs to, enforcing data isolation.
    /// </summary>
    public string TenantId { get; private set; }

    /// <summary>
    /// A collection of skills attributed to the champion.
    /// </summary>
    public IReadOnlyCollection<Skill> Skills { get; private set; }

    /// <summary>
    /// A calculated score representing the champion's influence and expertise.
    /// </summary>
    public double InfluenceScore { get; private set; }

    /// <summary>
    /// The total count of significant interactions logged for this champion.
    /// </summary>
    public int InteractionCount { get; private set; }

    /// <summary>
    /// The timestamp of the champion's last recorded significant activity.
    /// </summary>
    public DateTimeOffset LastActiveDate { get; private set; }

    /// <summary>
    /// A read-only collection of events that form the audit trail for this champion's data.
    /// </summary>
    public IReadOnlyCollection<ProvenanceEntry> Provenance { get; private set; }

    /// <summary>
    /// Private constructor for ORM/persistence frameworks.
    /// </summary>
    private Champion() 
    {
        // Initialize collections to prevent null reference exceptions
        Skills = new List<Skill>();
        Provenance = new List<ProvenanceEntry>();
        UserId = string.Empty;
        TenantId = string.Empty;
    }

    /// <summary>
    /// Creates a new instance of a Champion.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="tenantId">The tenant's unique identifier.</param>
    /// <param name="initialSkills">An initial collection of skills.</param>
    /// <param name="initialProvenance">The initial provenance entry explaining the champion's creation.</param>
    public Champion(string userId, string tenantId, IEnumerable<Skill> initialSkills, ProvenanceEntry initialProvenance)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or whitespace.", nameof(userId));
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be null or whitespace.", nameof(tenantId));
        if (initialProvenance == null)
            throw new ArgumentNullException(nameof(initialProvenance), "Initial provenance is required to create a champion.");

        ChampionId = Guid.NewGuid();
        UserId = userId;
        TenantId = tenantId;
        Skills = new List<Skill>(initialSkills ?? Enumerable.Empty<Skill>());
        Provenance = new List<ProvenanceEntry> { initialProvenance };
        LastActiveDate = initialProvenance.Timestamp;
        InteractionCount = 0;
        InfluenceScore = 0.0;
    }

    /// <summary>
    /// Updates the champion's influence score and adds a record to the provenance trail.
    /// </summary>
    /// <param name="newScore">The new influence score.</param>
    /// <param name="reason">The provenance entry explaining the reason for the update.</param>
    public void UpdateInfluenceScore(double newScore, ProvenanceEntry reason)
    {
        if (newScore < 0)
            throw new ArgumentOutOfRangeException(nameof(newScore), "Influence score cannot be negative.");

        InfluenceScore = newScore;
        AddProvenance(reason);
    }

    /// <summary>
    /// Logs a new interaction for the champion, updating their activity timestamp and count.
    /// </summary>
    /// <param name="reason">The provenance entry describing the interaction.</param>
    public void LogInteraction(ProvenanceEntry reason)
    {
        InteractionCount++;
        LastActiveDate = reason.Timestamp;
        AddProvenance(reason);
    }

    /// <summary>
    /// Adds or updates a skill for the champion.
    /// </summary>
    /// <param name="skill">The skill to add or update.</param>
    /// <param name="reason">The provenance entry explaining the change.</param>
    public void AddOrUpdateSkill(Skill skill, ProvenanceEntry reason)
    {
        var skills = Skills.ToList();
        // Remove existing skill with the same name to prevent duplicates
        skills.RemoveAll(s => s.Name.Equals(skill.Name, StringComparison.OrdinalIgnoreCase));
        skills.Add(skill);
        Skills = skills;

        AddProvenance(reason);
    }

    /// <summary>
    /// Adds a new entry to the provenance trail.
    /// </summary>
    /// <param name="entry">The provenance entry to add.</param>
    private void AddProvenance(ProvenanceEntry entry)
    {
        if (entry == null) return;
        var provenance = Provenance.ToList();
        provenance.Add(entry);
        Provenance = provenance;
    }
}