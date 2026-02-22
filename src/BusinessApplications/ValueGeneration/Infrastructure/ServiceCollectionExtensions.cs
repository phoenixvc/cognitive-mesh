using CognitiveMesh.BusinessApplications.ValueGeneration.Adapters;
using CognitiveMesh.ReasoningLayer.ValueGeneration.Engines;
using CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace CognitiveMesh.BusinessApplications.ValueGeneration.Infrastructure;

/// <summary>
/// Extension methods for registering Value Generation services in the
/// dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all Value Generation services to the specified
    /// <see cref="IServiceCollection"/>, including the reasoning-layer
    /// engines, in-memory repository adapters, and supporting
    /// infrastructure required by the Value Generation subsystem.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddValueGenerationServices(this IServiceCollection services)
    {
        // --- Reasoning-layer engine registrations (port -> engine) ---
        services.AddScoped<IValueDiagnosticPort, ValueGenerationDiagnosticEngine>();
        services.AddScoped<IOrgBlindnessDetectionPort, OrganizationalValueBlindnessEngine>();
        services.AddScoped<IEmployabilityPort, EmployabilityPredictorEngine>();

        // --- In-memory repository adapters (engine dependencies) ---
        services.AddSingleton<IValueDiagnosticDataRepository, InMemoryValueDiagnosticDataRepository>();
        services.AddSingleton<IOrganizationalDataRepository, InMemoryOrganizationalDataRepository>();
        services.AddSingleton<IEmployabilityDataRepository, InMemoryEmployabilityDataRepository>();

        // --- In-memory adapters for consent and manual review (engine dependencies) ---
        services.AddSingleton<IConsentVerifier, InMemoryConsentVerifier>();
        services.AddSingleton<IManualReviewRequester, InMemoryManualReviewRequester>();

        return services;
    }
}
