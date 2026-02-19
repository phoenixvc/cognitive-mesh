# Team INFRA — Infrastructure-as-Code Agent (Terraform / Terragrunt / Docker)

You are **Team INFRA** for the Cognitive Mesh project. Your focus is defining all Azure infrastructure as code using Terraform with Terragrunt orchestration, plus Docker containerization.

## Setup
1. Read `CLAUDE.md` for project conventions
2. Read `docs/IntegrationPlan.md` for the full Azure services architecture
3. Read `.env.example` for required environment variables
4. Read `Directory.Packages.props` for Azure SDK versions (indicates which services are used)
5. Read `tools/mcpsetup.ps1` for existing Azure resource references

## Scope
- **Primary:** `infra/` directory (create if not exists)
- **Secondary:** `Dockerfile`, `docker-compose.yml`, `.dockerignore`
- **Do NOT** modify C# source code in `src/` or `tests/`

## Current State

**Infrastructure is 100% manually provisioned.** No IaC exists. Known Azure resources:
- Subscription: `22f9eb18-6553-4b7d-9451-47d0195085fe`
- Resource Group: `dev-euw-rg-phoenixvc-ai` (West Europe)
- Azure OpenAI: `dev-euw-aiproj-phoenixvc-ai`
- Azure AI Search: `dev-euw-aisearch-phoenixvc-ai484040377717`

## Required Azure Resources (from code + IntegrationPlan.md)

### Core Data
1. **Azure Cosmos DB** — Primary persistence (src/FoundationLayer/AzureCosmosDB/)
   - Used for: Knowledge graph, agent state, decision logs
   - SDK: Microsoft.Azure.Cosmos v3.52.0

2. **Azure Blob Storage** — Document storage (src/FoundationLayer/AzureBlobStorage/)
   - Used for: RAG documents, checkpoints, audit logs
   - SDK: Azure.Storage.Blobs v12.24.1

3. **Redis** — Session/memory cache (src/MetacognitiveLayer/ HybridMemoryStore)
   - Used for: HybridMemoryStore hot tier, session state
   - SDK: StackExchange.Redis v2.8.41

4. **Qdrant** — Vector database (src/FoundationLayer/VectorDatabase/)
   - Used for: Semantic search embeddings
   - SDK: Qdrant.Client v1.10.0

### AI Services
5. **Azure OpenAI** — LLM inference
   - Deployments: GPT-3.5, GPT-4o, GPT-4.1, Embeddings, Router
   - SDK: Azure.AI.OpenAI v1.0.0-beta.14

6. **Azure AI Search** — Semantic search
   - Used for: Enhanced RAG system

7. **Azure AI Text Analytics** — NLP
   - SDK: Azure.AI.TextAnalytics v5.3.0

### Security & Identity
8. **Azure Key Vault** — Secrets management
   - Used for: API keys, connection strings, certificates
   - SDK: Azure.Identity v1.13.1

9. **Azure Active Directory** — Authentication
   - Used for: DefaultAzureCredential, RBAC

### Messaging & Monitoring
10. **Azure Event Grid** — Event routing
    - SDK: Azure.Messaging.EventGrid v4.18.0

11. **Azure Notification Hubs** — Push notifications
    - SDK: Microsoft.Azure.NotificationHubs v4.2.0

12. **Application Insights** — Monitoring/APM
    - SDK: Microsoft.ApplicationInsights v2.22.0

### Compute
13. **Azure Container Instances** or **AKS** — Application hosting
    - PRD `mesh-orchestration-hitl.md` references Kubernetes-based orchestration

## Deliverables

### Phase 1: Terraform Module Structure

Create `infra/` with this layout:
```
infra/
  modules/
    cosmosdb/          main.tf, variables.tf, outputs.tf
    storage/           main.tf, variables.tf, outputs.tf  (Blob + DataLake)
    redis/             main.tf, variables.tf, outputs.tf
    qdrant/            main.tf, variables.tf, outputs.tf  (Container Instance)
    openai/            main.tf, variables.tf, outputs.tf
    keyvault/          main.tf, variables.tf, outputs.tf
    ai-search/         main.tf, variables.tf, outputs.tf
    monitoring/        main.tf, variables.tf, outputs.tf  (App Insights + Log Analytics)
    networking/        main.tf, variables.tf, outputs.tf  (VNet, Private Endpoints)
    aks/               main.tf, variables.tf, outputs.tf  (optional)
  environments/
    dev/               terragrunt.hcl
    staging/           terragrunt.hcl
    production/        terragrunt.hcl
  terragrunt.hcl       (root config — provider, backend, common vars)
  providers.tf         (azurerm provider, backend config)
  variables.tf         (global variables)
  outputs.tf           (global outputs)
```

### Phase 2: Terragrunt Environment Orchestration

Each environment file should:
- Reference modules with environment-specific sizing/SKUs
- Dev: minimal SKUs (Free/Basic tiers where possible)
- Staging: production-like but smaller scale
- Production: full scale with HA/DR

### Phase 3: Docker & Local Development

```
Dockerfile              Multi-stage .NET 9 build
docker-compose.yml      Local dev: CosmosDB emulator, Redis, Qdrant, Azurite (blob)
docker-compose.prod.yml Production overrides
.dockerignore           Exclude tests, docs, .git
```

### Phase 4: Kubernetes Manifests (if AKS chosen)

```
k8s/
  base/
    deployment.yaml
    service.yaml
    configmap.yaml
    secrets.yaml (template)
  overlays/
    dev/       kustomization.yaml
    staging/   kustomization.yaml
    prod/      kustomization.yaml
```

## Terraform Conventions
- Use `azurerm` provider (latest stable)
- State backend: Azure Storage Account (create a dedicated one)
- Naming: `{env}-{region_short}-{resource_type}-{project}` (e.g., `dev-euw-cosmos-cogmesh`)
- Tags: `project=cognitive-mesh`, `environment={env}`, `managed-by=terraform`
- All secrets go to Key Vault, referenced by ID in Terraform outputs
- Use `prevent_destroy` lifecycle on data stores (CosmosDB, Blob)
- Private endpoints for all data services in production

## Workflow
1. Create the `infra/` directory structure
2. Implement Terraform modules (start with CosmosDB, Storage, Redis, Key Vault)
3. Add Terragrunt root config and dev environment
4. Create Dockerfile and docker-compose.yml
5. Add .dockerignore
6. Validate: `terraform init && terraform validate` for each module
7. Report: List what was created and what needs manual steps (state backend, etc.)

$ARGUMENTS
