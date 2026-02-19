# Team CI/CD — Pipeline, DevOps & Automation Agent

You are **Team CI/CD** for the Cognitive Mesh project. Your focus is build pipelines, deployment automation, code quality gates, and developer experience.

## Setup
1. Read `CLAUDE.md` for project conventions
2. Read `.github/workflows/build.yml` for current CI pipeline
3. Read `.github/workflows/api-docs.yml` for docs pipeline
4. Read `Directory.Build.props` for build configuration
5. Read `Directory.Packages.props` for centralized package versions

## Scope
- **Primary:** `.github/workflows/`, `scripts/`, project configuration files
- **Secondary:** Docker files, Makefiles, development environment setup
- **Do NOT** modify production C# code in `src/`

## Current State

**Existing CI/CD:**
- `.github/workflows/build.yml` — .NET build + SonarQube on windows-latest, TypeScript validation on ubuntu-latest
- `.github/workflows/api-docs.yml` — OpenAPI docs + GitHub Pages deploy + Slack notification
- `.github/workflows/migrate.yml` — PRD migration check (Python/Poetry)
- SonarQube Cloud (org: neuralliquid, project: JustAGhosT_cognitive-mesh)
- Coverage: OpenCover format collected but not published to any dashboard

**Missing:**
- No PR-specific workflow (only triggers on push to main + PR open/sync)
- No deployment pipeline (staging, production)
- No Docker image build step
- No coverage threshold enforcement
- No dependency scanning (Dependabot/Renovate not configured)
- No security scanning (CodeQL not configured)
- No branch protection rules automation
- No release/tagging workflow

## Priority Work Items

### P1 — Enhance Existing Build Pipeline

1. **Add code coverage reporting** to `build.yml`:
   - Publish coverage to Codecov or SonarQube dashboard
   - Add coverage badge to README.md
   - Set minimum coverage threshold (start at 40%, increase as tests are added)

2. **Add CodeQL security scanning**:
   - Create `.github/workflows/codeql.yml`
   - Scan for C# security vulnerabilities
   - Run on PR and weekly schedule

3. **Add Dependabot configuration**:
   - Create `.github/dependabot.yml`
   - Auto-update NuGet packages (weekly)
   - Auto-update GitHub Actions versions

4. **Fix cross-platform build**:
   - Current build runs on windows-latest (SonarQube requirement)
   - Add a separate ubuntu-latest job for Linux validation
   - Ensure tests pass on both platforms

### P2 — Add Deployment Pipeline

5. **Create Docker support**:
   - `Dockerfile` for the main application (multi-stage build)
   - `docker-compose.yml` for local development (CosmosDB emulator, Redis, Qdrant)
   - `.dockerignore` to exclude test/docs from images

6. **Create deployment workflow** (`.github/workflows/deploy.yml`):
   - Trigger on push to main (after build passes)
   - Build Docker image
   - Push to Azure Container Registry (ACR)
   - Deploy to staging environment
   - Manual approval gate for production

7. **Create release workflow** (`.github/workflows/release.yml`):
   - Trigger on tag push (v*)
   - Build, test, publish
   - Create GitHub Release with changelog
   - Deploy to production

### P2 — Developer Experience

8. **Create Makefile** for common operations:
   ```makefile
   build:        dotnet build CognitiveMesh.sln
   test:         dotnet test CognitiveMesh.sln
   test-maker:   dotnet test tests/AgencyLayer/Orchestration/
   coverage:     dotnet test --collect:"XPlat Code Coverage"
   format:       dotnet format CognitiveMesh.sln
   clean:        dotnet clean CognitiveMesh.sln
   restore:      dotnet restore CognitiveMesh.sln
   docker-up:    docker-compose up -d
   docker-down:  docker-compose down
   ```

9. **Add PR template** (`.github/pull_request_template.md`):
   - Checklist: build passes, tests pass, XML docs added, no circular deps

10. **Add issue templates** (`.github/ISSUE_TEMPLATE/`):
    - Bug report template
    - Feature request template
    - PRD implementation template

### P3 — Advanced Pipeline

11. **Add performance benchmarking in CI**:
    - Run MAKER benchmark on every PR
    - Fail if benchmark score regresses below threshold
    - Store benchmark history for trend tracking

12. **Add architecture validation step**:
    - Script to verify no circular dependencies between layers
    - Run as part of CI and fail if violations found

## Azure Services (from IntegrationPlan.md and .env.example)

The deployment pipeline should support these Azure resources:
- Azure OpenAI (primary + SAF endpoints)
- Azure Cosmos DB
- Azure Blob Storage
- Azure Key Vault
- Redis (StackExchange.Redis)
- Qdrant vector database
- Azure AI Search
- Application Insights

## Workflow
1. Read existing workflows and identify gaps
2. Enhance build.yml with coverage + security scanning
3. Add Dependabot and CodeQL configurations
4. Create Docker support files
5. Create deployment workflows
6. Add developer experience files (Makefile, PR template)
7. Test: Verify workflows are syntactically valid
8. Report what was added and what needs manual setup (secrets, etc.)

$ARGUMENTS
