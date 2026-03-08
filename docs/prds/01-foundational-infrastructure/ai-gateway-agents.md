# PRD: AI Gateway Domain Agents

**Project:** ai-gateway Agent Bootstrap
**Owner:** Hans Jurgens Smit — PhoenixVC / Nexamesh AI
**Status:** Draft v0.1
**Date:** 8 March 2026
**Classification:** Internal — Cognitive Mesh Infrastructure

---

## 1. Context

The `phoenixvc/ai-gateway` repository runs an OpenAI-compatible AI gateway via LiteLLM on Azure Container Apps. It normalizes Azure OpenAI endpoints for downstream tools (cognitive-mesh, Roo/Qoder). It includes a FastAPI state service (Redis-backed), an Nginx dashboard, and Terraform-managed infrastructure across 3 environments (dev/uat/prod). Currently has no agent configuration.

**Stack**: Python/LiteLLM, Terraform (IaC), FastAPI, Redis, Nginx, GitHub Actions
**Models**: gpt-5.3-codex, gpt-4o, text-embedding-3-large

---

## 2. Agent Specifications

### 2.1 gateway-ops (Team: devops)

**Purpose**: Manages LiteLLM proxy configuration, model routing, rate limiting, and the 3-environment deployment matrix.

**Capabilities**:
- Configure LiteLLM model routing rules and fallback chains
- Manage rate limiting settings (rpm_limit, tpm_limit, max_budget)
- Understand 3-environment deployment matrix (dev/uat/prod) and their differences
- Monitor Grafana dashboards and Prometheus metrics (`/metrics` endpoint)
- Manage smoke test patterns (`.github/actions/smoke-test-gateway/`)
- Configure Redis semantic caching (enable/disable, TTL settings)
- Handle environment-specific model configurations (codex for dev/uat, gpt-4o for prod)
- Manage Container App scaling (min_replicas, max_replicas)

**Trigger Conditions**:
- Changes to LiteLLM configuration
- Changes to Terraform environment configs (`infra/env/*/`)
- Changes to smoke test actions
- Deployment workflow modifications

---

### 2.2 model-registry (Team: backend)

**Purpose**: Manages model configuration, availability validation, and the state service that tracks model usage and user preferences.

**Capabilities**:
- Manage model configurations: deployment names, API versions, endpoints
- Validate model availability against Azure OpenAI deployments
- Configure fallback chains (primary model -> fallback model)
- Manage token budget policies per model
- Understand the FastAPI state service (`state-service/`) and its Redis backend
- Manage model catalog (`/state/catalog` endpoint)
- Track per-user model selections (`/state/selection` endpoint)
- Validate API version compatibility (2025-04-01-preview vs 2025-01-01-preview vs 2024-02-01)
- Monitor model usage and cost metrics

**Trigger Conditions**:
- New model deployment requests
- API version updates from Azure
- State service code changes (`state-service/**`)
- Model catalog or selection changes

---

## 3. AgentKit Forge Configuration

```yaml
# .agentkit/overlays/ai-gateway/agents.yaml
agents:
  - id: gateway-ops
    team: devops
    description: Manages LiteLLM proxy config, model routing, rate limiting, deployments
    capabilities:
      - litellm-configuration
      - model-routing
      - rate-limiting
      - environment-management
      - monitoring-grafana
      - smoke-testing
    trigger_paths:
      - "infra/**"
      - ".github/**"
      - "dashboard/**"

  - id: model-registry
    team: backend
    description: Manages model configuration, availability, state service, token budgets
    capabilities:
      - model-configuration
      - availability-validation
      - fallback-chain-management
      - token-budget-policy
      - state-service-management
    trigger_paths:
      - "state-service/**"
      - "infra/modules/aigateway_aca/**"
```

---

*PRD Owner: Hans Jurgens Smit | PhoenixVC / Nexamesh AI | House of Veritas*
