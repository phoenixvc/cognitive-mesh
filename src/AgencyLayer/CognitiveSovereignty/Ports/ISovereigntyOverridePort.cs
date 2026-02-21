using AgencyLayer.CognitiveSovereignty.Models;

namespace AgencyLayer.CognitiveSovereignty.Ports;

/// <summary>
/// Defines the port for managing sovereignty overrides that temporarily
/// change a user's sovereignty mode with expiration and revocation support.
/// </summary>
public interface ISovereigntyOverridePort
{
    /// <summary>
    /// Creates a new sovereignty override for a user, temporarily changing their mode.
    /// </summary>
    /// <param name="override">The override to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created override with its assigned identifier.</returns>
    Task<SovereigntyOverride> CreateOverrideAsync(SovereigntyOverride @override, CancellationToken ct = default);

    /// <summary>
    /// Revokes an active override, restoring the previous sovereignty mode.
    /// </summary>
    /// <param name="overrideId">The unique identifier of the override to revoke.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> if the override was found and revoked; <c>false</c> otherwise.</returns>
    Task<bool> RevokeOverrideAsync(string overrideId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all active (non-revoked, non-expired) overrides for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of active overrides ordered by creation time (newest first).</returns>
    Task<IReadOnlyList<SovereigntyOverride>> GetActiveOverridesAsync(string userId, CancellationToken ct = default);
}
