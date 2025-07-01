# NFR Implementation Guide  
_Path: docs/governance/nfr/implementation-guides/NFR-Implementation-Guide.md_

> “Compliance is not a checklist; it’s an engineering practice.”  
> — Cognitive Mesh Security Team

This guide translates the Global Non-Functional Requirements (NFR) Appendix into **hands-on implementation steps** for engineers.  
Each section covers:

* 🔑 Key requirement recap  
* 🛠️ Step-by-step implementation checklist  
* 💻 Code/config templates  
* ✔️ Validation tips & CI gating  

Follow the check-list **before every PR merge** and during quarterly NFR audits.

---

## 1  Security

### 1.1 Data in Transit (TLS ≥ 1.2)

| Step | Action |
|------|--------|
| 1 | Force HTTPS in ASP.NET Core: |
|   | ```csharp\r\nbuilder.WebHost.ConfigureKestrel(o =>\r\n{\r\n    o.ConfigureHttpsDefaults(cfg => cfg.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13);\r\n});\r\napp.UseHttpsRedirection();\r\n``` |
| 2 | Disable legacy protocols/load-balancer ciphers (`tls-policy.yaml` for Nginx/Envoy). |
| 3 | Use **HSTS** in production `app.UseHsts();`. |
| 4 | Unit-test with OWASP ZAP or `dotnet dev-cert https --trust`. |

### 1.2 Data at Rest (AES-256)

Terraform module snippet for Azure SQL:

```hcl
resource "azurerm_mssql_database" "mesh_db" {
  ...
  transparent_data_encryption_enabled = true   # AES-256
}

resource "azurerm_key_vault_key" "cmk" {
  name         = "mesh-cmk"
  key_type     = "RSA"
  key_size     = 4096
}
```

Rotation: schedule `az keyvault key rotate --name mesh-cmk`.

### 1.3 Access Control & Multitenancy

```csharp
// Repository base class – enforces tenant filter on EVERY query
public abstract class TenantRepository<T>
{
    protected readonly DbContext _db;
    protected readonly string _tenantId;

    protected IQueryable<T> Scoped() => _db.Set<T>().Where(e => e.TenantId == _tenantId);
}
```

Checklist:
1. **Every** auth token contains `TenantId` claim.  
2. Middleware injects claim into `HttpContext.Items["TenantId"]`.  
3. All EF/SQL/gRPC queries call `Scoped()`.

### 1.4 Secure SDLC

CI pipeline `azure-pipelines.yml`:

```yaml
- task: UseDotNet@2
  inputs: { packageType: 'sdk', version: '9.0.x' }

- script: dotnet build --configuration Release
- script: dotnet test --configuration Release --collect:"XPlat Code Coverage"

# Static analysis
- task: SonarCloudPrepare@1
- task: SonarCloudAnalyze@1
- task: SonarCloudPublish@1
# Dependency scan
- task: Snyk@1
```

Merge blocked on **zero high-severity** issues.

---

## 2  Telemetry & Audit Logging

