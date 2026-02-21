# ADR-001: Memory Store Selection for Cognitive Mesh

**Status:** Accepted
**Date:** 2026-02-21
**Decision Makers:** Architecture Team
**Context:** MetacognitiveLayer Memory Subsystem

---

## Context

Cognitive Mesh requires a memory subsystem that serves two distinct purposes:

1. **Session Context Memory** (MetacognitiveLayer) - Transient key-value storage for conversation sessions with vector similarity search for embedding retrieval
2. **Episodic Memory** (ReasoningLayer) - Long-term cognitive memory with multi-strategy recall (exact, fuzzy, semantic, temporal, hybrid)

The current architecture uses:
- **HybridMemoryStore** wrapping Redis (hot cache + vector search) and DuckDB (persistent OLAP fallback)
- **MemoryStrategyEngine** using in-memory `ConcurrentDictionary` for episodic memory (no persistence)

### Problems with Current Implementation

| Problem | Impact |
|---------|--------|
| DuckDB requires native library (`DuckDB.NET.Data`) that complicates CI/CD | Build failures, stub classes needed |
| Episodic memory is entirely in-memory | Data lost on restart, no multi-instance sharing |
| Write amplification in HybridMemoryStore (2x writes) | Increased latency, potential consistency gaps |
| Fixed 768-dimension vectors in Redis provider | Cannot use different embedding models |
| No transaction semantics between Redis and DuckDB | Partial write failures possible |
| No TTL/expiration on session data | Memory growth unbounded |
| Manual cosine similarity fallback in DuckDB is O(n*d) | Unusable at scale |

---

## Options Evaluated

### Option 1: Redis + Redis Search (Current Hot Path)
**Category:** In-memory cache + vector search
**Maturity:** Production-ready
**.NET SDK:** StackExchange.Redis (excellent)

| Strength | Weakness |
|----------|----------|
| Sub-millisecond latency | No persistence by default (RDB/AOF optional) |
| HNSW vector indexing via Redis Search | Requires Redis Stack or Redis Enterprise |
| Pub/sub for distributed events | Memory-bound (cost scales with data) |
| Excellent .NET ecosystem | Complex cluster setup for HA |

### Option 2: DuckDB (Current Cold Path)
**Category:** Embedded OLAP database
**Maturity:** Stable (v1.x)
**.NET SDK:** DuckDB.NET.Data (fair, native dependency)

| Strength | Weakness |
|----------|----------|
| Zero-config embedded database | Native library dependency complicates CI/CD |
| OLAP-optimized for analytical queries | Not designed for OLTP workloads |
| Built-in vector extension | File-based, single-writer limitation |
| SQL interface | Limited concurrent read/write support |

### Option 3: Qdrant (Existing in FoundationLayer)
**Category:** Purpose-built vector database
**Maturity:** Production-ready (v1.x)
**.NET SDK:** Qdrant.Client (good)

| Strength | Weakness |
|----------|----------|
| Purpose-built for vector similarity search | External service dependency |
| Supports HNSW, IVF, scalar quantization | Requires separate deployment |
| Filtering + payload storage | Overkill for simple key-value |
| Horizontal scaling | Additional operational cost |

### Option 4: SQLite + sqlite-vec
**Category:** Embedded relational + vector extension
**Maturity:** SQLite is battle-tested; sqlite-vec is newer
**.NET SDK:** Microsoft.Data.Sqlite (excellent, built-in)

| Strength | Weakness |
|----------|----------|
| Zero-config, no native dependency issues | sqlite-vec extension is newer/less mature |
| ACID transactions | Single-writer, readers don't block |
| Excellent .NET support (built into runtime) | Not optimized for vector operations |
| File-based, portable | No built-in distributed support |
| WAL mode supports concurrent reads | Vector search performance ~100ms-1s |

### Option 5: PostgreSQL + pgvector
**Category:** Production relational + vector extension
**Maturity:** Battle-tested
**.NET SDK:** Npgsql (excellent)

| Strength | Weakness |
|----------|----------|
| ACID + MVCC concurrency | External service required |
| pgvector: HNSW + IVF-Flat indexing | Operational overhead |
| Mature ecosystem, monitoring, backup | Higher resource requirements |
| Horizontal read replicas | pgvector HNSW rebuild on large updates |
| Full SQL + JSON support | Not cloud-native by default |

