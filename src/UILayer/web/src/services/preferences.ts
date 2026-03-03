// User Storage Preferences Service
// Manages polyglot database selection and user settings persistence

export interface ProviderConnectionSettings {
  connectionString: string;
  databaseName?: string;
  vectorDimension: number;
}

export interface UserStoragePreferences {
  userId: string;
  primaryStore: string;
  cacheStore: string | null;
  vectorSearchProvider: string;
  analyticsStore: string | null;
  enableHybridMode: boolean;
  preferCacheForRetrieval: boolean;
  setupCompleted: boolean;
  tourCompleted: boolean;
  connectionSettings: Record<string, ProviderConnectionSettings>;
  lastUpdated: string;
}

export interface StorageProviderInfo {
  key: string;
  displayName: string;
  description: string;
  category: "store" | "vector";
  pros: string[];
  cons: string[];
  recommendedFor: string[];
  requiresExternalService: boolean;
  decisionScore: number;
  supportsVectorSearch: boolean;
}

const PREFERENCES_KEY = "cognitive-mesh-storage-preferences";
const SETUP_COMPLETED_KEY = "cognitive-mesh-setup-completed";
const TOUR_COMPLETED_KEY = "cognitive-mesh-tour-completed";

// Default preferences for new users
const DEFAULT_PREFERENCES: UserStoragePreferences = {
  userId: "",
  primaryStore: "sqlite",
  cacheStore: "redis",
  vectorSearchProvider: "qdrant",
  analyticsStore: null,
  enableHybridMode: true,
  preferCacheForRetrieval: true,
  setupCompleted: false,
  tourCompleted: false,
  connectionSettings: {},
  lastUpdated: new Date().toISOString(),
};

// Complete registry of all available memory store providers
export const MEMORY_STORE_PROVIDERS: StorageProviderInfo[] = [
  {
    key: "sqlite",
    displayName: "SQLite",
    description:
      "Embedded relational database with ACID transactions and WAL mode. Zero-configuration, file-based storage.",
    category: "store",
    pros: [
      "Zero configuration needed",
      "ACID transactions",
      "File-based \u2014 easy backup",
      "Built into .NET runtime",
      "Excellent for single-instance",
    ],
    cons: [
      "Single-writer concurrency",
      "No built-in vector search",
      "Not suitable for distributed systems",
    ],
    recommendedFor: ["Development", "Single Instance", "Edge Deployment"],
    requiresExternalService: false,
    decisionScore: 3.96,
    supportsVectorSearch: false,
  },
  {
    key: "postgres",
    displayName: "PostgreSQL + pgvector",
    description:
      "Production-grade relational database with native vector similarity search via pgvector extension and HNSW indexing.",
    category: "store",
    pros: [
      "Full SQL + ACID transactions",
      "Native vector search (pgvector)",
      "HNSW index for fast ANN",
      "Handles billions of rows",
      "Mature ecosystem",
    ],
    cons: [
      "Requires PostgreSQL server",
      "More complex setup",
      "Higher memory footprint",
    ],
    recommendedFor: ["Production", "Cloud", "Enterprise"],
    requiresExternalService: true,
    decisionScore: 3.9,
    supportsVectorSearch: true,
  },
  {
    key: "litedb",
    displayName: "LiteDB",
    description:
      "Pure C# embedded NoSQL document store with BSON documents. Zero native dependencies.",
    category: "store",
    pros: [
      "Pure C# \u2014 no native deps",
      "Document/NoSQL model",
      "Embedded \u2014 no server needed",
      "LINQ query support",
      "Shared connection mode",
    ],
    cons: [
      "Smaller community",
      "No built-in vector search",
      "Limited concurrent writes",
    ],
    recommendedFor: ["Development", "Desktop Apps", "Portable"],
    requiresExternalService: false,
    decisionScore: 3.27,
    supportsVectorSearch: false,
  },
  {
    key: "cosmosdb",
    displayName: "Azure Cosmos DB",
    description:
      "Globally distributed, multi-model database with automatic scaling and 99.999% SLA.",
    category: "store",
    pros: [
      "Global distribution",
      "99.999% availability SLA",
      "Auto-scaling throughput",
      "Multi-model (SQL, MongoDB, Gremlin)",
      "1 RU point reads",
    ],
    cons: [
      "Azure-only",
      "Cost at scale (RU pricing)",
      "Vendor lock-in",
      "Cold start latency",
    ],
    recommendedFor: ["Azure Cloud", "Global Scale", "Enterprise"],
    requiresExternalService: true,
    decisionScore: 3.52,
    supportsVectorSearch: false,
  },
  {
    key: "redis",
    displayName: "Redis",
    description:
      "In-memory data store with sub-millisecond latency and optional vector search via Redis Stack.",
    category: "store",
    pros: [
      "Sub-millisecond latency",
      "Built-in vector search (Stack)",
      "Pub/Sub for real-time",
      "Rich data structures",
      "Excellent as cache layer",
    ],
    cons: [
      "Memory-bound (expensive at scale)",
      "Persistence is secondary",
      "Requires Redis server",
    ],
    recommendedFor: ["Cache Layer", "Real-time", "Session Store"],
    requiresExternalService: true,
    decisionScore: 3.57,
    supportsVectorSearch: true,
  },
  {
    key: "inmemory",
    displayName: "In-Memory (Dev/Test)",
    description:
      "ConcurrentDictionary-backed store for development and testing. No persistence across restarts.",
    category: "store",
    pros: [
      "Zero configuration",
      "Fastest possible reads/writes",
      "No dependencies",
      "Perfect for unit tests",
      "Thread-safe",
    ],
    cons: [
      "No persistence",
      "Memory-only \u2014 lost on restart",
      "Not suitable for production",
      "No vector indexing",
    ],
    recommendedFor: ["Unit Testing", "Development", "Prototyping"],
    requiresExternalService: false,
    decisionScore: 2.92,
    supportsVectorSearch: false,
  },
  {
    key: "duckdb",
    displayName: "DuckDB (Legacy)",
    description:
      "Embedded analytical database optimized for OLAP workloads. Legacy option, prefer SQLite for OLTP.",
    category: "store",
    pros: [
      "Excellent for analytics/OLAP",
      "Columnar storage",
      "Embedded \u2014 no server",
      "Vectorized execution",
    ],
    cons: [
      "Legacy in this codebase",
      "Not optimized for OLTP",
      "Limited .NET SDK maturity",
    ],
    recommendedFor: ["Analytics", "Batch Processing"],
    requiresExternalService: false,
    decisionScore: 2.86,
    supportsVectorSearch: false,
  },
];

