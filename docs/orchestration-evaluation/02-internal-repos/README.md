# Internal Repository Evaluation

Comparative evaluation of 4 internal repositories implementing agent orchestration patterns.

## Summary Comparison

### Per-Metric Scores (1.0–5.0 scale)

| Metric | agentkit-forge | codeflow-engine | cognitive-mesh | HouseOfVeritas |
|--------|:-:|:-:|:-:|:-:|
| Latency | 4.0 (80.0%) | 3.0 (60.0%) | 3.0 (60.0%) | 3.0 (60.0%) |
| Scalability | 3.0 (60.0%) | 4.0 (80.0%) | 3.0 (60.0%) | 4.0 (80.0%) |
| Efficiency | 4.0 (80.0%) | 3.0 (60.0%) | 3.0 (60.0%) | 3.0 (60.0%) |
| Fault Tolerance | 4.0 (80.0%) | 4.0 (80.0%) | 3.0 (60.0%) | 4.0 (80.0%) |
| Throughput | 3.0 (60.0%) | 3.0 (60.0%) | 3.0 (60.0%) | 4.0 (80.0%) |
| Maintainability | 4.0 (80.0%) | 3.0 (60.0%) | 3.0 (60.0%) | 3.0 (60.0%) |
| Determinism | 5.0 (100.0%) | 3.0 (60.0%) | 4.0 (80.0%) | 4.0 (80.0%) |
| Integration Ease | 4.0 (80.0%) | 3.0 (60.0%) | 3.0 (60.0%) | 2.0 (40.0%) |

### Weighted Totals by Profile

| Repository | Interactive | Batch | Durable | Event-Driven | Multi-Agent |
|-----------|:----------:|:-----:|:-------:|:------------:|:-----------:|
| **agentkit-forge** | **78.6%** | **73.8%** | **78.8%** | **79.2%** | **84.0%** |
| codeflow-engine | 64.4% | 68.8% | 67.2% | 62.0% | 62.4% |
| HouseOfVeritas | 64.2% | 70.6% | 69.6% | 61.2% | 66.4% |
| cognitive-mesh | 62.0% | 61.2% | 63.2% | 61.2% | 66.8% |

### Rankings by Profile

| Profile | 1st | 2nd | 3rd | 4th |
|---------|-----|-----|-----|-----|
| Interactive | agentkit-forge | codeflow-engine | HouseOfVeritas | cognitive-mesh |
| Batch | agentkit-forge | HouseOfVeritas | codeflow-engine | cognitive-mesh |
| Long-Running Durable | agentkit-forge | HouseOfVeritas | codeflow-engine | cognitive-mesh |
| Event-Driven Serverless | agentkit-forge | codeflow-engine | HouseOfVeritas / cognitive-mesh (tie) | — |
| Multi-Agent Reasoning | agentkit-forge | cognitive-mesh | HouseOfVeritas | codeflow-engine |

### Key Differentiators

- **agentkit-forge** dominates across all profiles due to its deterministic lifecycle pipeline (5.0 Determinism) and strong integration contracts.
- **codeflow-engine** and **HouseOfVeritas** trade positions depending on profile — codeflow-engine has better integration ease; HouseOfVeritas has better throughput and scalability.
- **cognitive-mesh** has the richest orchestration pattern set (4 coordination patterns + governance gates) but scores lower due to adapter-dependent runtime behavior and active churn (14 open PRs).

## Individual Analyses

- [agentkit-forge](agentkit-forge.md)
- [codeflow-engine](codeflow-engine.md)
- [cognitive-mesh](cognitive-mesh.md)
- [HouseOfVeritas](house-of-veritas.md)
