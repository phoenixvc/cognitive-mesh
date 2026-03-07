# Maturity Signals

## WIP / Churn Indicators

Open PR count serves as a proxy for integration stability — not quality. High churn can indicate active development (positive) or instability (negative). Context determines interpretation.

### Interpreting Open PR Count

| Range | Signal | Impact on Integration Ease |
|-------|--------|---------------------------|
| 0–5 | Stable or inactive | Low integration risk; verify it's not abandoned |
| 6–20 | Active development | Moderate risk; API surfaces may shift |
| 21–50 | Significant churn | Higher risk; pin versions aggressively |
| 50+ | Very high churn | Caution; integration surfaces likely unstable |

### Internal Repo Snapshot

| Repository | Open PRs | Interpretation |
|-----------|----------|----------------|
| agentkit-forge | 7 | Active, moderate churn |
| codeflow-engine | 3 | Stable |
| cognitive-mesh | 14 | Active churn; integration surfaces evolving |
| HouseOfVeritas | 0 | Stable or pre-active phase |

### External Engine Snapshot

| Engine | Open PRs (approx.) | Interpretation |
|--------|-----------|----------------|
| Temporal | ~158 | High activity; mature project with many contributors |
| Inngest | ~77 | Active development; manageable churn |
| n8n | ~947 | Very high churn; pin versions carefully |
| CrewAI | ~330 | High churn; API changes likely |
| AutoGen | ~190 | High churn; transition to MS Agent Framework underway |
| LangGraph | ~206 | High churn; evolving API surface |

## Additional Maturity Factors

Beyond PR count, consider:

- **Release cadence**: Regular releases with changelogs signal maturity
- **Breaking change frequency**: Semantic versioning adherence
- **Documentation currency**: Docs matching latest release
- **Test coverage**: Published coverage metrics or CI badges
- **Community size**: Contributors, Discord/Slack activity, StackOverflow questions
- **Corporate backing**: Venture-funded vs community-maintained
- **License stability**: License changes affect integration risk

## How Maturity Affects Scoring

Maturity signals primarily affect the **Integration Ease** metric:

- High churn reduces Integration Ease by 0.2–0.5 (depending on severity)
- Lack of versioned API contracts reduces score by 0.3–0.5
- Missing or outdated docs reduce score by 0.2–0.3
- Strong backward compatibility guarantees add 0.2–0.3

These adjustments are applied on top of the base architecture/feature score.
