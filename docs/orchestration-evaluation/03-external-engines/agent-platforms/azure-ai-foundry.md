# Azure AI Foundry

## What It Is

Azure AI Foundry (rebranded to "Microsoft Foundry" at Ignite 2025, formerly Azure AI Studio) is Microsoft's unified managed platform for building, deploying, and managing AI agents at enterprise scale. It integrates Azure OpenAI Service, Semantic Kernel, and the Microsoft Agent Framework into a single development surface with enterprise-grade security, compliance, and observability. The Foundry Agent Service runtime connects models, tools, and frameworks with **no additional charge** for the service itself — you only pay for model tokens and tools invoked.

## Architecture & Orchestration Pattern

**Pattern**: Managed agent runtime with integrated model catalog, tool orchestration, and enterprise connectors

```
┌─────────────────────────────────────────────────────┐
│                 Azure AI Foundry                     │
│                                                      │
│  ┌──────────────┐  ┌────────────────────────┐       │
│  │ Model Catalog │  │ Agent Runtime           │      │
│  │ Azure OpenAI  │  │ - Semantic Kernel       │      │
│  │ OSS Models    │  │ - MS Agent Framework    │      │
│  │ Fine-tuned    │  │ - AutoGen integration   │      │
│  └──────────────┘  └────────────────────────┘       │
│                                                      │
│  ┌──────────────┐  ┌────────────────────────┐       │
│  │ Tool Registry │  │ Enterprise Connectors   │      │
│  │ Code interp.  │  │ - Microsoft 365/Graph   │      │
│  │ File search   │  │ - Dynamics 365          │      │
│  │ Functions     │  │ - Azure services        │      │
│  │ MCP support   │  │ - Copilot Studio        │      │
│  └──────────────┘  └────────────────────────┘       │
│                                                      │
│  Observability: Azure Monitor + Application Insights │
│  Security: Azure AD + RBAC + Private Endpoints       │
│  Compliance: SOC2, ISO27001, HIPAA, FedRAMP          │
└──────────────────────────────────────────────────────┘
```

## Key Features for Agent Orchestration

- **Unified agent development surface**: Build, test, and deploy agents from a single portal with integrated evaluation and monitoring
- **Microsoft Agent Framework integration**: Access to AutoGen, Semantic Kernel, and multi-agent orchestration patterns (sequential, concurrent, group chat, handoff)
- **Enterprise model catalog**: Azure OpenAI models (GPT-4o, o3), open-source models (Llama, Mistral), and custom fine-tuned models
- **Built-in tools**: Code interpreter, file search, function calling, and Bing grounding — no infrastructure setup required. Hard limit of 5 nested tool calls per turn.
- **1,400+ action connectors**: Via Azure Logic Apps, plus native MCP (cloud-hosted at mcp.ai.azure.com) and A2A protocol support
- **Microsoft 365 / Copilot integration**: Agents can access organizational data via Microsoft Graph, integrate with Copilot Studio for citizen developer extension. One-click publishing to M365 Copilot and Teams
- **Model Router**: Dynamic model selection balancing cost, performance, and quality
- **Foundry IQ**: Agentic RAG that reimagines retrieval as dynamic reasoning
- **Built-in memory**: Fully managed long-term memory store (cross-session, cross-device)
- **Enterprise security**: Azure AD authentication, RBAC, private endpoints, data residency controls, content safety filters
- **Evaluation framework**: Built-in evaluation with metrics for groundedness, relevance, coherence, and safety

## Fault Tolerance

- Managed runtime handles retry logic and rate limiting for model calls
- Azure infrastructure provides regional failover and high availability
- Content safety filters prevent harmful outputs (configurable severity levels)
- No user-facing durable execution guarantees — agent state is session-scoped unless combined with external persistence (e.g., Cosmos DB)

## Scalability

- Scales with Azure infrastructure (auto-scaling compute, managed endpoints)
- Model deployment supports provisioned throughput (PTU) for predictable latency
- Rate limits per model deployment (configurable)
- Multi-region deployment for global availability

## Concurrency / Throughput

| Parameter | Default | Configurable |
|-----------|---------|:------------:|
| Concurrent requests per deployment | Varies by model/SKU | Yes |
| Tokens per minute (TPM) | Model-dependent (e.g., 120K TPM for GPT-4o standard) | Yes (via PTU) |
| Max nested tool calls | 5 (hard limit, not adjustable) | No |
| Agent orchestration charge | $0 (free — pay for tokens/tools only) | — |
| Request timeout | 120 seconds (API) | Yes |

## Integration / Plugin Architecture

