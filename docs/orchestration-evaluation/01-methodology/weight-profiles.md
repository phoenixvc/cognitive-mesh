# Weight Profiles

Five workload profiles define how the 8 metrics are weighted. Each profile reflects a different deployment context with distinct priorities.

## Profile 1: Interactive (Developer UX / Human-in-the-Loop)

Optimizes for responsiveness and ease of integration in developer-facing tools.

| Metric | Weight | Rationale |
|--------|--------|-----------|
| Latency | 0.22 | Users notice delays instantly; sub-second matters |
| Integration Ease | 0.18 | Developer tools must compose easily |
| Maintainability | 0.16 | Rapid iteration requires understandable code |
| Fault Tolerance | 0.12 | Failures need graceful handling but aren't constant |
| Scalability | 0.10 | Interactive workloads rarely need massive scale |
| Determinism | 0.10 | Reproducibility aids debugging |
| Throughput | 0.07 | Moderate parallelism; not batch-level |
| Efficiency | 0.05 | Less critical for interactive workloads |

## Profile 2: Batch (Enterprise Automation / Background Workloads)

Optimizes for reliability and scale in unattended, high-volume processing.

| Metric | Weight | Rationale |
|--------|--------|-----------|
| Scalability | 0.22 | Must handle growing workload volume |
| Fault Tolerance | 0.22 | Unattended jobs must self-recover |
| Throughput | 0.15 | Processing speed directly impacts SLAs |
| Integration Ease | 0.12 | Enterprise environments need clean integration |
| Maintainability | 0.10 | Long-lived systems need maintainability |
| Efficiency | 0.08 | Cost matters at scale |
| Determinism | 0.06 | Audit requirements but less than durable profile |
| Latency | 0.05 | Batch tolerance for latency is high |

## Profile 3: Long-Running Durable (Workflows That Survive Restarts)

Optimizes for durability, replay, and crash recovery in workflows spanning hours to days.

| Metric | Weight | Rationale |
|--------|--------|-----------|
| Fault Tolerance | 0.28 | Core requirement: survive crashes and resume |
| Determinism | 0.20 | Replay requires deterministic execution |
| Scalability | 0.15 | Long-running workflows accumulate state |
| Throughput | 0.10 | Step execution speed matters over long durations |
| Maintainability | 0.10 | Complex state machines need clarity |
| Integration Ease | 0.08 | Must integrate with external systems reliably |
| Efficiency | 0.05 | Resource use over long durations adds up |
| Latency | 0.04 | Per-step latency is less critical than durability |

## Profile 4: Event-Driven Serverless (Reactive, Function-Based Workloads)

Optimizes for low-latency event processing with minimal infrastructure overhead.

| Metric | Weight | Rationale |
|--------|--------|-----------|
| Latency | 0.20 | Event processing should be near-instant |
| Integration Ease | 0.20 | Serverless environments demand composability |
| Efficiency | 0.15 | Pay-per-invocation makes efficiency critical |
| Fault Tolerance | 0.12 | Events must not be lost |
| Throughput | 0.12 | Event bursts require high concurrency |
| Scalability | 0.10 | Auto-scaling is expected |
| Maintainability | 0.06 | Small functions are inherently simpler |
| Determinism | 0.05 | Event ordering matters less in many cases |

## Profile 5: Multi-Agent Reasoning (AI Agent Collaboration)

Optimizes for coordinating multiple AI agents with governance, convergence, and auditability.

| Metric | Weight | Rationale |
|--------|--------|-----------|
| Determinism | 0.22 | Agent decisions must be auditable and reproducible |
| Maintainability | 0.18 | Complex agent interactions need clean patterns |
| Fault Tolerance | 0.16 | Agent failures must not cascade |
| Integration Ease | 0.14 | Agents integrate with diverse tools and APIs |
| Latency | 0.10 | Agent response time affects user experience |
| Scalability | 0.08 | Agent count is typically bounded |
| Throughput | 0.07 | Parallel agent execution has moderate importance |
| Efficiency | 0.05 | LLM costs dominate; orchestration overhead is secondary |

## Choosing a Profile

| If your workload is... | Use profile... |
|------------------------|---------------|
| A developer CLI, IDE extension, or interactive tool | Interactive |
| Nightly data processing, ETL, scheduled jobs | Batch |
| Approval workflows, saga patterns, multi-day processes | Long-Running Durable |
| Webhook handlers, event stream processors, serverless functions | Event-Driven Serverless |
| Multi-LLM coordination, debate engines, collaborative AI | Multi-Agent Reasoning |

For hybrid workloads, compute weighted totals for multiple profiles and compare.
