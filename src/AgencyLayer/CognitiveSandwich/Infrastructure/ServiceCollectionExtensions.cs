using AgencyLayer.CognitiveSandwich.Adapters;
using AgencyLayer.CognitiveSandwich.Engines;
using AgencyLayer.CognitiveSandwich.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace AgencyLayer.CognitiveSandwich.Infrastructure;

/// <summary>
/// Provides extension methods for registering Cognitive Sandwich services
/// with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Cognitive Sandwich services including the engine, adapters,
    /// and in-memory implementations for development scenarios.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCognitiveSandwichServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register in-memory adapters as singletons (shared state across scoped engine instances)
        services.AddSingleton<ICognitiveDebtPort, InMemoryCognitiveDebtAdapter>();
        services.AddSingleton<IPhaseConditionPort, InMemoryPhaseConditionAdapter>();
        services.AddSingleton<IAuditLoggingAdapter, InMemoryAuditLoggingAdapter>();

        // Register the engine as scoped (one per request)
        services.AddScoped<IPhaseManagerPort, CognitiveSandwichEngine>();

        return services;
    }
}
