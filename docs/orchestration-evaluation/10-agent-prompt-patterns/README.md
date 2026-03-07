# Agent Prompt Patterns & System Instructions

Research into the actual prompts, system instructions, and implementation details used by major AI agent platforms and orchestration frameworks.

## Why This Matters

Agent behavior is fundamentally shaped by system prompts and instructions. Understanding how major platforms structure these prompts reveals:

- **Orchestration patterns** — How platforms coordinate multiple agents (handoffs vs. supervisor dispatch vs. speaker election)
- **Safety architecture** — Where and how safety rules are enforced (inline, pre-processing pipeline, or runtime guardrails)
- **Prompt engineering best practices** — What works at scale across millions of users
- **Design trade-offs** — Minimal prompts (Google ADK: ~20 words) vs. maximal prompts (Claude Code: ~5000+ words)

## Documents

| # | File | Lines | Platform | Key Content |
|---|------|-------|----------|-------------|
| 1 | [01-openai.md](01-openai.md) | 419 | OpenAI | ChatGPT system prompts (GPT-4.5 to GPT-5), Swarm, Agents SDK, Codex, Deep Research |
| 2 | [02-anthropic.md](02-anthropic.md) | 170 | Anthropic | Claude system prompt, Claude Code, tool use template |
| 3 | [03-aws-bedrock.md](03-aws-bedrock.md) | 146 | AWS Bedrock | ReAct orchestration, pre-processing classifier, Messages API template |
| 4 | [04-google-adk.md](04-google-adk.md) | 146 | Google ADK / Vertex AI | Identity prompt (exact source), transfer instructions, AutoFlow, A2A |
| 5 | [05-microsoft.md](05-microsoft.md) | 237 | Microsoft | AutoGen GroupChat, MagenticOne ledger, Semantic Kernel planners |
| 6 | [06-langchain.md](06-langchain.md) | 89 | LangChain / LangGraph | Supervisor agent, ReAct prompt, Plan-and-Execute |
| 7 | [07-crewai.md](07-crewai.md) | 115 | CrewAI | Role-Goal-Backstory, assembled prompt, hierarchical manager |
| 8 | [08-huggingface-smolagents.md](08-huggingface-smolagents.md) | 72 | HuggingFace smolagents | CodeAgent, ToolCallingAgent, planning prompts |
| 9 | [09-letta.md](09-letta.md) | 186 | Letta (MemGPT) | Self-editing memory blocks, inner thoughts, memory tools |
| 10 | [10-cross-platform-analysis.md](10-cross-platform-analysis.md) | 134 | Cross-Platform | Pattern comparison, orchestration taxonomy, prompt complexity spectrum |

> The original combined document is preserved in [agent-prompts-and-instructions.md](agent-prompts-and-instructions.md) (1,735 lines).

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
