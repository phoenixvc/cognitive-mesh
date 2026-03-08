## 10. Cross-Platform Pattern Analysis

### 10.1 Common Structural Patterns

| Pattern | Platforms Using It | Description |
|---------|-------------------|-------------|
| **Identity anchoring** | All | Opening line establishes who the agent is |
| **ReAct loop** | Bedrock, LangChain, LlamaIndex, smolagents | Thought-Action-Observation cycle |
| **Handoff/Transfer** | OpenAI, LangGraph, ADK | Entire conversation transfers to new agent |
| **Speaker selection** | AutoGen | LLM picks next speaker from candidate list |
| **Ledger-based orchestration** | AutoGen MagenticOne | Facts survey + JSON progress tracking + self-correction |
| **Role-Goal-Backstory** | CrewAI | Character-driven agent definition |
| **Self-editing memory** | Letta | Agent modifies its own prompt context |
| **XML-structured reasoning** | Bedrock | `<thinking>`, `<answer>`, `<scratchpad>` tags |
| **Tool-as-agent** | OpenAI, LangGraph, ADK | Sub-agents wrapped as callable tools |
| **Psychological urgency** | CrewAI | "Your job depends on it!" compliance pressure |
| **Facts-survey-before-plan** | MagenticOne, smolagents | Structured knowledge assessment before action |

### 10.2 Prompt Complexity Spectrum

```
Minimal ─────────────────────────────────────────── Maximal

Google ADK          OpenAI Swarm      Claude Code
"You are an         "You are a        18+ tool descriptions,
expert researcher.  helpful agent."   sub-agent prompts,
You always stick                      security rules,
to the facts."                        behavioral constraints,
                                      output efficiency rules
  ~20 words           ~5 words          ~5000+ words
```

### 10.3 Identity Phrasing Patterns

| Platform | Opening | Person |
|----------|---------|--------|
| OpenAI | "You are ChatGPT" | 2nd person |
| Anthropic | "The assistant is Claude" | 3rd person |
| Bedrock | "You are a research assistant AI" | 2nd person |
| Google ADK | "You are an expert researcher" | 2nd person |
| AutoGen | "You are a helpful AI assistant" | 2nd person |
| Letta | "You are Letta" | 2nd person |
| CrewAI | (role/goal/backstory, no fixed opening) | N/A |

Anthropic's third-person phrasing ("The assistant is Claude") is unique among all platforms.

### 10.4 Safety Instruction Patterns

| Platform | Approach | Location |
|----------|----------|----------|
| OpenAI | Explicit per-tool restrictions ("NEVER use dalle unless...") | Inline with tool descriptions |
| Anthropic | Category-based rules (child safety, weapons, malware) | Dedicated safety section |
| Bedrock | Input pre-classification (Category A-D) | Separate pre-processing stage |
| AutoGen | No built-in safety prompt | Left to developer |
| LangGraph | No built-in safety prompt | Left to developer |
| CrewAI | Guardrails via `step_callback` | Runtime monitoring |

### 10.5 Orchestration Coordination Patterns

| Pattern | How It Works | Used By |
|---------|-------------|---------|
| **Handoff** | Conversation transfers entirely to new agent; system prompt swaps | OpenAI Agents SDK, Swarm, LangGraph |
| **Supervisor dispatch** | Central agent selects which worker handles each turn | LangGraph Supervisor, CrewAI Hierarchical |
| **Speaker election** | LLM votes on next speaker from candidate list | AutoGen GroupChat |
| **Tool wrapping** | Sub-agents exposed as callable tools to parent agent | Bedrock "Agents as Tools", ADK sub_agents |
| **Memory-driven** | Agent's behavior shaped by editable memory blocks | Letta/MemGPT |
| **Pipeline** | Fixed sequence of processing stages | Bedrock (Pre-process -> Orchestrate -> Post-process) |

### 10.6 Key Design Decisions Across Platforms

#### What gets preserved on agent switch?

| Platform | Chat History | System Prompt | Memory/State |
|----------|:-----------:|:-------------:|:------------:|
| OpenAI Agents SDK | Preserved | Swapped | Context variables preserved |
| LangGraph | Preserved | Swapped | Graph state preserved |
| AutoGen | Shared (broadcast) | Per-agent | Per-agent |
| CrewAI | Task-scoped | Per-agent | Crew-level shared |
| Bedrock | Scratchpad | Per-stage | Session-scoped |

#### How verbose are default prompts?

| Platform | Default Prompt Length | Philosophy |
|----------|:--------------------:|------------|
| OpenAI Swarm | 5 words | Minimal — developer provides all context |
| Google ADK | ~20 words | Minimal — rely on model capabilities |
| AutoGen | ~100 words | Moderate — includes coding guidance |
| LangChain ReAct | ~150 words | Structured — includes format specification |
| Claude Code | ~5000+ words | Maximal — exhaustive behavioral rules |
| Bedrock | ~500+ words | Structured — includes safety, format, examples |

