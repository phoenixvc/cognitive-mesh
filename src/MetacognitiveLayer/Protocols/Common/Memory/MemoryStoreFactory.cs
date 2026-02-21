using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// Configuration options for memory stores and vector search providers.
    /// </summary>
    public class MemoryStoreOptions
    {
        /// <summary>
        /// The primary store type: "hybrid", "redis", "sqlite", "postgres", "litedb", "cosmosdb", or "duckdb" (legacy).
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
        /// Path to the LiteDB database file.
        /// </summary>
        public string LiteDbPath { get; set; } = "data/mesh_memory.litedb";

        /// <summary>
        /// Redis connection string for cache and vector search.
        /// </summary>
        public string RedisConnectionString { get; set; } = "localhost:6379";

        /// <summary>
        /// Whether to prefer the cache store for context retrieval in hybrid mode.
        /// </summary>
        public bool PreferRedisForRetrieval { get; set; } = true;

        /// <summary>
        /// The vector search provider: "redis", "qdrant", "milvus", "chroma", or "postgres".
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

        /// <summary>
        /// PostgreSQL connection string (for both store and pgvector search).
        /// </summary>
        public string PostgresConnectionString { get; set; } = "Host=localhost;Database=cognitive_mesh;";

        /// <summary>
        /// Milvus REST API endpoint.
        /// </summary>
        public string MilvusEndpoint { get; set; } = "http://localhost:19530";

        /// <summary>
        /// Milvus collection name.
        /// </summary>
        public string MilvusCollectionName { get; set; } = "mesh_embeddings";

        /// <summary>
        /// Milvus API key (for Zilliz Cloud managed Milvus).
        /// </summary>
        public string? MilvusApiKey { get; set; }

        /// <summary>
        /// ChromaDB REST API endpoint.
        /// </summary>
        public string ChromaEndpoint { get; set; } = "http://localhost:8000";

        /// <summary>
        /// ChromaDB collection name.
        /// </summary>
        public string ChromaCollectionName { get; set; } = "mesh_embeddings";

        /// <summary>
        /// Azure Cosmos DB connection string.
        /// </summary>
        public string CosmosDbConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Azure Cosmos DB database name.
        /// </summary>
        public string CosmosDbDatabaseId { get; set; } = "CognitiveMesh";

        /// <summary>
        /// Azure Cosmos DB container name.
        /// </summary>
        public string CosmosDbContainerId { get; set; } = "MemoryStore";
    }

    /// <summary>
    /// Factory for creating memory store instances based on configuration.
    /// Supports 7 store backends and 5 vector search providers, selectable at runtime.
    /// </summary>
    /// <remarks>
    /// <para><b>Store Types</b> (<c>MemoryStore:StoreType</c>):</para>
    /// <list type="bullet">
    ///   <item><c>hybrid</c> — SQLite persistent + Redis cache (default)</item>
    ///   <item><c>sqlite</c> — Embedded SQLite with ACID transactions</item>
    ///   <item><c>postgres</c> — PostgreSQL with pgvector for production</item>
    ///   <item><c>litedb</c> — Embedded NoSQL document store (pure C#)</item>
    ///   <item><c>cosmosdb</c> — Azure Cosmos DB for global distribution</item>
    ///   <item><c>redis</c> — Redis with vector search</item>
    ///   <item><c>duckdb</c> — DuckDB embedded OLAP (legacy)</item>
    /// </list>
    /// <para><b>Vector Search Providers</b> (<c>MemoryStore:VectorSearchProvider</c>):</para>
    /// <list type="bullet">
    ///   <item><c>redis</c> — Redis Search with HNSW index (default)</item>
    ///   <item><c>qdrant</c> — Qdrant purpose-built vector DB</item>
    ///   <item><c>postgres</c> — pgvector with HNSW index</item>
    ///   <item><c>milvus</c> — Milvus cloud-native vector DB</item>
    ///   <item><c>chroma</c> — ChromaDB AI-native embeddings</item>
    /// </list>
    /// </remarks>
    public static class MemoryStoreFactory
    {
        /// <summary>
        /// Registers memory store services with the dependency injection container.
        /// </summary>
        public static IServiceCollection AddMeshMemoryStores(this IServiceCollection services, IConfiguration configuration)
        {
            // Register options
            services.Configure<MemoryStoreOptions>(configuration.GetSection("MemoryStore"));

            // Register all concrete store implementations
            RegisterSqliteStore(services);
            RegisterDuckDbStore(services);
            RegisterLiteDbStore(services);
            RegisterPostgresStore(services);
            RegisterCosmosDbStore(services);
            RegisterRedisStore(services);
            RegisterVectorSearchProvider(services);

            // Register the appropriate implementation as IMeshMemoryStore
            services.AddSingleton<IMeshMemoryStore>(ResolveMemoryStore);

            return services;
        }

        private static void RegisterSqliteStore(IServiceCollection services)
        {
            services.AddSingleton<SqliteMemoryStore>(provider =>
            {
                var options = provider.GetRequiredService<MemoryStoreOptions>();
                var logger = provider.GetRequiredService<ILogger<SqliteMemoryStore>>();
                return new SqliteMemoryStore(options.SqliteDbPath, logger);
            });
        }

        private static void RegisterDuckDbStore(IServiceCollection services)
        {
            services.AddSingleton<DuckDbMemoryStore>(provider =>
            {
                var options = provider.GetRequiredService<MemoryStoreOptions>();
                var logger = provider.GetRequiredService<ILogger<DuckDbMemoryStore>>();
                return new DuckDbMemoryStore(options.DuckDbFilePath, logger);
            });
        }

        private static void RegisterLiteDbStore(IServiceCollection services)
        {
            services.AddSingleton<LiteDbMemoryStore>(provider =>
            {
                var options = provider.GetRequiredService<MemoryStoreOptions>();
                var logger = provider.GetRequiredService<ILogger<LiteDbMemoryStore>>();
                return new LiteDbMemoryStore(options.LiteDbPath, logger);
            });
        }

        private static void RegisterPostgresStore(IServiceCollection services)
        {
            services.AddSingleton<PostgresMemoryStore>(provider =>
            {
                var options = provider.GetRequiredService<MemoryStoreOptions>();
                var logger = provider.GetRequiredService<ILogger<PostgresMemoryStore>>();
                return new PostgresMemoryStore(
                    options.PostgresConnectionString,
                    options.VectorDimension,
                    logger);
            });
        }

        private static void RegisterCosmosDbStore(IServiceCollection services)
        {
            services.AddSingleton<CosmosDbMemoryStore>(provider =>
            {
                var options = provider.GetRequiredService<MemoryStoreOptions>();
                var logger = provider.GetRequiredService<ILogger<CosmosDbMemoryStore>>();
                return new CosmosDbMemoryStore(
                    options.CosmosDbConnectionString,
                    options.CosmosDbDatabaseId,
                    options.CosmosDbContainerId,
                    logger);
            });
        }

        private static void RegisterRedisStore(IServiceCollection services)
        {
            services.AddSingleton<RedisVectorMemoryStore>(provider =>
            {
                var vectorSearch = provider.GetRequiredService<IVectorSearchProvider>();
                var logger = provider.GetRequiredService<ILogger<RedisVectorMemoryStore>>();
                return new RedisVectorMemoryStore(vectorSearch, logger);
            });
        }

        private static void RegisterVectorSearchProvider(IServiceCollection services)
        {
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

                    "postgres" => provider.GetRequiredService<PostgresMemoryStore>(),

                    "milvus" => new MilvusVectorSearchProvider(
                        options.MilvusEndpoint,
                        options.MilvusCollectionName,
                        options.VectorDimension,
                        provider.GetRequiredService<ILogger<MilvusVectorSearchProvider>>(),
                        options.MilvusApiKey),

                    "chroma" => new ChromaVectorSearchProvider(
                        options.ChromaEndpoint,
                        options.ChromaCollectionName,
                        provider.GetRequiredService<ILogger<ChromaVectorSearchProvider>>()),

                    _ => new RedisVectorSearchProvider(
                        options.RedisConnectionString,
                        provider.GetRequiredService<ILogger<RedisVectorSearchProvider>>())
                };
            });
        }

        private static IMeshMemoryStore ResolveMemoryStore(IServiceProvider provider)
        {
            var options = provider.GetRequiredService<MemoryStoreOptions>();
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

            switch (options.StoreType.ToLower())
            {
                case "sqlite":
                    return provider.GetRequiredService<SqliteMemoryStore>();

                case "postgres":
                    return provider.GetRequiredService<PostgresMemoryStore>();

                case "litedb":
                    return provider.GetRequiredService<LiteDbMemoryStore>();

                case "cosmosdb":
                    return provider.GetRequiredService<CosmosDbMemoryStore>();

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
        }
    }
}
