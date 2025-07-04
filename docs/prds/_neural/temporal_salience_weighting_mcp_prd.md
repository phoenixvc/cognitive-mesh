---

## Module: Temporal‑Salience‑Weighting Engine (TSWE)

**Primary Personas:** Cognitive Architects, Threat Analysts, Product Owners\
**Core Value Proposition:** Dynamically amplifies or dampens temporal association scores in real‑time according to *situational salience* (threat, reward, novelty, user priority) so that only context‑critical edges survive gating—boosting recall when it matters and suppressing noise the rest of the time.\
**Priority:** P2\
**License Tier:** Enterprise\
**Platform Layers:** Metacognitive, Reasoning\
**Main Integration Points:** Temporal‑Decision‑Core (TDC), Constraint‑&‑Load‑Engine (CLE), Meta‑Orchestration‑Threat‑Balance (MOTB), Mesh Telemetry Bus, Widget Registry

# Product Requirements Document — Temporal‑Salience‑Weighting Engine (TSWE)

**Product:** Temporal‑Salience‑Weighting Engine MCP PRD\
**Author:** J\
**Date:** 2025‑07‑07

---

## 1  Overview

TDC already links events across time, but its promoter score alone cannot distinguish *important* from *irrelevant* edges under high load. **TSWE** inserts a Salience‑Weighting Layer that multiplies each candidate edge’s promoter score by a *Contextual Salience Factor (CSF)* derived from live signals (threat, reward, novelty, user‑tagged priority, mission phase).  A **Context Fusion Model** converts heterogeneous inputs into a single 0‑2× multiplier (boost or suppression).  A **Burst‑Normalizer** prevents runaway boosts during noisy “alert storms.”\
Net result: genuine high‑stakes patterns (“skid → crash” during test track) survive even in busy graphs, while low‑stakes chatter is automatically attenuated.

---

## 2  Goals

| Dimension        | Goal                                                                                                                                                        |
| ---------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Business**     | Increase precision of critical temporal edges by **≥20 %** without recall loss; reduce average promoter score variance by **≥35 %** in high‑load scenarios. |
| **User / Agent** | a) Salience factor applied <0.5 ms/edge. b) No single salience source can boost factor >2× alone. c) Full rationale string per weighted edge.               |
| **Tech**         | Handle ≥50 k edge weights/s; memory footprint <1 MB per active salience profile; adaptive decay of stale context ≤5 s.                                      |

---

## 3  Stakeholders

- **Product Owner:** J
- **Tech Lead:** Salience Ops Squad
- **SRE & Risk:** Platform Reliability
- **Compliance:** Explainability & Audit
- **End Users:** Mesh agents, Ops dashboards

---

## 4  Functional Requirements

| ID      | Requirement                                                                                                             | Phase | Priority |
| ------- | ----------------------------------------------------------------------------------------------------------------------- | ----- | -------- |
| FR‑S1   | **RegisterSalienceSource** API to declare a stream `{sourceId, weightRange, decayMs}` (e.g. CLE‑threat, user‑priority). | 1     | P2       |
| FR‑S2   | **ComputeCSF** for each candidate edge: fuse live salience signals with learned weights → factor *f* ∈ [0.5, 2.0].      | 1     | P2       |
| FR‑S3   | **ApplyWeight** multiplies promoter score from TDC by *f* before final gate decision.                                   | 1     | P2       |
| FR‑S4   | **Burst‑Normalizer** detects >𝜆 salience spikes within Δt and temp‑caps *f* to prevent over‑boost loops.               | 1     | P2       |
| FR‑S5   | **DecayScheduler** reduces stale salience values toward baseline with exponential decay τ.                              | 1     | P2       |
| FR‑S6   | **AdminPolicyAPI** CRUD for global weights, caps, decay τ; signed commits, RBAC `SalienceAdmin`.                        | 2     | P3       |
| FR‑S7   | **TSWE Dashboard Widget** shows live salience sources, CSF distribution, burst events, and policy editor.               | 3     | P3       |
| FR‑Gov1 | Emit `SalienceApplied`, `BurstNormalized`, `SalienceSourceUpdate` events to Audit Bus with full rationale.              | 1‑3   | P0       |

---

## 5  Non‑Functional Requirements

| Category           | Target                                                                           |
| ------------------ | -------------------------------------------------------------------------------- |
| **Performance**    | ≤0.5 ms CSF computation; ≤0.3 ms apply weight.                                   |
| **Reliability**    | ≥99.99 % salience event delivery; fallback to neutral factor 1.0 on source loss. |
| **Explainability** | 100 % weighted edges log contributing sources & weights.                         |
| **Security**       | Salience streams ACL‑protected; policy updates signed & versioned.               |