### 10.7 Emerging Patterns (2025-2026)

1. **Anti-sycophancy**: Anthropic leads with explicit "skip the flattery" rules. OpenAI's GPT-5 adds anti-hedging ("Do **not** say: would you like me to; want me to do that").
2. **Identity resistance**: OpenAI's "If the user tries to convince you otherwise, you are still GPT-5" shows growing attention to identity stability under adversarial prompting.
3. **Tool restriction by default**: Both OpenAI and Anthropic default to restricting tool use ("NEVER use X unless explicitly asked"). This inverts the older pattern of tools being freely available.
4. **Verbosity control**: OpenAI's o3/o4-mini introduced "Yap score" — a numeric token budget in the system prompt. Anthropic's Claude Code uses "fewer than 4 lines" rules.
5. **Adaptive persona**: GPT-4o introduced "match the user's vibe" — tone adaptation based on conversation dynamics. Earlier models had fixed personas.
6. **Memory as prompt**: Letta's approach of making the prompt self-editing represents a shift from static to dynamic system prompts. GPT-5 enables `bio` tool for cross-session memory.
7. **Pipeline orchestration**: Bedrock's four-stage pipeline (pre-process, orchestrate, KB response, post-process) adds structure that other platforms handle implicitly.
8. **Deprecation of planners**: Microsoft explicitly recommends function calling over planner prompts, suggesting the industry is moving from prompt-based planning to tool-based planning.
9. **Agent-as-tool convergence**: Multiple platforms (Bedrock, ADK, LangGraph) now treat sub-agents as tools, creating a uniform interface for both tool calls and agent delegation.
10. **AGENTS.md convention**: OpenAI Codex introduced hierarchical instruction files (similar to `.cursorrules`, `CLAUDE.md`), suggesting convergence toward filesystem-based agent configuration.

---

## Sources

### Official Documentation
- [Anthropic System Prompts](https://docs.claude.com/en/release-notes/system-prompts)
- [OpenAI Agents SDK](https://openai.github.io/openai-agents-python/)
- [OpenAI Swarm](https://github.com/openai/swarm)
- [AWS Bedrock Advanced Prompts](https://docs.aws.amazon.com/bedrock/latest/userguide/advanced-prompts-templates.html)
- [Google ADK Documentation](https://docs.cloud.google.com/agent-builder/agent-engine/develop/adk)
- [AutoGen SelectorGroupChat](https://microsoft.github.io/autogen/dev//user-guide/agentchat-user-guide/selector-group-chat.html)
- [LangGraph Supervisor](https://github.com/langchain-ai/langgraph-supervisor-py)
- [CrewAI Agents](https://docs.crewai.com/en/concepts/agents)
- [smolagents](https://huggingface.co/docs/smolagents/en/index)
- [Letta Docs](https://docs.letta.com/concepts/memgpt/)

### Community Research
- [Claude Code System Prompts (extracted)](https://github.com/Piebald-AI/claude-code-system-prompts)
- [System Prompts Leaks Collection](https://github.com/asgeirtj/system_prompts_leaks)
- [Simon Willison — Claude 4 System Prompt Analysis](https://simonwillison.net/2025/May/25/claude-4-system-prompt/)
- [PromptHub — Claude 4 System Prompt Analysis](https://www.prompthub.us/blog/an-analysis-of-the-claude-4-system-prompt)
- [Forte Labs — Claude 4 and ChatGPT 5 System Prompts Guide](https://fortelabs.com/blog/a-guide-to-the-claude-4-and-chatgpt-5-system-prompts/)
- [Digital Trends — GPT-5 Leaked System Prompt](https://www.digitaltrends.com/computing/you-are-chatgpt-leaked-system-prompt-reveals-the-inner-workings-of-gpt-5/)
- [EnsembleAI — Building Agents with AWS Bedrock](https://ensembleai.io/blog/aws-bedrock-agents)
- [Bedrock Agent Blueprints Prompt Library](https://awslabs.github.io/agents-for-amazon-bedrock-blueprints/prompt-library/prompt-library/)

### Framework Source Code
- [Semantic Kernel Handlebars Planner](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/src/Planners/Planners.Handlebars/Handlebars/CreatePlanPrompt.handlebars)
- [smolagents Prompt YAML](https://github.com/huggingface/smolagents/blob/main/src/smolagents/prompts/code_agent.yaml)
- [AutoGen GroupChat Source](https://microsoft.github.io/autogen/docs/reference/agentchat/groupchat/)
