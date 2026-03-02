using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// User-level storage preferences supporting polyglot persistence,
    /// where multiple database backends can be active simultaneously
    /// with different roles (primary, cache, vector search, analytics).
    /// </summary>
    public class UserStoragePreferences
    {
        /// <summary>
        /// Unique user identifier.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Primary memory store for context persistence.
        /// </summary>
        public string PrimaryStore { get; set; } = "sqlite";

        /// <summary>
        /// Optional cache layer store (e.g., Redis) for low-latency reads.
        /// Set to null to disable caching.
        /// </summary>
        public string? CacheStore { get; set; } = "redis";

        /// <summary>
        /// Vector search provider for semantic similarity queries.
        /// </summary>
        public string VectorSearchProvider { get; set; } = "qdrant";

        /// <summary>
        /// Optional analytics/OLAP store for aggregation queries.
        /// </summary>
        public string? AnalyticsStore { get; set; }

        /// <summary>
        /// Whether hybrid mode is enabled (dual-write to primary + cache).
        /// </summary>
        public bool EnableHybridMode { get; set; } = true;

        /// <summary>
        /// Whether to prefer cache for retrieval when hybrid mode is active.
        /// </summary>
        public bool PreferCacheForRetrieval { get; set; } = true;

        /// <summary>
        /// Whether the user has completed the initial setup wizard.
        /// </summary>
        public bool SetupCompleted { get; set; }

        /// <summary>
        /// Whether the user has completed the UI guided tour.
        /// </summary>
        public bool TourCompleted { get; set; }

        /// <summary>
        /// Provider-specific connection settings, keyed by store type.
        /// </summary>
        public Dictionary<string, ProviderConnectionSettings> ConnectionSettings { get; set; } = new();

        /// <summary>
        /// Timestamp of the last preference update.
        /// </summary>
        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Connection settings for a specific storage provider.
    /// </summary>
    public class ProviderConnectionSettings
    {
        /// <summary>
        /// Connection string or endpoint URL.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Optional database/collection name.
        /// </summary>
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Optional API key (stored securely, not exposed in UI).
        /// </summary>
        [JsonIgnore]
        public string? ApiKey { get; set; }

        /// <summary>
        /// Vector embedding dimension (relevant for vector providers).
        /// </summary>
        public int VectorDimension { get; set; } = 768;
    }

    /// <summary>
    /// Metadata describing a storage provider's capabilities, used to render
    /// the setup wizard's pros/cons comparison UI.
    /// </summary>
    public class StorageProviderInfo
    {
        /// <summary>
        /// Machine-readable provider key (e.g., "sqlite", "postgres", "qdrant").
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable display name.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Short description of the provider.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Provider category: "store" for IMeshMemoryStore, "vector" for IVectorSearchProvider.
        /// </summary>
        public string Category { get; set; } = "store";

        /// <summary>
        /// List of advantages.
        /// </summary>
        public List<string> Pros { get; set; } = new();

        /// <summary>
        /// List of disadvantages or limitations.
        /// </summary>
        public List<string> Cons { get; set; } = new();

        /// <summary>
        /// Recommended use-case labels (e.g., "Development", "Production", "Cloud").
        /// </summary>
        public List<string> RecommendedFor { get; set; } = new();

        /// <summary>
        /// Whether this provider requires an external server or service.
        /// </summary>
        public bool RequiresExternalService { get; set; }

        /// <summary>
        /// Weighted score from the ADR-001 decision matrix (0-5).
        /// </summary>
        public double DecisionScore { get; set; }

        /// <summary>
        /// Whether this provider also implements IVectorSearchProvider (dual-use).
        /// </summary>
        public bool SupportsVectorSearch { get; set; }
    }

    /// <summary>
    /// Static registry of all available storage providers with their metadata.
    /// </summary>
    public static class StorageProviderRegistry
    {
        /// <summary>
        /// Returns metadata for all available IMeshMemoryStore implementations.
        /// </summary>
        public static IReadOnlyList<StorageProviderInfo> GetMemoryStoreProviders() => new List<StorageProviderInfo>
        {
            new()
            {
                Key = "sqlite",
                DisplayName = "SQLite",
                Description = "Embedded relational database with ACID transactions and WAL mode. Zero-configuration, file-based storage.",
                Category = "store",
                Pros = new() { "Zero configuration needed", "ACID transactions", "File-based — easy backup", "Built into .NET runtime", "Excellent for single-instance" },
                Cons = new() { "Single-writer concurrency", "No built-in vector search", "Not suitable for distributed systems" },
                RecommendedFor = new() { "Development", "Single Instance", "Edge Deployment" },
                RequiresExternalService = false,
                DecisionScore = 3.96
            },
            new()
            {
                Key = "postgres",
                DisplayName = "PostgreSQL + pgvector",
                Description = "Production-grade relational database with native vector similarity search via pgvector extension and HNSW indexing.",
                Category = "store",
                Pros = new() { "Full SQL + ACID transactions", "Native vector search (pgvector)", "HNSW index for fast ANN", "Handles billions of rows", "Mature ecosystem" },
                Cons = new() { "Requires PostgreSQL server", "More complex setup", "Higher memory footprint" },
                RecommendedFor = new() { "Production", "Cloud", "Enterprise" },
                RequiresExternalService = true,
                DecisionScore = 3.90,
                SupportsVectorSearch = true
            },
            new()
            {
                Key = "litedb",
                DisplayName = "LiteDB",
                Description = "Pure C# embedded NoSQL document store with BSON documents. Zero native dependencies.",
                Category = "store",
                Pros = new() { "Pure C# — no native deps", "Document/NoSQL model", "Embedded — no server needed", "LINQ query support", "Shared connection mode" },
                Cons = new() { "Smaller community", "No built-in vector search", "Limited concurrent writes" },
                RecommendedFor = new() { "Development", "Desktop Apps", "Portable" },
                RequiresExternalService = false,
                DecisionScore = 3.27
            },
            new()
            {
                Key = "cosmosdb",
                DisplayName = "Azure Cosmos DB",
                Description = "Globally distributed, multi-model database with automatic scaling and 99.999% SLA.",
                Category = "store",
                Pros = new() { "Global distribution", "99.999% availability SLA", "Auto-scaling throughput", "Multi-model (SQL, MongoDB, Gremlin)", "1 RU point reads" },
                Cons = new() { "Azure-only", "Cost at scale (RU pricing)", "Vendor lock-in", "Cold start latency" },
                RecommendedFor = new() { "Azure Cloud", "Global Scale", "Enterprise" },
                RequiresExternalService = true,
                DecisionScore = 3.52
            },
            new()
            {
                Key = "redis",
                DisplayName = "Redis",
                Description = "In-memory data store with sub-millisecond latency and optional vector search via Redis Stack.",
                Category = "store",
                Pros = new() { "Sub-millisecond latency", "Built-in vector search (Stack)", "Pub/Sub for real-time", "Rich data structures", "Excellent as cache layer" },
                Cons = new() { "Memory-bound (expensive at scale)", "Persistence is secondary", "Requires Redis server" },
                RecommendedFor = new() { "Cache Layer", "Real-time", "Session Store" },
                RequiresExternalService = true,
                DecisionScore = 3.57,
                SupportsVectorSearch = true
            },
            new()
            {
                Key = "inmemory",
                DisplayName = "In-Memory (Dev/Test)",
                Description = "ConcurrentDictionary-backed store for development and testing. No persistence across restarts.",
                Category = "store",
                Pros = new() { "Zero configuration", "Fastest possible reads/writes", "No dependencies", "Perfect for unit tests", "Thread-safe" },
                Cons = new() { "No persistence", "Memory-only — lost on restart", "Not suitable for production", "No vector indexing" },
                RecommendedFor = new() { "Unit Testing", "Development", "Prototyping" },
                RequiresExternalService = false,
                DecisionScore = 2.92
            },
            new()
            {
                Key = "duckdb",
                DisplayName = "DuckDB (Legacy)",
                Description = "Embedded analytical database optimized for OLAP workloads. Legacy option, prefer SQLite for OLTP.",
                Category = "store",
                Pros = new() { "Excellent for analytics/OLAP", "Columnar storage", "Embedded — no server", "Vectorized execution" },
                Cons = new() { "Legacy in this codebase", "Not optimized for OLTP", "Limited .NET SDK maturity" },
                RecommendedFor = new() { "Analytics", "Batch Processing" },
                RequiresExternalService = false,
                DecisionScore = 2.86
            }
        };

        /// <summary>
        /// Returns metadata for all available IVectorSearchProvider implementations.
        /// </summary>
        public static IReadOnlyList<StorageProviderInfo> GetVectorSearchProviders() => new List<StorageProviderInfo>
        {
            new()
            {
                Key = "qdrant",
                DisplayName = "Qdrant",
                Description = "Purpose-built vector database with gRPC API, filtering, and payload storage. Optimized for ANN search.",
                Category = "vector",
                Pros = new() { "Purpose-built for vectors", "Fast HNSW search", "Rich filtering on payloads", "gRPC + REST APIs", "Docker-friendly" },
                Cons = new() { "Requires Qdrant server", "Newer ecosystem", "Additional infrastructure" },
                RecommendedFor = new() { "Production", "RAG Pipelines", "Semantic Search" },
                RequiresExternalService = true,
                DecisionScore = 3.93
            },
            new()
            {
                Key = "redis",
                DisplayName = "Redis Search",
                Description = "Vector search via Redis Stack with HNSW indexing. Reuses existing Redis infrastructure.",
                Category = "vector",
                Pros = new() { "Reuses Redis infrastructure", "HNSW index", "Combined cache + vector", "Sub-millisecond latency" },
                Cons = new() { "Requires Redis Stack (not vanilla Redis)", "Memory-bound", "Less mature than dedicated vector DBs" },
                RecommendedFor = new() { "Existing Redis Users", "Cache + Vector Combined" },
                RequiresExternalService = true,
                DecisionScore = 3.57
            },
            new()
            {
                Key = "postgres",
                DisplayName = "pgvector (PostgreSQL)",
                Description = "Vector similarity search as a PostgreSQL extension. HNSW index with cosine distance.",
                Category = "vector",
                Pros = new() { "Uses existing PostgreSQL", "Full SQL alongside vectors", "ACID on vector operations", "HNSW + IVFFlat indexes", "No separate vector DB needed" },
                Cons = new() { "Requires pgvector extension", "Slower than dedicated vector DBs at scale", "Index rebuild on large updates" },
                RecommendedFor = new() { "PostgreSQL Users", "Unified Store + Vector" },
                RequiresExternalService = true,
                DecisionScore = 3.90,
                SupportsVectorSearch = true
            },
            new()
            {
                Key = "milvus",
                DisplayName = "Milvus",
                Description = "Cloud-native vector database supporting billions of vectors with GPU-accelerated indexing.",
                Category = "vector",
                Pros = new() { "Billions of vectors", "GPU-accelerated indexing", "Cloud-native (Zilliz Cloud)", "Multiple index types", "REST + gRPC APIs" },
                Cons = new() { "Complex infrastructure", "Higher operational overhead", "Overkill for small datasets" },
                RecommendedFor = new() { "Large Scale", "GPU Available", "Cloud Native" },
                RequiresExternalService = true,
                DecisionScore = 3.30
            },
            new()
            {
                Key = "chroma",
                DisplayName = "ChromaDB",
                Description = "AI-native embedding database designed for LLM applications with automatic embedding generation.",
                Category = "vector",
                Pros = new() { "AI-native design", "Auto embedding generation", "Simple REST API", "Metadata filtering", "Good for RAG" },
                Cons = new() { "Younger ecosystem", "Limited .NET SDK", "Performance at scale unproven" },
                RecommendedFor = new() { "RAG Pipelines", "LLM Apps", "Prototyping" },
                RequiresExternalService = true,
                DecisionScore = 2.85
            }
        };

        /// <summary>
        /// Returns all providers (both store and vector) as a combined list.
        /// </summary>
        public static IReadOnlyList<StorageProviderInfo> GetAllProviders()
        {
            var all = new List<StorageProviderInfo>();
            all.AddRange(GetMemoryStoreProviders());
            all.AddRange(GetVectorSearchProviders().Where(v => !all.Any(a => a.Key == v.Key)));
            return all;
        }
    }
}
