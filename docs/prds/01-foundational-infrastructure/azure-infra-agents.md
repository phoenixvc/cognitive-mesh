# PRD: Azure Infrastructure Domain Agents

**Project:** azure-infrastructure Agent Bootstrap
**Owner:** Hans Jurgens Smit — PhoenixVC / Nexamesh AI
**Status:** Draft v0.1
**Date:** 8 March 2026
**Classification:** Internal — Cognitive Mesh Infrastructure

---

## 1. Context

The `phoenixvc/azure-infrastructure` repository is the foundational IaC repo for all PhoenixVC projects. It contains 17 reusable Bicep modules, Terraform alternatives, application templates (Python/FastAPI, C#/.NET), and 8 CI/CD workflows. It currently has no agent configuration.

**Stack**: PowerShell, Python, Bicep (primary IaC), Terraform (alternative), C#/.NET
**Key Components**: 17 Bicep modules (app-service, container-apps, function-app, key-vault, postgres, redis, service-bus, storage, vnet, etc.)

---

## 2. Agent Specifications

### 2.1 bicep-validator (Team: infra)

**Purpose**: Validates Bicep module structure, parameter naming, and output consistency across all 17 infrastructure modules.

**Capabilities**:
- Run `az bicep build` validation on all modules
- Check parameter naming follows `{env}-{region}-{resourceType}-{project}` convention
- Verify module outputs are consistent across related modules
- Validate parameter files (dev.bicepparam, staging.bicepparam, prod.bicepparam) reference correct module parameters
- Detect breaking changes in module interfaces (parameter additions/removals/renames)
- Cross-reference modules used in `main.bicep` with available modules directory

**Trigger Conditions**:
- Any change to `infra/modules/**/*.bicep`
- Any change to `infra/*.bicepparam`
- Any change to `infra/main.bicep`

---

### 2.2 module-publisher (Team: devops)

**Purpose**: Manages the Bicep module publishing pipeline, versioning, and changelog generation.

**Capabilities**:
- Manage publish-modules.yml workflow configuration
- Generate changelogs for module updates (breaking vs non-breaking)
- Version module updates following semver
- Ensure backward compatibility of module interfaces
- Coordinate multi-module updates (when one module change affects others)
- Validate module registry references are correct

**Trigger Conditions**:
- PR modifying any module in `infra/modules/`
- Release workflow trigger
- Module version bump request

---

### 2.3 naming-enforcer (Team: quality)

**Purpose**: Enforces Azure naming conventions across all configuration files, Bicep parameters, and documentation.

**Capabilities**:
- Validate resource names follow `{env}-{region}-{resourceType}-{project}` pattern
- Check Bicep parameter defaults and examples for naming compliance
- Validate naming in Terraform modules matches Bicep conventions
- Cross-reference naming in CI/CD workflows (validate-naming.yml)
- Check documentation examples use correct naming patterns
- Detect naming inconsistencies between environments (dev vs staging vs prod)

**Trigger Conditions**:
- Any change to parameter files or configuration
- Any change to documentation or examples
- New module or resource type added

---

## 3. AgentKit Forge Configuration

```yaml
# .agentkit/overlays/azure-infrastructure/agents.yaml
agents:
  - id: bicep-validator
    team: infra
    description: Validates Bicep module structure, parameter naming, output consistency
    capabilities:
      - bicep-syntax-validation
      - parameter-naming-check
      - module-output-consistency
      - breaking-change-detection
    trigger_paths:
      - "infra/modules/**/*.bicep"
      - "infra/*.bicepparam"
      - "infra/main.bicep"

  - id: module-publisher
    team: devops
    description: Manages module publishing pipeline, versioning, changelog
    capabilities:
      - module-versioning
      - changelog-generation
      - backward-compatibility-check
      - registry-validation
    trigger_paths:
      - "infra/modules/**"
      - ".github/workflows/publish-modules.yml"

  - id: naming-enforcer
    team: quality
    description: Enforces Azure naming conventions across all config files
    capabilities:
      - naming-convention-validation
      - cross-environment-consistency
      - documentation-compliance
    trigger_paths:
      - "infra/**"
      - "config/**"
      - "docs/**"
```

---

*PRD Owner: Hans Jurgens Smit | PhoenixVC / Nexamesh AI | House of Veritas*
