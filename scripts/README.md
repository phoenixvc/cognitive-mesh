# Scripts Directory

This folder contains utility PowerShell scripts that automate common maintenance and development tasks for the **Cognitive Mesh** code-base.  
They are intentionally isolated from source so that CI/CD pipelines, local developers, and automated agents can invoke them in a predictable way.

## Prerequisites

| Requirement | Recommended Version |
|-------------|---------------------|
| Windows PowerShell 7 (or PowerShell Core on macOS/Linux) | ≥ 7.3 |
| .NET SDK | ≥ 9.0 preview |

Ensure the PowerShell execution policy allows local script execution:

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

---

## Script Catalogue

| Script | Purpose | Typical Usage |
|--------|---------|---------------|
| **add-required-packages.ps1** | Installs all production NuGet packages declared in `Directory.Packages.props` and synchronises package versions across every `*.csproj`. | `pwsh ./scripts/add-required-packages.ps1` |
| **add-test-packages.ps1** | Adds or upgrades testing dependencies (xUnit, Moq, coverlet, etc.) to every test project under `tests/`. Safe to run repeatedly— idempotent. | `pwsh ./scripts/add-test-packages.ps1` |
| **capture_test_output.ps1** | Executes `dotnet test` and captures the console output into `build/test_output_full.txt`, `build/test_errors.txt`, and a condensed `build/test_output.txt` summary. Useful for CI logs. | `pwsh ./scripts/capture_test_output.ps1` |
| **replace-newtonsoft.ps1** | Scans the solution for `Newtonsoft.Json` references and replaces them with `System.Text.Json`. Updates `*.csproj` and code files where feasible, then runs `dotnet format`. | `pwsh ./scripts/replace-newtonsoft.ps1` |
| **run_tests.ps1** | Quick wrapper around `dotnet test` with sensible defaults (`--no-build --verbosity minimal`). Ideal for local TDD loops. | `pwsh ./scripts/run_tests.ps1` |
| **update-to-net9.ps1** | Bulk-updates all project files to target **.NET 9.0**, updates SDK version in `global.json`, restores packages, and triggers a solution build. | `pwsh ./scripts/update-to-net9.ps1` |

---

## Examples

### Install production packages and run the solution

```powershell
pwsh ./scripts/add-required-packages.ps1
dotnet build CognitiveMesh.sln
```

### Upgrade to the latest .NET 9 preview and run tests

```powershell
pwsh ./scripts/update-to-net9.ps1
pwsh ./scripts/run_tests.ps1
```

### Capture detailed test logs for CI troubleshooting

```powershell
pwsh ./scripts/capture_test_output.ps1
# Logs will appear under /build
```

---

## Contributing

If you add new scripts:

1. Place them in this folder.  
2. Document them here with a short description and usage example.  
3. Keep scripts idempotent and non-interactive wherever possible.
