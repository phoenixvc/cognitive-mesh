# Managed Agent Platforms & Protocols

Evaluation of cloud-provider managed agent platforms, emerging stateful agent frameworks, and interoperability protocols. These represent the "platform layer" above workflow engines and agent runtimes — they bundle model hosting, tool integration, memory, and orchestration into managed services.

## Cloud Provider Platforms

Enterprise-grade managed platforms from the Big 3 cloud providers. All offer model hosting + agent orchestration + enterprise connectors. Key differentiator: ecosystem lock-in and language support.

| Platform | Interactive | Batch | Durable | Event-Driven | Multi-Agent |
|----------|:----------:|:-----:|:-------:|:------------:|:-----------:|
| [Azure AI Foundry](azure-ai-foundry.md) | **76.6%** | **77.8%** | **74.6%** | **75.4%** | **76.2%** |
| [AWS Bedrock Agents](aws-bedrock-agents.md) | 72.6% | 75.6% | 71.6% | 73.2% | 72.0% |
| [Google Vertex AI Agent Builder](google-vertex-agent-builder.md) | 72.6% | 73.2% | 68.8% | 71.2% | 70.8% |

### Key Differentiators

| Dimension | Azure AI Foundry | AWS Bedrock Agents | Google Vertex AI |
|-----------|-----------------|-------------------|-----------------|
| **Best for** | Microsoft stack enterprises | AWS-native, framework-flexible | GCP + Gemini users |
| **Agent framework** | Semantic Kernel + MS Agent Framework | Strands + any framework | ADK + any framework |
| **.NET support** | Excellent (native) | Good (SDK available) | None (Python/Java/Go only) |
| **Enterprise connectors** | M365, Dynamics, Copilot Studio | Lambda, Step Functions, SQS | Apigee (100+ connectors) |
| **Durable execution** | No (needs external) | No (needs external) | No (needs external) |
| **Unique strength** | Compliance certifications | Modular AgentCore services | Visual Agent Designer |
| **Inter-agent protocol** | A2A support (emerging) | — | A2A (native, Google-led) |
| **Tool protocol** | MCP support | — | Custom |
| **Pricing** | Token + PTU + compute | Token + AgentCore services + compute | Token + Agent Engine + compute |

**Critical gap across all three**: None provide durable execution. For production workflows requiring crash recovery and replay, all three need to be combined with a workflow engine (Temporal, Inngest, or their native equivalents like Azure Durable Functions / AWS Step Functions).

## Emerging Platforms

| Platform | Focus | Score Range | Key Differentiator |
|----------|-------|:-----------:|-------------------|
| [Letta (MemGPT)](letta.md) | Stateful memory | 61-66% | Hierarchical self-editing memory; #1 on Terminal-Bench |

### Platforms Not Evaluated (Pivoted or Insufficient Maturity)

| Platform | Status | Notes |
|----------|--------|-------|
| **Fixie.ai / AI.JSX** | Pivoted to voice AI (Ultravox) | No longer an agent orchestration platform. AI.JSX framework appears inactive. |
| **Langbase** | Early stage | Serverless AI agent deployment. Insufficient public documentation for full evaluation. |
| **Relevance AI** | Niche | No-code agent builder. More business automation tool than orchestration platform. |

## Interoperability Protocols

Standards that shape how agents and tools integrate, independent of any single platform.

| Protocol | What It Connects | Backed By | Adoption | Detailed Doc |
|----------|-----------------|-----------|----------|:------------:|
| **A2A** (Agent-to-Agent) | Agent ↔ Agent | Google | Early | [Details](agent-protocols.md) |
| **MCP** (Model Context Protocol) | Agent ↔ Tools/Data | Anthropic | Production-ready | [Details](agent-protocols.md) |

**Recommendation**: Adopt MCP now (production-ready, immediate integration benefits). Evaluate A2A as it matures.

## How Managed Platforms Compare to Standalone Engines

| Dimension | Managed Platforms (Foundry, Bedrock, Vertex) | Workflow Engines (Temporal, Inngest) | Agent Runtimes (LangGraph, AutoGen) |
|-----------|----------------------------------------------|--------------------------------------|--------------------------------------|
| **Model hosting** | Included | Not included | Not included |
| **Durable execution** | Not included | Included (core strength) | Partial (checkpoint-based) |
| **Tool integration** | Managed connectors | Custom | Framework-native |
| **Enterprise security** | Full (compliance certs) | Self-managed | Self-managed |
| **Vendor lock-in** | High | Low-Medium | Low |
| **Multi-agent** | Basic (supervisor pattern) | Not native | Core strength |
| **Cost model** | Pay-per-token + platform fees | Self-hosted or cloud | Open source |

**Takeaway**: Managed platforms excel at model hosting and enterprise integration but lack durable execution. The optimal architecture combines a managed platform (for models and enterprise connectors) with a workflow engine (for durability) and an agent runtime (for multi-agent coordination).
