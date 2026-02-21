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
        /// <summary>
        /// The primary store type: "hybrid", "redis", "sqlite", or "duckdb" (legacy).
        /// </summary>
        public string StoreType { get; set; } = "hybrid";

        /// <summary>
        /// Path to the SQLite database file for persistent storage.
        /// </summary>
        public string SqliteDbPath { get; set; } = "data/mesh_memory.db";

        /// <summary>
        /// Path to the DuckDB database file (legacy, use SqliteDbPath instead).
        /// </summary>
        public string DuckDbFilePath { get; set; } = "data/mesh_memory.duckdb";

        /// <summary>
        /// Redis connection string for cache and vector search.
        /// </summary>
        public string RedisConnectionString { get; set; } = "localhost:6379";

        /// <summary>
        /// Whether to prefer Redis for context retrieval in hybrid mode.
        /// </summary>
        public bool PreferRedisForRetrieval { get; set; } = true;

        /// <summary>
        /// The vector search provider: "redis" or "qdrant".
        /// </summary>
        public string VectorSearchProvider { get; set; } = "redis";

        /// <summary>
        /// Qdrant server hostname.
        /// </summary>
        public string QdrantHost { get; set; } = "localhost";

        /// <summary>
        /// Qdrant gRPC port.
        /// </summary>
        public int QdrantPort { get; set; } = 6334;

        /// <summary>
        /// Qdrant collection name for embeddings.
        /// </summary>
        public string QdrantCollectionName { get; set; } = "mesh_embeddings";

        /// <summary>
        /// Dimension of embedding vectors (must match the embedding model).
        /// </summary>
        public int VectorDimension { get; set; } = 768;
    }

    /// <summary>
    /// Factory for creating memory store instances based on configuration.
    /// Supports SQLite (default persistent), Redis (cache), Qdrant (vector search),
    /// and hybrid combinations.
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

            // Register SQLite store
            services.AddSingleton<SqliteMemoryStore>(provider =>
            {
                var options = provider.GetRequiredService<MemoryStoreOptions>();
                var logger = provider.GetRequiredService<ILogger<SqliteMemoryStore>>();
                return new SqliteMemoryStore(options.SqliteDbPath, logger);
            });

            // Register DuckDB store (legacy)
            services.AddSingleton<DuckDbMemoryStore>(provider =>
            {
                var options = provider.GetRequiredService<MemoryStoreOptions>();
                var logger = provider.GetRequiredService<ILogger<DuckDbMemoryStore>>();
                return new DuckDbMemoryStore(options.DuckDbFilePath, logger);
            });

            // Register vector search provider based on configuration
            services.AddSingleton<IVectorSearchProvider>(provider =>
            {
                var options = provider.GetRequiredService<MemoryStoreOptions>();

                return options.VectorSearchProvider.ToLower() switch
                {
                    "qdrant" => new QdrantVectorSearchProvider(
                        options.QdrantHost,
                        options.QdrantPort,
                        options.QdrantCollectionName,
                        options.VectorDimension,
                        provider.GetRequiredService<ILogger<QdrantVectorSearchProvider>>()),

                    _ => new RedisVectorSearchProvider(
                        options.RedisConnectionString,
                        provider.GetRequiredService<ILogger<RedisVectorSearchProvider>>())
                };
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
                    case "sqlite":
                        return provider.GetRequiredService<SqliteMemoryStore>();

                    case "duckdb":
                        return provider.GetRequiredService<DuckDbMemoryStore>();

                    case "redis":
                        return provider.GetRequiredService<RedisVectorMemoryStore>();

                    case "hybrid":
                    default:
                        var sqlite = provider.GetRequiredService<SqliteMemoryStore>();
                        var redis = provider.GetRequiredService<RedisVectorMemoryStore>();
                        var logger = loggerFactory.CreateLogger<HybridMemoryStore>();
                        return new HybridMemoryStore(sqlite, redis, logger, options.PreferRedisForRetrieval);
                }
            });

            return services;
        }
    }
}
