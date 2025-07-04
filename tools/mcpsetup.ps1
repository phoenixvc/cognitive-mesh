<#
.SYNOPSIS
  Sets env-vars for Roo! MCP using Azure CLI credentials.
.DESCRIPTION
  - Uses your az-login via DefaultAzureCredential (no SP creation).
  - Prompts for Azure OpenAI if it can’t auto-retrieve.
  - Attempts to pull Search, Storage & Key Vault settings, else warns.
#>

# 1. Ensure you’re logged in and subscription is selected
az login
az account set --subscription "22f9eb18-6553-4b7d-9451-47d0195085fe"

# 2. Capture subscription & tenant
$acct = az account show --query '{id:id,tenantId:tenantId}' -o json | ConvertFrom-Json
$env:AZURE_SUBSCRIPTION_ID = $acct.id
$env:AZURE_TENANT_ID       = $acct.tenantId

Write-Host "▶ Subscription: $($env:AZURE_SUBSCRIPTION_ID)"
Write-Host "▶ Tenant:       $($env:AZURE_TENANT_ID)"

# 3. Skip SP creation – use existing az login credentials
Write-Host "▶ Authentication: using DefaultAzureCredential (az login)"

# 4. Resource group
$rg = "dev-euw-rg-phoenixvc-ai"

# 5. Azure OpenAI – prompt if CLI lookup fails
try {
  $key = az cognitiveservices account keys list -g $rg -n dev-euw-aiproj-phoenixvc-ai --query key1 -o tsv
  if ($key) {
    $env:AZURE_OPENAI_KEY      = $key.Trim()
    $env:AZURE_OPENAI_ENDPOINT = "https://dev-euw-aiproj-phoenixvc-ai.openai.azure.com/"
    Write-Host "✔ Retrieved Azure OpenAI key & endpoint"
  } else { throw "empty" }
}
catch {
  Write-Warning "Could not auto-retrieve Azure OpenAI key. Please enter manually."
  $env:AZURE_OPENAI_KEY      = Read-Host "Enter your Azure OpenAI KEY"
  $env:AZURE_OPENAI_ENDPOINT = Read-Host "Enter your Azure OpenAI ENDPOINT (e.g. https://<name>.openai.azure.com/)"
}

# 6. Azure Cognitive Search
try {
  $searchKey  = az search admin-key list -g $rg -n dev-euw-aisearch-phoenixvc-ai484040377717 --query primaryKey -o tsv
  $searchHost = az search service show    -g $rg -n dev-euw-aisearch-phoenixvc-ai484040377717 --query hostName   -o tsv
  $env:AZURE_COGSEARCH_KEY      = $searchKey.Trim()
  $env:AZURE_COGSEARCH_ENDPOINT = "https://$searchHost"
  Write-Host "✔ Retrieved Azure Cognitive Search key & endpoint"
}
catch {
  Write-Warning "Could not auto-retrieve Cognitive Search settings. Please set AZURE_COGSEARCH_KEY and AZURE_COGSEARCH_ENDPOINT manually."
}

# 7. Azure Storage (connection string)
try {
  $connStr = az storage account show-connection-string -g $rg -n stdeveuwhubp484040377717 -o tsv
  $env:AZURE_STORAGE_CONNECTION_STRING = $connStr.Trim()
  Write-Host "✔ Retrieved Azure Storage connection string"
}
catch {
  Write-Warning "Could not auto-retrieve Storage connection string. Please set AZURE_STORAGE_CONNECTION_STRING manually."
}

# 8. Azure Key Vault URI
try {
  $vaultUri = az keyvault show -g $rg -n kv-deveuwhu484040377717 --query properties.vaultUri -o tsv
  $env:AZURE_KEYVAULT_URI = $vaultUri.Trim()
  Write-Host "✔ Retrieved Key Vault URI"
}
catch {
  Write-Warning "Could not auto-retrieve Key Vault URI. Please set AZURE_KEYVAULT_URI manually."
}

Write-Host ""
Write-Host "✅  Environment setup complete."
