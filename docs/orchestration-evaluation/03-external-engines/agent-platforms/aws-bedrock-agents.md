# AWS Bedrock Agents & AgentCore

## What It Is

Amazon Bedrock Agents is a managed agent orchestration service within AWS Bedrock that enables developers to build, deploy, and scale AI agents with multi-agent collaboration, memory, policy controls, and enterprise-grade infrastructure. In 2025, AWS expanded this with **AgentCore**, an agentic platform supporting any framework and model with no infrastructure management required.

## Architecture & Orchestration Pattern

**Pattern**: Supervisor-based multi-agent collaboration with managed infrastructure services

```
┌─────────────────────────────────────────────────────┐
│            AWS Bedrock Agents + AgentCore             │
│                                                      │
│  ┌──────────────────────────────────────────┐        │
│  │       Multi-Agent Collaboration           │       │
│  │  Supervisor Agent                         │       │
│  │  ├── Specialized Agent A (own scratchpad) │       │
│  │  ├── Specialized Agent B                  │       │
│  │  └── Specialized Agent C                  │       │
│  │  Task delegation + output consolidation   │       │
│  └──────────────────────────────────────────┘        │
│                                                      │
│  AgentCore Services:                                 │
│  ┌────────┐ ┌────────┐ ┌──────────┐ ┌────────┐     │
│  │Identity│ │Gateway │ │  Memory  │ │  Code  │     │
│  │Auth/   │ │Tool    │ │Episodic +│ │Interp. │     │
│  │AuthZ   │ │Access  │ │Long-term │ │Sandbox │     │
│  └────────┘ └────────┘ └──────────┘ └────────┘     │
│  ┌────────┐ ┌────────────────┐ ┌───────────┐       │
│  │Browser │ │  Evaluations   │ │  Policy   │       │
│  │Web     │ │Quality testing │ │NL-defined │       │
│  │Actions │ │& profiling     │ │boundaries │       │
│  └────────┘ └────────────────┘ └───────────┘       │
│                                                      │
│  Framework Support: Strands Agents, LangGraph,       │
│    LlamaIndex, CrewAI, custom frameworks             │
│  Models: Bedrock Catalog + external models           │
│  Orchestration: "Agents as Tools" pattern            │
└──────────────────────────────────────────────────────┘
```

## Key Features for Agent Orchestration

- **Multi-agent collaboration**: Supervisor-based architecture where specialized agents maintain own scratchpads; supervisor delegates tasks and consolidates outputs
- **AgentCore services**: Modular building blocks — Identity (auth), Gateway (tools), Memory (episodic + long-term), Code Interpreter, Browser, Evaluations, Policy
- **"Agents as Tools" pattern**: Each specialized agent is wrapped as a callable tool for modular, hierarchical orchestration via Strands Agents
- **Policy controls**: Natural language-defined boundaries for what tools and data agents can access, under what conditions
- **Episodic memory**: Agents learn from past experiences, improving decision-making over time
- **Framework agnostic**: AgentCore supports LangGraph, LlamaIndex, CrewAI, Strands Agents, and custom frameworks
- **Model agnostic**: Bedrock Catalog models + external models (not locked to AWS-hosted models)
- **Built-in evaluations**: Continuous quality inspection based on agent behavior

## Fault Tolerance

- AWS managed infrastructure provides high availability and regional redundancy
- Elastic scaling with automatic load distribution
- AgentCore handles retry and error recovery at the infrastructure level
- Episodic memory allows agents to learn from failures
- No explicit durable execution / event-sourcing guarantees documented

## Scalability

- Elastic scaling of multi-agent workflows on AWS infrastructure
- Strands Agents + AgentCore + AWS services enable production-scale deployment
- Integration with AWS auto-scaling, Lambda, and container services
- NVIDIA NeMo Agent Toolkit integration for GPU-accelerated agent workloads

## Concurrency / Throughput

| Parameter | Default | Configurable |
|-----------|---------|:------------:|
| Concurrent agent invocations | Service quota-based | Yes |
| Model throughput | On-demand or provisioned | Yes |
| Agent timeout | Session-based | Yes |
| Memory retention | Episodic (persistent) | Yes |

## Integration / Plugin Architecture