- **SDKs**: .NET (`Azure.AI.Projects` v2.x preview for new portal, v1.x GA for classic), Python, JavaScript/TypeScript, Java, REST API
- **Framework support**: Semantic Kernel, AutoGen/MS Agent Framework, LangChain, LlamaIndex
- **Protocol support**: OpenAI-compatible API, MCP (Model Context Protocol) for tool integration
- **Enterprise connectors**: Microsoft 365, Dynamics 365, SharePoint, Azure SQL, Azure Cosmos DB
- **Copilot Studio**: Low-code agent builder that extends Foundry agents to business users

## Config Defaults

| Setting | Default | Configurable |
|---------|---------|:------------:|
| Model temperature | 1.0 | Yes |
| Max tokens | Model-dependent | Yes |
| Top-p | 1.0 | Yes |
| Content safety | Enabled (medium severity) | Yes |
| Tool choice | Auto | Yes |
| Agent state persistence | Session-scoped | Yes (with external store) |

## Maturity Signals

- **Corporate backing**: Microsoft (Tier 1 cloud provider)
- **Scale**: 70,000+ customers, 100 trillion tokens processed per quarter, 2 billion daily enterprise search queries
- **Release cadence**: Monthly (Dec 2025, Jan 2026 updates documented)
- **Compliance**: SOC 2, ISO 27001, HIPAA, FedRAMP, GDPR — broadest compliance coverage among agent platforms
- **Ecosystem**: Largest enterprise AI ecosystem (Azure + M365 + Dynamics)
- **Community**: Large (Azure AI community, Semantic Kernel open source ~27k stars)

## Per-Metric Scores

| Metric | Score | % | Confidence | Evidence & Justification |
|--------|:-----:|:-:|:----------:|--------------------------|
| Latency | 3.5 | 70.0% | Medium | Model inference latency dominates. Managed overhead is minimal. PTU reduces variability. |
| Scalability | 4.5 | 90.0% | High | Azure-native auto-scaling. Multi-region. Provisioned throughput. |
| Efficiency | 3.5 | 70.0% | Medium | Pay-per-token model is efficient for variable workloads. PTU is efficient for steady-state. Platform overhead unknown. |
| Fault Tolerance | 3.5 | 70.0% | Medium | Managed retry/rate limiting. Regional failover. No durable execution (session-scoped state). |
| Throughput | 4.0 | 80.0% | Medium | Provisioned throughput for predictable performance. Rate limits configurable per deployment. |
| Maintainability | 4.0 | 80.0% | High | Unified portal, integrated evaluation, strong SDK support across languages. |
| Determinism | 3.5 | 70.0% | Medium | Evaluation framework tracks quality. No replay/event-sourcing. Audit via Azure Monitor. |
| Integration Ease | 4.5 | 90.0% | High | Deepest enterprise integration (M365, Dynamics, Copilot Studio). Multi-language SDKs. MCP support. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.83 | 76.6% |
| Batch | 3.89 | 77.8% |
| Long-Running Durable | 3.73 | 74.6% |
| Event-Driven Serverless | 3.77 | 75.4% |
| Multi-Agent Reasoning | 3.81 | 76.2% |

## When to Use

- Enterprise environments already on Azure / Microsoft 365 stack
- Teams needing compliance certifications (HIPAA, FedRAMP, SOC 2) out of the box
- Multi-language teams wanting consistent SDK experience across C#, Python, TypeScript, Java
- Organizations that need integrated agent + model + data platform

## When NOT to Use

- Teams not on Azure (vendor lock-in is significant)
- Cost-sensitive projects (Azure AI pricing can be substantial at scale)
- Workloads requiring durable execution guarantees (combine with Temporal or Durable Functions)
- Teams wanting full control over orchestration logic (managed platform abstracts away details)

## Known Weaknesses

- **Vendor lock-in**: Deep Azure integration makes multi-cloud difficult
- **No durable execution**: Agent state is session-scoped; crash recovery requires external persistence
- **Pricing complexity**: Token-based pricing + compute + storage + networking costs are hard to predict
- **Tool call limit**: Hard limit of 5 nested tool calls per turn cannot be increased
- **Token waste**: Tool metadata is always passed to the model even if tool is not invoked, consuming tokens silently
- **Rate limiting**: Default rate limits can be restrictive; PTU is expensive
- **Rapid rebranding**: Azure AI Studio → Azure AI Foundry → Microsoft Foundry creates documentation confusion

## Pricing Model

- **Pay-per-token**: Standard pricing per input/output token (varies by model)
- **Provisioned Throughput Units (PTU)**: Reserved capacity for predictable latency
- **Compute**: Additional costs for hosted compute (evaluation, fine-tuning)
- **Storage**: Charges for file storage, vector search, knowledge bases
- **Free tier**: Limited free credits for new accounts
