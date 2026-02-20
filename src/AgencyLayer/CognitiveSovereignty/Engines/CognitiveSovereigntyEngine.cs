using System.Collections.Concurrent;
using AgencyLayer.CognitiveSovereignty.Models;
using AgencyLayer.CognitiveSovereignty.Ports;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.CognitiveSovereignty.Engines;

/// <summary>
/// Core engine implementing the Cognitive Sovereignty framework.
/// Manages sovereignty profiles, mode resolution (overrides, domain rules, defaults),
/// and autonomy level calculation based on the agency-sovereignty spectrum.
/// </summary>
public class CognitiveSovereigntyEngine : ISovereigntyPort
{
    private readonly ISovereigntyOverridePort _overridePort;
    private readonly ILogger<CognitiveSovereigntyEngine> _logger;

    private readonly ConcurrentDictionary<string, SovereigntyProfile> _profiles = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CognitiveSovereigntyEngine"/> class.
    /// </summary>
    /// <param name="overridePort">Port for managing sovereignty overrides.</param>
    /// <param name="logger">Logger instance.</param>
    public CognitiveSovereigntyEngine(
        ISovereigntyOverridePort overridePort,
        ILogger<CognitiveSovereigntyEngine> logger)
    {
        _overridePort = overridePort ?? throw new ArgumentNullException(nameof(overridePort));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<SovereigntyProfile?> GetProfileAsync(string userId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        _profiles.TryGetValue(userId, out var profile);

        _logger.LogDebug(
            "Retrieved sovereignty profile for user {UserId}: {Found}",
            userId, profile != null);

        return Task.FromResult(profile);
    }

    /// <inheritdoc />
    public Task<SovereigntyProfile> UpdateProfileAsync(SovereigntyProfile profile, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentException.ThrowIfNullOrWhiteSpace(profile.UserId);

        if (!_profiles.ContainsKey(profile.UserId))
        {
            throw new InvalidOperationException($"Profile for user '{profile.UserId}' not found.");
        }

        profile.UpdatedAt = DateTime.UtcNow;
        _profiles[profile.UserId] = profile;

        _logger.LogInformation(
            "Updated sovereignty profile for user {UserId}, default mode: {DefaultMode}",
            profile.UserId, profile.DefaultMode);

        return Task.FromResult(profile);
    }

    /// <inheritdoc />
    public Task<SovereigntyProfile> SetModeAsync(string userId, SovereigntyMode mode, string? domain = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var profile = _profiles.GetOrAdd(userId, _ => new SovereigntyProfile
        {
            UserId = userId,
            DefaultMode = SovereigntyMode.GuidedAutonomy
        });

        var previousMode = domain != null && profile.DomainOverrides.TryGetValue(domain, out var domainMode)
            ? domainMode
            : profile.DefaultMode;

        if (domain != null)
        {
            profile.DomainOverrides[domain] = mode;

            _logger.LogInformation(
                "Set sovereignty mode for user {UserId} in domain '{Domain}': {PreviousMode} -> {NewMode}",
                userId, domain, previousMode, mode);
        }
        else
        {
            profile.DefaultMode = mode;

            _logger.LogInformation(
                "Set default sovereignty mode for user {UserId}: {PreviousMode} -> {NewMode}",
                userId, previousMode, mode);
        }

        profile.UpdatedAt = DateTime.UtcNow;

        return Task.FromResult(profile);
    }

    /// <inheritdoc />
    public async Task<SovereigntyMode> GetCurrentModeAsync(string userId, string? domain = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        // Priority 1: Check active overrides (newest first)
        var overrides = await _overridePort.GetActiveOverridesAsync(userId, ct);
        if (overrides.Count > 0)
        {
            var activeOverride = overrides[0]; // Most recent active override
            _logger.LogDebug(
                "Resolved sovereignty mode for user {UserId} from override {OverrideId}: {Mode}",
                userId, activeOverride.OverrideId, activeOverride.NewMode);

            return activeOverride.NewMode;
        }

        // Priority 2: Check domain-specific rules
        if (!_profiles.TryGetValue(userId, out var profile))
        {
            _logger.LogDebug(
                "No sovereignty profile for user {UserId}, returning default GuidedAutonomy",
                userId);
            return SovereigntyMode.GuidedAutonomy;
        }

        if (domain != null && profile.DomainOverrides.TryGetValue(domain, out var domainMode))
        {
            _logger.LogDebug(
                "Resolved sovereignty mode for user {UserId} from domain '{Domain}': {Mode}",
                userId, domain, domainMode);

            return domainMode;
        }

        // Priority 3: Default mode
        _logger.LogDebug(
            "Resolved sovereignty mode for user {UserId} from default: {Mode}",
            userId, profile.DefaultMode);

        return profile.DefaultMode;
    }

    /// <summary>
    /// Calculates the autonomy level (0.0 to 1.0) for a given sovereignty mode.
    /// </summary>
    /// <param name="mode">The sovereignty mode to calculate the autonomy level for.</param>
    /// <returns>The autonomy level as a double between 0.0 and 1.0.</returns>
    public static double CalculateAutonomyLevel(SovereigntyMode mode)
    {
        return mode switch
        {
            SovereigntyMode.FullAutonomy => 1.0,
            SovereigntyMode.GuidedAutonomy => 0.75,
            SovereigntyMode.CoAuthorship => 0.5,
            SovereigntyMode.HumanLed => 0.25,
            SovereigntyMode.FullManual => 0.0,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown sovereignty mode.")
        };
    }
}
