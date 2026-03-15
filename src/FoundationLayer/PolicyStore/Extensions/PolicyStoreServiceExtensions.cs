using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using CognitiveMesh.FoundationLayer.PolicyStore.Adapters;
using CognitiveMesh.FoundationLayer.PolicyStore.Options;
using CognitiveMesh.FoundationLayer.PolicyStore.Ports;
using CognitiveMesh.FoundationLayer.PolicyStore.Seed;

namespace CognitiveMesh.FoundationLayer.PolicyStore.Extensions;

/// <summary>
/// Extension methods for registering PolicyStore services with the dependency injection container.
/// </summary>
public static class PolicyStoreServiceExtensions
{
    /// <summary>
    /// Adds the policy store services to the specified <see cref="IServiceCollection"/>,
    /// including Cosmos DB–backed policy persistence, in-memory caching, and seed initialisation.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration used to bind <see cref="PolicyStoreOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddPolicyStore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<PolicyStoreOptions>(configuration.GetSection(nameof(PolicyStoreOptions)));
        services.AddMemoryCache();
        services.AddSingleton<IRemediationPolicyPort, CosmosDbRemediationPolicyAdapter>();
        services.AddTransient<PolicyStoreInitializer>();

        return services;
    }
}
