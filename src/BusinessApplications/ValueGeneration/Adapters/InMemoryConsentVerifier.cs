using System.Collections.Concurrent;
using CognitiveMesh.ReasoningLayer.ValueGeneration.Engines;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.BusinessApplications.ValueGeneration.Adapters;

/// <summary>
/// In-memory implementation of <see cref="IConsentVerifier"/>
/// for development, testing, and local environments.  Consent decisions
/// are held in a <see cref="ConcurrentDictionary{TKey,TValue}"/> and default
/// to <c>true</c> when no explicit record exists.
/// </summary>
public class InMemoryConsentVerifier : IConsentVerifier
{
    private readonly ILogger<InMemoryConsentVerifier> _logger;
    private readonly ConcurrentDictionary<string, bool> _consents = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryConsentVerifier"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public InMemoryConsentVerifier(ILogger<InMemoryConsentVerifier> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<bool> VerifyConsentExistsAsync(string userId, string tenantId, string consentType)
    {
        var key = $"{tenantId}:{userId}:{consentType}";
        var hasConsent = _consents.GetOrAdd(key, true);

        _logger.LogDebug(
            "Consent check for user '{UserId}', type '{ConsentType}': {HasConsent}.",
            userId, consentType, hasConsent);

        return Task.FromResult(hasConsent);
    }

    /// <summary>
    /// Seeds the in-memory consent store with an explicit decision.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="consentType">The consent type.</param>
    /// <param name="granted">Whether consent is granted.</param>
    public void Seed(string userId, string tenantId, string consentType, bool granted)
    {
        var key = $"{tenantId}:{userId}:{consentType}";
        _consents[key] = granted;
    }
}
