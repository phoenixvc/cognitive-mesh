using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// Configuration options for memory stores.
    /// </summary>
    public class MemoryStoreOptions
    {
        public string StoreType { get; set; } = "hybrid";
        public string DuckDbFilePath { get; set; } = "data/mesh_memory.duckdb";
        public string RedisConnectionString { get; set; } = "localhost:6379";
        public bool PreferRedisForRetrieval { get; set; } = true;
    }

    /// <summary>
    /// Factory for creating memory store instances based on configuration.
    /// </summary>
    public static class MemoryStoreFactory
    {
        /// <summary>
        /// Registers memory store services with the dependency injection container.
        /// </summary>
        public static IServiceCollection AddMeshMemoryStores(this IServiceCollection services, IConfiguration configuration)
        {
            // Register options
            services.Configure<MemoryStoreOptions>(configuration.GetSection("MemoryStore"));

            // Register DuckDB store
            services.AddSingleton<DuckDbMemoryStore>(provider =>
            {
                var options = provider.GetRequiredService<MemoryStoreOptions>();
                var logger = provider.GetRequiredService<ILogger<DuckDbMemoryStore>>();
                return new DuckDbMemoryStore(options.DuckDbFilePath, logger);
            });

            // Register the vector search provider
            services.AddSingleton<IVectorSearchProvider>(provider =>
            {
                var options = provider.GetRequiredService<MemoryStoreOptions>();
                var logger = provider.GetRequiredService<ILogger<RedisVectorSearchProvider>>();
                return new RedisVectorSearchProvider(options.RedisConnectionString, logger);
            });

            // Register Redis store
            services.AddSingleton<RedisVectorMemoryStore>(provider =>
            {
                var vectorSearch = provider.GetRequiredService<IVectorSearchProvider>();
                var logger = provider.GetRequiredService<ILogger<RedisVectorMemoryStore>>();
                return new RedisVectorMemoryStore(vectorSearch, logger);
            });

            // Register the appropriate implementation as IMeshMemoryStore
            services.AddSingleton<IMeshMemoryStore>(provider =>
            {
                var options = provider.GetRequiredService<MemoryStoreOptions>();
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

                switch (options.StoreType.ToLower())
                {
                    case "duckdb":
                        return provider.GetRequiredService<DuckDbMemoryStore>();
                    
                    case "redis":
                        return provider.GetRequiredService<RedisVectorMemoryStore>();
                    
                    case "hybrid":
                    default:
                        var duckDb = provider.GetRequiredService<DuckDbMemoryStore>();
                        var redis = provider.GetRequiredService<RedisVectorMemoryStore>();
                        var logger = loggerFactory.CreateLogger<HybridMemoryStore>();
                        return new HybridMemoryStore(duckDb, redis, logger, options.PreferRedisForRetrieval);
                }
            });

            return services;
        }
    }
}