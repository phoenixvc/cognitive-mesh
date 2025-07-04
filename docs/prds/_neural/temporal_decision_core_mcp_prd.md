---

## Module: Temporal‑Decision‑Core (TDC) Primary Personas: Cognitive Architects, Backend AI Engineers, Data Scientists Core Value Proposition: Human‑like temporal causality reasoning and adaptive decision gating that prevents spurious chains while preserving true long‑gap cause→effect links. Priority: P0 License Tier: Enterprise Platform Layers: Foundation, Reasoning, Metacognitive, Agency Main Integration Points: Dual‑Circuit Temporal Association Gate, Adaptive Temporal Window Controller, Mesh Event Bus, Memory & Flexibility Suite

# Product Requirements Document: Temporal‑Decision‑Core (TDC)

**Product:** Temporal‑Decision‑Core MCP PRD\
**Author:** J\
**Date:** 2025‑07‑04

---

## 1. Overview

TDC is the Mesh’s biologically‑inspired backbone for relating events across time *safely*.  Borrowing from layer‑3→CA1 promoter circuits and island‑cell L2 suppressors, it decides **when** two temporally separated observations should be linked and **when** the connection must be suppressed.  A dynamic eligibility window (0–20 s) adapts according to salience, threat, cognitive load, and strategy confidence, eliminating run‑away association graphs while preserving essential delayed causality (e.g. “skid → crash”).\
TDC exposes clean ports so every Mesh agent can *record*, *link*, or *query* temporal edges with full RBAC, audit, and live metrics.

---

## 2. Goals

### Business Goals

- **Trustworthy Causality:** Cut false temporal associations by ≥ 60 % in pilot workloads (support‑ticket triage, threat analysis).
- **Explainability:** 100 % of created temporal edges include a machine‑readable rationale and confidence score for audit.
- **Platform Enablement:** Provide low‑latency (< 1 ms P95) edge‑gating service so higher‑layer agents (Reasoning, Agency) can consume without performance hit.

### User Goals

- **Correct Recall:** When an agent queries a chain, irrelevant edges are absent (> 95 % precision).
- **Adaptive Windows:** Window auto‑expands during high‑threat sequences without manual tuning.
- **Edge Inspection:** Dev / Ops can inspect, replay, and override any temporal link from the Admin widget.

### Non‑Goals

- Not responsible for storing bulk episodic memory payloads (delegated to Memory & Flexibility Suite).
- Not aimed at sub‑millisecond hardware planning (handled by Neural‑Trajectory Planner).

---

## 3. Stakeholders

- **Product Owner:** J
- **Engineering Lead:** Core Mesh Infrastructure
- **Security & Compliance:** Zero‑Trust Council
- **Site Reliability:** Mesh SRE
- **End Users:** Mesh agents, cognitive architects, SOC analysts

---

## 4. Functional Requirements

| ID      | Requirement                                                                                               | Phase | Priority |
| ------- | --------------------------------------------------------------------------------------------------------- | ----- | -------- |
| FR1     | **RecordEvent** port accepts `{eventId,timestamp,salience,context}` and buffers into CA1 eligibility gate | 1     | P0       |
| FR2     | **LinkEdges** service promotes buffered pairs when promoter≥θ and suppressor≤σ within *AdaptiveWindow*    | 1     | P0       |
| FR3     | **QueryTemporalGraph** returns ordered list of linked events with confidence ≥τ                           | 1     | P0       |
| FR4     | **AdjustWindow** module enlarges/shrinks `maxGap` (0–20 s) based on threat, load, precision signals       | 2     | P1       |
| FR5     | **ShadowMode** flag writes edges to *shadow graph* for A/B evaluation without affecting live agents       | 2     | P1       |
| FR6     | **AdminWidget** visualises link creation, promoter/suppressor activity, window size over time             | 3     | P2       |
| FR‑Gov1 | All actions emit `TemporalEdgeLog` to Audit bus with rationale and confidence                             | 1‑3   | P0       |

---

## 5. Non‑Functional Requirements

| ID   | Category      | Requirement                          | Target          |
| ---- | ------------- | ------------------------------------ | --------------- |
| NFR1 | Performance   | P95 gating latency                   | ≤ 1 ms          |
| NFR2 | Throughput    | Sustained event rate                 | ≥ 10 k events/s |
| NFR3 | Security      | All ports require mutual‑TLS + JWT   | 100 % requests  |
| NFR4 | Reliability   | Edge‑creation durability             | ≥ 99.9 %        |
| NFR5 | Observability | Promoter/suppressor metrics exported | 1 s scrape      |

---

## 6. User Experience / UX Flow

### Agent Flow

1. Agent calls `RecordEvent` when salience≥low.\
2. TDC buffers event in CA1 gate (expires after decay).\
3. On second event, promoter & suppressor nets vote; if approved, `TemporalEdgeCreated` fires.\
4. Downstream agent queries `QueryTemporalGraph(eventId)` to reason backwards.

