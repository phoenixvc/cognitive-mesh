---

## Module: Hardware‑State‑Abstraction Engine (HSAE) Primary Personas: Platform Architects, SRE/Ops, Capacity Planners Core Value Proposition: Hot‑swap cognitive mesh roles and resource profiles on identical hardware—maximising utilisation while guaranteeing hard SLOs. Priority: P2 License Tier: Enterprise Platform Layers: Foundation, Metacognitive, Agency Main Integration Points: Constraint‑&‑Load‑Engine (CLE), Meta‑Orchestration‑Threat‑Balance (MOTB), Mesh Telemetry Bus, Admin UI

# Product Requirements Document — Hardware‑State‑Abstraction Engine (HSAE)

**Product:** Hardware‑State‑Abstraction Engine MCP PRD\
**Author:** J\
**Date:** 2025‑07‑06

---

## 1  Overview

Modern GPU/TPU servers frequently idle or bottleneck because each mesh service assumes exclusive hardware bindings. **HSAE** introduces a dynamic *StatePack* abstraction—encapsulating model weights, runtime parameters, and IO quotas—allowing the same physical node to flip between roles (chat inference, vector search, RAG pipeline) in <100 ms without restarts.  A lightweight **State‑Router** orchestrates packs per incoming *RoleRequest*, enforcing SLO‑aware scheduling and cgroup isolation.

---

## 2  Goals

| Dimension        | Goal                                                                                                        |
| ---------------- | ----------------------------------------------------------------------------------------------------------- |
| **Business**     | ↑ average GPU utilisation from 45 %→75 %; cut hardware spend by ≥30 % at equal SLA.                         |
| **User / Agent** | Role switch in <100 ms; guaranteed per‑role quotas (mem, vRAM, IO); zero cross‑role memory bleed.           |
| **Tech**         | Support ≥8 concurrent StatePacks per node; pack serialisation/deserialisation <50 ms; daemon memory <50 MB. |

---

## 3  Stakeholders

- **Product Owner:** J
- **Infra Eng Lead:** Runtime Platform Team
- **SRE & Capacity:** Global Platform Ops
- **Security & Compliance:** Isolation validation
- **End Users:** Mesh agents, Admin UI, Finance (cost reporting)

---

## 4  Functional Requirements

| ID      | Requirement                                                                                                         | Phase | Priority |
| ------- | ------------------------------------------------------------------------------------------------------------------- | ----- | -------- |
| FR‑H1   | **StatePack Spec** defines `{modelId, weightsRef, envVars, quota.cpu, quota.gpuMem, mounts}` JSON schema.           | 1     | P2       |
| FR‑H2   | **StateRouter** listens on `/role/request` gRPC; loads/unloads packs via OCI hooks + lazy weight mmap.              | 1     | P2       |
| FR‑H3   | **QuotaEnforcer** applies cgroup v2 limits; publishes `QuotaBreach` events to MOTB.                                 | 1     | P2       |
| FR‑H4   | **PackRegistry** stores signed packs in local object store; supports `GET /pack/{id}/download` with mTLS.           | 1     | P2       |
| FR‑H5   | **HotSwapAPI** allows Agency Layer to call `switchRole({agentId, roleId})`; target node responds ≤100 ms.           | 1     | P2       |
| FR‑H6   | **Auto‑Consolidation** algorithm groups low‑traffic roles onto fewer nodes; emits `MigrationPlan` to Orchestrator.  | 2     | P3       |
| FR‑H7   | **Admin UI Widget** visualises StatePack inventory, live role maps, utilisation heatmap, and migration suggestions. | 3     | P3       |
| FR‑Gov1 | All `RoleSwitch`, `QuotaBreach`, `MigrationPlan` events logged to Audit Bus with pack hash & signature.             | 1‑3   | P0       |

---

## 5  Non‑Functional Requirements

| Category          | Target                                                                      |
| ----------------- | --------------------------------------------------------------------------- |
| **Performance**   | Pack swap <100 ms (P95); mmap weights lazily.                               |
| **Reliability**   | ≥99.9 % successful role switch operations.                                  |
| **Security**      | Signed pack manifests; cgroup & seccomp hardening.                          |
| **Observability** | Emit Prom‑compatible metrics for switch latency, GPU utilisation, breaches. |

---

## 6  User / Agent Experience

