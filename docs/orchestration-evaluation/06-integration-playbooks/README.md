# Integration Playbooks

How to integrate each internal repository with the top-ranked external engines. Playbooks are organized by target engine, covering all 4 internal repos.

## Target Engine Selection

Based on rankings across all 5 use-case profiles, these are the top engines to integrate with:

| Engine | Why Selected |
|--------|-------------|
| [Temporal](temporal-integration.md) | #1 for Batch (91.4%) and Durable (94.4%). Gold standard for production workflows. |
| [Inngest](inngest-integration.md) | #1 for Interactive (80.0%) and Event-Driven (82.0%). Best developer experience. |
| [LangGraph](langgraph-integration.md) | #1 for Multi-Agent (80.4%). Purpose-built for AI agent coordination. |

## Quick Decision Guide

| If your primary need is... | Integrate with... | Playbook |
|---------------------------|-------------------|----------|
| Durable workflows that survive crashes | Temporal | [temporal-integration.md](temporal-integration.md) |
| Event-driven serverless with great DX | Inngest | [inngest-integration.md](inngest-integration.md) |
| Multi-agent AI reasoning with graphs | LangGraph | [langgraph-integration.md](langgraph-integration.md) |
| All of the above (hybrid) | Temporal + Inngest | See hybrid approach in each playbook |
