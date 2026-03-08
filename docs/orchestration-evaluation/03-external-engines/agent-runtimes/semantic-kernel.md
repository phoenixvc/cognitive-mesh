# Semantic Kernel

## What It Is

Microsoft Semantic Kernel is an open-source SDK for building AI agents and integrating LLMs into applications. Available for C#, Python, and Java. It provides a kernel abstraction that coordinates plugins (skills), planners, and memory with LLM providers. Part of the broader Microsoft AI platform alongside Azure AI services.

## Architecture & Orchestration Pattern

**Pattern**: Kernel-based plugin orchestration with planners and function calling

```text
┌────────────────────────────────────┐
│         Semantic Kernel           │
│  ┌──────────────────────────────┐ │
│  │          Kernel              │ │
│  │  ┌────────┐  ┌───────────┐  │ │
│  │  │Plugins │  │ AI Service│  │ │
│  │  │(Skills)│  │ Connectors│  │ │
│  │  └────────┘  └───────────┘  │ │
│  │  ┌────────┐  ┌───────────┐  │ │
│  │  │Planner │  │  Memory   │  │ │
│  │  │        │  │           │  │ │
│  │  └────────┘  └───────────┘  │ │
│  └──────────────────────────────┘ │
│  ┌──────────────────────────────┐ │
│  │   Filters (pre/post hooks)  │ │
│  └──────────────────────────────┘ │
└────────────────────────────────────┘
```

- **Kernel**: Central coordinator for AI services, plugins, and memory
- **Plugins**: Functions (native code or LLM prompts) that agents can invoke
- **Planners**: Automatic function composition (Handlebars, Stepwise)
- **AI Connectors**: OpenAI, Azure OpenAI, Hugging Face, etc.
- **Memory**: Vector store integration for RAG
- **Filters**: Pre/post execution hooks for logging, telemetry, governance
- **Process Framework**: Multi-step workflow with state machine support (newer feature)

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 3.8 | 76.0% | Medium | In-process execution is fast. LLM call latency dominates. No polling overhead. |
| Scalability | 3.5 | 70.0% | Medium | Scales with host application. No built-in distributed coordination. Azure integration for cloud scale. |
| Efficiency | 3.5 | 70.0% | Medium | C# is efficient. Plugin model is lightweight. Memory/vector operations add I/O. |
| Fault Tolerance | 3.2 | 64.0% | Medium | Plugin-level retry possible. Filter hooks for error handling. But no built-in durable execution or checkpoint. |
| Throughput | 3.2 | 64.0% | Medium | Async execution. Parallel function calls. But single-kernel coordination. |
| Maintainability | 4.2 | 84.0% | High | Clean SDK design. Strong typing (C#). Good documentation. Microsoft backing ensures stability. |
| Determinism | 3.8 | 76.0% | Medium | Filter hooks provide observability. Planner outputs are logged. But no built-in replay. |
| Integration Ease | 4.2 | 84.0% | High | Multi-language (C#, Python, Java). Azure ecosystem. Rich connector library. Microsoft support. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.74 | 74.8% |
| Batch | 3.44 | 68.8% |
| Long-Running Durable | 3.30 | 66.0% |
| Event-Driven Serverless | 3.40 | 68.0% |
| Multi-Agent Reasoning | 3.84 | 76.8% |

## When to Use (Ranked by Use Case)

| Use Case | Rank | Fit |
|----------|:----:|-----|
| .NET AI agent development | **1st** | Best-in-class for C# ecosystem. Microsoft backing. |
| Multi-agent AI coordination | **2nd** | Multi-agent support with process framework. LangGraph is more mature for graphs. |
| Enterprise AI integration | **1st** | Azure integration. Enterprise support. Multi-language. |
| Interactive developer tools | **2nd** | Good SDK DX. In-process execution is fast. |

## When NOT to Use

- Durable long-running workflows (use Temporal + SK plugins)
- Non-Microsoft ecosystems where Azure isn't relevant
- Simple workflow automation (overkill for non-AI orchestration)
- Teams needing graph-based agent execution (LangGraph is more specialized)

## Maturity Signals

- **GitHub stars**: ~27k ([GitHub: microsoft/semantic-kernel](https://github.com/microsoft/semantic-kernel))
- **Corporate backing**: Microsoft (direct investment)
- **Multi-language**: C# (primary), Python, Java
- **Release cadence**: Regular; following Microsoft release practices
- **Community**: Active; Microsoft-backed documentation and samples

## Sources

- [Semantic Kernel GitHub Repository](https://github.com/microsoft/semantic-kernel) — star count, contributors, release cadence
- [Semantic Kernel Documentation](https://learn.microsoft.com/en-us/semantic-kernel/) — plugin model, planner, AI connectors, filters
- [Azure AI Integration](https://learn.microsoft.com/en-us/azure/ai-services/) — Azure OpenAI, Azure AI Search connectors
- [Semantic Kernel Process Framework](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/process/process-framework) — multi-step workflow / state machine support (newer feature, experimental)
