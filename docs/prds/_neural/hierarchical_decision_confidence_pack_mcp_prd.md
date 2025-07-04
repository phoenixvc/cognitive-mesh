---

## Module: Hierarchical‑Decision‑Confidence Pack (HDCP)

**Primary Personas:** Cognitive Architects, Product Ops Leads, Risk & Compliance Analysts\
**Core Value Proposition:** Adds human‑expert‑like hierarchical decision decomposition, live confidence propagation, and automatic strategy switching—so Mesh agents solve complex problems faster while gracefully recovering from missteps.\
**Priority:** P1\
**License Tier:** Enterprise\
**Platform Layers:** Reasoning, Metacognitive, Agency\
**Main Integration Points:** Temporal‑Decision‑Core (TDC), Memory & Flexibility Suite (MFS), Temporal‑Robustness‑Pack (TRP), Mesh Audit Bus

# Product Requirements Document: Hierarchical‑Decision‑Confidence Pack (HDCP)

**Product:** Hierarchical‑Decision‑Confidence Pack MCP PRD\
**Author:** J\
**Date:** 2025‑07‑05

---

## 1  Overview

HDCP equips the Cognitive Mesh with three tightly‑coupled capabilities:

1. **Hierarchical Confidence‑Weighted Decision Maker (HCW‑DM)** — recursively decomposes a goal into sub‑decisions, allocates confidence budgets, and routes each node through the cheapest reasoning mode that satisfies target confidence. 2. **ACC‑Driven Strategy‑Switch Trigger (ACC‑SST)** — monitors consecutive high‑confidence errors; emits a `strategySwitch` flag when error streaks exceed adaptive thresholds. 3. **DMFC Belief Tracker (DMFC‑BT)** — maintains Bayesian posteriors over currently active “rules” or strategies, providing priors to HCW‑DM and surfacing *why* a strategy failed.

Together they cut brittle all‑or‑nothing failures, provide fine‑grained audit trails for each reasoning step, and unblock regulated deployments demanding deterministic error attribution.

---

## 2  Goals

| Dimension        | Goal                                                                                                                                                                                     |
| ---------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Business**     | Reduce mean task completion time by **≥30 %** on multi‑step workflows; cut catastrophic failure rate (all‑steps‑wrong) by **≥50 %**; provide line‑item error attribution for compliance. |
| **User / Agent** | a) Break problems into sub‑steps automatically.  b) Escalate to higher‑cost reasoning only when confidence shortfall detected.  c) Switch to alternate strategy within 2 errors.         |
| **Tech**         | Decomposition & routing overhead ≤3 ms per decision tree; DMFC posterior update <1 ms; memory overhead <4 MB per active tree.                                                            |

---

## 3  Stakeholders

- **Product Owner:** J
- **Tech Lead:** Decision Integrity Squad
- **Compliance & Safety:** Mesh Risk Council
- **Site Reliability:** SRE‑Core
- **End Users:** Autonomous agents, Ops analysts, RegTech auditors

---

## 4  Functional Requirements

| ID      | Requirement                                                                                                                                                          | Phase                                                            | Priority |    |
| ------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------- | -------- | -- |
| FR‑H1   | **DecomposeGoal** API accepts `{goal, context}`; returns decision tree with confidence targets per node.                                                             | 1                                                                | P1       |    |
| FR‑H2   | **RouteNode** selects reasoning path (`heuristic`, `counterfactual`, `LLM‑solver`) based on `confTarget‑vs‑prior` gap; executes node, emits `DecisionNodeCompleted`. | 1                                                                | P1       |    |
| FR‑H3   | **ACC‑SST** listens to `DecisionNodeFailed` events; if `consecutiveFailures ≥ θ` *and* `avgConf ≥ φ`, emits `strategySwitch(goalId)`.                                | 1                                                                | P1       |    |
| FR‑H4   | **DMFC‑BT** updates belief posterior \`P(rule                                                                                                                        | evidence)`after each node; exposes`/beliefs/{goalId}\` endpoint. | 1        | P1 |
| FR‑H5   | **RollbackHandler** on strategySwitch rolls back affected subtree, re‑routes remaining nodes through alternate strategy.                                             | 1                                                                | P1       |    |
| FR‑H6   | **ShadowMode** toggle mirrors decisions without affecting live output for A/B error attribution.                                                                     | 2                                                                | P2       |    |
| FR‑H7   | **HDCP AdminConsole** CLI to view belief tables, tune θ/φ, inspect decomposition graph.                                                                              | 3                                                                | P2       |    |
| FR‑Gov1 | Emit `DecisionNodeCompleted`, `DecisionNodeFailed`, `StrategySwitched`, `BeliefUpdated` to Mesh Audit Bus with rationale payloads.                                   | 1‑3                                                              | P0       |    |

---

## 5  Non‑Functional Requirements

| Category           | Target                                                   |
| ------------------ | -------------------------------------------------------- |
| **Performance**    | Added overhead ≤3 ms per node; belief update ≤1 ms.      |
| **Reliability**    | ≥99.9 % decision event delivery.                         |
| **Explainability** | 100 % nodes store `confBefore`, `confAfter`, rationale.  |
| **Security**       | All tuning endpoints require RBAC `DecisionAdmin`; mTLS. |

---

## 6  User / Agent Experience

**Nominal Flow**

1. Agent calls `DecomposeGoal("Investigate abnormal spike in errors")`. 2. HCW‑DM returns decision tree with confidence targets. 3. Each node executes via `RouteNode`; cheap heuristics first. 4. DMFC‑BT updates beliefs; confidence shortfall triggers routing to LLM solver. 5. If two high‑conf nodes fail, ACC‑SST emits `strategySwitch`; subtree reruns via alternate solver set. 6. Audit widget shows tree with node outcomes, confidences, belief shifts.

**Admin Flow** *(Phase 3)*

- Run `hdcp beliefs goal‑123` to display current rule posteriors.
- `hdcp set‑theta 3` adjusts error threshold θ.
- View shadow metrics comparing live vs. alt routing.

---

## 7  Technical Architecture & Integrations

```
                   ┌──────────────────┐  DecisionNodeCompleted  ┌────────────────┐
