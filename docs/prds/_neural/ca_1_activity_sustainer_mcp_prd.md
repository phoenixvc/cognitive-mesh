---

## Module: CA1‑Activity‑Sustainer / Eligibility‑Trace (CASE)

**Primary Personas:** Cognitive Architects, Temporal‑Reasoning Engineers, SRE/Performance Ops\
**Core Value Proposition:** Keeps high‑salience recent cues “alive” for milliseconds‑to‑seconds without full memory writes—enabling low‑latency temporal linking, higher recall precision, and graceful degradation under load.\
**Priority:** P2\
**License Tier:** Enterprise\
**Platform Layers:** Foundation, Reasoning, Metacognitive\
**Main Integration Points:** Temporal‑Decision‑Core (TDC), Temporal‑Robustness‑Pack (TRP), Constraint‑&‑Load‑Engine (CLE), Mesh Telemetry Bus

# Product Requirements Document — CA1‑Activity‑Sustainer / Eligibility‑Trace (CASE)

**Product:** CA1‑Activity‑Sustainer MCP PRD\
**Author:** J\
**Date:** 2025‑07‑07

---

## 1  Overview

In biological hippocampus, CA1 neurons maintain a fading “echo” of recent activity, allowing downstream circuits to associate temporally separated events. **CASE** replicates this via an **Exponential‑Trace Buffer** that stores lightweight fingerprints of high‑salience events for ≤30 s with exponential decay. A **Sustain‑Boost Gate** extends traces during high‑threat or high‑reward windows, while a **Garbage‑Collector** purges traces when CLE signals memory pressure.\
This lets TDC link “skid → crash” even if the gap exceeds the normal window, without persisting every sensory frame to long‑term memory.

---

## 2  Goals

| Dimension        | Goal                                                                                                        |
| ---------------- | ----------------------------------------------------------------------------------------------------------- |
| **Business**     | Raise valid long‑gap edge recall by **≥18 %** vs baseline; reduce graph‑store IO by **≥35 %**.              |
| **User / Agent** | a) Trace insert ≤0.3 ms. b) Trace decay accurate ±5 %. c) Automatic sustain during CLE‑flagged high‑threat. |
| **Tech**         | Buffer RAM footprint ≤512 KB/agent; sustain boost decision ≤0.5 ms; handle ≥100 k trace ops/s cluster‑wide. |

---

## 3  Stakeholders

- **Product Owner:** J
- **Tech Lead:** Temporal Memory Squad
- **SRE & Perf:** Mesh SRE‑Core
- **Compliance & Safety:** Risk Council
- **End Users:** Mesh agents, Ops dashboards

---

## 4  Functional Requirements

| ID      | Requirement                                                                                        | Phase | Priority |
| ------- | -------------------------------------------------------------------------------------------------- | ----- | -------- |
| FR‑A1   | **InsertTrace** API `insertTrace(eventId,fingerprint,salience)` adds/refreshes trace with decay τ. | 1     | P2       |
| FR‑A2   | **DecayScheduler** updates in‑memory weights every Δt ≤ 250 ms; removes when weight<ε.             | 1     | P2       |
| FR‑A3   | **SustainBoostGate** prolongs trace lifetime (×β) if CLE `threatLevel>θ` or `reward>ρ`.            | 1     | P2       |
| FR‑A4   | **QueryTrace** `getTrace(eventId)` returns current weight & TTL for TDC promoter net.              | 1     | P2       |
| FR‑A5   | **PressurePurge** listens to CLE `memoryPressure` and accelerates decay on low‑salience traces.    | 2     | P3       |
| FR‑A6   | **CASE Metrics** export active trace count, avg TTL, sustain events, purge count.                  | 2     | P3       |
| FR‑Gov1 | Emit `TraceInserted`, `TraceDecayed`, `TracePurged`, `SustainBoostApplied` to Audit Bus.           | 1‑2   | P0       |

---

## 5  Non‑Functional Requirements

| Category           | Target                                                               |
| ------------------ | -------------------------------------------------------------------- |
| **Performance**    | Insert ≤0.3 ms; query ≤0.2 ms.                                       |
| **Reliability**    | ≥99.99 % API uptime; no trace loss on node restart (journal replay). |
| **Security**       | mTLS on all ports; salience values signed from source.               |
| **Explainability** | 100 % sustain or purge ops include rationale payload.                |