1. Agent A sends `switchRole(role="vector_search")` to StateRouter.\
2. Router checks PackRegistry → pack `vs‑4b‑int8` present; unloads current chat pack, applies new cgroups, mmap weights; returns `ack` 62 ms later.\
3. CLE observes GPU utilisation drop, MOTB no action.\
4. Admin UI shows node N‑14 now 2× chat, 1× vector\_search; utilisation 82 %.

---

## 7  Technical Architecture & Integrations

```
┌──────────────┐ pack upload  ┌──────────────┐ mmap weights ┌──────────────┐
│ PackRegistry │────────────►│ StateRouter  │──────────────►│ Role Runtime │
└──────────────┘             └──────────────┘   cgroups     └──────────────┘
        ▲ mTLS                         │ gRPC switch      ▲ metrics
        │                              ▼                   │
┌──────────────┐<────────────Audit/Telem───────────────┐──────────────┐
│ Admin Widget │                                      │ MOTB / CLE   │
└──────────────┘                                      └──────────────┘
```

---

## 8  Mesh Layer Mapping

| Mesh Layer        | Component                     | Responsibility / Port                                      |
| ----------------- | ----------------------------- | ---------------------------------------------------------- |
| **Foundation**    | PackRegistry, QuotaEnforcer   | Storage of packs; enforce hardware quotas via `IQuotaPort` |
| **Metacognitive** | StateRouter, AutoConsolidator | Decide pack switches, migrations (`IRoleSwitchPort`)       |
| **Agency**        | RoleSwitchClient              | Agents request role changes (`IRoleRequestPort`)           |
| **Business Apps** | HSAEAdminWidget               | Visual UI + API calls to PackRegistry and monitors         |

---

## 9  Ports & Adapters

| Port Interface     | Layer              | Purpose                            | Adapter Example      |
| ------------------ | ------------------ | ---------------------------------- | -------------------- |
| `IRoleRequestPort` | Agency→Metacog     | Agent → StateRouter switch request | gRPC service         |
| `IRoleSwitchPort`  | Metacog→Agency     | Router → Agent ack / deny          | Kafka topic          |
| `IPackStorePort`   | Metacog→Foundation | Fetch/store StatePacks             | MinIO / S3 adapter   |
| `IQuotaPort`       | Foundation         | Apply + monitor cgroup limits      | libcontainer wrapper |

---

## 10  Main APIs

### Switch Role

```http
POST /hsae/role/switch
{
  "agentId": "agent‑7",
  "role": "vector_search"
}
```

Response – 200 OK

```json
{ "status":"switching","node":"N‑14","etaMs":62 }
```

### Upload Pack

```http
PUT /hsae/pack/{id}
Content‑Type: application/x‑tar
X‑Signature: ed25519‑base64
```

---

## 11  Timeline & Milestones

| Phase | Duration | Exit Criteria                                                |
| ----- | -------- | ------------------------------------------------------------ |
| 1     | 2 wks    | FR‑H1‑H5 live on staging; pack swap P95 <100 ms.             |
| 2     | 2 wks    | Auto‑Consolidation algorithm; 30 % GPU savings in load test. |
| 3     | 1 wk     | Admin Widget GA; production rollout to 2 clusters.           |

---

## 12  Success Metrics

- **Utilisation**: Avg GPU utilisation ≥75 % in production.
- **Swap Latency**: 95 th ≤ 100 ms.
- **Hardware Spend**: ≥30 % cost reduction quarter‑over‑quarter.
- **Isolation Breach**: 0 confirmed cross‑role leaks.

---

## 13  Risks & Mitigations

| Risk                         | Mitigation                                         |
| ---------------------------- | -------------------------------------------------- |
| Weight mmap thrash           | LRU cache, warm‑up predictions from CLE load graph |
| Cgroup mis‑config stalls GPU | Rigorous e2e tests + fallback safe quotas          |
| Pack tampering               | Signed manifests, SHA‑256 validation before load   |

---

## 14  Open Questions

1. Should Auto‑Consolidation respect per‑tenant isolation by default?
2. Support heterogeneous GPU types in v1 or postpone?
3. Where to surface cost‑savings analytics—inside HSAE widget or Value‑Gen suite?

---

> **HSAE:** Flip the switch—same silicon, new super‑power. Maximise utilisation while your mesh stays fast, safe, and compliant.

