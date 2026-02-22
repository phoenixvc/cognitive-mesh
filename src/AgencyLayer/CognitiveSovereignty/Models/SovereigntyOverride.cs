namespace AgencyLayer.CognitiveSovereignty.Models;

/// <summary>
/// Represents an active override that temporarily changes a user's sovereignty mode,
/// with an expiration time and reason for the override.
/// </summary>
public class SovereigntyOverride
{
    /// <summary>
    /// Unique identifier for this override.
    /// </summary>
    public string OverrideId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identifier of the user this override applies to.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable reason for activating this override.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// The sovereignty mode that was active before this override.
    /// </summary>
    public SovereigntyMode PreviousMode { get; set; }

    /// <summary>
    /// The sovereignty mode imposed by this override.
    /// </summary>
    public SovereigntyMode NewMode { get; set; }

    /// <summary>
    /// When this override expires and the previous mode is restored.
    /// A <c>null</c> value indicates the override does not expire automatically.
    /// </summary>
    public DateTime? Expiry { get; set; }

    /// <summary>
    /// Timestamp when this override was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates whether this override has been revoked before its expiry.
    /// </summary>
    public bool IsRevoked { get; set; }
}