- **SDKs**: .NET (`AWSSDK.BedrockAgent` + `AWSSDK.BedrockAgentRuntime`, v4.x GA since April 2025), Python (Boto3), JavaScript, Java, Go, CLI
- **Framework support**: Strands Agents (AWS-native), LangGraph, LlamaIndex, CrewAI
- **AWS service integration**: Lambda, Step Functions, S3, DynamoDB, SQS, EventBridge, Bedrock Knowledge Bases
- **AgentCore Gateway**: Centralized tool access management
- **NVIDIA partnership**: NeMo Agent Toolkit for advanced orchestration and GPU workloads

## Config Defaults

| Setting | Default | Configurable |
|---------|---------|:------------:|
| Supervisor max rounds | Framework-dependent | Yes |
| Model | Selectable from Bedrock Catalog | Yes |
| Memory type | Session + optional episodic | Yes |
| Policy | Must be explicitly defined | Yes |
| Code interpreter | Sandboxed environment | — |

## Maturity Signals

- **Corporate backing**: Amazon Web Services (Tier 1 cloud provider)
- **Production users**: Amazon (internal), S&P Global, Cox Automotive, Thomson Reuters, Workday, PGA TOUR
- **Release cadence**: Rapid; AgentCore announced mid-2025 with continuous updates
- **Ecosystem**: Largest cloud infrastructure ecosystem; deep integration with AWS services
- **Strands Agents**: AWS-contributed open-source agent framework
- **Community**: Large AWS developer community; active AWS blogs and tutorials

## Per-Metric Scores

| Metric | Score | % | Confidence | Evidence & Justification |
|--------|:-----:|:-:|:----------:|--------------------------|
| Latency | 3.5 | 70.0% | Medium | Model inference latency dominates. Managed infrastructure is optimized. |
| Scalability | 4.5 | 90.0% | High | AWS elastic infrastructure. Proven at Amazon scale. |
| Efficiency | 3.5 | 70.0% | Medium | Pay-per-use model. AgentCore adds managed overhead (not quantified). |
| Fault Tolerance | 3.5 | 70.0% | Medium | AWS managed HA. Episodic memory for learning from failures. No durable execution replay. |
| Throughput | 4.0 | 80.0% | Medium | On-demand and provisioned throughput. AWS infrastructure handles scaling. |
| Maintainability | 3.5 | 70.0% | Medium | Multiple frameworks and patterns increase complexity. AgentCore services are modular. |
| Determinism | 3.0 | 60.0% | Low | Evaluations framework exists. Policy controls add boundaries. No event-sourcing / replay. |
| Integration Ease | 4.0 | 80.0% | High | Deep AWS ecosystem integration. Multi-language SDKs including .NET. Framework agnostic. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.63 | 72.6% |
| Batch | 3.78 | 75.6% |
| Long-Running Durable | 3.58 | 71.6% |
| Event-Driven Serverless | 3.66 | 73.2% |
| Multi-Agent Reasoning | 3.60 | 72.0% |

## When to Use

- AWS-native environments with existing infrastructure investments
- Teams needing framework flexibility (not locked to one agent framework)
- Organizations wanting modular agent capabilities (pick services à la carte from AgentCore)
- Workloads requiring GPU-accelerated agents (NVIDIA NeMo integration)

## When NOT to Use

- Teams not on AWS (significant vendor lock-in)
- Workloads requiring replay-based durable execution (combine with Step Functions or Temporal)
- Teams wanting opinionated, single-framework simplicity (AgentCore's flexibility adds complexity)
- Cost-sensitive projects (AWS pricing at scale can be substantial)

## Known Weaknesses

- **No durable execution**: No event-sourcing or replay-based crash recovery
- **Complexity**: Multiple overlapping services (Bedrock Agents, AgentCore, Strands Agents, Step Functions) create confusion about which to use when
- **Vendor lock-in**: Deep AWS integration; multi-cloud portability is limited
- **Hidden token costs**: A single query can consume 5-10x expected tokens due to internal reasoning/scratchpad traces
- **Output token burndown**: 5x multiplier for Anthropic Claude 3.7+ output tokens makes cost estimation difficult
- **No visual builder**: No visual agent designer comparable to Azure or Google
- **Quota complexity**: Managing quotas across models, regions, and 4 pricing tiers (Standard, Priority, Flex, Batch) is complex
- **Newer platform**: AgentCore is newer than Azure AI Foundry; fewer production case studies

## Pricing Model

- **Pay-per-token**: Model inference charges per input/output token
- **AgentCore services**: Per-use charges for Identity, Gateway, Memory, Code Interpreter, Browser
- **Provisioned throughput**: Reserved model capacity for predictable latency
- **Compute**: Standard AWS compute pricing for hosted agents
- **Free tier**: Limited free tier for Bedrock models