[Agent Goal] ───►  │  HCW‑Decisioner  ├────────────────────────►│   Audit Bus    │
                   └────────┬─────────┘                         └────────────────┘
                            │
                            ▼
                   ┌──────────────────┐
                   │  RouteNode()     │─────► Heuristic Solver
                   └────────┬─────────┘
                            │ fallback                 ▲ failure
                            ▼                          │
                   Counterfactual Solver ◄─────────────┘
                            │
                            ▼  high‑cost
                         LLM Solver
                            │
           failure▲         ▼ result
                   ┌──────────────────┐
                   │  ACC‑SST         │──► strategySwitch
                   └────────┬─────────┘
                            ▼
                   ┌──────────────────┐ beliefUpdate
                   │  DMFC‑BT         │──────────────► /beliefs
                   └──────────────────┘
```

- **Ports (Reasoning):** `IDecompositionPort`, `IRoutingPort`, `IBeliefTrackerPort`, `IStrategySwitchPort`
- **Adapters (Foundation):** gRPC adapter for fast decision events; REST adapter for admin queries.
- **Storage:** Postgres table `decision_nodes`; Redis in‑memory belief cache.

---

## 8  Mesh Layer Mapping

| Mesh Layer        | Component                  | Responsibility / Port                              |
| ----------------- | -------------------------- | -------------------------------------------------- |
| **Foundation**    | DecisionNodeStore          | Durable node outcomes; audit event adapter         |
| **Reasoning**     | HCWDecisionEngine          | Tree decomposition, routing decisions              |
| **Metacognitive** | ACC‑SST, DMFC‑BT           | Error streak monitor; belief posterior maintenance |
| **Agency**        | StrategySwitchOrchestrator | Applies strategySwitch flags to running agents     |
| **Business Apps** | HDCPAdminWidget            | Visual tree, belief heat‑map, threshold tuning     |

---

## 9  Main APIs & Schemas

### DecomposeGoal

```json
POST /hdcp/decompose
{ "goal": "Investigate error spike", "context": {"service":"auth"} }
```

Response:

```json
{ "goalId":"g‑123", "nodes":[{"nodeId":"n1","type":"root","confTarget":0.95}, ...] }
```

### RouteNode

```json
POST /hdcp/route
{ "goalId":"g‑123", "nodeId":"n1", "input":{...} }
```

Response: `{ "result":"partialReport", "confAchieved":0.87, "nextNode":"n2" }`

### Belief Query

```json
GET /hdcp/beliefs/g‑123
```

Response: `{ "rulePosteriors": {"rootCause‑A":0.6,"rootCause‑B":0.3} }`

---

## 10  Timeline & Milestones

| Phase | Duration | Exit Criteria                                                                             |
| ----- | -------- | ----------------------------------------------------------------------------------------- |
| 1     | 2 wks    | FR‑H1‑H5 live; decision overhead ≤3 ms; belief query API; audit events flowing            |
| 2     | 2 wks    | ShadowMode metrics; ≥30 % task time reduction vs baseline; strategySwitch proven in chaos |
| 3     | 1 wk     | HDCPAdminWidget & CLI; compliance review; production roll‑out to 3 pilot agents           |

---

## 11  Success Metrics

- **Task Completion Time:** ‑30 % vs baseline.
- **Catastrophic Failure Rate:** ‑50 %.
- **Belief Update Latency:** 99 % <1 ms.
- **Explainability Coverage:** 100 % nodes have rationale.

---

## 12  Risks & Mitigations

| Risk                                | Mitigation                                                     |
| ----------------------------------- | -------------------------------------------------------------- |
| Strategy oscillation (thrashing)    | Hysteresis window; min cool‑down before next switch            |
| Overhead explosion on deep trees    | Depth cap + pruning; parallel execution where safe             |
| Belief drift due to noisy evidence  | Confidence‑weighted evidence, decay old evidence, human review |
| Shadow vs live divergence unnoticed | Continuous diff metrics; alert if divergence >ε                |

---

## 13  Open Questions

1. Should confidence thresholds be static per agent class or learned online?
2. Do we expose partial belief states to end users via widget, or summary only?
3. Interaction with Constraint‑&‑Load‑Engine—should low resources lower confidence targets?

---

> **HDCP:** turning complex, uncertain problems into structured, confidence‑aware decision trees—while seamlessly pivoting strategies when the facts change.

