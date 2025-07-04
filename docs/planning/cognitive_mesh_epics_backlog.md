# Cognitive Mesh Epics Backlog

> A single source‑of‑truth for programme‑level planning.  Each **Epic** bundles one or more PRDs that must ship together to deliver end‑to‑end value.  Child user‑stories / tasks will be broken out in Jira after backlog grooming.

---

## 🏛 Foundational Infrastructure (P0)

| Epic ID | Epic Name | Linked PRDs | Outcome / Success Criteria | Dependencies | Status |
| --- | --- | --- | --- | --- | --- |
| **FI‑01** | **Zero‑Trust Security Backbone** | `security-zero-trust-infrastructure-framework.md` | End‑to‑end authNZ, secrets, audit. **Δ**: external penetration test passes; ≥99% policy coverage. | None | 🟢 Complete |
| **FI‑02** | **Ethical & Legal Compliance Core** | `ethical-legal-compliance-framework.md` | Policy engine blocks non‑compliant outputs; automated DPIA reports. | FI‑01 | 🟠 Ready |
| **FI‑03** | **NIST AI RMF Governance Suite** | Backend + Widget PRDs | Full AI‑risk register, maturity scoring dashboard live. | FI‑01‑02 | ⚪ Not Started |
| **FI‑04** | **Adaptive Balance & Continuous Improvement** | Backend + Widget PRDs | Live spectrums tune risk↔reward; P95 decision error ≤1%. | FI‑01‑02 | ⚪ Not Started |

---

## 🤖 Agentic & Cognitive Frameworks (P1)

| Epic ID | Epic Name | Linked PRDs | Outcome / Success Criteria | Dependencies | Status |
| --- | --- | --- | --- | --- | --- |
| **AC‑01** | **Agentic AI System** | Backend + Widget PRDs | Registry & orchestrator support ≥10 agents, SLA‑based routing. | FI‑01‑04 | 🟡 In Progress |
| **AC‑02** | **Cognitive Sandwich Workflow** | Backend PRD | Phase‑based HITL workflow reduces hallucinations by ≥40%. | AC‑01 | ⚪ Not Started |
| **AC‑03** | **Cognitive Sovereignty Control** | Widget PRD | Users toggle autonomy level; audit trail proves authorship. | AC‑02 | ⚪ Not Started |

---

## 🧠 Temporal & Flexible Reasoning (P0 / P1 Mixed)

| Epic ID | Epic Name | Constituent Capabilities (Concept IDs) | Linked PRD(s) | Key Metric | Status |
| --- | --- | --- | --- | --- | --- |
| **TR‑01** | **Temporal Decision Core (TDC)** | 1 Dual‑Circuit Gate, 2 Adaptive Window, 6 Confidence‑Weighted Decision Maker | *TDC PRD* | ≤5% spurious temporal links; strategy‑switch latency <100 ms | Draft |
| **TR‑02** | **Memory & Flexible Strategy (MFS)** | 3 Episodic What‑Where‑When, 7 ACC Trigger, 10 Switch & Dial Engine | *MFS PRD* | Recall F1 ↑ 30%, sub‑goal failure recovery ↑ 50% | Draft |
| **TR‑03** | **Constraint‑Aware Reasoning Suite** | 14 Multi‑Dim Constraints, 15 Constraint Interaction, 16 Meta‑Cognitive Orchestrator | *CAR PRD (planned)* | SLA breaches ↓ 40%; auto‑satisficing decisions logged | Planned |

---

## 🖥 Local‑First Tools (P2)

| Epic ID | Epic Name | Linked PRDs | Outcome / Success Criteria | Dependencies | Status |
| --- | --- | --- | --- | --- | --- |
| **LT‑01** | **PrivateMesh (On‑Prem MCP Client)** | `Local & Private MCP Client PRD` | Install→first chat <10 min; zero egress verified; P95 lat <800 ms. | FI‑01 | Planned |
| **LT‑02** | **MemoryAgent (IDE Context Recall)** | `MemoryAgent PRD` | Recall latency <200 ms; ≥5 DAU devs; 100% local encryption. | LT‑01 (for shared libs) | Planned |
| **LT‑03** | **SynthDataGen (QA/ML Synthetic Data)** | `SynthDataGen PRD` | Gen+eval+viz <2 s for 1 k rows; pass PII scan. | FI‑01‑04 | Planned |

---

## 📊 Value & Impact (P2)

| Epic ID | Epic Name | Linked PRDs | Outcome / Success Criteria | Dependencies | Status |
| --- | --- | --- | --- | --- | --- |
| **VI‑01** | **Value Generation Analytics** | Backend + Widget PRDs | ROI dashboard live; feature adoption telemetry ≥90% coverage. | FI‑01‑04, AC‑01 | 🟡 In Progress |
| **VI‑02** | **Impact‑Driven AI Metrics** | Backend + Widget PRDs | Psychological‑safety score >= 80/100; mission impact OKR tracked. | VI‑01 | ⚪ Not Started |

---

## 🚀 Innovation & Gamification (P3)

| Epic ID | Epic Name | Linked PRDs | Outcome / Success Criteria | Dependencies | Status |
| --- | --- | --- | --- | --- | --- |
| **IG‑01** | **AI Usage Heatmap + Coaching** | `_more_prd/ai-usage-heatmap-automated-coaching.md` | DAU +40%; personalised tips delivered within 24 h. | VI‑01 | ⚪ Not Started |
| **IG‑02** | **AI Maturity Gamification Track** | Backend + Widgets PRDs | Maturity score ↑ 60% in 3 mo; weekly challenge opt‑in ≥30%. | IG‑01 | ⚪ Not Started |

---

## 🔐 Marketplace & Widget Governance

| Epic ID | Epic Name | Linked Docs | Outcome / Success Criteria | Dependencies | Status |
| --- | --- | --- | --- | --- | --- |
| **MW‑01** | **Secure Widget Lifecycle Pipeline** | *Widget Security & Lifecycle Mini‑Spec* | 100% widgets pass automated review; mean publish time <24 h. | FI‑01 | Draft |
| **MW‑02** | **Persona→Widget→Platform Mapping** | Flow Map Doc | Dynamic dashboard templates auto‑generated per persona. | MW‑01 | Draft |

---

### 📌 Next Steps
1. **Product & Engineering** review this epic backlog; confirm scope & ownership.
2. Import epics into Jira → break into user‑stories/tasks.
3. Begin PI planning: prioritise FI‑02, FI‑03, LT‑01 for upcoming sprint.

