namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - UX Pattern
// Reason: Current identity model is sufficient
// Reconsideration: If decentralized identity becomes a requirement
// ============================================================================

/// <summary>
/// Soulbound token.
/// </summary>
public class SoulboundToken
{
    /// <summary>Token identifier.</summary>
    public required string TokenId { get; init; }

    /// <summary>Owner.</summary>
    public required string Owner { get; init; }

    /// <summary>Claims.</summary>
    public IReadOnlyList<string> Claims { get; init; } = Array.Empty<string>();

    /// <summary>Issued at.</summary>
    public DateTimeOffset IssuedAt { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for soulbound identity verification.
/// Implements the "Soulbound Identity Verification" pattern.
///
/// This is a low-priority pattern because the current identity
/// model is sufficient for existing use cases.
/// </summary>
public interface ISoulboundIdentityPort
{
    /// <summary>Issues soulbound token.</summary>
    Task<SoulboundToken> IssueTokenAsync(string owner, IEnumerable<string> claims, CancellationToken cancellationToken = default);

    /// <summary>Verifies token.</summary>
    Task<bool> VerifyAsync(string tokenId, CancellationToken cancellationToken = default);

    /// <summary>Gets token.</summary>
    Task<SoulboundToken?> GetTokenAsync(string tokenId, CancellationToken cancellationToken = default);
}