### Quick-Start Template (Serilog + OpenTelemetry)

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("TenantId", () => TenantContext.Id)
    .WriteTo.OpenTelemetry(opts =>
    {
        opts.Endpoint = "http://otelcollector:4317";
        opts.ResourceAttributes = new Dictionary<string,string>{
          ["service.name"]="cognitive-mesh-api"};
    })
    .WriteTo.File("audit.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 365)
    .CreateLogger();
```

Checklist:
1. Log **actor**, **action**, **resource**, **result**, **correlationId**.  
2. Use JSON logs → easier KQL/Splunk queries.  
3. **Immutability**: write audit logs to append-only storage (`S3 Object Lock`, `Azure WORM`).  

---

## 3  Versioning & Compatibility

### Semantic Version Bump Script (`scripts/bump-version.ps1`)

```powershell
param([ValidateSet('major','minor','patch')][string]$level)

(Get-Content WidgetDefinition.json | ConvertFrom-Json) `
    | ForEach-Object {
        $ver=[version]$_.version
        $new= [version]::new($ver.Major + ($level -eq 'major'),
                             $ver.Minor + ($level -eq 'minor'),
                             $ver.Build + ($level -eq 'patch'))
        $_.version = "$new"
        $_
    } | ConvertTo-Json | Set-Content WidgetDefinition.json
git commit -am "chore: bump version -> $new"
```

CI gate: reject PR if breaking change label present **and** semver patch only.

---

## 4  Privacy & Data Governance

### Consent Dialog Component (React)

```tsx
export function ConsentPrompt({widget, onAccept}: Props){
  const [open,setOpen]=useState(true)
  return <Modal open={open}>
    <h2>{widget.title} – Data Access Request</h2>
    <ul>
      {widget.permissions.map(p=> <li key={p}>{p}</li>)}
    </ul>
    <button onClick={() => {saveConsent(widget.id); setOpen(false); onAccept();}}>
        I Agree
    </button>
  </Modal>
}
```

Server enforcement (`WidgetOrchestrator.ValidateRequest`) throws if consent record missing.

---

## 5  Compliance

### SOC 2 Evidence Automation

* **IaC snapshots** stored in `artifacts/iac/`.
* Daily `az policy state list` export into Audit Storage.  
* Quarterly **access review** script:

```bash
az ad group member list --group "cognitive-mesh-prod-admins" \
   | jq '.[].userPrincipalName' > reports/access-review-$(date +%F).txt
```

---

## 6  Quality Gates & SLAs

Jarvis load test config (`k6`):

```js
import http from 'k6/http';
export let options={vus:200,duration:'30s',thresholds:{
  http_req_duration:['p(95)<2000'] // 95% under 2s
}};
export default () => { http.get('https://mesh.example.com/dashboard'); }
```

Fail pipeline if thresholds unmet.

---

## 7  Observability & Tracing

**.NET 9 auto-instrumentation**

```csharp
builder.Services.AddOpenTelemetry()
   .WithTracing(b => b
      .AddAspNetCoreInstrumentation()
      .AddHttpClientInstrumentation()
      .AddSource("CognitiveMesh.*"))
   .WithMetrics(m => m.AddRuntimeInstrumentation());
```

Check: traces visible in Jaeger with consistent `traceId`.

---

## 8  Rate Limiting & Throttling

Nginx config:

```
limit_req_zone $binary_remote_addr zone=global:10m rate=30r/s;
limit_req_zone $token zone=tenant:10m rate=10r/s;

map $http_authorization $token {
    default "anon";
    "~^Bearer (.+)$" $1;
}
```

Unit test: call `/api/ping` > limit and expect `429`.

---

## 9  Backup & DR

* **Backups**: `az postgres flexible-server backup schedule update --frequency 60`.  
* **DR drills**: quarterly run `drill-runbook.ps1` → restores infra to **dr‐sandbox** region and runs smoke tests.

---

## 10  Configuration & Secrets

### Key Vault Reference in `appsettings.json`

```json
"ConnectionStrings": {
  "MeshDb": "@Microsoft.KeyVault(SecretUri=https://kv-prod.vault.azure.net/secrets/mesh-db)"
}
```

CI check: `grep -R "password=" src/` must be empty.

---

## 11  Incident Response

* **PagerDuty** schedule file at `runbooks/ir/pagerduty.json`.  
* Severity matrix in `runbooks/ir/severity.md`.  
* Post-mortem template in `docs/governance/nfr/templates/postmortem.md`.

---

## 12  Data Residency

Terraform tag example:

```hcl
resource "azurerm_storage_account" "eu" {
  location            = "northeurope"
  min_tls_version     = "TLS1_2"
  tags = { "DataRegion" = "EU" }
}
```

Audit query:

```kql
AzureActivity | where ResourceTags.DataRegion != TenantPreferredRegion
```

---

## 13  Chaos Engineering

* Schedule `chaos run k8s-node-shutdown --percent 30 --duration 5m`.  
* Record outcome in `chaos-results` table; attach to SLA report.

---

## 14  Developer Sandbox

Docker-compose for local shell:

```yaml
version: "3.9"
services:
  shell:
    image: cognitive-mesh/shell:latest
    ports: ["3000:80"]
    environment:
      - ENV=dev
      - DISABLE_PROD_DATA=true
```

---

## PRD Boilerplate Snippet

```md
### NFR Compliance
| Category | Compliance | Notes |
|----------|------------|-------|
| Security | ✅ |  TLS enforced |
| Telemetry| ✅ |  OpenTelemetry enabled |
| … | … | … |

> _Deviation:_ **None** (or fill table below)

| NFR Ref | Deviation | Justification | Mitigation |
|---------|-----------|---------------|------------|
```

---

## CI Pipeline “NFR Gate”

Add **`/eng/nfr-check.ps1`**:

```powershell
./scripts/run_security_scan.ps1
./scripts/run_k6_tests.ps1
./scripts/check_access_control.ps1
if ($LASTEXITCODE -ne 0) { exit 1 }
```

Pipeline fails if **any** NFR script exits non-zero.

---

## Quarterly Audit Playbook

1. Run `eng/nfr-full-audit.ps1`  
2. Export results to `audit/$(date +%Y-Q%q)/`  
3. Tag Jira issues for failed controls  
4. Present findings at Security Council review

---

### Need Help?

* **#mesh-security** – slack channel  
* **security@mesh** – email  
* Confluence → _Cognitive Mesh / NFR_ for live docs & updates

---

_Last updated: 2025-07-01_  
_Maintainer: Platform Security & Reliability Guild_