// Complete registry of all available vector search providers
export const VECTOR_SEARCH_PROVIDERS: StorageProviderInfo[] = [
  {
    key: "qdrant",
    displayName: "Qdrant",
    description:
      "Purpose-built vector database with gRPC API, filtering, and payload storage. Optimized for ANN search.",
    category: "vector",
    pros: [
      "Purpose-built for vectors",
      "Fast HNSW search",
      "Rich filtering on payloads",
      "gRPC + REST APIs",
      "Docker-friendly",
    ],
    cons: [
      "Requires Qdrant server",
      "Newer ecosystem",
      "Additional infrastructure",
    ],
    recommendedFor: ["Production", "RAG Pipelines", "Semantic Search"],
    requiresExternalService: true,
    decisionScore: 3.93,
    supportsVectorSearch: true,
  },
  {
    key: "redis-vector",
    displayName: "Redis Search",
    description:
      "Vector search via Redis Stack with HNSW indexing. Reuses existing Redis infrastructure.",
    category: "vector",
    pros: [
      "Reuses Redis infrastructure",
      "HNSW index",
      "Combined cache + vector",
      "Sub-millisecond latency",
    ],
    cons: [
      "Requires Redis Stack (not vanilla)",
      "Memory-bound",
      "Less mature than dedicated vector DBs",
    ],
    recommendedFor: ["Existing Redis Users", "Cache + Vector Combined"],
    requiresExternalService: true,
    decisionScore: 3.57,
    supportsVectorSearch: true,
  },
  {
    key: "pgvector",
    displayName: "pgvector (PostgreSQL)",
    description:
      "Vector similarity search as a PostgreSQL extension. HNSW index with cosine distance.",
    category: "vector",
    pros: [
      "Uses existing PostgreSQL",
      "Full SQL alongside vectors",
      "ACID on vector operations",
      "HNSW + IVFFlat indexes",
      "No separate vector DB needed",
    ],
    cons: [
      "Requires pgvector extension",
      "Slower than dedicated vector DBs at scale",
      "Index rebuild on large updates",
    ],
    recommendedFor: ["PostgreSQL Users", "Unified Store + Vector"],
    requiresExternalService: true,
    decisionScore: 3.9,
    supportsVectorSearch: true,
  },
  {
    key: "milvus",
    displayName: "Milvus",
    description:
      "Cloud-native vector database supporting billions of vectors with GPU-accelerated indexing.",
    category: "vector",
    pros: [
      "Billions of vectors",
      "GPU-accelerated indexing",
      "Cloud-native (Zilliz Cloud)",
      "Multiple index types",
      "REST + gRPC APIs",
    ],
    cons: [
      "Complex infrastructure",
      "Higher operational overhead",
      "Overkill for small datasets",
    ],
    recommendedFor: ["Large Scale", "GPU Available", "Cloud Native"],
    requiresExternalService: true,
    decisionScore: 3.3,
    supportsVectorSearch: true,
  },
  {
    key: "chroma",
    displayName: "ChromaDB",
    description:
      "AI-native embedding database designed for LLM applications with automatic embedding generation.",
    category: "vector",
    pros: [
      "AI-native design",
      "Auto embedding generation",
      "Simple REST API",
      "Metadata filtering",
      "Good for RAG",
    ],
    cons: [
      "Younger ecosystem",
      "Limited .NET SDK",
      "Performance at scale unproven",
    ],
    recommendedFor: ["RAG Pipelines", "LLM Apps", "Prototyping"],
    requiresExternalService: true,
    decisionScore: 2.85,
    supportsVectorSearch: true,
  },
];

