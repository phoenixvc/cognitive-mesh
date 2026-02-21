using AgencyLayer.CognitiveSovereignty.Models;

namespace AgencyLayer.CognitiveSovereignty.Ports;

/// <summary>
/// Defines the primary port for managing cognitive sovereignty profiles and modes.
/// This port handles profile CRUD operations and sovereignty mode resolution
/// following hexagonal architecture conventions.
/// </summary>
public interface ISovereigntyPort
{
    /// <summary>
    /// Retrieves the sovereignty profile for a given user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The user's sovereignty profile, or <c>null</c> if no profile exists.</returns>
    Task<SovereigntyProfile?> GetProfileAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing sovereignty profile with the provided values.
    /// </summary>
    /// <param name="profile">The updated profile.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated profile.</returns>
    Task<SovereigntyProfile> UpdateProfileAsync(SovereigntyProfile profile, CancellationToken ct = default);

    /// <summary>
    /// Sets the sovereignty mode for a user, optionally specifying a domain.
    /// When a domain is specified, sets a domain-specific override.
    /// When no domain is specified, sets the default mode.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="mode">The sovereignty mode to set.</param>
    /// <param name="domain">Optional domain for domain-specific mode. <c>null</c> sets the default mode.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated sovereignty profile.</returns>
    Task<SovereigntyProfile> SetModeAsync(string userId, SovereigntyMode mode, string? domain = null, CancellationToken ct = default);

    /// <summary>
    /// Resolves the effective sovereignty mode for a user in a given context.
    /// Checks active overrides first, then domain-specific rules, then the default mode.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="domain">Optional domain to consider for domain-specific resolution.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The currently effective sovereignty mode.</returns>
    Task<SovereigntyMode> GetCurrentModeAsync(string userId, string? domain = null, CancellationToken ct = default);
}
