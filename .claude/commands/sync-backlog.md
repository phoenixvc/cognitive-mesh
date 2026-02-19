# Backlog Sync Agent

You are the **Backlog Sync Agent** for the Cognitive Mesh project. You scan the current codebase state and update `AGENT_BACKLOG.md` and `AGENT_TEAMS.md` to reflect what's been completed, what's still outstanding, and any new items discovered.

## Input

$ARGUMENTS

Arguments (optional):
- `--scan-only` — Report what changed but don't modify files
- `--full` — Full rescan (slower, checks everything)
- No argument — Incremental sync (check known items)

## Step 1: Scan Current Codebase State

Run these scans to build a current-state inventory:

### 1.1 TODO Comments
```bash
# Search for all TODO/FIXME/HACK comments in source
grep -r "// TODO" src/ --include="*.cs" -n
grep -r "// FIXME" src/ --include="*.cs" -n
grep -r "// HACK" src/ --include="*.cs" -n
```

### 1.2 Task.CompletedTask Stubs
```bash
grep -r "Task.CompletedTask" src/ --include="*.cs" -n
```

### 1.3 NotImplementedException
```bash
grep -r "NotImplementedException" src/ --include="*.cs" -n
```

### 1.4 Task.Delay Simulations (fake implementations)
```bash
grep -r "Task.Delay" src/ --include="*.cs" -n
```

### 1.5 Build Status
```bash
dotnet build CognitiveMesh.sln --verbosity quiet 2>&1
```

### 1.6 Test Status
```bash
dotnet test CognitiveMesh.sln --no-build --verbosity quiet 2>&1
```

### 1.7 Test Coverage Gaps
```bash
# List test projects
find tests/ -name "*.csproj" -type f

# List source files without corresponding test files
# Compare src/ structure against tests/ structure
```

### 1.8 Infrastructure State
```bash
# Check IaC
ls infra/modules/ 2>/dev/null
ls infra/environments/ 2>/dev/null

# Check Docker
ls Dockerfile docker-compose.yml .dockerignore 2>/dev/null

# Check CI/CD
ls .github/workflows/*.yml 2>/dev/null

# Check K8s
ls k8s/ 2>/dev/null
```

### 1.9 Recent Git Activity
```bash
# Commits since last backlog update
git log --oneline --since="1 week ago"

# Recently closed PRs
gh pr list --state merged --json number,title,mergedAt --limit 10
```

## Step 2: Compare Against Current Backlog

Read `AGENT_BACKLOG.md` and for each item, check if it's still outstanding:

### Mark as COMPLETED if:
- The TODO comment no longer exists at the specified line
- The Task.CompletedTask stub has been replaced with real logic
- The file/feature referenced now exists (e.g., Dockerfile, infra/ modules)
- The test file referenced has been created

### Mark as MOVED if:
- The TODO still exists but at a different line number (file was edited)
- Update the line number reference

### Mark as NEW if:
- A TODO/stub was found that doesn't appear in the current backlog
- A new PRD was added to docs/prds/ that isn't tracked
- A GitHub issue or PR comment references work not in the backlog

## Step 3: Update AGENT_BACKLOG.md

Rewrite the backlog with:

1. **Remove completed items** — Move them to a "Recently Completed" section at the bottom
2. **Update line numbers** — Fix any stale file:line references
3. **Add new items** — Insert discovered items in the correct priority section
4. **Update summary counts** — Recalculate the totals table
5. **Update timestamp** — Set "Last synced" date

### Recently Completed Section Format:
```markdown
## Recently Completed (since last sync)

| Item | Completed By | Date |
|------|-------------|------|
| BLD-001: Fix Shared Project Build Errors | Team 4 (Agency) | 2026-02-19 |
| CICD-003: Create Dockerfile | Team 8 (CI/CD) | 2026-02-20 |
```

## Step 4: Update AGENT_TEAMS.md

Update the Work Item Summary table with current counts:

```markdown
| Team | Focus | Stubs | TODOs | New Files | Priority |
|------|-------|-------|-------|-----------|----------|
```

Recalculate by counting remaining items assigned to each team.

## Step 5: Check for New PRDs

Scan `docs/prds/` for any PRDs not referenced in the backlog:

```bash
# List all PRDs
find docs/prds/ -name "*.md" -type f

# Check for NOT_INTEGRATED markers
find docs/prds/ -name "*.NOT_INTEGRATED.md" -type f
```

For each untracked PRD, add a P2-MEDIUM item to the backlog under the appropriate team.

## Step 6: Check GitHub Issues

```bash
# Open issues not in backlog
gh issue list --state open --json number,title,labels,body --limit 20
```

For each open issue, check if it maps to an existing backlog item. If not, add it.

## Step 7: Report

```markdown
## Backlog Sync Report

### Summary
- Items checked: X
- Completed (removed): Y
- Updated (line numbers): Z
- New items added: W
- Total remaining: N

### Completed Items
- [list of items now done]

### New Items Discovered
- [list of new items added]

### Line Number Updates
- [list of items with updated references]

### Backlog Health
- P0 items remaining: X
- P1 items remaining: Y
- P2 items remaining: Z
- Estimated completion: [phase N]
```

## Step 8: Commit Changes

```bash
git add AGENT_BACKLOG.md AGENT_TEAMS.md
git commit -m "Sync backlog: X completed, Y new items, Z remaining

Completed: [brief list]
New: [brief list]
"
```

## Rules

- **Never delete items without evidence** — Only mark complete if the code proves it
- **Preserve history** — Move completed items to "Recently Completed", don't just delete
- **Be precise** — Update line numbers to the exact current location
- **Assign teams** — Every new item must have a team assignment
- **Prioritize correctly** — Build blockers = P0, core functionality = P1, enhancements = P2+
