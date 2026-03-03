namespace AgencyLayer.CognitiveSovereignty.Models;

/// <summary>
/// Represents a user's sovereignty preferences, defining how agentic autonomy
/// is balanced with human control across domains and tasks.
/// </summary>
public class SovereigntyProfile
{
    /// <summary>
    /// Unique identifier for this profile.
    /// </summary>
    public string ProfileId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identifier of the user this profile belongs to.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the tenant this profile belongs to, for multi-tenancy isolation.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Default sovereignty mode used when no domain-specific override applies.
    /// </summary>
    public SovereigntyMode DefaultMode { get; set; } = SovereigntyMode.GuidedAutonomy;

    /// <summary>
    /// Domain-specific sovereignty mode overrides. Keys are domain names
    /// (e.g., "financial", "creative", "routine") and values are the mode for that domain.
    /// </summary>
    public Dictionary<string, SovereigntyMode> DomainOverrides { get; set; } = new();

    /// <summary>
    /// Timestamp when the profile was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the profile was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
