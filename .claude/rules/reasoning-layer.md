---
paths:
  - "src/ReasoningLayer/**/*.cs"
---

# Reasoning Layer Rules

## ConclAIve Recipes
Three structured reasoning strategies, auto-selected by the orchestrator:
- **Debate & Vote** (DebateReasoningEngine): 4-6 perspectives, cross-examination, synthesis
- **Sequential** (SequentialReasoningEngine): 3-5 specialized phases with context passing
- **Strategic Simulation** (StrategicSimulationEngine): SWOT/Porter/PESTEL pattern analysis

All recipes must output confidence levels and full reasoning traces.

## Ethical Reasoning
- **Normative Agency** (NormativeAgencyEngine): Brandom-based — validates agent actions against normative commitments
- **Informational Dignity** (InformationEthicsEngine): Floridi-based — validates user data handling
- Both are blocking checks called from the Agency Layer's `ExecuteSingleAgent`
- Never skip ethical checks for user-facing workflows

## Adding New Reasoning Engines
1. Implement the port interface (e.g., `IMyReasoningPort`)
2. Engine must be stateless — all state via method parameters
3. Return structured results with confidence scores
4. Wire into ConclAIve orchestrator's recipe selection logic