export class PreferencesService {
  private static instance: PreferencesService;

  private constructor() {}

  static getInstance(): PreferencesService {
    if (!PreferencesService.instance) {
      PreferencesService.instance = new PreferencesService();
    }
    return PreferencesService.instance;
  }

  getPreferences(): UserStoragePreferences {
    if (typeof window === "undefined") return DEFAULT_PREFERENCES;

    const stored = localStorage.getItem(PREFERENCES_KEY);
    if (!stored) return { ...DEFAULT_PREFERENCES };

    try {
      return JSON.parse(stored) as UserStoragePreferences;
    } catch {
      return { ...DEFAULT_PREFERENCES };
    }
  }

  savePreferences(prefs: UserStoragePreferences): void {
    if (typeof window === "undefined") return;

    prefs.lastUpdated = new Date().toISOString();
    localStorage.setItem(PREFERENCES_KEY, JSON.stringify(prefs));

    if (prefs.setupCompleted) {
      localStorage.setItem(SETUP_COMPLETED_KEY, "true");
    }
    if (prefs.tourCompleted) {
      localStorage.setItem(TOUR_COMPLETED_KEY, "true");
    }
  }

  isSetupCompleted(): boolean {
    if (typeof window === "undefined") return false;
    return localStorage.getItem(SETUP_COMPLETED_KEY) === "true";
  }

  isTourCompleted(): boolean {
    if (typeof window === "undefined") return false;
    return localStorage.getItem(TOUR_COMPLETED_KEY) === "true";
  }

  markSetupCompleted(): void {
    const prefs = this.getPreferences();
    prefs.setupCompleted = true;
    this.savePreferences(prefs);
  }

  markTourCompleted(): void {
    const prefs = this.getPreferences();
    prefs.tourCompleted = true;
    this.savePreferences(prefs);
  }

  resetSetup(): void {
    if (typeof window === "undefined") return;
    localStorage.removeItem(PREFERENCES_KEY);
    localStorage.removeItem(SETUP_COMPLETED_KEY);
    localStorage.removeItem(TOUR_COMPLETED_KEY);
  }

  getRecommendedConfig(
    useCase: "development" | "production" | "cloud" | "testing"
  ): Partial<UserStoragePreferences> {
    switch (useCase) {
      case "development":
        return {
          primaryStore: "sqlite",
          cacheStore: null,
          vectorSearchProvider: "qdrant",
          enableHybridMode: false,
        };
      case "production":
        return {
          primaryStore: "postgres",
          cacheStore: "redis",
          vectorSearchProvider: "qdrant",
          enableHybridMode: true,
          preferCacheForRetrieval: true,
        };
      case "cloud":
        return {
          primaryStore: "cosmosdb",
          cacheStore: "redis",
          vectorSearchProvider: "milvus",
          enableHybridMode: true,
          preferCacheForRetrieval: true,
        };
      case "testing":
        return {
          primaryStore: "inmemory",
          cacheStore: null,
          vectorSearchProvider: "chroma",
          enableHybridMode: false,
        };
    }
  }
}
