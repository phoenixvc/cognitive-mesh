# Migration Risk Matrix

Risk assessment for migrating between orchestration systems. Use this when planning transitions from internal repos to external engines or consolidating internal repos.

## Risk Categories

| Category | Description |
|----------|-------------|
| **Data Loss** | Risk of losing in-flight task/workflow state during migration |
| **Downtime** | Expected service interruption during cutover |
| **Complexity** | Implementation effort and cognitive load |
| **Rollback** | Difficulty of reverting if migration fails |
| **Side Effects** | Risk of duplicate processing, missed events, or inconsistent state |

## Internal → External Migration Risks

### agentkit-forge → Temporal

| Risk | Level | Mitigation |
|------|:-----:|------------|
| Data Loss | Low | File-based state can be read and migrated; tasks have terminal states |
| Downtime | Low | Dual-write during transition; cutover when queues drain |
| Complexity | Medium | 5-phase lifecycle maps well to Temporal workflow steps |
| Rollback | Low | File-based state persists independently |
| Side Effects | Low | Handoff locks prevent double-processing during transition |

**Overall Migration Risk: LOW-MEDIUM**

### agentkit-forge → Inngest

| Risk | Level | Mitigation |
|------|:-----:|------------|
| Data Loss | Low | Same as Temporal path |
| Downtime | Low | Event-driven allows parallel operation |
| Complexity | Low | Lifecycle phases map to Inngest step functions |
| Rollback | Low | File backend remains operational |
| Side Effects | Medium | Need idempotency keys to prevent duplicate event processing |

**Overall Migration Risk: LOW**

### codeflow-engine → Temporal

| Risk | Level | Mitigation |
|------|:-----:|------------|
| Data Loss | Medium | In-memory state (running_workflows) has no durable backup |
| Downtime | Medium | Active workflows must drain before cutover |
| Complexity | Medium | Workflow engine semantics align; entry-point plugins need wrapping |
| Rollback | Medium | No persistent state to fall back to |
| Side Effects | Medium | Event fan-out could duplicate during transition |

**Overall Migration Risk: MEDIUM**

### codeflow-engine → Inngest

| Risk | Level | Mitigation |
|------|:-----:|------------|
| Data Loss | Medium | Same in-memory risk |
| Downtime | Low | Event-driven allows gradual migration |
| Complexity | Low-Medium | Workflows map to Inngest functions; plugins need adaptation |
| Rollback | Medium | Same persistence gap |
| Side Effects | Medium | Ensure event deduplication during transition |

**Overall Migration Risk: MEDIUM**

### cognitive-mesh → Temporal

| Risk | Level | Mitigation |
|------|:-----:|------------|
| Data Loss | Medium-High | _activeTasks is in-memory; adapter state unknown |
| Downtime | Medium | Multi-agent orchestration is complex to migrate live |
| Complexity | High | 4 coordination patterns + governance gates need careful mapping |
| Rollback | Medium | Depends on adapter state persistence |
| Side Effects | High | Ethics/approval gates must not be bypassed during transition |

**Overall Migration Risk: HIGH**

### cognitive-mesh → Inngest

| Risk | Level | Mitigation |
|------|:-----:|------------|
| Data Loss | Medium-High | Same in-memory risk |
| Downtime | Medium | Same complexity |
| Complexity | High | Swarm/competitive patterns don't map directly to step functions |
| Rollback | Medium | Same adapter dependency |
| Side Effects | High | Governance gate integrity is critical |

**Overall Migration Risk: HIGH**

### HouseOfVeritas → Temporal

| Risk | Level | Mitigation |
|------|:-----:|------------|
| Data Loss | Low | Inngest manages state; can run both in parallel |
| Downtime | Low | Event-driven allows gradual migration |
| Complexity | Medium | Inngest step functions map to Temporal activities |
| Rollback | Low | Inngest remains operational during/after migration |
| Side Effects | Medium | Duplicate event processing if both systems active |

**Overall Migration Risk: LOW-MEDIUM**

## Migration Effort Estimates

| Migration Path | Effort | Duration Estimate | Team Size |
|---------------|:------:|:-----------------:|:---------:|
| agentkit-forge → Inngest | Small | 1–2 weeks | 1 engineer |
| agentkit-forge → Temporal | Medium | 2–4 weeks | 1–2 engineers |
| codeflow-engine → Inngest | Medium | 2–3 weeks | 1–2 engineers |
| codeflow-engine → Temporal | Medium | 3–5 weeks | 2 engineers |
| cognitive-mesh → Temporal | Large | 6–10 weeks | 2–3 engineers |
| cognitive-mesh → Inngest | Large | 6–10 weeks | 2–3 engineers |
| HouseOfVeritas → Temporal | Medium | 2–4 weeks | 1–2 engineers |

## Migration Strategy Patterns

### Blue-Green Migration
Run both old and new systems in parallel. Route new tasks to the new system while old tasks drain.
- **Best for**: agentkit-forge, HouseOfVeritas (stateless or externally-managed state)
- **Risk**: Duplicate processing if routing isn't precise

### Strangler Fig
Gradually migrate individual workflows/tasks to the new system, one at a time.
- **Best for**: codeflow-engine (entry-point plugin architecture allows gradual migration)
- **Risk**: Longer migration window; both systems must be maintained

### Big Bang
Migrate everything at once during a maintenance window.
- **Best for**: Only when running both systems is impractical
- **Risk**: Highest risk; longest rollback time

### Recommended per Repo

| Repo | Strategy | Rationale |
|------|----------|-----------|
| agentkit-forge | Blue-Green | File-based state is self-contained; easy parallel run |
| codeflow-engine | Strangler Fig | Plugin architecture allows per-workflow migration |
| cognitive-mesh | Strangler Fig | Complex patterns need per-pattern validation |
| HouseOfVeritas | Blue-Green | Already event-driven; parallel operation is natural |