---

## 6  User / Agent Experience

1. **Edge Proposal:** TDC sends promoter=0.42 for e1→e2.\
2. **Salience Fuse:** CLE threat=0.8 (weight 0.6), user priority=high (weight 0.4) → CSF 1.48.\
3. **Weight Apply:** New promoter′ = 0.62 (> θ) → edge accepted.\
4. **Burst Event:** 100 threat spikes in 2 s → Burst‑Normalizer caps factor at 1.6 temporarily.\
5. **Dashboard:** Ops sees salience gauge, recent burst cap event, can adjust decay τ.

---

## 7  Technical Architecture & Integrations

```
[TDC Promoter] ─┐                               ┌─► [Edge Gate Decision]
                │  promoter                    │
[Salience Streams] ─► [Context Fusion Model] ─►│ factor f
                │                               │
[Burst Normalizer] ◄────────── back‑pressure ───┘
```

- **Fusion Model:** Weighted linear combination with optional small neural corrector (ONNX).
- **Burst Normalizer:** Sliding‑window counter + exponential backoff.
- **Storage:** Redis time‑series for salience values; Postgres for policy store.

---

## 8  Mesh Layer Mapping

| Mesh Layer        | Component                           | Responsibility / Port                                |
| ----------------- | ----------------------------------- | ---------------------------------------------------- |
| **Foundation**    | SalienceStoreAdapter                | Durable salience streams via `ISalienceStorePort`    |
| **Metacognitive** | ContextFusionModel, BurstNormalizer | Compute CSF; apply caps                              |
| **Reasoning**     | SalienceWeightApplier               | Multiply promoter; expose `ISalienceWeightPort`      |
| **Agency**        | SalienceClientAdapter               | Agents push priority salience (`IAgentSaliencePort`) |
| **Business Apps** | TSWEAdminWidget                     | Policy editor, live gauges, burst event feed         |

---

## 9  Ports & Adapters

| Port Interface (Layer)         | Purpose                              | Adapter Example            |
| ------------------------------ | ------------------------------------ | -------------------------- |
| `ISalienceSourcePort` (Meta)   | Publish salience value streams       | gRPC stream `/salience`    |
| `ISalienceWeightPort` (Reason) | Apply CSF & return weighted promoter | Shared‑mem + gRPC fallback |
| `ISaliencePolicyPort` (Meta)   | CRUD global weights / caps / decay τ | REST `/tswe/policy`        |
| `ISalienceTelemetryPort`       | Emit audit & metrics events          | Kafka topic `tswe.events`  |

---

## 10  Main APIs & Schemas

### Publish Salience Value

```json
POST /tswe/salience
{ "sourceId":"cle_threat", "agentId":"agent‑7", "value":0.84, "ts":"2025‑07‑07T12:34:56Z" }
```

### Query Weighted Promoter (internal)

```json
POST /tswe/weight
{ "edgeId":"uuid", "rawPromoter":0.42, "contextId":"ctx‑55" }
```

Response: `{ "factor":1.48, "weightedPromoter":0.62 }`

---

## 11  Timeline & Milestones

| Phase | Duration | Exit Criteria                                                            |
| ----- | -------- | ------------------------------------------------------------------------ |
| 1     | 2 wks    | FR‑S1‑S5 live; CSF calc <0.5 ms; precision +20 % vs no‑salience baseline |
| 2     | 1 wk     | AdminPolicyAPI; Burst‑Normalizer chaos test; audit coverage 100 %        |
| 3     | 1 wk     | TSWEAdminWidget GA; production rollout to 2 pilot clusters               |

---

## 12  Success Metrics

- **Critical Edge Precision:** +20 % vs baseline.
- **Promoter Variance Reduction:** ‑35 % in high‑load tests.
- **Weight Latency:** 99 % ≤0.5 ms.
- **Explainability Coverage:** 100 % weighted edges store salience factors.

---

## 13  Risks & Mitigations

| Risk                            | Mitigation                                                |
| ------------------------------- | --------------------------------------------------------- |
| Salience source spoofing        | Signed source IDs, ACL, anomaly detection on value spikes |
| Over‑boost leads to false edges | Burst‑Normalizer caps; daily drift reports                |
| Policy mis‑config breaks recall | Shadow comparison, staged rollout, policy signing         |

---

## 14  Open Questions

1. Minimum salience sources for v1—threat & reward only, or include novelty?
2. Should agent‑provided *priority* salience be capped lower than system threat?
3. Does MOTB need authority to *force* salience factor to 1.0 during lockdown?

---

> **TSWE:** turning raw context signals into *just‑right* temporal weights—so your Mesh links what matters and ignores the rest.

