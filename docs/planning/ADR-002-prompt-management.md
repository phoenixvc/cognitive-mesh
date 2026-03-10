# ADR-002: Prompt Management for Agent Teams

**Status:** Accepted
**Date:** 2026-03-10
**Decision Makers:** Architecture Team
**Context:** AgencyLayer AgentTeamFramework

---

## Context

Agent teams built on `AgentTeamEngineBase` define system prompts as inline `const string` fields in their engine classes. RoadmapCrewEngine, the first team, has 6 prompts totaling ~200 lines of prompt text embedded directly in C#. As more teams are added (RepoGuardian, BacklogCrew, CommsCrew), this approach creates several problems:

| Problem | Impact |
|---------|--------|
| Prompts embedded in C# code | Prompt changes require recompilation |
| No versioning | Cannot A/B test or roll back prompts |
| No separation of concerns | Prompt engineering mixed with orchestration logic |
| No reuse | Common patterns (JSON output format, role preamble) duplicated |
| No validation | Malformed prompts discovered only at runtime |

## Decision

Introduce a **prompt registry** abstraction in `AgentTeamFramework` with YAML-based file storage as the default adapter.

### Architecture

```
AgentTeamFramework/
    Prompts/
        IPromptRegistryPort.cs      — Port for loading prompt templates
        PromptTemplate.cs           — Immutable prompt data model
        PromptMetadata.cs           — Version, author, tags
        YamlPromptRegistryAdapter.cs — Loads prompts from YAML files
```

### Design Decisions

#### 1. YAML over JSON for prompt files

| Factor | YAML | JSON |
|--------|------|------|
| Multiline strings | Native block scalars (`\|`) | Escaped `\n` or arrays |
| Comments | Supported | Not supported |
| Human readability | Superior for long text | Cluttered with quotes |
| .NET support | YamlDotNet (mature, MIT) | Built-in `System.Text.Json` |

**Decision:** YAML. Prompts are primarily long-form natural language text. YAML's block scalar syntax (`|`) preserves formatting without escaping, and comments enable inline documentation for prompt engineering decisions.

#### 2. File-per-team layout

```
src/AgencyLayer/RoadmapCrew/Prompts/
    vision-keeper.prompt.yaml
    market-scanner.prompt.yaml
    priority-ranker.prompt.yaml
    dependency-mapper.prompt.yaml
    milestone-tracker.prompt.yaml
    roadmap-synthesizer.prompt.yaml
```

Each team owns its prompts colocated with its engine. The registry adapter scans by convention (`Prompts/*.prompt.yaml`).

#### 3. Port in AgentTeamFramework, not FoundationLayer

The prompt registry is specific to the agent team pattern. Placing it in `AgentTeamFramework` keeps it cohesive with `AgentDefinitionRecord` and `AgentTeamEngineBase`. If a more general prompt service is needed later, it can be extracted to Foundation.

#### 4. Immutable templates with metadata

```yaml
# vision-keeper.prompt.yaml
id: roadmapcrew-vision-keeper
version: "1.0"
name: VisionKeeper
description: Evaluates product vision alignment for roadmap items
tags: [roadmap, vision, alignment]

prompt: |
  You are VisionKeeper, responsible for evaluating
  whether roadmap items align with the product vision...

defaults:
  temperature: 0.3
  maxTokens: 2048
```

Metadata enables filtering, version tracking, and configuration override without touching prompt text.

#### 5. Optional integration, not mandatory

`AgentTeamEngineBase` continues to support inline prompts via `AgentDefinitionRecord.SystemPrompt`. The registry is opt-in — teams migrate at their own pace. The base class gains a `LoadPromptsAsync()` hook that teams can override.

## Consequences

### Positive
- Prompt text separated from orchestration logic
- YAML files are diffable and reviewable in PRs
- Version metadata enables prompt lifecycle management
- Teams can migrate incrementally (inline → YAML)
- Temperature and model defaults colocated with prompt text

### Negative
- Adds YamlDotNet NuGet dependency
- Two ways to define prompts (inline + YAML) during migration period
- File I/O at startup (mitigated by caching after first load)

### Risks
- YAML indentation sensitivity could cause subtle prompt corruption → mitigated by validation on load
- File discovery by convention requires consistent naming → enforced by `*.prompt.yaml` glob pattern

## Alternatives Considered

1. **JSON files** — Rejected: poor multiline string support for natural language prompts
2. **Embedded resources** — Rejected: still requires recompilation, no external override
3. **Database storage** — Rejected: over-engineering for current scale, adds infrastructure dependency
4. **Razor templates** — Rejected: heavy dependency, template syntax overkill for system prompts
5. **Markdown with frontmatter** — Considered: good readability, but YAML frontmatter parsing adds complexity equivalent to full YAML without the structured data benefits