### Option 6: LiteDB
**Category:** Embedded NoSQL document database
**Maturity:** Stable (v5.x)
**.NET SDK:** LiteDB (native C#, no native deps)

| Strength | Weakness |
|----------|----------|
| Pure C# (no native dependencies) | No vector search capability |
| BSON document model | Limited query optimization |
| ACID transactions | Single-file concurrency limits |
| Zero-config embedded | Smaller community than SQLite |
| Ideal for .NET projects | No distributed support |

### Option 7: RocksDB (via RocksDbSharp)
**Category:** LSM-tree key-value store
**Maturity:** Battle-tested (Facebook/Meta)
**.NET SDK:** RocksDbSharp (fair, native dependency)

| Strength | Weakness |
|----------|----------|
| Extreme write throughput | Native library dependency |
| Efficient storage compression | No SQL interface |
| Tunable consistency | No built-in vector search |
| Used by many databases internally | Complex tuning required |
| Excellent read performance for hot data | Write amplification in LSM |

### Option 8: Milvus
**Category:** Cloud-native vector database
**Maturity:** Production-ready (v2.x)
**.NET SDK:** Milvus.Client (fair)

| Strength | Weakness |
|----------|----------|
| Purpose-built for vectors at scale | Heavy operational footprint (etcd, MinIO) |
| Multiple index types (HNSW, IVF, DiskANN) | .NET SDK less mature than Python |
| Supports billions of vectors | Minimum 3 nodes recommended |
| GPU acceleration | Overkill for <1M vectors |

### Option 9: ChromaDB
**Category:** AI-native embedding database
**Maturity:** Growing (v0.5.x)
**.NET SDK:** Community-maintained (limited)

| Strength | Weakness |
|----------|----------|
| Designed for LLM/RAG pipelines | Not yet stable (v0.x) |
| Automatic embedding generation | Weak .NET SDK |
| Simple API | Python-first ecosystem |
| Metadata filtering | Limited production track record |
| Embedded or client-server modes | Not enterprise-grade yet |

### Option 10: Azure Cosmos DB (Existing in FoundationLayer)
**Category:** Cloud-native multi-model database
**Maturity:** Production-ready
**.NET SDK:** Microsoft.Azure.Cosmos (excellent)

| Strength | Weakness |
|----------|----------|
| Global distribution | Azure lock-in |
| Multiple APIs (SQL, MongoDB, Gremlin) | Cost at scale (RU pricing) |
| Vector search support (preview) | Vector search is relatively new |
| Auto-scaling | Complex partition key design |
| Built-in change feed for events | Cold start latency on serverless |

### Option 11: In-Memory ConcurrentDictionary (Current Episodic)
**Category:** In-process memory
**Maturity:** Built-in .NET
**.NET SDK:** System.Collections.Concurrent

| Strength | Weakness |
|----------|----------|
| Zero latency | No persistence |
| No dependencies | No cross-process sharing |
| Thread-safe | Memory-bound scalability |
| Simplest implementation | Lost on restart |

### Option 12: Semantic Kernel Memory (Microsoft)
**Category:** AI memory framework
**Maturity:** Growing (v1.x)
**.NET SDK:** Microsoft.SemanticKernel.Memory (good)

| Strength | Weakness |
|----------|----------|
| Designed for AI agent memory | Tied to Semantic Kernel ecosystem |
| Pluggable vector stores | Additional abstraction layer |
| Built-in chunking + embedding | May conflict with existing architecture |
| .NET-first | Evolving API surface |
| Supports multiple backends | Adds dependency weight |

---

## Decision Matrix

### Criteria Weights

| Criterion | Weight | Rationale |
|-----------|--------|-----------|
| .NET SDK Quality | 15% | Must integrate cleanly with C# codebase |
| Vector Search Performance | 15% | Core requirement for similarity matching |
| Operational Simplicity | 15% | Minimize deployment/maintenance burden |
| Persistence & Durability | 12% | Session data and episodic memory must survive restarts |
| Latency (Read/Write) | 10% | Real-time agent interactions require low latency |
| Scalability | 8% | Must handle growth from prototype to production |
| Cost | 8% | Infrastructure and licensing costs |
| ACID / Consistency | 7% | Data integrity for critical operations |
| Cloud-Native Readiness | 5% | Azure deployment target |
| Embedding Flexibility | 5% | Support different embedding dimensions |

### Scoring (1-5 scale: 1=Poor, 3=Adequate, 5=Excellent)

| Option | .NET SDK (15%) | Vector (15%) | Ops Simple (15%) | Persist (12%) | Latency (10%) | Scale (8%) | Cost (8%) | ACID (7%) | Cloud (5%) | Embed Flex (5%) | **Weighted** |
|--------|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
| 1. Redis | 5 | 4 | 3 | 2 | 5 | 4 | 3 | 2 | 4 | 4 | **3.57** |
| 2. DuckDB | 2 | 3 | 2 | 4 | 3 | 2 | 5 | 4 | 2 | 3 | **2.86** |
| 3. Qdrant | 4 | 5 | 2 | 5 | 4 | 5 | 3 | 3 | 4 | 5 | **3.93** |
| **4. SQLite** | **5** | **3** | **5** | **5** | **3** | **2** | **5** | **5** | **3** | **3** | **3.96** |
| 5. PostgreSQL | 5 | 4 | 2 | 5 | 3 | 5 | 3 | 5 | 4 | 5 | **3.90** |
| 6. LiteDB | 5 | 1 | 5 | 4 | 4 | 2 | 5 | 4 | 2 | 1 | **3.27** |
| 7. RocksDB | 3 | 1 | 2 | 5 | 4 | 4 | 4 | 3 | 2 | 1 | **2.80** |
| 8. Milvus | 2 | 5 | 1 | 5 | 4 | 5 | 2 | 3 | 3 | 5 | **3.30** |
| 9. ChromaDB | 1 | 4 | 3 | 3 | 3 | 3 | 4 | 2 | 3 | 4 | **2.85** |
| 10. Cosmos DB | 5 | 3 | 3 | 5 | 3 | 5 | 1 | 4 | 5 | 3 | **3.52** |
| 11. ConcurrentDict | 5 | 2 | 5 | 1 | 5 | 1 | 5 | 1 | 1 | 2 | **2.92** |
| 12. SK Memory | 4 | 4 | 3 | 4 | 3 | 3 | 4 | 3 | 4 | 4 | **3.53** |

### Top 3 Results

| Rank | Option | Score | Role |
|------|--------|-------|------|
| 1 | **SQLite + sqlite-vec** | 3.96 | Embedded persistent store (replace DuckDB) |
| 2 | **Qdrant** | 3.93 | Production vector search (already in FoundationLayer) |
| 3 | **PostgreSQL + pgvector** | 3.90 | Production relational + vector (cloud deployments) |

---

## Decision

### Tier 1: Implement Now

1. **SQLite** (via `Microsoft.Data.Sqlite`) as the embedded persistent store, replacing DuckDB
   - Zero native dependency issues (built into .NET SDK)
   - ACID transactions for consistency
   - WAL mode for concurrent reads
   - Eliminates DuckDB stub workaround
   - Manual cosine similarity in C# for vector search (acceptable at <100k records)

2. **Qdrant integration** for production vector search (enhance existing FoundationLayer adapter)
   - Already has `QdrantVectorDatabaseAdapter` in FoundationLayer
   - Wire into `MemoryStoreFactory` as a selectable `IVectorSearchProvider`
   - Replace Redis-only vector search with Qdrant option

### Tier 2: Keep As-Is

3. **Redis** remains as the hot cache layer
   - Sub-millisecond reads for active sessions
   - Keep Redis Search for environments that have Redis Stack
   - TTL support for automatic session expiration

4. **ConcurrentDictionary** remains for MemoryStrategyEngine
   - Add optional SQLite persistence backend for episodic memory
   - Lazy-load from SQLite on startup, write-through on changes

### Tier 3: Future Consideration

5. **PostgreSQL + pgvector** for cloud-native deployments
   - Implement when Azure deployment is prioritized
   - Can replace both SQLite and Redis in cloud environments

---

## Implementation Plan

### Phase 1: SQLite Memory Store (Replace DuckDB)
- Create `SqliteMemoryStore : IMeshMemoryStore` adapter
- Implement context table with UPSERT semantics
- Implement embeddings table with manual cosine similarity
- Update `MemoryStoreFactory` to support `"sqlite"` store type
- Add `Microsoft.Data.Sqlite` to `Protocols.csproj`
- Remove DuckDB stub classes

### Phase 2: Qdrant Vector Search Provider
- Create `QdrantVectorSearchProvider : IVectorSearchProvider` adapter
- Wire into `MemoryStoreFactory` for `IVectorSearchProvider` registration
- Support configurable collection names and vector dimensions
- Add fallback from Qdrant to SQLite for offline/development scenarios

### Phase 3: Configuration & Factory Updates
- Add `VectorSearchProvider` option: `"redis"` | `"qdrant"` | `"sqlite"`
- Add `EpisodicPersistence` option: `"memory"` | `"sqlite"`
- Update `MemoryStoreOptions` with new configuration
- Update DI registration in `MemoryStoreFactory`

---

## Consequences

### Positive
- Eliminates DuckDB native dependency (CI/CD simplification)
- ACID transactions for persistent memory operations
- Production-grade vector search via Qdrant
- Configurable backend selection per deployment environment
- No breaking changes to `IMeshMemoryStore` interface

### Negative
- SQLite vector search is slower than Redis/Qdrant (acceptable for fallback)
- Additional adapter code to maintain
- Qdrant requires external service for production vector search

### Risks
- SQLite WAL mode has a single-writer limitation (mitigated by short write duration)
- Qdrant service availability depends on deployment infrastructure
- Migration from DuckDB schema to SQLite schema needs careful data handling

---

## References

- [Microsoft.Data.Sqlite Documentation](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/)
- [Qdrant Documentation](https://qdrant.tech/documentation/)
- [Redis Search Vector Similarity](https://redis.io/docs/latest/develop/interact/search-and-query/advanced-concepts/vectors/)
- [sqlite-vec Extension](https://github.com/asg017/sqlite-vec)
- [pgvector PostgreSQL Extension](https://github.com/pgvector/pgvector)