---

## 6  User / Agent Experience

1. **Event Fired:** TDC calls `insertTrace(e123,fprint,0.9)` → trace weight 1.0, TTL 30 s.
2. **Normal Decay:** After 10 s weight ≈0.37.
3. **Threat Spike:** CLE raises `threatLevel 0.85` → SustainBoostGate multiplies remaining TTL ×2.
4. **TDC Query:** promoter net `getTrace(e123)` returns weight; decides edge still eligible.
5. **Pressure Purge:** Memory pressure high → low‑salience traces force‑decayed.

---

## 7  Technical Architecture & Integrations

```
[TDC Event Stream]──►[Trace Inserter]───►[Exponential‑Trace Buffer (RocksDB + mem‑index)]
                                         ▲            │ decay tick Δt
[CLE Signals]──►[SustainBoost Gate]──────┘            ▼
                                   [Pressure Purger]─────► Audit Bus
```

---

## 8  Mesh Layer Mapping

| Mesh Layer        | Component                        | Responsibility / Port                                          |
| ----------------- | -------------------------------- | -------------------------------------------------------------- |
| **Foundation**    | TraceBufferStore                 | RocksDB WAL, snapshot restore via `ITraceStorePort`            |
| **Reasoning**     | TraceQueryService                | Fast weight lookup for promoter net (`ITraceQueryPort`)        |
| **Metacognitive** | SustainBoostGate, PressurePurger | Apply CLE signals; manage buffer health (`IBufferControlPort`) |
| **Agency**        | TraceClientAdapter               | Convenience client for agents (`ITraceClientPort`)             |
| **Business Apps** | CASEAdminWidget                  | Live buffer stats, sustain events, purge controls              |

---

## 9  Ports & Adapters

| Port Interface (Layer)         | Purpose                  | Adapter Example            |
| ------------------------------ | ------------------------ | -------------------------- |
| `ITraceInsertPort` (Reasoning) | Add/refresh trace        | gRPC `/case/insert`        |
| `ITraceQueryPort` (Reasoning)  | Retrieve weight & TTL    | Shared‑mem + gRPC fallback |
| `IBufferControlPort` (Metacog) | Sustain / purge commands | REST `/case/control`       |
| `ICleSignalListener` (Metacog) | Subscribe to CLE events  | Kafka topic `cle.signals`  |

---

## 10  Main APIs & Schemas

### Insert Trace

```json
POST /case/insert
{ "eventId":"e123", "fingerprint":"sha1‑…", "salience":0.9 }
```

Response: `{ "ttlMs":30000, "weight":1.0 }`

### Query Trace

```json
GET /case/trace/e123
```

Response:

```json
{ "eventId":"e123", "weight":0.42, "ttlMs":12000 }
```

---

## 11  Timeline & Milestones

| Phase | Duration | Exit Criteria                                                              |
| ----- | -------- | -------------------------------------------------------------------------- |
| 1     | 2 wks    | FR‑A1–A4 live; insert/query within latency targets; audit events captured. |
| 2     | 1 wk     | PressurePurge + SustainBoost; RAM ⩽512 KB/agent in load test.              |
| 3     | 1 wk     | CASEAdminWidget GA; production rollout to 2 pilot clusters.                |

---

## 12  Success Metrics

- **Recall Boost:** ≥18 % valid long‑gap edges.
- **IO Reduction:** ≥35 % fewer full memory writes.
- **Latency:** Insert/query P95 ≤ 0.3 / 0.2 ms.
- **Explainability Coverage:** 100 % sustain/purge events logged.

---

## 13  Risks & Mitigations

| Risk                          | Mitigation                                        |
| ----------------------------- | ------------------------------------------------- |
| Trace buffer overflow         | CLE‑driven purge; spill to disk with capped size. |
| Sustain boost hides decay bug | Telemetry diff; unit tests on decay math.         |
| Fingerprint collision         | Use 128‑bit hash; include event timestamp salt.   |

---

## 14  Open Questions

1. Optimal default decay τ—static or per‑agent learned?
2. Should sustain consider *reward* and *threat* separately with different β?
3. How to expose per‑trace TTL to downstream explainability widgets?

---

> **CASE:** the Mesh’s short‑term memory echo—keeping the right cues **just alive enough** for smarter temporal reasoning, without flooding storage or breaking SLOs.

