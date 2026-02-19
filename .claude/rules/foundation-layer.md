---
paths:
  - "src/FoundationLayer/**/*.cs"
---

# Foundation Layer Rules

## Persistence Adapters
- **CosmosDB**: Retry policy = 9 attempts, 60s max wait, exponential backoff on 429 (rate limit)
- **DuckDB**: File-based OLAP at `data/mesh_memory.duckdb`. Always create directory before opening.
- **Qdrant**: Vector DB adapter via `IVectorDatabaseAdapter`. Dispose connections.
- **Blob Storage**: For RAG documents. Use managed identity in production.

## Security
- `SecurityPolicyEnforcementEngine` for RBAC enforcement
- mTLS for inter-service communication in production
- Never log secrets, PII, or auth tokens

## Circuit Breaker
- 3-state pattern: Closed → Open → HalfOpen
- Default threshold: 3 consecutive failures
- Use for all external service calls (CosmosDB, Redis, OpenAI)

## Knowledge Graph
- `IKnowledgeGraphManager` for node/relationship operations
- Node types: ReasoningTrace, ReasoningStep, DecisionRationale, ActionPlan
- Always use cancellation tokens on graph queries

## Notifications
- Priority-based retry queue: 50 items per 15s cycle
- Max 10 retries per notification, then dead-letter
