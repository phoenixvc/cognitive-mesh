# PRD: AgentKit Bootstrap Playbook

**Project:** Repo Agent Bootstrapping
**Owner:** Hans Jurgens Smit — PhoenixVC / Nexamesh AI
**Status:** Draft v0.1
**Date:** 8 March 2026
**Classification:** Internal — Cognitive Mesh Infrastructure

---

## 1. Problem Statement

Six PhoenixVC repositories lack agent team configurations. Each needs to be bootstrapped with agentkit-forge to get standard teams, domain-specific agents, and multi-tool output (Claude Code, Copilot, Cursor). Currently there is no documented standard process.

---

## 2. Objective

Define a repeatable, documented procedure for bootstrapping any PhoenixVC repository with agentkit-forge agent teams, including domain-specific agent customization via overlays.

---

## 3. Bootstrap Pipeline

### Step 1: Analyze Repository

```bash
# agentkit-forge scans the repo and generates project.yaml
agentkit init --repo <path-or-url>
```

Auto-detects:
- Languages and frameworks (from package.json, *.csproj, Cargo.toml, pyproject.toml, etc.)
- Project structure (monorepo vs single-app)
- Existing CI/CD (GitHub Actions workflows)
- IaC patterns (Bicep, Terraform, Terragrunt)
- Testing frameworks

### Step 2: Generate Standard Teams

```bash
agentkit sync
```

Generates the standard 10-team structure:
- backend, frontend, data, infra, devops
- testing, security, docs, product, quality

Plus: orchestrator, slash commands, rules, hooks

### Step 3: Add Domain Overlays

Create `.agentkit/overlays/<project>/agents.yaml` for domain-specific agents:

```yaml
# .agentkit/overlays/azure-infrastructure/agents.yaml
agents:
  - id: bicep-validator
    team: infra
    description: Validates Bicep module structure and naming conventions
    capabilities:
      - bicep-syntax-validation
      - parameter-naming-check
      - module-output-consistency
```

### Step 4: Regenerate with Overlays

```bash
agentkit sync --overlay azure-infrastructure
```

### Step 5: Commit and Push

```bash
git add .agentkit/ .claude/ AGENTS.md
git commit -m "Bootstrap agent teams via agentkit-forge"
git push
```

### Step 6: Verify

```bash
agentkit validate  # Check generated files are consistent
```

---

## 4. Repos to Bootstrap

| # | Repo | Stack | Domain Agents | Priority |
|---|------|-------|--------------|----------|
| 1 | azure-infrastructure | PowerShell, Python, Bicep, Terraform, C# | bicep-validator, module-publisher, naming-enforcer | P1 |
| 2 | ai-gateway | Python/LiteLLM, Terraform, FastAPI, Redis | gateway-ops, model-registry | P1 |
| 3 | codeflow-engine | Python, Tauri, Next.js | (standard teams sufficient) | P2 |
| 4 | Mystira.workspace | C#, TypeScript, Python (9-component monorepo) | (standard teams, per-component scoping) | P2 |
| 5 | PhoenixVC-Website | React, Vite, TypeScript, Tailwind, Azure Functions | (standard teams sufficient) | P2 |
| 6 | PhoenixRooivalk | Rust, TypeScript, Python, Solana | (alignment: preserve existing hand-crafted as overlays) | P2 |

---

## 5. Domain Agent Specifications

### azure-infrastructure

| Agent | Team | Purpose |
|-------|------|---------|
| `bicep-validator` | infra | Validate Bicep module structure, parameter naming, output consistency across all 17 modules. Run `az bicep build` validation. Check module compatibility with dev/staging/prod parameter files. |
| `module-publisher` | devops | Manage the module publishing pipeline (publish-modules.yml). Handle versioning, changelog generation. Ensure backward compatibility of module interfaces. |
| `naming-enforcer` | quality | Enforce the `{env}-{region}-{resourceType}-{project}` naming convention. Validate across Bicep parameters, config files, examples, and documentation. Integrate with validate-naming.yml workflow. |

### ai-gateway

| Agent | Team | Purpose |
|-------|------|---------|
| `gateway-ops` | devops | Manage LiteLLM proxy configuration, model routing rules, rate limiting settings. Understand 3-environment deployment matrix (dev/uat/prod). Monitor Grafana dashboards and Prometheus metrics. Manage smoke test patterns. |
| `model-registry` | backend | Manage model configuration (gpt-5.3-codex, gpt-4o, text-embedding-3-large). Validate model availability and API version compatibility. Manage fallback chains and token budget policies. Understand the FastAPI state service (Redis-backed) for model/user preferences. |

---

## 6. Special Cases

### PhoenixRooivalk (Alignment, not full bootstrap)

PhoenixRooivalk already has hand-crafted `.claude/commands/` files (detector.md, migrate.md, dev.md). These should be preserved as custom overlays:

1. Run `agentkit init` to generate standard `project.yaml`
2. Import existing commands into `.agentkit/overlays/phoenixrooivalk/`
3. Run `agentkit sync` — generates standard teams alongside existing commands
4. Existing domain commands become overlay agents

### Mystira.workspace (Monorepo scoping)

Mystira is a 9-component monorepo. agentkit-forge's `project.yaml` supports monorepo mode:

```yaml
project:
  name: Mystira.workspace
  type: monorepo
  components:
    - name: mystira-core
      path: src/core
      stack: [csharp]
    - name: mystira-web
      path: src/web
      stack: [typescript, react]
    # ...
```

Each component gets scoped agent rules while sharing the orchestrator.

---

## 7. Verification Checklist

For each bootstrapped repo:

- [ ] `.agentkit/spec/project.yaml` exists and accurately describes the repo
- [ ] `.agentkit/spec/teams.yaml` has standard 10 teams
- [ ] `.agentkit/spec/agents.yaml` has domain agents (if applicable)
- [ ] `.claude/commands/` has generated team commands
- [ ] `AGENTS.md` exists at repo root
- [ ] `agentkit validate` passes
- [ ] arch-context-mcp can parse the repo's agent config via `get_agent_config`
- [ ] Domain agents are functional (manual smoke test)

---

## 8. Post-Bootstrap

After bootstrapping, each repo should:
1. Be indexed by arch-context-mcp (via GitHub webhook or manual trigger)
2. Appear in the ecosystem team registry
3. Have CI drift detection (compare generated vs committed)
4. Be accessible to cross-repo teams via MCP tools

---

*PRD Owner: Hans Jurgens Smit | PhoenixVC / Nexamesh AI | House of Veritas*
