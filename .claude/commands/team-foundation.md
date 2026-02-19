# Team FOUNDATION — FoundationLayer Agent

You are **Team FOUNDATION** for the Cognitive Mesh project.

## Setup
1. Read `CLAUDE.md` for project conventions
2. Read `.claude/rules/foundation-layer.md` for layer-specific rules
3. Read `.claude/rules/architecture.md` for dependency rules

## Scope
- **Source:** `src/FoundationLayer/` ONLY
- **Tests:** `tests/FoundationLayer/` ONLY
- **Do NOT** modify files in any other layer

## Backlog (read AGENT_BACKLOG.md for full details)

### P1 — Stub Completions
1. `src/FoundationLayer/DocumentProcessing/DocumentIngestionFunction.cs:52` — Fabric integration placeholder
2. `src/FoundationLayer/SemanticSearch/EnhancedRAGSystem.cs:208,214` — Pipeline connection stubs
3. `src/FoundationLayer/Security/Engines/SecretsManagementEngine.cs:117` — Placeholder

### P2 — PRD Implementations
- Ethical & Legal Compliance Core (FI-02): `docs/prds/01-foundational/ethical-legal-compliance-framework.md`
- NIST AI RMF (FI-03): `docs/prds/01-foundational/nist-ai-rmf-maturity/`
- Adaptive Balance (FI-04): `docs/prds/02-adaptive-balance/backend-architecture.md`

### Tests
- Expand `tests/FoundationLayer/Security/` coverage
- Add tests for DocumentProcessing, SemanticSearch, VectorDatabase adapters

## Workflow
1. Assess: Find all remaining stubs/TODOs in `src/FoundationLayer/`
2. Fix: Complete each stub with real implementation
3. Test: Add unit tests for each implementation (xUnit + Moq + FluentAssertions)
4. Build: `dotnet build CognitiveMesh.sln` — must pass with zero warnings (TreatWarningsAsErrors=true)
5. Test: `dotnet test tests/FoundationLayer/ --no-build` — all tests must pass
6. Report: List what was completed and what remains

$ARGUMENTS
