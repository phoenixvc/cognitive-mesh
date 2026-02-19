---
paths:
  - "src/MetacognitiveLayer/**/*.cs"
---

# Metacognitive Layer Rules

## Memory Store
- HybridMemoryStore dual-writes to Redis (fast) AND DuckDB (durable)
- Always initialize stores before use (`InitializeAsync()`)
- Use `SaveContextAsync` with session-scoped keys for isolation
- Embeddings stored as `float[]` serialized to JSON strings

## Session Management
- Sessions are in-memory (`ConcurrentDictionary`) — they don't survive process restart
- Default timeout: 1 hour. Cleanup runs every 5 minutes.
- For durable sessions, persist to DuckDB via the memory store

## Transparency
- Log every reasoning step via `TransparencyManager.LogReasoningStepAsync()`
- Include confidence scores and rationale on all reasoning outputs
- Reports can be JSON or Markdown format

## Continuous Learning
- Feedback records go to Cosmos DB partitioned by type ("Feedback", "Interaction")
- Insight generation calls Azure OpenAI — guard with try/catch and feature flags
