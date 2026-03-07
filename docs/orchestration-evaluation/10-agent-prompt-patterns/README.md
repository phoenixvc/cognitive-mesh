# Agent Prompt Patterns & System Instructions

Research into the actual prompts, system instructions, and implementation details used by major AI agent platforms and orchestration frameworks.

## Why This Matters

Agent behavior is fundamentally shaped by system prompts and instructions. Understanding how major platforms structure these prompts reveals:

- **Orchestration patterns** — How platforms coordinate multiple agents (handoffs vs. supervisor dispatch vs. speaker election)
- **Safety architecture** — Where and how safety rules are enforced (inline, pre-processing pipeline, or runtime guardrails)
- **Prompt engineering best practices** — What works at scale across millions of users
- **Design trade-offs** — Minimal prompts (Google ADK: ~20 words) vs. maximal prompts (Claude Code: ~5000+ words)

## Contents

| Document | Description |
|----------|-------------|
| [Agent Prompts & Instructions](agent-prompts-and-instructions.md) | Comprehensive catalog of prompts across 9 platforms with cross-platform analysis |

## Platforms Covered

| Platform | What's Documented |
|----------|------------------|
| **OpenAI** | ChatGPT system prompt (GPT-4.5/5), Swarm defaults, Agents SDK handoff pattern, Assistants API, GPT-5 Agent Mode |
| **Anthropic** | Claude 4 system prompt (official), Claude Code prompt structure, tool use format |
| **AWS Bedrock** | Default orchestration prompt, pre-processing classifier, scratchpad format, 4-stage pipeline |
| **Google ADK** | Agent definition structure, YAML config, multi-agent orchestration, A2A Agent Cards |
| **Microsoft** | AutoGen GroupChat speaker selection prompt, SelectorGroupChat, AssistantAgent defaults, Semantic Kernel planner |
| **LangGraph** | Supervisor prompt, ReAct agent format, Plan-and-Execute pattern |
| **CrewAI** | Role-Goal-Backstory pattern, hierarchical manager, delegation control, prompt templates |
| **smolagents** | CodeAgent prompt (Thought-Code-Observation), ToolCallingAgent, planning prompts |
| **Letta (MemGPT)** | Self-editing memory blocks, inner thoughts pattern, memory management tools |

## Key Findings

### Orchestration Patterns

| Pattern | Platforms | How It Works |
|---------|-----------|-------------|
| Handoff/Transfer | OpenAI, LangGraph | Full conversation transfer; system prompt swaps |
| Supervisor Dispatch | LangGraph, CrewAI | Central agent selects workers |
| Speaker Election | AutoGen | LLM votes on next speaker |
| Tool Wrapping | Bedrock, ADK | Sub-agents as callable tools |
| Self-Editing Memory | Letta | Agent modifies its own context |
| Pipeline | Bedrock | Fixed sequence of processing stages |

### Prompt Complexity Spectrum

```
Minimal ───────────────────────────────────────────── Maximal

Google ADK    Swarm    AutoGen    LangChain    Bedrock    Claude Code
 ~20 words   ~5 words  ~100 words ~150 words  ~500+ words ~5000+ words
```

### Notable Design Decisions

1. **Anthropic uses third person** — "The assistant is Claude" vs. everyone else's "You are X"
2. **Anti-sycophancy is explicit** — Claude's prompt lists specific banned adjectives (good, great, fascinating, profound, excellent)
3. **OpenAI restricts tools by default** — "NEVER use dalle unless explicitly asked"
4. **Bedrock has a safety pipeline** — Input classification (Categories A-D) happens before orchestration
5. **CrewAI is character-driven** — Agents get role, goal, and backstory rather than instructions
6. **Letta makes prompts dynamic** — Memory blocks are editable by the agent itself

## Relationship to Other Evaluation Sections

This section complements:
- [03-external-engines/agent-platforms/](../03-external-engines/agent-platforms/) — Platform-level evaluation and scoring
- [03-external-engines/agent-runtimes/](../03-external-engines/agent-runtimes/) — Runtime framework comparisons
- [09-custom-vs-established/](../09-custom-vs-established/) — Build vs. buy analysis
