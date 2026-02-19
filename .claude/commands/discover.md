# Discovery Agent — Fresh Codebase Scanner

You are the **Discovery Agent** for the Cognitive Mesh project. You perform a **full, fresh scan** of the codebase to find ALL remaining work — not just what's in the backlog. This is critical for autonomous operation because other teams may have introduced new TODOs, broken imports, or interface mismatches that the backlog doesn't know about.

## Why This Exists

The backlog (`AGENT_BACKLOG.md`) tracks known items, but it goes stale:
- Team 3 might introduce a new `// TODO` while fixing a stub
- Team 8 might create a Dockerfile that references a missing env var
- Team 4 might change an interface that breaks Team 5's code
- A build fix might create a new warning elsewhere

This agent finds ALL of that by scanning from scratch.

## Input

$ARGUMENTS

Arguments (optional):
- `--layer foundation|reasoning|metacognitive|agency|business` — Scan only one layer
- `--category stubs|todos|tests|build|infra|cicd` — Scan only one category
- `--quick` — Fast scan (stubs + TODOs + build only)
- No argument — Full comprehensive scan

## Scan Categories

### 1. Build Health
```bash
dotnet build CognitiveMesh.sln --verbosity normal 2>&1
```
Extract:
- Error count and file locations
- Warning count and file locations (especially CS1591)
- Any new warnings not previously tracked

### 2. TODO / FIXME / HACK Comments
```bash
# Search ALL source files — not just known locations
grep -rn "// TODO" src/ --include="*.cs"
grep -rn "// FIXME" src/ --include="*.cs"
grep -rn "// HACK" src/ --include="*.cs"
grep -rn "// INCOMPLETE" src/ --include="*.cs"
grep -rn "// PLACEHOLDER" src/ --include="*.cs"
```
For each hit: record file, line, content, which layer it belongs to.

### 3. Stub Implementations
```bash
# Task.CompletedTask stubs
grep -rn "Task\.CompletedTask" src/ --include="*.cs"

# Task.Delay simulations (fake work)
grep -rn "Task\.Delay" src/ --include="*.cs"

# NotImplementedException
grep -rn "NotImplementedException" src/ --include="*.cs"

# Methods returning default/null as placeholders
grep -rn "return default;" src/ --include="*.cs"
grep -rn "return null!;" src/ --include="*.cs"

# Empty method bodies (just opening/closing braces)
grep -rn "{ }" src/ --include="*.cs"
```

### 4. Test Coverage Gaps
```bash
# List all production .cs files
find src/ -name "*.cs" -not -name "*.Designer.cs" | sort

# List all test .cs files
find tests/ -name "*Tests.cs" -o -name "*Test.cs" | sort

# Cross-reference: which production files have no test file?
```

For each production class, check if a corresponding test class exists.
Flag any public class with >2 public methods that has zero test coverage.

### 5. Interface / Implementation Gaps
```bash
# Find all port interfaces
grep -rn "public interface I.*Port" src/ --include="*.cs"

# Find all adapters
grep -rn "public class.*Adapter" src/ --include="*.cs"

# Find all engines
grep -rn "public class.*Engine" src/ --include="*.cs"

# Cross-reference: any ports without implementations?
```

### 6. Dependency Violations
For each .csproj file, check ProjectReferences against the allowed dependency direction:
- FoundationLayer → nothing
- ReasoningLayer → Foundation, Shared
- MetacognitiveLayer → Foundation, Reasoning, Shared
- AgencyLayer → all below
- BusinessApplications → all below

Flag any violations.

### 7. Infrastructure State
```bash
# Terraform modules
ls infra/modules/ 2>/dev/null

# Docker files
ls Dockerfile docker-compose.yml .dockerignore 2>/dev/null

# CI/CD workflows
ls .github/workflows/*.yml 2>/dev/null

# K8s manifests
ls k8s/ 2>/dev/null

# Missing expected files
for f in Dockerfile docker-compose.yml .dockerignore Makefile .github/dependabot.yml; do
  test -f "$f" || echo "MISSING: $f"
done
```

### 8. Stale References
```bash
# Check for references to files/classes that no longer exist
# Check for broken using statements
# Check for obsolete configuration
dotnet build CognitiveMesh.sln 2>&1 | grep "CS0246\|CS0234\|CS1061"
```

### 9. Security Quick Scan
```bash
# Hardcoded secrets/keys
grep -rn "password\s*=\|apikey\s*=\|secret\s*=\|connectionstring\s*=" src/ --include="*.cs" -i

# TODO: check for proper use of Azure.Identity vs hardcoded credentials
grep -rn "new DefaultAzureCredential\|AzureKeyVaultConfigurationOptions" src/ --include="*.cs"
```

## Output Format

Generate a structured discovery report:

```markdown
## Discovery Report — {date}

### Summary
| Category | Found | In Backlog | New | Delta |
|----------|-------|-----------|-----|-------|
| Build errors | X | Y | Z | +/-N |
| Build warnings | X | Y | Z | +/-N |
| TODO comments | X | Y | Z | +/-N |
| Stub implementations | X | Y | Z | +/-N |
| Test coverage gaps | X | Y | Z | +/-N |
| Interface gaps | X | Y | Z | +/-N |
| Dependency violations | X | Y | Z | +/-N |
| Missing infra files | X | Y | Z | +/-N |
| Security concerns | X | Y | Z | +/-N |

### New Items (not in backlog)
[List of newly discovered items with file:line, category, suggested team, suggested priority]

### Changed Items (moved/modified since backlog was written)
[List of items where line numbers shifted or context changed]

### Resolved Items (in backlog but no longer present in code)
[List of items that appear to be completed]

### Layer Health Scores
| Layer | Stubs | TODOs | Test Coverage | Build Clean | Score |
|-------|-------|-------|---------------|-------------|-------|
| Foundation | X | Y | Z% | yes/no | A-F |
| Reasoning | X | Y | Z% | yes/no | A-F |
| Metacognitive | X | Y | Z% | yes/no | A-F |
| Agency | X | Y | Z% | yes/no | A-F |
| Business | X | Y | Z% | yes/no | A-F |
| Infrastructure | -- | -- | -- | -- | A-F |
| CI/CD | -- | -- | -- | -- | A-F |

### Recommended Next Phase
Based on layer health scores, recommend which phase/teams to run next.
```

## Post-Discovery Actions

After generating the report:

1. **Update backlog**: Write new items to `AGENT_BACKLOG.md` (or recommend updates)
2. **Update state**: Write current metrics to `.claude/state/orchestrator.json`
3. **Flag blockers**: If any team's work would be blocked by another team's incomplete output, flag it prominently

## Integration with Orchestrator

The orchestrator should run `/discover` instead of (or in addition to) its Step 1 assessment for a more thorough picture. The discovery report feeds directly into phase selection.