### Admin Widget Flow *(Phase 3)*

- Live promoter/suppressor heat‑map, adjustable θ/σ for Shadow mode, replay explorer to inspect rationales.

---

## 7. Technical Architecture & Integrations

```
┌──────────┐   RecordEvent      ┌────────────┐   Promoter Vote   ┌──────────────┐
│  Agents  ├──────────────────►│  CA1 Gate  ├──────────────────►│ Edge Creator │
└──────────┘                   └────────────┘◄──────────────────┤ Suppressor   │
                                      ▲  AdjustWindow            └─────┬──────┘
                                      │                               Audit Bus
                                      │ ShadowMode                     │
                               ┌──────────────┐                       │
                               │  MetaConf    │◄──────────────────────┘
                               └──────────────┘
```

- **Ports (Reasoning):** `ITemporalRecorder`, `ITemporalLinker`, `ITemporalQuery`
- **Adapters (Foundation):** gRPC/MQ adapter for high‑volume ingest, HTTP adapter for low‑rate clients
- **Event Bus:** Streams `TemporalEdgeCreated`, `EdgeSuppressed`, `WindowAdjusted`
- **Storage:** RocksDB time‑series for buffer, Neo4j for persisted graph

---

## 8. Mesh Layer Mapping

| Mesh Layer        | Component                    | Responsibility / Port                        |
| ----------------- | ---------------------------- | -------------------------------------------- |
| **Foundation**    | TemporalBuffer, GraphStore   | Durable buffer & edge graph adapters         |
| **Reasoning**     | PromoterNet, SuppressorNet   | Determine edge eligibility & confidence      |
| **Metacognitive** | WindowController, ShadowEval | Dynamic window tuning, A/B safety evaluation |
| **Agency**        | TemporalEdgeOrchestrator     | High‑level API for agents to record/query    |
| **Business Apps** | TDCAdminWidget               | Dashboards & override controls               |

---

## 9. Main APIs & Schemas

### RecordEvent

```
POST /tdc/event
{ "eventId":"uuid", "timestamp":"2025‑07‑04T12:34:56Z", "salience":0.9,
  "context": {"actor":"user‑123","type":"skid"} }
```

Response: `{ "buffered":true, "expiresInMs":15000 }`

### QueryTemporalGraph

```
GET /tdc/graph?eventId=uuid&depth=3&minConf=0.7
```

Response: `{"edges":[{"from":"e1","to":"e2","conf":0.82,"msGap":4500}, ...]}`

### AdjustWindow (internal)

```
PATCH /tdc/window {"maxGapMs":18000,"reason":"threatEscalation"}
```

---

## 10. Audit Log Event Taxonomy

| Event Type          | Key Fields                            |
| ------------------- | ------------------------------------- |
| TemporalEdgeCreated | fromId,toId,conf,msGap,windowSize,why |
| EdgeSuppressed      | eventId,candidateId,score,suppressor  |
| WindowAdjusted      | oldGap,newGap,trigger,actor           |
| ShadowComparison    | liveConf,shadowConf,divergence        |

---

## 11. Timeline & Milestones

| Phase | Duration | Exit Criteria                                            |
| ----- | -------- | -------------------------------------------------------- |
| 1     | 2 wks    | FR1–FR3 live; <1 ms P95; audit logs; unit+load tests     |
| 2     | 2 wks    | AdaptiveWindow + ShadowMode; ≥60 % FP drop vs baseline   |
| 3     | 1 wk     | AdminWidget; Edge replay; security & compliance sign‑off |

---

## 12. Success Metrics

- **Edge Precision:** ≥ 95 % on benchmark dataset.
- **False‑Positive Rate:** ≤ 40 % of baseline system.
- **Latency:** P95 gating ≤ 1 ms.
- **Explainability Coverage:** 100 % edges include rationale.

---

## 13. Risks & Mitigations

| Risk                   | Mitigation                                      |
| ---------------------- | ----------------------------------------------- |
| Promoter over‑linking  | Dynamic σ thresholds; ShadowMode A/B validation |
| Buffer memory pressure | Exponential decay + high‑salience pinning       |
| Graph store bloat      | TTL edges < α confidence; nightly prune job     |
| Window mis‑tuning      | Metacognitive feedback loop auto‑optimises      |

---

## 14. Open Questions

1. Should long‑gap (>20 s) events ever be allowed under special salience?
2. Would a distributed buffer (Kafka Streams) be required for >50 k events/s workloads?
3. Preferred admin override semantics—hard delete or soft suppress?

---

> **TDC:** The Mesh’s temporal backbone—forming the right links, at the right moment, for trustworthy, human‑like causality reasoning.

